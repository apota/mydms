using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.ServiceManagement.API.Controllers;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DMS.ServiceManagement.API.Tests.Controllers
{
    public class RepairOrdersControllerTests
    {
        private readonly Mock<IRepairOrderService> _mockRepairOrderService;
        private readonly Mock<ILogger<RepairOrdersController>> _mockLogger;
        private readonly RepairOrdersController _controller;

        public RepairOrdersControllerTests()
        {
            _mockRepairOrderService = new Mock<IRepairOrderService>();
            _mockLogger = new Mock<ILogger<RepairOrdersController>>();
            _controller = new RepairOrdersController(_mockRepairOrderService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllRepairOrders_ReturnsOkResult_WithRepairOrders()
        {
            // Arrange
            var repairOrders = GetTestRepairOrders();
            _mockRepairOrderService.Setup(service => service.GetAllRepairOrdersAsync())
                .ReturnsAsync(repairOrders);

            // Act
            var result = await _controller.GetAllRepairOrders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<RepairOrder>>(okResult.Value);
            Assert.Equal(repairOrders.Count, returnValue.Count);
        }

        [Fact]
        public async Task GetRepairOrderById_ReturnsOkResult_WithRepairOrder_WhenIdExists()
        {
            // Arrange
            var repairOrders = GetTestRepairOrders();
            var testId = repairOrders[0].Id;
            _mockRepairOrderService.Setup(service => service.GetRepairOrderByIdAsync(testId))
                .ReturnsAsync(repairOrders[0]);

            // Act
            var result = await _controller.GetRepairOrderById(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<RepairOrder>(okResult.Value);
            Assert.Equal(testId, returnValue.Id);
        }

        [Fact]
        public async Task GetRepairOrderById_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockRepairOrderService.Setup(service => service.GetRepairOrderByIdAsync(testId))
                .ReturnsAsync((RepairOrder)null);

            // Act
            var result = await _controller.GetRepairOrderById(testId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateRepairOrder_ReturnsCreatedAtAction_WithRepairOrder()
        {
            // Arrange
            var newRepairOrder = new RepairOrder
            {
                CustomerId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid()
            };

            var createdRepairOrder = new RepairOrder
            {
                Id = Guid.NewGuid(),
                CustomerId = newRepairOrder.CustomerId,
                VehicleId = newRepairOrder.VehicleId,
                Number = "RO-12345",
                Status = RepairOrderStatus.Open
            };

            _mockRepairOrderService.Setup(service => service.CreateRepairOrderAsync(It.IsAny<RepairOrder>()))
                .ReturnsAsync(createdRepairOrder);

            // Act
            var result = await _controller.CreateRepairOrder(newRepairOrder);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<RepairOrder>(createdAtResult.Value);
            Assert.Equal(createdRepairOrder.Id, returnValue.Id);
        }

        [Fact]
        public async Task CreateRepairOrder_ReturnsBadRequest_WhenRepairOrderIsNull()
        {
            // Arrange & Act
            var result = await _controller.CreateRepairOrder(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateRepairOrder_ReturnsOkResult_WithUpdatedRepairOrder()
        {
            // Arrange
            var existingRepairOrder = GetTestRepairOrders()[0];
            var updatedRepairOrder = new RepairOrder
            {
                Id = existingRepairOrder.Id,
                CustomerId = existingRepairOrder.CustomerId,
                VehicleId = existingRepairOrder.VehicleId,
                Status = RepairOrderStatus.InProgress
            };

            _mockRepairOrderService.Setup(service => service.GetRepairOrderByIdAsync(existingRepairOrder.Id))
                .ReturnsAsync(existingRepairOrder);
            _mockRepairOrderService.Setup(service => service.UpdateRepairOrderAsync(It.IsAny<RepairOrder>()))
                .ReturnsAsync(updatedRepairOrder);

            // Act
            var result = await _controller.UpdateRepairOrder(existingRepairOrder.Id, updatedRepairOrder);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<RepairOrder>(okResult.Value);
            Assert.Equal(updatedRepairOrder.Status, returnValue.Status);
        }

        [Fact]
        public async Task UpdateRepairOrderStatus_ReturnsOkResult_WithUpdatedStatus()
        {
            // Arrange
            var existingRepairOrder = GetTestRepairOrders()[0];
            var newStatus = RepairOrderStatus.InProgress;
            var request = new RepairOrderStatusUpdateRequest { Status = newStatus };

            var updatedRepairOrder = new RepairOrder
            {
                Id = existingRepairOrder.Id,
                CustomerId = existingRepairOrder.CustomerId,
                VehicleId = existingRepairOrder.VehicleId,
                Status = newStatus
            };

            _mockRepairOrderService.Setup(service => service.UpdateRepairOrderStatusAsync(existingRepairOrder.Id, newStatus))
                .ReturnsAsync(updatedRepairOrder);

            // Act
            var result = await _controller.UpdateRepairOrderStatus(existingRepairOrder.Id, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<RepairOrder>(okResult.Value);
            Assert.Equal(newStatus, returnValue.Status);
        }

        [Fact]
        public async Task CloseRepairOrder_ReturnsOkResult_WithClosedRepairOrder()
        {
            // Arrange
            var existingRepairOrder = GetTestRepairOrders()[0];
            var closedRepairOrder = new RepairOrder
            {
                Id = existingRepairOrder.Id,
                CustomerId = existingRepairOrder.CustomerId,
                VehicleId = existingRepairOrder.VehicleId,
                Status = RepairOrderStatus.Closed,
                CompletionDate = DateTime.UtcNow
            };

            _mockRepairOrderService.Setup(service => service.CloseRepairOrderAsync(existingRepairOrder.Id))
                .ReturnsAsync(closedRepairOrder);

            // Act
            var result = await _controller.CloseRepairOrder(existingRepairOrder.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<RepairOrder>(okResult.Value);
            Assert.Equal(RepairOrderStatus.Closed, returnValue.Status);
        }

        private List<RepairOrder> GetTestRepairOrders()
        {
            return new List<RepairOrder>
            {
                new RepairOrder
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    VehicleId = Guid.NewGuid(),
                    AdvisorId = Guid.NewGuid(),
                    Status = RepairOrderStatus.Open,
                    Number = "RO-12345",
                    OpenDate = DateTime.UtcNow,
                    PromiseDate = DateTime.UtcNow.AddDays(2),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new RepairOrder
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    VehicleId = Guid.NewGuid(),
                    AdvisorId = Guid.NewGuid(),
                    Status = RepairOrderStatus.Open,
                    Number = "RO-67890",
                    OpenDate = DateTime.UtcNow,
                    PromiseDate = DateTime.UtcNow.AddDays(1),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }
    }
}
