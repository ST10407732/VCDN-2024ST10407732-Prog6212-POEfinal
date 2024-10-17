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

        // Action to approve a claim
        public async Task<IActionResult> ApproveClaim(int id)
        {
            try
            {
                var claim = await _context.LecturerClaims.FindAsync(id);
                if (claim == null)
                {
                    _logger.LogWarning("Claim not found with ID: {ClaimId}", id);
                    return NotFound();
                }

                claim.Status = ClaimStatus.Approved;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Claim approved with ID: {ClaimId}", id);
                return RedirectToAction(nameof(AllClaims)); // Redirect to AllClaims
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while approving the claim.");
                return RedirectToAction(nameof(AllClaims)); // Redirect to AllClaims on error
            }
        }

        // Action to reject a claim
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
