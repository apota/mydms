using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Repositories;
using DMS.ServiceManagement.Infrastructure.Data;
using DMS.ServiceManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DMS.ServiceManagement.API.Tests.Repositories
{
    public class ServiceJobRepositoryTests
    {
        private readonly DbContextOptions<ServiceManagementDbContext> _options;
        private readonly Mock<ILogger<ServiceJobRepository>> _mockLogger;

        public ServiceJobRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ServiceManagementDbContext>()
                .UseInMemoryDatabase(databaseName: $"ServiceJobRepositoryTest-{Guid.NewGuid()}")
                .Options;
            
            _mockLogger = new Mock<ILogger<ServiceJobRepository>>();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllServiceJobs()
        {
            // Arrange
            using (var context = new ServiceManagementDbContext(_options))
            {
                await SeedDbWithTestData(context);
            }

            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new ServiceJobRepository(context, _mockLogger.Object);
                var result = await repository.GetAllAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnServiceJob()
        {
            // Arrange
            Guid jobId = Guid.NewGuid();
            using (var context = new ServiceManagementDbContext(_options))
            {
                await SeedDbWithTestData(context, jobId);
            }

            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new ServiceJobRepository(context, _mockLogger.Object);
                var result = await repository.GetByIdAsync(jobId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(jobId, result.Id);
                Assert.Equal("Oil Change", result.Description);
            }
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            using (var context = new ServiceManagementDbContext(_options))
            {
                await SeedDbWithTestData(context);
            }

            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new ServiceJobRepository(context, _mockLogger.Object);
                var result = await repository.GetByIdAsync(Guid.NewGuid());

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task CreateAsync_ShouldAddNewServiceJob()
        {
            // Arrange
            using (var context = new ServiceManagementDbContext(_options))
            {
                await SeedDbWithTestData(context);
            }

            var newJob = new ServiceJob
            {
                Id = Guid.NewGuid(),
                RepairOrderId = Guid.NewGuid(),
                Description = "Brake Replacement",
                EstimatedTime = TimeSpan.FromHours(2),
                Status = "Pending",
                TechnicianId = Guid.NewGuid(),
                ServiceBayId = Guid.NewGuid(),
                ActualStartTime = null,
                ActualEndTime = null,
                LaborCost = 150.00m,
                Notes = "Front brake pads replacement",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new ServiceJobRepository(context, _mockLogger.Object);
                await repository.CreateAsync(newJob);
            }

            // Assert
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new ServiceJobRepository(context, _mockLogger.Object);
                var result = await repository.GetByIdAsync(newJob.Id);

                Assert.NotNull(result);
                Assert.Equal(newJob.Id, result.Id);
                Assert.Equal("Brake Replacement", result.Description);
                Assert.Equal(TimeSpan.FromHours(2), result.EstimatedTime);
                Assert.Equal(150.00m, result.LaborCost);
            }
        }

        [Fact]
        public async Task UpdateAsync_WithValidJob_ShouldUpdateServiceJob()
        {
            // Arrange
            Guid jobId = Guid.NewGuid();
            using (var context = new ServiceManagementDbContext(_options))
            {
                await SeedDbWithTestData(context, jobId);
            }

            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new ServiceJobRepository(context, _mockLogger.Object);
                var job = await repository.GetByIdAsync(jobId);
                
                job.Description = "Updated Oil Change";
                job.Status = "Completed";
                job.ActualEndTime = DateTime.UtcNow;

                await repository.UpdateAsync(job);
            }

            // Assert
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new ServiceJobRepository(context, _mockLogger.Object);
                var result = await repository.GetByIdAsync(jobId);

                Assert.NotNull(result);
                Assert.Equal("Updated Oil Change", result.Description);
                Assert.Equal("Completed", result.Status);
                Assert.NotNull(result.ActualEndTime);
            }
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldRemoveServiceJob()
        {
            // Arrange
            Guid jobId = Guid.NewGuid();
            using (var context = new ServiceManagementDbContext(_options))
            {
                await SeedDbWithTestData(context, jobId);
            }

            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new ServiceJobRepository(context, _mockLogger.Object);
                await repository.DeleteAsync(jobId);
            }

            // Assert
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new ServiceJobRepository(context, _mockLogger.Object);
                var result = await repository.GetByIdAsync(jobId);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetByRepairOrderIdAsync_ShouldReturnRelatedJobs()
        {
            // Arrange
            Guid repairOrderId = Guid.NewGuid();
            using (var context = new ServiceManagementDbContext(_options))
            {
                // Create three jobs, two with the same repair order ID
                var jobs = new List<ServiceJob>
                {
                    new ServiceJob
                    {
                        Id = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        Description = "Oil Change",
                        EstimatedTime = TimeSpan.FromHours(1),
                        Status = "Pending",
                        TechnicianId = Guid.NewGuid(),
                        ServiceBayId = Guid.NewGuid(),
                        LaborCost = 50.00m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new ServiceJob
                    {
                        Id = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        Description = "Tire Rotation",
                        EstimatedTime = TimeSpan.FromMinutes(30),
                        Status = "Pending",
                        TechnicianId = Guid.NewGuid(),
                        ServiceBayId = Guid.NewGuid(),
                        LaborCost = 25.00m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new ServiceJob
                    {
                        Id = Guid.NewGuid(),
                        RepairOrderId = Guid.NewGuid(), // Different repair order ID
                        Description = "Brake Replacement",
                        EstimatedTime = TimeSpan.FromHours(2),
                        Status = "Pending",
                        TechnicianId = Guid.NewGuid(),
                        ServiceBayId = Guid.NewGuid(),
                        LaborCost = 150.00m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };
                
                context.ServiceJobs.AddRange(jobs);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ServiceManagementDbContext(_options))
            {
                var repository = new ServiceJobRepository(context, _mockLogger.Object);
                var result = await repository.GetByRepairOrderIdAsync(repairOrderId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count);
                Assert.All(result, job => Assert.Equal(repairOrderId, job.RepairOrderId));
            }
        }

        private async Task SeedDbWithTestData(ServiceManagementDbContext context, Guid? specificId = null)
        {
            var jobs = new List<ServiceJob>
            {
                new ServiceJob
                {
                    Id = specificId ?? Guid.NewGuid(),
                    RepairOrderId = Guid.NewGuid(),
                    Description = "Oil Change",
                    EstimatedTime = TimeSpan.FromHours(1),
                    Status = "Pending",
                    TechnicianId = Guid.NewGuid(),
                    ServiceBayId = Guid.NewGuid(),
                    LaborCost = 50.00m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ServiceJob
                {
                    Id = Guid.NewGuid(),
                    RepairOrderId = Guid.NewGuid(),
                    Description = "Tire Rotation",
                    EstimatedTime = TimeSpan.FromMinutes(30),
                    Status = "Pending",
                    TechnicianId = Guid.NewGuid(),
                    ServiceBayId = Guid.NewGuid(),
                    LaborCost = 25.00m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ServiceJob
                {
                    Id = Guid.NewGuid(),
                    RepairOrderId = Guid.NewGuid(),
                    Description = "Brake Replacement",
                    EstimatedTime = TimeSpan.FromHours(2),
                    Status = "Pending",
                    TechnicianId = Guid.NewGuid(),
                    ServiceBayId = Guid.NewGuid(),
                    LaborCost = 150.00m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            
            context.ServiceJobs.AddRange(jobs);
            await context.SaveChangesAsync();
        }
    }
}
