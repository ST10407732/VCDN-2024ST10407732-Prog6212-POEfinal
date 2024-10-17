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
    [Authorize(Roles = "ProgrammeCoordinator")]
    public class CoordinatorClaimsController : Controller
    {
        private readonly ProgPoePart2_6212Context _context;
        private readonly ILogger<CoordinatorClaimsController> _logger;

        public CoordinatorClaimsController(ProgPoePart2_6212Context context, ILogger<CoordinatorClaimsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Display pending claims
        // Display pending claims
        public IActionResult PendingClaims()
        {
            var claims = _context.LecturerClaims
                .Include(c => c.User) // Ensure User is included
                .Include(c => c.Documents) // Ensure Documents are included
                .ToList(); // Get all claims regardless of status

            return View(claims);
        }



        // Action to verify a claim
        public async Task<IActionResult> VerifyClaim(int id)
        {
            try
            {
                var claim = await _context.LecturerClaims.FindAsync(id);
                if (claim == null)
                {
                    _logger.LogWarning("Claim not found with ID: {ClaimId}", id);
                    return NotFound();
                }

                claim.Status = ClaimStatus.PendingApproval;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Claim verified with ID: {ClaimId}", id);
                return RedirectToAction(nameof(PendingClaims));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying the claim.");
                return RedirectToAction(nameof(PendingClaims)); // Or show an error view
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

                claim.Status = ClaimStatus.Rejected; // Set status to Rejected
                await _context.SaveChangesAsync();

                _logger.LogInformation("Claim rejected with ID: {ClaimId}", id);
                return RedirectToAction(nameof(PendingClaims));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rejecting the claim.");
                return RedirectToAction(nameof(PendingClaims)); // Or show an error view
            }
        }

    }
}
