using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Repositories;
using DMS.ServiceManagement.Core.Services.Implementations;
using Moq;
using Xunit;

namespace DMS.ServiceManagement.API.Tests.Services
{
    public class RepairOrderServiceTests
    {
        private readonly Mock<IRepairOrderRepository> _mockRepairOrderRepository;
        private readonly Mock<IServiceJobRepository> _mockServiceJobRepository;
        private readonly RepairOrderService _repairOrderService;

        public RepairOrderServiceTests()
        {
            _mockRepairOrderRepository = new Mock<IRepairOrderRepository>();
            _mockServiceJobRepository = new Mock<IServiceJobRepository>();
            _repairOrderService = new RepairOrderService(
                _mockRepairOrderRepository.Object,
                _mockServiceJobRepository.Object);
        }

        [Fact]
        public async Task GetAllRepairOrdersAsync_ShouldReturnAllRepairOrders()
        {
            // Arrange
            var repairOrders = GetTestRepairOrders();
            _mockRepairOrderRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(repairOrders);

            // Act
            var result = await _repairOrderService.GetAllRepairOrdersAsync();

            // Assert
            Assert.Equal(repairOrders.Count, result.Count());
            _mockRepairOrderRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetRepairOrderByIdAsync_ShouldReturnCorrectRepairOrder()
        {
            // Arrange
            var repairOrders = GetTestRepairOrders();
            var testId = repairOrders[0].Id;
            var testRepairOrder = repairOrders[0];

            _mockRepairOrderRepository.Setup(repo => repo.GetByIdAsync(testId))
                .ReturnsAsync(testRepairOrder);

            // Act
            var result = await _repairOrderService.GetRepairOrderByIdAsync(testId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testId, result.Id);
            _mockRepairOrderRepository.Verify(repo => repo.GetByIdAsync(testId), Times.Once);
        }

        [Fact]
        public async Task GetRepairOrderByNumberAsync_ShouldReturnCorrectRepairOrder()
        {
            // Arrange
            var repairOrders = GetTestRepairOrders();
            var testNumber = repairOrders[0].Number;
            var testRepairOrder = repairOrders[0];

            _mockRepairOrderRepository.Setup(repo => repo.GetByNumberAsync(testNumber))
                .ReturnsAsync(testRepairOrder);

            // Act
            var result = await _repairOrderService.GetRepairOrderByNumberAsync(testNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testNumber, result.Number);
            _mockRepairOrderRepository.Verify(repo => repo.GetByNumberAsync(testNumber), Times.Once);
        }

        [Fact]
        public async Task CreateRepairOrderAsync_ShouldCreateAndReturnRepairOrder()
        {
            // Arrange
            var newRepairOrder = new RepairOrder
            {
                CustomerId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                AdvisorId = Guid.NewGuid()
            };

            _mockRepairOrderRepository.Setup(repo => repo.CreateAsync(It.IsAny<RepairOrder>()))
                .ReturnsAsync((RepairOrder ro) => ro);

            // Act
            var result = await _repairOrderService.CreateRepairOrderAsync(newRepairOrder);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RepairOrderStatus.Open, result.Status);
            Assert.NotEmpty(result.Number);
            Assert.NotEqual(default, result.CreatedAt);
            Assert.NotEqual(default, result.UpdatedAt);
            Assert.NotEqual(default, result.OpenDate);
            _mockRepairOrderRepository.Verify(repo => repo.CreateAsync(It.IsAny<RepairOrder>()), Times.Once);
        }

        [Fact]
        public async Task UpdateRepairOrderAsync_ShouldUpdateAndReturnRepairOrder()
        {
            // Arrange
            var repairOrders = GetTestRepairOrders();
            var existingRepairOrder = repairOrders[0];
            var updatedRepairOrder = new RepairOrder
            {
                Id = existingRepairOrder.Id,
                CustomerId = existingRepairOrder.CustomerId,
                VehicleId = existingRepairOrder.VehicleId,
                AdvisorId = existingRepairOrder.AdvisorId,
                Status = RepairOrderStatus.InProgress,
                Number = existingRepairOrder.Number,
                Mileage = 15000,
                CustomerNotes = "Updated notes"
            };

            _mockRepairOrderRepository.Setup(repo => repo.GetByIdAsync(existingRepairOrder.Id))
                .ReturnsAsync(existingRepairOrder);
            _mockRepairOrderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()))
                .ReturnsAsync((RepairOrder ro) => ro);

            // Act
            var result = await _repairOrderService.UpdateRepairOrderAsync(updatedRepairOrder);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedRepairOrder.Status, result.Status);
            Assert.Equal(updatedRepairOrder.Mileage, result.Mileage);
            Assert.Equal(updatedRepairOrder.CustomerNotes, result.CustomerNotes);
            _mockRepairOrderRepository.Verify(repo => repo.GetByIdAsync(existingRepairOrder.Id), Times.Once);
            _mockRepairOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()), Times.Once);
        }

        [Fact]
        public async Task UpdateRepairOrderStatusAsync_ShouldUpdateStatusAndReturnRepairOrder()
        {
            // Arrange
            var repairOrders = GetTestRepairOrders();
            var existingRepairOrder = repairOrders[0];
            var newStatus = RepairOrderStatus.InProgress;

            _mockRepairOrderRepository.Setup(repo => repo.GetByIdAsync(existingRepairOrder.Id))
                .ReturnsAsync(existingRepairOrder);
            _mockRepairOrderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()))
                .ReturnsAsync((RepairOrder ro) => ro);

            // Act
            var result = await _repairOrderService.UpdateRepairOrderStatusAsync(existingRepairOrder.Id, newStatus);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newStatus, result.Status);
            _mockRepairOrderRepository.Verify(repo => repo.GetByIdAsync(existingRepairOrder.Id), Times.Once);
            _mockRepairOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<RepairOrder>()), Times.Once);
        }

        [Fact]
        public async Task CloseRepairOrderAsync_ShouldSetStatusToClosedAndReturnRepairOrder()
        {
            // Arrange
            var repairOrders = GetTestRepairOrders();
            var existingRepairOrder = repairOrders[0];
            existingRepairOrder.Status = RepairOrderStatus.Completed;

            _mockRepairOrderRepository.Setup(repo => repo.GetByIdAsync(existingRepairOrder.Id))
                .ReturnsAsync(existingRepairOrder);
            _mockRepairOrderRepository.Setup(repo => repo.CloseRepairOrderAsync(existingRepairOrder.Id))
                .ReturnsAsync(existingRepairOrder);

            // Act
            var result = await _repairOrderService.CloseRepairOrderAsync(existingRepairOrder.Id);

            // Assert
            Assert.NotNull(result);
            _mockRepairOrderRepository.Verify(repo => repo.GetByIdAsync(existingRepairOrder.Id), Times.Once);
            _mockRepairOrderRepository.Verify(repo => repo.CloseRepairOrderAsync(existingRepairOrder.Id), Times.Once);
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
