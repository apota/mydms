using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.ServiceManagement.API.Controllers;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.IO;
using System.Text;

namespace DMS.ServiceManagement.API.Tests.Controllers
{
    public class InspectionsControllerTests
    {
        private readonly Mock<IServiceInspectionService> _mockInspectionService;
        private readonly Mock<ILogger<InspectionsController>> _mockLogger;
        private readonly InspectionsController _controller;

        public InspectionsControllerTests()
        {
            _mockInspectionService = new Mock<IServiceInspectionService>();
            _mockLogger = new Mock<ILogger<InspectionsController>>();
            _controller = new InspectionsController(_mockInspectionService.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetInspectionById_ReturnsOkResult_WithInspection_WhenIdExists()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var inspection = new ServiceInspection
            {
                Id = testId,
                Type = InspectionType.MultiPoint,
                Status = InspectionStatus.Completed,
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                Notes = "Test inspection",
                TechnicianId = Guid.NewGuid(),
                RepairOrderId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            };

            // Act
            var result = _controller.GetInspectionById(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceInspection>(okResult.Value);
            Assert.Equal(testId, returnValue.Id);
        }

        [Fact]
        public void GetInspectionsByRepairOrderId_ReturnsOkResult_WithInspections()
        {
            // Arrange
            var repairOrderId = Guid.NewGuid();

            // Act
            var result = _controller.GetInspectionsByRepairOrderId(repairOrderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<ServiceInspection>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.All(returnValue, inspection => Assert.Equal(repairOrderId, inspection.RepairOrderId));
        }

        [Fact]
        public async Task CreateInspection_ReturnsCreatedAtAction_WithInspection()
        {
            // Arrange
            var newInspection = new ServiceInspection
            {
                RepairOrderId = Guid.NewGuid(),
                TechnicianId = Guid.NewGuid(),
                Type = InspectionType.Safety,
                Status = InspectionStatus.NotStarted,
                Notes = "New inspection test"
            };

            var createdInspection = new ServiceInspection
            {
                Id = Guid.NewGuid(),
                RepairOrderId = newInspection.RepairOrderId,
                TechnicianId = newInspection.TechnicianId,
                Type = newInspection.Type,
                Status = newInspection.Status,
                Notes = newInspection.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockInspectionService.Setup(service => service.CreateInspectionAsync(It.IsAny<ServiceInspection>()))
                .ReturnsAsync(createdInspection);

            // Act
            var result = await _controller.CreateInspection(newInspection);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<ServiceInspection>(createdAtResult.Value);
            Assert.Equal(createdInspection.Id, returnValue.Id);
            Assert.Equal(newInspection.Type, returnValue.Type);
            Assert.Equal(newInspection.Notes, returnValue.Notes);
        }

        [Fact]
        public async Task CreateInspection_ReturnsBadRequest_WhenInspectionIsNull()
        {
            // Act
            var result = await _controller.CreateInspection(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateInspection_ReturnsBadRequest_WhenValidationFails()
        {
            // Arrange
            var invalidInspection = new ServiceInspection
            {
                // Missing required RepairOrderId
                Type = InspectionType.Safety,
                Status = InspectionStatus.NotStarted
            };

            // Act
            var result = await _controller.CreateInspection(invalidInspection);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void UpdateInspection_ReturnsOkResult_WithUpdatedInspection()
        {
            // Arrange
            var existingInspectionId = Guid.NewGuid();
            var updatedInspection = new ServiceInspection
            {
                Id = existingInspectionId,
                RepairOrderId = Guid.NewGuid(),
                TechnicianId = Guid.NewGuid(),
                Type = InspectionType.MultiPoint,
                Status = InspectionStatus.Completed,
                Notes = "Updated inspection notes",
                StartTime = DateTime.UtcNow.AddHours(-3),
                EndTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var result = _controller.UpdateInspection(existingInspectionId, updatedInspection);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceInspection>(okResult.Value);
            Assert.Equal(existingInspectionId, returnValue.Id);
            Assert.Equal(updatedInspection.Notes, returnValue.Notes);
            Assert.Equal(updatedInspection.Status, returnValue.Status);
        }

        [Fact]
        public void UpdateInspection_ReturnsBadRequest_WhenInspectionIsNull()
        {
            // Act
            var result = _controller.UpdateInspection(Guid.NewGuid(), null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void UpdateInspection_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var id = Guid.NewGuid();
            var differentId = Guid.NewGuid();
            var inspection = new ServiceInspection { Id = differentId };

            // Act
            var result = _controller.UpdateInspection(id, inspection);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void UploadInspectionImages_ReturnsOkResult_WithUpdatedInspection()
        {
            // Arrange
            var inspectionId = Guid.NewGuid();
            var mockFiles = new Mock<IFormFileCollection>();
            var mockFile1 = new Mock<IFormFile>();
            var mockFile2 = new Mock<IFormFile>();
            
            mockFile1.Setup(f => f.FileName).Returns("test1.jpg");
            mockFile2.Setup(f => f.FileName).Returns("test2.jpg");
            
            var files = new List<IFormFile> { mockFile1.Object, mockFile2.Object };
            mockFiles.Setup(f => f.Count).Returns(files.Count);
            mockFiles.Setup(f => f.GetEnumerator()).Returns(files.GetEnumerator());

            // Act
            var result = _controller.UploadInspectionImages(inspectionId, mockFiles.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceInspection>(okResult.Value);
            Assert.Equal(inspectionId, returnValue.Id);
            Assert.Equal(2, returnValue.InspectionImages.Count);
            Assert.All(returnValue.InspectionImages, url => Assert.StartsWith("https://example.com/", url));
        }

        [Fact]
        public void UploadInspectionImages_ReturnsBadRequest_WhenNoFilesUploaded()
        {
            // Arrange
            var inspectionId = Guid.NewGuid();
            var mockFiles = new Mock<IFormFileCollection>();
            mockFiles.Setup(f => f.Count).Returns(0);

            // Act
            var result = _controller.UploadInspectionImages(inspectionId, mockFiles.Object);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void AddRecommendations_ReturnsOkResult_WithUpdatedRecommendations()
        {
            // Arrange
            var inspectionId = Guid.NewGuid();
            var recommendations = new List<RecommendedService>
            {
                new RecommendedService 
                { 
                    Description = "Replace brake pads", 
                    Urgency = ServiceUrgency.Soon, 
                    EstimatedPrice = 250.00m 
                },
                new RecommendedService 
                { 
                    Description = "Replace air filter", 
                    Urgency = ServiceUrgency.Future, 
                    EstimatedPrice = 75.00m 
                }
            };

            // Act
            var result = _controller.AddRecommendations(inspectionId, recommendations);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceInspection>(okResult.Value);
            Assert.Equal(inspectionId, returnValue.Id);
            Assert.Equal(recommendations.Count, returnValue.RecommendedServices.Count);
            Assert.All(returnValue.RecommendedServices, r => Assert.Equal(inspectionId, r.ServiceInspectionId));
            Assert.Collection(returnValue.RecommendedServices,
                r => Assert.Equal("Replace brake pads", r.Description),
                r => Assert.Equal("Replace air filter", r.Description));
        }

        [Fact]
        public void AddRecommendations_ReturnsBadRequest_WhenRecommendationsAreNull()
        {
            // Act
            var result = _controller.AddRecommendations(Guid.NewGuid(), null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void AddRecommendations_ReturnsBadRequest_WhenRecommendationsAreEmpty()
        {
            // Act
            var result = _controller.AddRecommendations(Guid.NewGuid(), new List<RecommendedService>());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
