using DMS.PartsManagement.API.Controllers;
using DMS.PartsManagement.Core.DTOs;
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
    public class PartsControllerTests
    {
        private readonly Mock<IPartService> _mockPartService;
        private readonly Mock<ILogger<PartsController>> _mockLogger;
        private readonly PartsController _controller;

        public PartsControllerTests()
        {
            _mockPartService = new Mock<IPartService>();
            _mockLogger = new Mock<ILogger<PartsController>>();
            _controller = new PartsController(_mockPartService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllParts_ReturnsOkResult_WithParts()
        {
            // Arrange
            var expectedParts = new List<PartSummaryDto>
            {
                new PartSummaryDto
                {
                    Id = Guid.NewGuid(),
                    PartNumber = "ABC123",
                    Description = "Test Part 1"
                },
                new PartSummaryDto
                {
                    Id = Guid.NewGuid(),
                    PartNumber = "DEF456",
                    Description = "Test Part 2"
                }
            };

            _mockPartService.Setup(s => s.GetAllPartsAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedParts);

            // Act
            var result = await _controller.GetAllParts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedParts = Assert.IsAssignableFrom<IEnumerable<PartSummaryDto>>(okResult.Value);
            Assert.Equal(2, returnedParts.Count());
            Assert.Equal(expectedParts, returnedParts);
        }

        [Fact]
        public async Task GetPartById_WithValidId_ReturnsOkResult_WithPart()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var expectedPart = new PartDetailDto
            {
                Id = partId,
                PartNumber = "ABC123",
                Description = "Test Part",
                CategoryName = "Test Category",
                ManufacturerName = "Test Manufacturer",
                UnitCost = 19.99m,
                UnitPrice = 29.99m
            };

            _mockPartService.Setup(s => s.GetPartByIdAsync(
                It.Is<Guid>(id => id == partId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPart);

            // Act
            var result = await _controller.GetPartById(partId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPart = Assert.IsType<PartDetailDto>(okResult.Value);
            Assert.Equal(partId, returnedPart.Id);
            Assert.Equal(expectedPart.PartNumber, returnedPart.PartNumber);
            Assert.Equal(expectedPart.Description, returnedPart.Description);
        }

        [Fact]
        public async Task GetPartById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var partId = Guid.NewGuid();
            _mockPartService.Setup(s => s.GetPartByIdAsync(
                It.Is<Guid>(id => id == partId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartDetailDto)null);

            // Act
            var result = await _controller.GetPartById(partId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SearchParts_ReturnsOkResult_WithMatchingParts()
        {
            // Arrange
            var searchTerm = "brake";
            var expectedParts = new List<PartSummaryDto>
            {
                new PartSummaryDto
                {
                    Id = Guid.NewGuid(),
                    PartNumber = "BR123",
                    Description = "Front Brake Pad"
                },
                new PartSummaryDto
                {
                    Id = Guid.NewGuid(),
                    PartNumber = "BR456",
                    Description = "Rear Brake Disk"
                }
            };

            _mockPartService.Setup(s => s.SearchPartsAsync(
                It.Is<string>(term => term == searchTerm), 
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedParts);

            // Act
            var result = await _controller.SearchParts(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedParts = Assert.IsAssignableFrom<IEnumerable<PartSummaryDto>>(okResult.Value);
            Assert.Equal(2, returnedParts.Count());
        }

        [Fact]
        public async Task GetPartByPartNumber_WithValidNumber_ReturnsOkResult_WithPart()
        {
            // Arrange
            var partNumber = "ABC123";
            var expectedPart = new PartDetailDto
            {
                Id = Guid.NewGuid(),
                PartNumber = partNumber,
                Description = "Test Part",
                CategoryName = "Test Category"
            };

            _mockPartService.Setup(s => s.GetPartByPartNumberAsync(
                It.Is<string>(number => number == partNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPart);

            // Act
            var result = await _controller.GetPartByPartNumber(partNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPart = Assert.IsType<PartDetailDto>(okResult.Value);
            Assert.Equal(partNumber, returnedPart.PartNumber);
        }

        [Fact]
        public async Task GetPartByPartNumber_WithInvalidNumber_ReturnsNotFound()
        {
            // Arrange
            var partNumber = "NONEXISTENT";
            _mockPartService.Setup(s => s.GetPartByPartNumberAsync(
                It.Is<string>(number => number == partNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartDetailDto)null);

            // Act
            var result = await _controller.GetPartByPartNumber(partNumber);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreatePart_WithValidDto_ReturnsCreatedAtAction()
        {
            // Arrange
            var createPartDto = new CreatePartDto
            {
                PartNumber = "NEW123",
                Description = "New Test Part",
                CategoryId = Guid.NewGuid(),
                ManufacturerId = Guid.NewGuid(),
                UnitCost = 15.99m,
                UnitPrice = 24.99m
            };

            var createdPart = new PartDetailDto
            {
                Id = Guid.NewGuid(),
                PartNumber = createPartDto.PartNumber,
                Description = createPartDto.Description,
                UnitCost = createPartDto.UnitCost,
                UnitPrice = createPartDto.UnitPrice
            };

            _mockPartService.Setup(s => s.PartExistsByPartNumberAsync(
                It.Is<string>(number => number == createPartDto.PartNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockPartService.Setup(s => s.CreatePartAsync(
                It.IsAny<CreatePartDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdPart);

            // Act
            var result = await _controller.CreatePart(createPartDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PartsController.GetPartById), createdAtActionResult.ActionName);
            Assert.Equal(createdPart.Id, createdAtActionResult.RouteValues["id"]);
            var returnedPart = Assert.IsType<PartDetailDto>(createdAtActionResult.Value);
            Assert.Equal(createPartDto.PartNumber, returnedPart.PartNumber);
        }

        [Fact]
        public async Task CreatePart_WithExistingPartNumber_ReturnsBadRequest()
        {
            // Arrange
            var createPartDto = new CreatePartDto
            {
                PartNumber = "EXISTING123",
                Description = "Test Part"
            };

            _mockPartService.Setup(s => s.PartExistsByPartNumberAsync(
                It.Is<string>(number => number == createPartDto.PartNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CreatePart(createPartDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains(createPartDto.PartNumber, badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task UpdatePart_WithValidData_ReturnsOkResult_WithUpdatedPart()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var updatePartDto = new UpdatePartDto
            {
                Description = "Updated Description",
                UnitPrice = 39.99m
            };

            var updatedPart = new PartDetailDto
            {
                Id = partId,
                PartNumber = "ABC123",
                Description = updatePartDto.Description,
                UnitPrice = updatePartDto.UnitPrice
            };

            _mockPartService.Setup(s => s.PartExistsAsync(
                It.Is<Guid>(id => id == partId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockPartService.Setup(s => s.UpdatePartAsync(
                It.Is<Guid>(id => id == partId), 
                It.IsAny<UpdatePartDto>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedPart);

            // Act
            var result = await _controller.UpdatePart(partId, updatePartDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPart = Assert.IsType<PartDetailDto>(okResult.Value);
            Assert.Equal(partId, returnedPart.Id);
            Assert.Equal(updatePartDto.Description, returnedPart.Description);
            Assert.Equal(updatePartDto.UnitPrice, returnedPart.UnitPrice);
        }

        [Fact]
        public async Task DeletePart_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var partId = Guid.NewGuid();
            _mockPartService.Setup(s => s.PartExistsAsync(
                It.Is<Guid>(id => id == partId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockPartService.Setup(s => s.DeletePartAsync(
                It.Is<Guid>(id => id == partId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeletePart(partId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task FindPartsByVehicleFitment_ReturnsOkResult_WithMatchingParts()
        {
            // Arrange
            var year = 2023;
            var make = "Toyota";
            var model = "Camry";
            var expectedParts = new List<PartSummaryDto>
            {
                new PartSummaryDto
                {
                    Id = Guid.NewGuid(),
                    PartNumber = "CAM123",
                    Description = "Air Filter for Camry"
                },
                new PartSummaryDto
                {
                    Id = Guid.NewGuid(),
                    PartNumber = "CAM456",
                    Description = "Oil Filter for Camry"
                }
            };

            _mockPartService.Setup(s => s.FindPartsByVehicleFitmentAsync(
                It.Is<int>(y => y == year),
                It.Is<string>(m => m == make),
                It.Is<string>(m => m == model),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedParts);

            // Act
            var result = await _controller.FindPartsByVehicleFitment(year, make, model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedParts = Assert.IsAssignableFrom<IEnumerable<PartSummaryDto>>(okResult.Value);
            Assert.Equal(2, returnedParts.Count());
        }
    }
}
