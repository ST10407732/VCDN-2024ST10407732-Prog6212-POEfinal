using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProgPoePart2_6212.Controllers;
using ProgPoePart2_6212.Data;
using ProgPoePart2_6212.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProgPoePart2_6212.Tests
{
    [TestClass]
    public class CoordinatorClaimsControllerTests
    {
        private CoordinatorClaimsController _controller;
        private ProgPoePart2_6212Context _context;
        private Mock<ILogger<CoordinatorClaimsController>> _loggerMock;

        [TestInitialize]
        public void TestInitialize()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<ProgPoePart2_6212Context>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ProgPoePart2_6212Context(options);

            // Add a user to the in-memory database
            var user = new User
            {
                Id = "test_user_id",
                UserName = "coordinator@example.com",
                Email = "coordinator@example.com",
                FullName = "Coordinator User"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Mock claims (simulating the identity of the ProgrammeCoordinator)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, "ProgrammeCoordinator")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Mock logger
            _loggerMock = new Mock<ILogger<CoordinatorClaimsController>>();

            // Set up the controller
            _controller = new CoordinatorClaimsController(_context, _loggerMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [TestMethod]
        public void PendingClaims_ShouldReturnViewWithClaims()
        {
            // Arrange
            var claims = new List<LecturerClaim>
            {
                new LecturerClaim { HoursWorked = 5, HourlyRate = 500, Status = ClaimStatus.PendingVerification, Name = "Test Lecturer 1",  Notes = "Initial notes" },
                new LecturerClaim { HoursWorked = 10, HourlyRate = 300, Status = ClaimStatus.PendingVerification, Name = "Test Lecturer 2",  Notes = "Initial notes" }
            };
            _context.LecturerClaims.AddRange(claims);
            _context.SaveChanges();

            // Act
            var result = _controller.PendingClaims() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(List<LecturerClaim>));
            var model = result.Model as List<LecturerClaim>;
            Assert.AreEqual(2, model.Count); // Check that there are 2 claims in the view
        }

        [TestMethod]
        public async Task VerifyClaim_ValidId_ShouldRedirectToPendingClaims()
        {
            // Arrange
            var claim = new LecturerClaim { Id = 1, HoursWorked = 5, HourlyRate = 500, Status = ClaimStatus.PendingVerification, Name = "Test Lecturer", Notes = "Initial notes" };
            _context.LecturerClaims.Add(claim);
            _context.SaveChanges();

            // Act
            var result = await _controller.VerifyClaim(claim.Id) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(CoordinatorClaimsController.PendingClaims), result.ActionName);
            Assert.AreEqual(ClaimStatus.PendingApproval, claim.Status); // Ensure the claim status was updated
        }

        [TestMethod]
        public async Task VerifyClaim_InvalidId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.VerifyClaim(99); // Invalid ID

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task RejectClaim_ValidId_ShouldRedirectToPendingClaims()
        {
            // Arrange
            var claim = new LecturerClaim { Id = 1, HoursWorked = 5, HourlyRate = 500, Status = ClaimStatus.PendingVerification, Name = "Test Lecturer", Notes = "Initial notes" };
            _context.LecturerClaims.Add(claim);
            _context.SaveChanges();

            // Act
            var result = await _controller.RejectClaim(claim.Id) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(CoordinatorClaimsController.PendingClaims), result.ActionName);
            Assert.AreEqual(ClaimStatus.Rejected, claim.Status); // Ensure the claim status was updated
        }

        [TestMethod]
        public async Task RejectClaim_InvalidId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.RejectClaim(99); // Invalid ID

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up in-memory database after each test
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}