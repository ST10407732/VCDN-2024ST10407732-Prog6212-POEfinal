using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProgPoePart2_6212.Controllers;
using ProgPoePart2_6212.Data;
using ProgPoePart2_6212.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProgPoePart2_6212.Tests
{
    [TestClass]
    public class ManagerClaimsControllerTests
    {
        private ManagerClaimsController _controller;
        private ProgPoePart2_6212Context _context;
        private Mock<ILogger<ManagerClaimsController>> _loggerMock;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ProgPoePart2_6212Context>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new ProgPoePart2_6212Context(options);

            _loggerMock = new Mock<ILogger<ManagerClaimsController>>();
            _controller = new ManagerClaimsController(_context, _loggerMock.Object);

            // Seed data
            _context.LecturerClaims.AddRange(
                new LecturerClaim
                {
                    Id = 1,
                    Status = ClaimStatus.PendingApproval,

                    Notes = "Pending approval",
                    Name = "Test",
                },
                new LecturerClaim
                {
                    Id = 2,
                    Status = ClaimStatus.Approved,

                    Notes = "Approved claim",
                    Name = "Test",
                },
                new LecturerClaim
                {
                    Id = 3,
                    Status = ClaimStatus.Rejected,

                    Notes = "Rejected claim",
                    Name = "Test",
                }
            );
            _context.SaveChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // Test: Approving a claim
        [TestMethod]
        public async Task ApproveClaim_ValidId_ShouldRedirectToAllClaims()
        {
            // Act
            var result = await _controller.ApproveClaim(1);

            // Assert
            var claim = await _context.LecturerClaims.FindAsync(1);
            Assert.AreEqual(ClaimStatus.Approved, claim.Status);

            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("AllClaims", redirectResult.ActionName);
        }

        // Test: Rejecting a claim
        [TestMethod]
        public async Task RejectClaim_ValidId_ShouldRedirectToAllClaims()
        {
            // Act
            var result = await _controller.RejectClaim(1);

            // Assert
            var claim = await _context.LecturerClaims.FindAsync(1);
            Assert.AreEqual(ClaimStatus.Rejected, claim.Status);

            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("AllClaims", redirectResult.ActionName);
        }

        // Test: Approving a non-existing claim
        [TestMethod]
        public async Task ApproveClaim_InvalidId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.ApproveClaim(999); // Non-existing ID

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // Test: Rejecting a non-existing claim
        [TestMethod]
        public async Task RejectClaim_InvalidId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.RejectClaim(999); // Non-existing ID

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // Test: Display pending claims
        // Test: Display pending claims
        [TestMethod]
        public void PendingClaims_ShouldReturnPendingClaims()
        {
            // Act
            var result = _controller.PendingClaims() as ViewResult;
            var model = result.Model as List<LecturerClaim>; // Cast to List instead of IQueryable

            // Assert
            Assert.IsNotNull(model); // Ensure model is not null
            Assert.AreEqual(1, model.Count()); // Check for one pending claim
            Assert.AreEqual(ClaimStatus.PendingApproval, model.First().Status);
        }

        // Test: Display approved claims
        [TestMethod]
        public void ApprovedClaims_ShouldReturnApprovedClaims()
        {
            // Act
            var result = _controller.ApprovedClaims() as ViewResult;
            var model = result.Model as List<LecturerClaim>; // Cast to List instead of IQueryable

            // Assert
            Assert.IsNotNull(model); // Ensure model is not null
            Assert.AreEqual(1, model.Count()); // Check for one approved claim
            Assert.AreEqual(ClaimStatus.Approved, model.First().Status);
        }

        // Test: Display rejected claims
        [TestMethod]
        public void RejectedClaims_ShouldReturnRejectedClaims()
        {
            // Act
            var result = _controller.RejectedClaims() as ViewResult;
            var model = result.Model as List<LecturerClaim>; // Cast to List instead of IQueryable

            // Assert
            Assert.IsNotNull(model); // Ensure model is not null
            Assert.AreEqual(1, model.Count()); // Check for one rejected claim
            Assert.AreEqual(ClaimStatus.Rejected, model.First().Status);
        }

        // Test: Display all claims
        [TestMethod]
        public void AllClaims_ShouldReturnAllClaims()
        {
            // Act
            var result = _controller.AllClaims() as ViewResult;
            var model = result.Model as List<LecturerClaim>; // Cast to List instead of IQueryable

            // Assert
            Assert.IsNotNull(model); // Ensure model is not null
            Assert.AreEqual(3, model.Count()); // Check for all three claims
        }

    }
}