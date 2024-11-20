using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Add this for Include
using ProgPoePart2_6212.Data;
using ProgPoePart2_6212.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ProgPoePart2_6212.Controllers
{
    [Authorize(Roles = "AcademicManager")]
    public class ManagerClaimsController : Controller
    {
        private readonly ProgPoePart2_6212Context _context;
        private readonly ILogger<ManagerClaimsController> _logger;

        public ManagerClaimsController(ProgPoePart2_6212Context context, ILogger<ManagerClaimsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Display pending claims
        public IActionResult PendingClaims()
        {
            var claims = _context.LecturerClaims
                .Include(c => c.User) // Ensure User is included
                .Include(c => c.Documents) // Ensure Documents are included
                .Where(c => c.Status == ClaimStatus.PendingApproval) // Pending approval only
                .ToList();

            return View(claims);
        }
        [HttpPost]
        public async Task<IActionResult> BulkApproveClaims(int[] selectedClaims)
        {
            if (selectedClaims == null || selectedClaims.Length == 0)
            {
                TempData["Message"] = "No claims selected for bulk approval.";
                return RedirectToAction(nameof(AllClaims));
            }

            try
            {
                // Fetch claims that need to be approved
                var claimsToApprove = await _context.LecturerClaims
                    .Include(c => c.User)  // Ensure User is loaded
                    .Where(c => selectedClaims.Contains(c.Id) && c.Status == ClaimStatus.PendingApproval)
                    .ToListAsync();

                // Loop through each claim, approve it, and generate a report
                foreach (var claim in claimsToApprove)
                {
                    claim.Status = ClaimStatus.Approved;

                    // Generate a report for the approved claim
                    await GeneratePdfReportForClaim(claim);
                }

                // Save the changes to the database
                await _context.SaveChangesAsync();

                TempData["Message"] = $"{claimsToApprove.Count} claims approved and reports generated successfully.";
                return RedirectToAction(nameof(AllClaims));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while bulk approving claims.");
                TempData["Error"] = "An error occurred while processing your request.";
                return RedirectToAction(nameof(AllClaims));
            }
        }




        // Display approved claims
        public IActionResult ApprovedClaims()
        {
            var claims = _context.LecturerClaims
                .Include(c => c.User)
                .Include(c => c.Documents)
                .Where(c => c.Status == ClaimStatus.Approved) // Approved claims only
                .ToList();

            return View(claims);
        }

        // Display rejected claims
        public IActionResult RejectedClaims()
        {
            var claims = _context.LecturerClaims
                .Include(c => c.User)
                .Include(c => c.Documents)
                .Where(c => c.Status == ClaimStatus.Rejected) // Rejected claims only
                .ToList();

            return View(claims);
        }
        // Approve a claim and auto-generate a report
        public async Task<IActionResult> ApproveClaim(int id)
        {
            try
            {
                var claim = await _context.LecturerClaims.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
                if (claim == null)
                {
                    _logger.LogWarning("Claim not found with ID: {ClaimId}", id);
                    return NotFound();
                }

                claim.Status = ClaimStatus.Approved;
                await _context.SaveChangesAsync();

                // Generate PDF report for the approved claim
                await GeneratePdfReportForClaim(claim);

                _logger.LogInformation("Claim approved and report generated for ID: {ClaimId}", id);
                return RedirectToAction(nameof(AllClaims));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while approving the claim.");
                return RedirectToAction(nameof(AllClaims));
            }
        }
        private async Task GeneratePdfReportForClaim(LecturerClaim claim)
        {
            if (claim.User == null)
            {
                _logger.LogWarning($"No user associated with claim ID: {claim.Id}");
                return;
            }

            using (var memoryStream = new MemoryStream())
            {
                var writer = new iText.Kernel.Pdf.PdfWriter(memoryStream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                // Add content to the PDF
                var title = new iText.Layout.Element.Paragraph($"Approved Claim Report: {claim.Id}")
                    .SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(title);

                document.Add(new iText.Layout.Element.Paragraph($"Lecturer: {claim.User.FullName}"));
                document.Add(new iText.Layout.Element.Paragraph($"Hours Worked: {claim.HoursWorked}"));
                document.Add(new iText.Layout.Element.Paragraph($"Hourly Rate: {claim.HourlyRate:C}"));
                document.Add(new iText.Layout.Element.Paragraph($"Total Amount: {(claim.HoursWorked * claim.HourlyRate):C}"));
                document.Add(new iText.Layout.Element.Paragraph($"Notes: {claim.Notes ?? "N/A"}"));

                document.Close();

                // Generate a meaningful filename
                var sanitizedLecturerName = string.Concat(claim.User.FullName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
                var filename = $"Claim_{sanitizedLecturerName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var filePath = Path.Combine("wwwroot/reports", filename);

                // Save the PDF
                await System.IO.File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
            }
        }


        public async Task<IActionResult> RejectClaim(int id)
        {
            try
            {
                var claim = await _context.LecturerClaims.FindAsync(id);
                if (claim == null)
                {
                    _logger.LogWarning("Claim not found with ID: {ClaimId}", id);
                    return NotFound();
                }

                claim.Status = ClaimStatus.Rejected;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Claim rejected with ID: {ClaimId}", id);
                return RedirectToAction(nameof(AllClaims)); // Redirect to AllClaims
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rejecting the claim.");
                return RedirectToAction(nameof(AllClaims)); // Redirect to AllClaims on error
            }
        }


        // Display all claims
        public IActionResult AllClaims()
        {
            var claims = _context.LecturerClaims
                .Include(c => c.User)
                .Include(c => c.Documents)
                .ToList();

            return View(claims);
        }
    }
}
