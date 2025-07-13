using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Infrastructure.Data;
using DMS.ServiceManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DMS.ServiceManagement.API.Tests.Repositories
{
    public class RepairOrderRepositoryTests
    {
        private readonly DbContextOptions<ServiceManagementDbContext> _options;

        public RepairOrderRepositoryTests()
        {
            // Use in-memory database for testing
            _options = new DbContextOptionsBuilder<ServiceManagementDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRepairOrders()
        {
            // Arrange
            var testRepairOrders = GetTestRepairOrders();
            
            using (var context = new ServiceManagementDbContext(_options))
            {
                await context.Database.EnsureCreatedAsync();
                context.RepairOrders.AddRange(testRepairOrders);
                await context.SaveChangesAsync();
            }
            
            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new RepairOrderRepository(context);
                var result = await repository.GetAllAsync();
                
                // Assert
                Assert.Equal(testRepairOrders.Count, result.Count());
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectRepairOrder()
        {
            // Arrange
            var testRepairOrders = GetTestRepairOrders();
            var testId = testRepairOrders[0].Id;
            
            using (var context = new ServiceManagementDbContext(_options))
            {
                await context.Database.EnsureCreatedAsync();
                context.RepairOrders.AddRange(testRepairOrders);
                await context.SaveChangesAsync();
            }
            
            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new RepairOrderRepository(context);
                var result = await repository.GetByIdAsync(testId);
                
                // Assert
                Assert.NotNull(result);
                Assert.Equal(testId, result.Id);
            }
        }

        [Fact]
        public async Task CreateAsync_ShouldAddRepairOrder()
        {
            // Arrange
            var newRepairOrder = new RepairOrder
            {
                CustomerId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                AdvisorId = Guid.NewGuid(),
                Status = RepairOrderStatus.Open,
                Number = "RO-12345",
                OpenDate = DateTime.UtcNow,
                PromiseDate = DateTime.UtcNow.AddDays(2),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                await context.Database.EnsureCreatedAsync();
                var repository = new RepairOrderRepository(context);
                var result = await repository.CreateAsync(newRepairOrder);
                
                // Assert
                Assert.NotNull(result);
                Assert.Equal(newRepairOrder.Id, result.Id);
                
                var dbRepairOrder = await context.RepairOrders.FindAsync(result.Id);
                Assert.NotNull(dbRepairOrder);
                Assert.Equal(newRepairOrder.Number, dbRepairOrder.Number);
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateRepairOrder()
        {
            // Arrange
            var testRepairOrders = GetTestRepairOrders();
            var testId = testRepairOrders[0].Id;
            const string updatedNumber = "RO-UPDATED";
            
            using (var context = new ServiceManagementDbContext(_options))
            {
                await context.Database.EnsureCreatedAsync();
                context.RepairOrders.AddRange(testRepairOrders);
                await context.SaveChangesAsync();
            }
            
            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new RepairOrderRepository(context);
                var repairOrderToUpdate = await repository.GetByIdAsync(testId);
                repairOrderToUpdate.Number = updatedNumber;
                
                await repository.UpdateAsync(repairOrderToUpdate);
            }
            
            // Assert
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new RepairOrderRepository(context);
                var updatedRepairOrder = await repository.GetByIdAsync(testId);
                
                Assert.NotNull(updatedRepairOrder);
                Assert.Equal(updatedNumber, updatedRepairOrder.Number);
            }
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveRepairOrder()
        {
            // Arrange
            var testRepairOrders = GetTestRepairOrders();
            var testId = testRepairOrders[0].Id;
            
            using (var context = new ServiceManagementDbContext(_options))
            {
                await context.Database.EnsureCreatedAsync();
                context.RepairOrders.AddRange(testRepairOrders);
                await context.SaveChangesAsync();
            }
            
            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new RepairOrderRepository(context);
                var result = await repository.DeleteAsync(testId);
                
                // Assert
                Assert.True(result);
                
                var deletedRepairOrder = await repository.GetByIdAsync(testId);
                Assert.Null(deletedRepairOrder);
            }
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldUpdateRepairOrderStatus()
        {
            // Arrange
            var testRepairOrders = GetTestRepairOrders();
            var testId = testRepairOrders[0].Id;
            var newStatus = RepairOrderStatus.InProgress;
            
            using (var context = new ServiceManagementDbContext(_options))
            {
                await context.Database.EnsureCreatedAsync();
                context.RepairOrders.AddRange(testRepairOrders);
                await context.SaveChangesAsync();
            }
            
            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new RepairOrderRepository(context);
                var result = await repository.UpdateStatusAsync(testId, newStatus);
                
                // Assert
                Assert.NotNull(result);
                Assert.Equal(newStatus, result.Status);
            }
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
