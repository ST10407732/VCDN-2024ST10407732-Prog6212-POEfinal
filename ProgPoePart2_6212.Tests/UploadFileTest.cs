
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProgPoePart2_6212.Models;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace ContractApp.Tests
{
    [TestClass]
    public class FileUploadManagerTest
    {
        private FileUpload _fileUpload;

        // Sample data for testing
        private static List<LecturerClaim> _claims = new List<LecturerClaim>
        {
            new LecturerClaim { Id = 1, Name = "Claim 1", Documents = new List<SuppDocument>() },
            new LecturerClaim { Id = 2, Name = "Claim 2", Documents = new List<SuppDocument>() }
        };

        [TestInitialize]
        public void TestInitialize()
        {
            _fileUpload = new FileUpload(_claims);
        }

        [TestMethod]
        public async Task UploadDocument_ValidFile_ShouldUploadFile()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var fileName = "testfile.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write("This is a test PDF file.");
            writer.Flush();
            ms.Position = 0;

            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");

            // Act
            var result = await _fileUpload.UploadDocument(1, mockFile.Object);

            // Assert
            Assert.AreEqual("FileUploaded", result);
            Assert.AreEqual(1, _claims[0].Documents.Count); // Claim 1 should now have a document

        }

        [TestMethod]
        public async Task UploadDocument_UnsupportedFileType_ShouldReturnError()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("testfile.txt");
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("text/plain");

            // Act
            var result = await _fileUpload.UploadDocument(1, mockFile.Object);

            // Assert
            Assert.AreEqual("UnsupportedFileType", result);
            Assert.AreEqual(1, _claims[0].Documents.Count); // No document should be added
        }

        [TestMethod]
        public async Task UploadDocument_InvalidFile_ShouldReturnError()
        {
            // Act
            var result = await _fileUpload.UploadDocument(1, null); // Passing null to simulate no file

            // Assert
            Assert.AreEqual("InvalidFile", result);
            Assert.AreEqual(1, _claims[0].Documents.Count); // No document should be added
        }

        [TestMethod]
        public async Task UploadDocument_NonExistentClaim_ShouldReturnClaimNotFound()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("testfile.pdf");
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");

            // Act
            var result = await _fileUpload.UploadDocument(999, mockFile.Object); // Non-existent claim ID

            // Assert
            Assert.AreEqual("ClaimNotFound", result);
        }
    }
}