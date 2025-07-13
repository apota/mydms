using DMS.PartsManagement.API.Controllers;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;
using DMS.PartsManagement.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DMS.PartsManagement.API.Tests.Controllers
{
    public class CoreTrackingControllerTests
    {
        private readonly Mock<ICoreTrackingService> _mockCoreTrackingService;
        private readonly Mock<ILogger<CoreTrackingController>> _mockLogger;
        private readonly CoreTrackingController _controller;

        public CoreTrackingControllerTests()
        {
            _mockCoreTrackingService = new Mock<ICoreTrackingService>();
            _mockLogger = new Mock<ILogger<CoreTrackingController>>();
            _controller = new CoreTrackingController(_mockCoreTrackingService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllCoreTracking_ReturnsOkResult_WithCoreTrackingRecords()
        {
            // Arrange
            var expectedCores = new List<CoreTrackingDto>
            {
                new CoreTrackingDto
                {
                    Id = Guid.NewGuid(),
                    PartId = Guid.NewGuid(),
                    PartNumber = "P12345",
                    CorePartNumber = "CP12345",
                    CoreValue = 100m,
                    Status = CoreTrackingStatus.Sold.ToString()
                },
                new CoreTrackingDto
                {
                    Id = Guid.NewGuid(),
                    PartId = Guid.NewGuid(),
                    PartNumber = "P67890",
                    CorePartNumber = "CP67890",
                    CoreValue = 150m,
                    Status = CoreTrackingStatus.Returned.ToString()
                }
            };

            _mockCoreTrackingService.Setup(s => s.GetAllCoreTrackingAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCores);

            // Act
            var result = await _controller.GetAllCoreTracking();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCores = Assert.IsAssignableFrom<IEnumerable<CoreTrackingDto>>(okResult.Value);
            Assert.Equal(2, returnedCores.Count());
            Assert.Equal(expectedCores, returnedCores);
        }

        [Fact]
        public async Task GetCoreTrackingById_WithValidId_ReturnsOkResult_WithCoreTracking()
        {
            // Arrange
            var coreId = Guid.NewGuid();
            var expectedCore = new CoreTrackingDto
            {
                Id = coreId,
                PartId = Guid.NewGuid(),
                PartNumber = "P12345",
                PartDescription = "Test Part",
                CorePartNumber = "CP12345",
                CoreValue = 100m,
                Status = CoreTrackingStatus.Sold.ToString(),
                SoldDate = DateTime.UtcNow.AddDays(-30),
                SoldReferenceId = Guid.NewGuid(),
                SoldReferenceNumber = "SO-12345"
            };

            _mockCoreTrackingService.Setup(s => s.GetCoreTrackingByIdAsync(
                It.Is<Guid>(id => id == coreId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCore);

            // Act
            var result = await _controller.GetCoreTrackingById(coreId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCore = Assert.IsType<CoreTrackingDto>(okResult.Value);
            Assert.Equal(coreId, returnedCore.Id);
            Assert.Equal(expectedCore.PartNumber, returnedCore.PartNumber);
            Assert.Equal(expectedCore.Status, returnedCore.Status);
        }

        [Fact]
        public async Task GetCoreTrackingById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var coreId = Guid.NewGuid();
            _mockCoreTrackingService.Setup(s => s.GetCoreTrackingByIdAsync(
                It.Is<Guid>(id => id == coreId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CoreTrackingDto)null);

            // Act
            var result = await _controller.GetCoreTrackingById(coreId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetCoreTrackingByPartId_ReturnsOkResult_WithCoreTrackingRecords()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var expectedCores = new List<CoreTrackingDto>
            {
                new CoreTrackingDto
                {
                    Id = Guid.NewGuid(),
                    PartId = partId,
                    PartNumber = "P12345",
                    CorePartNumber = "CP12345",
                    CoreValue = 100m,
                    Status = CoreTrackingStatus.Sold.ToString()
                },
                new CoreTrackingDto
                {
                    Id = Guid.NewGuid(),
                    PartId = partId,
                    PartNumber = "P12345",
                    CorePartNumber = "CP12345",
                    CoreValue = 100m,
                    Status = CoreTrackingStatus.Returned.ToString()
                }
            };

            _mockCoreTrackingService.Setup(s => s.GetCoreTrackingByPartIdAsync(
                It.Is<Guid>(id => id == partId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCores);

            // Act
            var result = await _controller.GetCoreTrackingByPartId(partId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCores = Assert.IsAssignableFrom<IEnumerable<CoreTrackingDto>>(okResult.Value);
            Assert.Equal(2, returnedCores.Count());
            Assert.All(returnedCores, c => Assert.Equal(partId, c.PartId));
        }

        [Fact]
        public async Task GetCoreTrackingByStatus_ReturnsOkResult_WithCoreTrackingRecords()
        {
            // Arrange
            var status = CoreTrackingStatus.Sold;
            var expectedCores = new List<CoreTrackingDto>
            {
                new CoreTrackingDto
                {
                    Id = Guid.NewGuid(),
                    PartId = Guid.NewGuid(),
                    PartNumber = "P12345",
                    CorePartNumber = "CP12345",
                    CoreValue = 100m,
                    Status = status.ToString()
                },
                new CoreTrackingDto
                {
                    Id = Guid.NewGuid(),
                    PartId = Guid.NewGuid(),
                    PartNumber = "P67890",
                    CorePartNumber = "CP67890",
                    CoreValue = 150m,
                    Status = status.ToString()
                }
            };

            _mockCoreTrackingService.Setup(s => s.GetCoreTrackingByStatusAsync(
                It.Is<CoreTrackingStatus>(s => s == status), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCores);

            // Act
            var result = await _controller.GetCoreTrackingByStatus(status);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCores = Assert.IsAssignableFrom<IEnumerable<CoreTrackingDto>>(okResult.Value);
            Assert.Equal(2, returnedCores.Count());
            Assert.All(returnedCores, c => Assert.Equal(status.ToString(), c.Status));
        }

        [Fact]
        public async Task CreateCoreTracking_WithValidDto_ReturnsCreatedAtAction()
        {
            // Arrange
            var createCoreTrackingDto = new CreateCoreTrackingDto
            {
                PartId = Guid.NewGuid(),
                CorePartNumber = "CP12345",
                CoreValue = 100m,
                SoldReferenceId = Guid.NewGuid(),
                SoldReferenceNumber = "SO-12345"
            };

            var createdCore = new CoreTrackingDto
            {
                Id = Guid.NewGuid(),
                PartId = createCoreTrackingDto.PartId,
                PartNumber = "P12345",
                CorePartNumber = createCoreTrackingDto.CorePartNumber,
                CoreValue = createCoreTrackingDto.CoreValue,
                Status = CoreTrackingStatus.Sold.ToString(),
                SoldDate = DateTime.UtcNow,
                SoldReferenceId = createCoreTrackingDto.SoldReferenceId,
                SoldReferenceNumber = createCoreTrackingDto.SoldReferenceNumber
            };

            _mockCoreTrackingService.Setup(s => s.CreateCoreTrackingAsync(
                It.IsAny<CreateCoreTrackingDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdCore);

            // Act
            var result = await _controller.CreateCoreTracking(createCoreTrackingDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(CoreTrackingController.GetCoreTrackingById), createdAtActionResult.ActionName);
            Assert.Equal(createdCore.Id, createdAtActionResult.RouteValues["id"]);
            var returnedCore = Assert.IsType<CoreTrackingDto>(createdAtActionResult.Value);
            Assert.Equal(createdCore.Id, returnedCore.Id);
        }

        [Fact]
        public async Task CreateCoreTracking_WhenServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var createCoreTrackingDto = new CreateCoreTrackingDto
            {
                PartId = Guid.NewGuid(),
                CorePartNumber = "CP12345",
                CoreValue = 100m,
                SoldReferenceId = Guid.NewGuid(),
                SoldReferenceNumber = "SO-12345"
            };

            var errorMessage = "Part does not exist or is not a core part";
            _mockCoreTrackingService.Setup(s => s.CreateCoreTrackingAsync(
                It.IsAny<CreateCoreTrackingDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await _controller.CreateCoreTracking(createCoreTrackingDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task ProcessCoreReturn_WithValidData_ReturnsOkResult_WithUpdatedCore()
        {
            // Arrange
            var coreId = Guid.NewGuid();
            var processReturnDto = new ProcessCoreReturnDto
            {
                ReturnedDate = DateTime.UtcNow,
                Notes = "Core returned in good condition"
            };

            var updatedCore = new CoreTrackingDto
            {
                Id = coreId,
                PartId = Guid.NewGuid(),
                PartNumber = "P12345",
                CorePartNumber = "CP12345",
                CoreValue = 100m,
                Status = CoreTrackingStatus.Returned.ToString(),
                SoldDate = DateTime.UtcNow.AddDays(-30),
                ReturnedDate = processReturnDto.ReturnedDate,
                Notes = processReturnDto.Notes
            };

            _mockCoreTrackingService.Setup(s => s.ProcessCoreReturnAsync(
                It.Is<Guid>(id => id == coreId), 
                It.IsAny<ProcessCoreReturnDto>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedCore);

            // Act
            var result = await _controller.ProcessCoreReturn(coreId, processReturnDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCore = Assert.IsType<CoreTrackingDto>(okResult.Value);
            Assert.Equal(coreId, returnedCore.Id);
            Assert.Equal(CoreTrackingStatus.Returned.ToString(), returnedCore.Status);
            Assert.Equal(processReturnDto.ReturnedDate, returnedCore.ReturnedDate);
        }

        [Fact]
        public async Task ProcessCoreReturn_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var coreId = Guid.NewGuid();
            var processReturnDto = new ProcessCoreReturnDto
            {
                ReturnedDate = DateTime.UtcNow
            };

            _mockCoreTrackingService.Setup(s => s.ProcessCoreReturnAsync(
                It.Is<Guid>(id => id == coreId), 
                It.IsAny<ProcessCoreReturnDto>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((CoreTrackingDto)null);

            // Act
            var result = await _controller.ProcessCoreReturn(coreId, processReturnDto);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task ApplyCredit_WithValidData_ReturnsOkResult_WithUpdatedCore()
        {
            // Arrange
            var coreId = Guid.NewGuid();
            var applyCreditDto = new ApplyCreditDto
            {
                CreditedDate = DateTime.UtcNow,
                CreditAmount = 100m,
                Notes = "Full credit applied"
            };

            var updatedCore = new CoreTrackingDto
            {
                Id = coreId,
                PartId = Guid.NewGuid(),
                PartNumber = "P12345",
                CorePartNumber = "CP12345",
                CoreValue = 100m,
                Status = CoreTrackingStatus.Credited.ToString(),
                SoldDate = DateTime.UtcNow.AddDays(-30),
                ReturnedDate = DateTime.UtcNow.AddDays(-1),
                CreditedDate = applyCreditDto.CreditedDate,
                CreditAmount = applyCreditDto.CreditAmount,
                Notes = applyCreditDto.Notes
            };

            _mockCoreTrackingService.Setup(s => s.ApplyCreditAsync(
                It.Is<Guid>(id => id == coreId), 
                It.IsAny<ApplyCreditDto>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedCore);

            // Act
            var result = await _controller.ApplyCredit(coreId, applyCreditDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCore = Assert.IsType<CoreTrackingDto>(okResult.Value);
            Assert.Equal(coreId, returnedCore.Id);
            Assert.Equal(CoreTrackingStatus.Credited.ToString(), returnedCore.Status);
            Assert.Equal(applyCreditDto.CreditedDate, returnedCore.CreditedDate);
            Assert.Equal(applyCreditDto.CreditAmount, returnedCore.CreditAmount);
        }

        [Fact]
        public async Task GetTotalOutstandingCoreValue_ReturnsOkResult_WithValue()
        {
            // Arrange
            var expectedValue = 1250.50m;

            _mockCoreTrackingService.Setup(s => s.GetTotalOutstandingCoreValueAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _controller.GetTotalOutstandingCoreValue();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedValue = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(expectedValue, returnedValue);
        }
    }
}
