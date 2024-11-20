using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProgPoePart2_6212.Data;
using ProgPoePart2_6212.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProgPoePart2_6212.Validators;

namespace ProgPoePart2_6212.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerClaimsController : Controller
    {
        private readonly ProgPoePart2_6212Context _context;
        private readonly ILogger<LecturerClaimsController> _logger;
        private readonly UserManager<User> _userManager;

        public LecturerClaimsController(ProgPoePart2_6212Context context, ILogger<LecturerClaimsController> logger, UserManager<User> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: SubmitClaim
        public IActionResult SubmitClaim()
        {
            return View();
        }
      
        // GET: SubmitClaim

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(LecturerClaim claim, IFormFile DocumentUpload)
        {
            try
            {
                // Step 1: Validate the claim using the custom validator
                var validator = new LecturerClaimValidator();
                var validationResult = await validator.ValidateAsync(claim);

                // Step 2: If validation fails, add the errors to ModelState and return the view
                if (!validationResult.IsValid)
                {
                    foreach (var failure in validationResult.Errors)
                    {
                        ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
                    }
                    return View(claim); // Return to the form with validation errors
                }

                // Step 3: Retrieve the current user (lecturer) from _userManager
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    _logger.LogWarning("User not found.");
                    return Challenge(); // Redirect to login if no user found
                }

                // Step 4: Associate the claim with the current user and set its status
                claim.User = currentUser;
                claim.Status = ClaimStatus.PendingVerification;

                // Step 5: Save the claim to the database
                await _context.LecturerClaims.AddAsync(claim);
                await _context.SaveChangesAsync();

                // Step 6: Handle file upload (if any)
                if (DocumentUpload != null)
                {
                    // Construct the file path where the document will be saved
                    var uploadDirectory = Path.Combine("Uploads", claim.Id.ToString());
                    Directory.CreateDirectory(uploadDirectory); // Ensure the directory exists

                    var filePath = Path.Combine(uploadDirectory, DocumentUpload.FileName);

                    // Save the document to the specified path
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await DocumentUpload.CopyToAsync(stream);
                    }

                    // Create and save the document entry in the database
                    var document = new SuppDocument
                    {
                        Claim = claim,
                        FileName = DocumentUpload.FileName,
                        FilePath = filePath,
                        FileType = Path.GetExtension(DocumentUpload.FileName),
                        UploadDate = DateTime.Now
                    };

                    _context.Documents.Add(document);
                    await _context.SaveChangesAsync();
                }

                // Step 7: Log the successful claim submission
                _logger.LogInformation("Claim submitted successfully with ID: {ClaimId}", claim.Id);

                // Redirect to TrackClaims page
                return RedirectToAction(nameof(TrackClaims));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting the claim.");
                return View(claim); // Return to the form with the error
            }
        }



        // GET: UploadDocument
        public IActionResult UploadDocument()
        {
            ViewBag.Claims = _context.LecturerClaims
                .Where(c => c.User.Email == User.Identity.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Name} "
                })
                .ToList();

            return View();
        }
        public async Task<IActionResult> ClaimDetails(int id)
        {
            var claim = await _context.LecturerClaims
                .Include(c => c.Documents) // Include associated documents
                .FirstOrDefaultAsync(c => c.Id == id && c.User.Email == User.Identity.Name);

            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        // POST: UploadDocument
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int claimId, IFormFile document)
        {
            if (document == null || document.Length == 0)
            {
                ModelState.AddModelError("Document", "Please upload a valid document.");
                return RedirectToAction(nameof(TrackClaims));
            }

            var claim = await _context.LecturerClaims.FindAsync(claimId);
            if (claim == null)
            {
                _logger.LogWarning("Claim not found with ID: {ClaimId}", claimId);
                return NotFound();
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", claim.Id.ToString());
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, document.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await document.CopyToAsync(stream);
            }

            var suppDocument = new SuppDocument
            {
                Claim = claim,
                FileName = document.FileName,
                FilePath = filePath,
                FileType = Path.GetExtension(document.FileName),
                UploadDate = DateTime.Now
            };

            _context.Documents.Add(suppDocument);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Document uploaded successfully.";
            _logger.LogInformation("Document uploaded for Claim ID: {ClaimId}", claimId);

            return RedirectToAction(nameof(TrackClaims));
        }

        // Track Claims
        public async Task<IActionResult> TrackClaims()
        {
            var claims = await _context.LecturerClaims
                .Include(c => c.User)
                .Include(c => c.Documents)  // Include associated documents
                .Where(c => c.User.Email == User.Identity.Name)
                .ToListAsync();

            return View(claims);
        }
        // Lecturer Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Get the currently logged-in user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge(); // User not logged in, redirect to login
            }

            // Fetch all claims related to the logged-in lecturer
            var claims = await _context.LecturerClaims
                .Where(c => c.User.Id == currentUser.Id) // Only fetch claims for the current lecturer
                .ToListAsync();

            return View(claims); // Pass the claims to the view
        }
        // Action to track individual claim details
        public async Task<IActionResult> TrackClaim(int id)
        {
            var claim = await _context.LecturerClaims
                .Include(c => c.Documents) // Include documents if applicable
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                return NotFound();
            }

            return View(claim); // Pass the claim details to the view
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            // Include the related User entity in the query
            var claim = await _context.LecturerClaims
                .Include(c => c.User) // Ensure that the User entity is loaded
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null || claim.User == null || claim.User.Email != User.Identity.Name)
            {
                return NotFound();
            }

            _context.LecturerClaims.Remove(claim);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Claim deleted successfully.";
            return RedirectToAction(nameof(Dashboard)); // Redirect to the dashboard
        }
        // Action to handle resubmission logic
        // This action handles both GET and POST for ResubmitClaim
        [HttpGet]
        public async Task<IActionResult> ResubmitClaim(int id)
        {
            var claim = await _context.LecturerClaims.FindAsync(id);
            if (claim == null || claim.Status != ClaimStatus.Rejected)
            {
                return NotFound();
            }
            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResubmitClaim(LecturerClaim updatedClaim)
        {
            try
            {
                var claim = await _context.LecturerClaims.FindAsync(updatedClaim.Id);
                if (claim == null || claim.Status != ClaimStatus.Rejected)
                {
                    return NotFound();
                }

                claim.HoursWorked = updatedClaim.HoursWorked;
                claim.HourlyRate = updatedClaim.HourlyRate;
                claim.Notes = updatedClaim.Notes;
                claim.Status = ClaimStatus.PendingApproval;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(TrackClaims));
            }
            catch (Exception)
            {
                return View(updatedClaim);
            }
        }
    }


}

