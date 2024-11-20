using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ProgPoePart2_6212.Data;
using ProgPoePart2_6212.Models;
using ProgPoePart2_6212.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;


namespace ProgPoePart2_6212.Controllers
{
     [Authorize(Roles = "HR")]
    public class HRDashboardController : Controller
    {
        private readonly ProgPoePart2_6212Context _context;
        private readonly ILogger<HRDashboardController> _logger;

        public HRDashboardController(ProgPoePart2_6212Context context, ILogger<HRDashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // HR Dashboard Home (Index)
        public IActionResult Index()
        {
            return View();
        }

        // Display all lecturers (ManageLecturers)
        public IActionResult ManageLecturers()
        {
            var lecturers = _context.Users.Where(u => u.UserName != null).ToList();
            return View(lecturers);
        }

        // Edit Lecturer (for managing individual lecturer details)
        public async Task<IActionResult> EditLecturer(string id)
        {
            var lecturer = await _context.Users.FindAsync(id);
            if (lecturer == null)
            {
                _logger.LogWarning("Lecturer not found with ID: {LecturerId}", id);
                return NotFound();
            }

            return View(lecturer);
        }
        [HttpPost]
        public async Task<IActionResult> EditLecturer(User lecturer)
        {
            if (ModelState.IsValid)
            {
                var existingLecturer = await _context.Users.FindAsync(lecturer.Id);
                if (existingLecturer != null)
                {
                    existingLecturer.FullName = lecturer.FullName;
                    existingLecturer.Email = lecturer.Email;

                    try
                    {
                        _context.Users.Update(existingLecturer);
                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Lecturer details updated successfully.";
                        return RedirectToAction(nameof(ManageLecturers));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating lecturer details.");
                        ModelState.AddModelError("", "An error occurred while updating the lecturer.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Lecturer not found.");
                }
            }

            return View(lecturer); // Return the view with validation errors
        }
        [HttpPost]
        public async Task<IActionResult> DeleteLecturer(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid lecturer ID.";
                return RedirectToAction(nameof(ManageLecturers));
            }

            var lecturer = await _context.Users.FindAsync(id);
            if (lecturer != null)
            {
                try
                {
                    _context.Users.Remove(lecturer);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Lecturer deleted successfully.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting lecturer.");
                    TempData["Error"] = "An error occurred while deleting the lecturer.";
                }
            }
            else
            {
                TempData["Error"] = "Lecturer not found.";
            }

            return RedirectToAction(nameof(ManageLecturers));
        }

        public IActionResult ManageClaims()
        {
            var claims = _context.LecturerClaims
                .Include(c => c.User) // Include Lecturer info
                .ToList();
            return View(claims);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteClaim(int claimId)
        {
            var claim = await _context.LecturerClaims.FindAsync(claimId);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction(nameof(ManageClaims));
            }

            try
            {
                _context.LecturerClaims.Remove(claim);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Claim deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting claim with ID: {ClaimId}", claimId);
                TempData["Error"] = "An error occurred while deleting the claim.";
            }

            return RedirectToAction(nameof(ManageClaims));
        }



        
        // Reject Claim
        public async Task<IActionResult> RejectClaim(int id)
        {
            var claim = await _context.LecturerClaims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = ClaimStatus.Rejected;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageClaims));
            }
            return NotFound();
        }

        // Automatically generate invoice report for approved claims (GenerateReports)
        public IActionResult GenerateReports()
        {
            var claims = _context.LecturerClaims
                .Include(c => c.User)
                .Where(c => c.Status == ClaimStatus.Approved)
                .ToList();

            // Summary Data for the HR Report
            var totalClaims = claims.Count;
            var totalAmountClaimed = claims.Sum(c => c.HoursWorked * c.HourlyRate);
            var averageClaim = claims.Any() ? claims.Average(c => c.HoursWorked * c.HourlyRate) : 0;
            var highestClaim = claims.Any() ? claims.Max(c => c.HoursWorked * c.HourlyRate) : 0;
            var lowestClaim = claims.Any() ? claims.Min(c => c.HoursWorked * c.HourlyRate) : 0;

            // Pass both claims and summary data to the view
            var reportData = new HRReportViewModel
            {
                Claims = claims,
                TotalClaims = totalClaims,
                TotalAmountClaimed = totalAmountClaimed,
                AverageClaim = averageClaim,
                HighestClaim = highestClaim,
                LowestClaim = lowestClaim
            };

            return View(reportData);
        }


        [HttpPost]
        public async Task<IActionResult> BulkProcessClaims(int[] selectedClaims, string action)
        {
            if (selectedClaims == null || !selectedClaims.Any())
            {
                TempData["Error"] = "No claims selected.";
                return RedirectToAction(nameof(ManageClaims));
            }

            var claims = _context.LecturerClaims
     .Where(c => selectedClaims.Contains(c.Id)) // Use 'Id' instead of 'ID'
     .ToList();


            foreach (var claim in claims)
            {
                if (action == "approve")
                {
                    claim.Status = ClaimStatus.Approved;
                }
                else if (action == "reject")
                {
                    claim.Status = ClaimStatus.Rejected;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = $"{claims.Count} claims {action}d successfully.";
            return RedirectToAction(nameof(ManageClaims));
        }
        public IActionResult DownloadPdfReport()
        {
            var claims = _context.LecturerClaims
                .Include(c => c.User)
                .Where(c => c.Status == ClaimStatus.Approved)
                .ToList();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new iText.Kernel.Pdf.PdfWriter(memoryStream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                // Use a bold font for the title
                var titleFont = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                var title = new iText.Layout.Element.Paragraph("Approved Claims Report")
                    .SetFont(titleFont)
                    .SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(title);

                // Create a table with column widths
                var table = new iText.Layout.Element.Table(new float[] { 2, 2, 2, 2, 3 });
                table.SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100));

                // Add header cells
                table.AddHeaderCell("Lecturer");
                table.AddHeaderCell("Hours Worked");
                table.AddHeaderCell("Hourly Rate");
                table.AddHeaderCell("Total Amount");
                table.AddHeaderCell("Notes");

                // Add rows to the table
                foreach (var claim in claims)
                {
                    table.AddCell(claim.User.FullName);
                    table.AddCell(claim.HoursWorked.ToString());
                    table.AddCell(claim.HourlyRate.ToString("C"));
                    table.AddCell((claim.HoursWorked * claim.HourlyRate).ToString("C"));
                    table.AddCell(claim.Notes ?? "N/A");
                }

                document.Add(table);
                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", "ApprovedClaimsReport.pdf");
            }
        }


        public IActionResult DownloadExcelReport()
        {
            var claims = _context.LecturerClaims
                .Include(c => c.User)
                .Where(c => c.Status == ClaimStatus.Approved)
                .ToList();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Approved Claims");
                worksheet.Cell(1, 1).Value = "Lecturer";
                worksheet.Cell(1, 2).Value = "Hours Worked";
                worksheet.Cell(1, 3).Value = "Hourly Rate";
                worksheet.Cell(1, 4).Value = "Total Amount";
                worksheet.Cell(1, 5).Value = "Notes";

                for (int i = 0; i < claims.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = claims[i].Name;
                    worksheet.Cell(i + 2, 2).Value = claims[i].HoursWorked;
                    worksheet.Cell(i + 2, 3).Value = claims[i].HourlyRate.ToString("C");
                    worksheet.Cell(i + 2, 4).Value = (claims[i].HoursWorked * claims[i].HourlyRate).ToString("C");
                    worksheet.Cell(i + 2, 5).Value = claims[i].Notes ?? "N/A";
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ApprovedClaimsReport.xlsx");
                }
            }
        }

        

        public IActionResult ViewReports()
        {
            var reportFiles = Directory.GetFiles("wwwroot/reports")
                .Select(Path.GetFileName)
                .ToList();

            return View(reportFiles);
        }

        public IActionResult DownloadReport(string fileName)
        {
            var filePath = Path.Combine("wwwroot/reports", fileName);
            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "File not found.";
                return RedirectToAction(nameof(ViewReports));
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }
    }
}
