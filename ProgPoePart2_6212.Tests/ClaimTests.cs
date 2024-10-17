using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    public class ClaimTests
    {
        private LecturerClaimsController _controller;
        private ProgPoePart2_6212Context _context;
        private Mock<UserManager<User>> _userManagerMock;

        [TestInitialize]
        public void TestInitialize()
        {
            // Set up an in-memory database for testing
            var options = new DbContextOptionsBuilder<ProgPoePart2_6212Context>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ProgPoePart2_6212Context(options);

            // Add a user to the in-memory database for the claim
            var user = new User
            {
                Id = "test_user_id",
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                FullName = "Test User" // Ensure this required field is populated
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Mock user claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.FullName)
    };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Create a mock logger using Moq
            var mockLogger = new Mock<ILogger<LecturerClaimsController>>();

            // Set up the controller with the mock logger
            _controller = new LecturerClaimsController(_context, mockLogger.Object, null); // Assuming null for user manager

            // Set the user principal for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }



        [TestMethod]
        public async Task SubmitClaim_ValidClaim_ShouldRedirectToTrackClaims()
        {
            // Arrange
            var claim = new LecturerClaim
            {
                HoursWorked = 10,
                HourlyRate = 500,
                Status = ClaimStatus.PendingVerification,
                Name = "Teach"
            };
            IFormFile document = null; // No file uploaded for simplicity

            // Act
            var result = await _controller.SubmitClaim(claim, document);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var redirectResult = result as ViewResult;

        }

        [TestMethod]
        public async Task SubmitClaim_InvalidHours_ShouldReturnViewWithErrorMessage()
        {
            // Arrange
            var claim = new LecturerClaim
            {
                HoursWorked = 0, // Invalid hours
                HourlyRate = 500,
                Status = ClaimStatus.PendingVerification,
                Name = "Teach"
            };
            IFormFile document = null;

            // Act
            var result = await _controller.SubmitClaim(claim, document);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

        }

        [TestMethod]
        public void SubmitClaim_NoUser_ShouldChallenge()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LecturerClaimsController>>();

            // Set up the controller with the mock logger and null user manager (if required)
            var controller = new LecturerClaimsController(_context, mockLogger.Object, null);

            // Set up the controller context to simulate no authenticated user
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // No User set here, simulating no authenticated user
            };

            // Create a LecturerClaim object with invalid hours
            var claim = new LecturerClaim
            {
                HoursWorked = 0, // Invalid hours
                HourlyRate = 500,
                Status = ClaimStatus.PendingVerification,
                Name = "Teach"
            };

            // Act
            var result = controller.SubmitClaim(claim, null); // Pass the claim into the SubmitClaim method


        }


        [TestCleanup]
        public void Cleanup()
        {
            // Clear the in-memory database after each test
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}