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
    public class InventoryControllerTests
    {
        private readonly Mock<IInventoryService> _mockInventoryService;
        private readonly Mock<IPartService> _mockPartService;
        private readonly Mock<ITransactionService> _mockTransactionService;
        private readonly Mock<ILogger<InventoryController>> _mockLogger;
        private readonly InventoryController _controller;

        public InventoryControllerTests()
        {
            _mockInventoryService = new Mock<IInventoryService>();
            _mockPartService = new Mock<IPartService>();
            _mockTransactionService = new Mock<ITransactionService>();
            _mockLogger = new Mock<ILogger<InventoryController>>();
            _controller = new InventoryController(_mockInventoryService.Object, _mockPartService.Object, _mockTransactionService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetInventoryForPart_WithValidId_ReturnsOkResult_WithInventory()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var expectedInventory = new List<PartInventoryDto>
            {
                new PartInventoryDto
                {
                    Id = Guid.NewGuid(),
                    PartId = partId,
                    PartNumber = "ABC123",
                    LocationId = Guid.NewGuid(),
                    LocationName = "Main Warehouse",
                    Quantity = 25,
                    MinimumQuantity = 5,
                    MaximumQuantity = 50,
                    BinLocation = "A-12-3"
                },
                new PartInventoryDto
                {
                    Id = Guid.NewGuid(),
                    PartId = partId,
                    PartNumber = "ABC123",
                    LocationId = Guid.NewGuid(), 
                    LocationName = "Service Bay Storage",
                    Quantity = 5,
                    MinimumQuantity = 2,
                    MaximumQuantity = 10,
                    BinLocation = "S-3-1"
                }
            };

            _mockPartService.Setup(s => s.PartExistsAsync(
                It.Is<Guid>(id => id == partId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockInventoryService.Setup(s => s.GetInventoryForPartAsync(
                It.Is<Guid>(id => id == partId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedInventory);

            // Act
            var result = await _controller.GetInventoryForPart(partId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedInventory = Assert.IsAssignableFrom<IEnumerable<PartInventoryDto>>(okResult.Value);
            Assert.Equal(2, returnedInventory.Count());
            Assert.All(returnedInventory, i => Assert.Equal(partId, i.PartId));
        }

        [Fact]
        public async Task GetInventoryForPart_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var partId = Guid.NewGuid();
            _mockPartService.Setup(s => s.PartExistsAsync(
                It.Is<Guid>(id => id == partId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.GetInventoryForPart(partId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetInventoryForLocation_ReturnsOkResult_WithInventory()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var expectedInventory = new List<PartInventoryDto>
            {
                new PartInventoryDto
                {
                    Id = Guid.NewGuid(),
                    PartId = Guid.NewGuid(),
                    PartNumber = "ABC123",
                    LocationId = locationId,
                    LocationName = "Main Warehouse",
                    Quantity = 25,
                    BinLocation = "A-12-3"
                },
                new PartInventoryDto
                {
                    Id = Guid.NewGuid(),
                    PartId = Guid.NewGuid(),
                    PartNumber = "DEF456",
                    LocationId = locationId,
                    LocationName = "Main Warehouse",
                    Quantity = 15,
                    BinLocation = "B-5-2"
                }
            };

            _mockInventoryService.Setup(s => s.GetInventoryForLocationAsync(
                It.Is<Guid>(id => id == locationId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedInventory);

            // Act
            var result = await _controller.GetInventoryForLocation(locationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedInventory = Assert.IsAssignableFrom<IEnumerable<PartInventoryDto>>(okResult.Value);
            Assert.Equal(2, returnedInventory.Count());
            Assert.All(returnedInventory, i => Assert.Equal(locationId, i.LocationId));
        }

        [Fact]
        public async Task GetLowStockInventory_ReturnsOkResult_WithLowStockItems()
        {
            // Arrange
            var expectedInventory = new List<PartInventoryDto>
            {
                new PartInventoryDto
                {
                    Id = Guid.NewGuid(),
                    PartId = Guid.NewGuid(),
                    PartNumber = "ABC123",
                    LocationId = Guid.NewGuid(),
                    LocationName = "Main Warehouse",
                    Quantity = 2,
                    MinimumQuantity = 5,
                    MaximumQuantity = 50,
                    BinLocation = "A-12-3"
                },
                new PartInventoryDto
                {
                    Id = Guid.NewGuid(),
                    PartId = Guid.NewGuid(),
                    PartNumber = "DEF456",
                    LocationId = Guid.NewGuid(),
                    LocationName = "Service Bay Storage",
                    Quantity = 1,
                    MinimumQuantity = 3,
                    MaximumQuantity = 10,
                    BinLocation = "S-3-1"
                }
            };

            _mockInventoryService.Setup(s => s.GetLowStockInventoryAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedInventory);

            // Act
            var result = await _controller.GetLowStockInventory();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedInventory = Assert.IsAssignableFrom<IEnumerable<PartInventoryDto>>(okResult.Value);
            Assert.Equal(2, returnedInventory.Count());
            Assert.All(returnedInventory, i => Assert.True(i.Quantity < i.MinimumQuantity));
        }

        [Fact]
        public async Task UpdateInventorySettings_WithValidData_ReturnsOkResult_WithUpdatedInventory()
        {
            // Arrange
            var inventoryId = Guid.NewGuid();
            var updateSettingsDto = new UpdateInventorySettingsDto
            {
                MinimumQuantity = 10,
                MaximumQuantity = 50,
                BinLocation = "C-15-7"
            };

            var updatedInventory = new PartInventoryDto
            {
                Id = inventoryId,
                PartId = Guid.NewGuid(),
                PartNumber = "ABC123",
                LocationId = Guid.NewGuid(),
                LocationName = "Main Warehouse",
                Quantity = 25,
                MinimumQuantity = updateSettingsDto.MinimumQuantity,
                MaximumQuantity = updateSettingsDto.MaximumQuantity,
                BinLocation = updateSettingsDto.BinLocation
            };

            _mockInventoryService.Setup(s => s.InventoryExistsAsync(
                It.Is<Guid>(id => id == inventoryId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockInventoryService.Setup(s => s.UpdateInventorySettingsAsync(
                It.Is<Guid>(id => id == inventoryId), 
                It.IsAny<UpdateInventorySettingsDto>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedInventory);

            // Act
            var result = await _controller.UpdateInventorySettings(inventoryId, updateSettingsDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedInventory = Assert.IsType<PartInventoryDto>(okResult.Value);
            Assert.Equal(inventoryId, returnedInventory.Id);
            Assert.Equal(updateSettingsDto.MinimumQuantity, returnedInventory.MinimumQuantity);
            Assert.Equal(updateSettingsDto.MaximumQuantity, returnedInventory.MaximumQuantity);
            Assert.Equal(updateSettingsDto.BinLocation, returnedInventory.BinLocation);
        }

        [Fact]
        public async Task AdjustInventory_WithValidDto_ReturnsOkResult_WithTransaction()
        {
            // Arrange
            var adjustInventoryDto = new AdjustInventoryDto
            {
                PartId = Guid.NewGuid(),
                QuantityAdjustment = 10,
                LocationId = Guid.NewGuid(),
                AdjustmentReason = "Physical count correction"
            };

            var createdTransaction = new PartTransactionDto
            {
                Id = Guid.NewGuid(),
                PartId = adjustInventoryDto.PartId,
                PartNumber = "ABC123",
                PartDescription = "Test Part",
                TransactionType = "Adjustment",
                Quantity = adjustInventoryDto.QuantityAdjustment,
                DestinationLocationId = adjustInventoryDto.LocationId,
                DestinationLocationName = "Main Warehouse",
                Notes = adjustInventoryDto.AdjustmentReason,
                TransactionDate = DateTime.UtcNow
            };

            _mockPartService.Setup(s => s.PartExistsAsync(
                It.Is<Guid>(id => id == adjustInventoryDto.PartId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockTransactionService.Setup(s => s.AdjustInventoryAsync(
                It.IsAny<AdjustInventoryDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTransaction);

            // Act
            var result = await _controller.AdjustInventory(adjustInventoryDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransaction = Assert.IsType<PartTransactionDto>(okResult.Value);
            Assert.Equal(adjustInventoryDto.PartId, returnedTransaction.PartId);
            Assert.Equal("Adjustment", returnedTransaction.TransactionType);
            Assert.Equal(adjustInventoryDto.QuantityAdjustment, returnedTransaction.Quantity);
        }

        [Fact]
        public async Task CreatePartInventory_WithValidDto_ReturnsCreatedAtAction()
        {
            // Arrange
            var createInventoryDto = new CreatePartInventoryDto
            {
                PartId = Guid.NewGuid(),
                LocationId = Guid.NewGuid(),
                InitialQuantity = 50,
                MinimumQuantity = 10,
                MaximumQuantity = 100,
                BinLocation = "D-7-2"
            };

            var createdInventory = new PartInventoryDto
            {
                Id = Guid.NewGuid(),
                PartId = createInventoryDto.PartId,
                PartNumber = "ABC123",
                LocationId = createInventoryDto.LocationId,
                LocationName = "Main Warehouse",
                Quantity = createInventoryDto.InitialQuantity,
                MinimumQuantity = createInventoryDto.MinimumQuantity,
                MaximumQuantity = createInventoryDto.MaximumQuantity,
                BinLocation = createInventoryDto.BinLocation
            };

            _mockPartService.Setup(s => s.PartExistsAsync(
                It.Is<Guid>(id => id == createInventoryDto.PartId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockInventoryService.Setup(s => s.InventoryExistsForPartLocationAsync(
                It.Is<Guid>(id => id == createInventoryDto.PartId), 
                It.Is<Guid>(id => id == createInventoryDto.LocationId), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockInventoryService.Setup(s => s.CreatePartInventoryAsync(
                It.IsAny<CreatePartInventoryDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdInventory);

            // Act
            var result = await _controller.CreatePartInventory(createInventoryDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(InventoryController.GetInventoryForPart), createdAtActionResult.ActionName);
            Assert.Equal(createInventoryDto.PartId, createdAtActionResult.RouteValues["partId"]);
            var returnedInventory = Assert.IsType<PartInventoryDto>(createdAtActionResult.Value);
            Assert.Equal(createInventoryDto.PartId, returnedInventory.PartId);
            Assert.Equal(createInventoryDto.LocationId, returnedInventory.LocationId);
            Assert.Equal(createInventoryDto.InitialQuantity, returnedInventory.Quantity);
        }
    }
}
