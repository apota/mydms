using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Repositories;
using DMS.ServiceManagement.Core.Services.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DMS.ServiceManagement.API.Tests.Services
{
    public class ServiceJobServiceTests
    {
        private readonly Mock<IServiceJobRepository> _mockRepository;
        private readonly Mock<ILogger<ServiceJobService>> _mockLogger;
        private readonly ServiceJobService _service;

        public ServiceJobServiceTests()
        {
            _mockRepository = new Mock<IServiceJobRepository>();
            _mockLogger = new Mock<ILogger<ServiceJobService>>();
            _service = new ServiceJobService(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllServiceJobsAsync_ShouldReturnAllJobs()
        {
            // Arrange
            var expectedJobs = new List<ServiceJob>
            {
                new ServiceJob { Id = Guid.NewGuid(), Description = "Oil Change" },
                new ServiceJob { Id = Guid.NewGuid(), Description = "Tire Rotation" },
                new ServiceJob { Id = Guid.NewGuid(), Description = "Brake Inspection" }
            };
            
            _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(expectedJobs);

            // Act
            var result = await _service.GetAllServiceJobsAsync();

            // Assert
            Assert.Equal(expectedJobs.Count, result.Count);
            _mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetServiceJobByIdAsync_WithValidId_ShouldReturnJob()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var expectedJob = new ServiceJob 
            { 
                Id = jobId, 
                Description = "Oil Change",
                EstimatedTime = TimeSpan.FromHours(1)
            };
            
            _mockRepository.Setup(repo => repo.GetByIdAsync(jobId)).ReturnsAsync(expectedJob);

            // Act
            var result = await _service.GetServiceJobByIdAsync(jobId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(jobId, result.Id);
            Assert.Equal("Oil Change", result.Description);
            _mockRepository.Verify(repo => repo.GetByIdAsync(jobId), Times.Once);
        }

        [Fact]
        public async Task GetServiceJobByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.GetByIdAsync(jobId)).ReturnsAsync((ServiceJob)null);

            // Act
            var result = await _service.GetServiceJobByIdAsync(jobId);

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(repo => repo.GetByIdAsync(jobId), Times.Once);
        }

        [Fact]
        public async Task CreateServiceJobAsync_ShouldCreateAndReturnJob()
        {
            // Arrange
            var newJob = new ServiceJob
            {
                Description = "Oil Change",
                EstimatedTime = TimeSpan.FromHours(1),
                Status = "Pending",
                TechnicianId = Guid.NewGuid(),
                ServiceBayId = Guid.NewGuid(),
                RepairOrderId = Guid.NewGuid(),
                LaborCost = 50.00m
            };
            
            _mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<ServiceJob>()))
                .Callback<ServiceJob>(job => job.Id = Guid.NewGuid())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateServiceJobAsync(newJob);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Oil Change", result.Description);
            Assert.Equal(TimeSpan.FromHours(1), result.EstimatedTime);
            Assert.Equal("Pending", result.Status);
            Assert.Equal(50.00m, result.LaborCost);
            _mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<ServiceJob>()), Times.Once);
        }

        [Fact]
        public async Task UpdateServiceJobAsync_WithValidJob_ShouldUpdateAndReturnJob()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var existingJob = new ServiceJob
            {
                Id = jobId,
                Description = "Oil Change",
                Status = "Pending",
                EstimatedTime = TimeSpan.FromHours(1)
            };
            
            var updatedJob = new ServiceJob
            {
                Id = jobId,
                Description = "Oil Change with Filter",
                Status = "In Progress",
                EstimatedTime = TimeSpan.FromHours(1.5m)
            };
            
            _mockRepository.Setup(repo => repo.GetByIdAsync(jobId)).ReturnsAsync(existingJob);
            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<ServiceJob>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateServiceJobAsync(updatedJob);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(jobId, result.Id);
            Assert.Equal("Oil Change with Filter", result.Description);
            Assert.Equal("In Progress", result.Status);
            Assert.Equal(TimeSpan.FromHours(1.5m), result.EstimatedTime);
            _mockRepository.Verify(repo => repo.GetByIdAsync(jobId), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<ServiceJob>()), Times.Once);
        }

        [Fact]
        public async Task UpdateServiceJobAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var updatedJob = new ServiceJob
            {
                Id = jobId,
                Description = "Oil Change with Filter",
                Status = "In Progress"
            };
            
            _mockRepository.Setup(repo => repo.GetByIdAsync(jobId)).ReturnsAsync((ServiceJob)null);

            // Act
            var result = await _service.UpdateServiceJobAsync(updatedJob);

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(repo => repo.GetByIdAsync(jobId), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<ServiceJob>()), Times.Never);
        }

        [Fact]
        public async Task DeleteServiceJobAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var existingJob = new ServiceJob
            {
                Id = jobId,
                Description = "Oil Change"
            };
            
            _mockRepository.Setup(repo => repo.GetByIdAsync(jobId)).ReturnsAsync(existingJob);
            _mockRepository.Setup(repo => repo.DeleteAsync(jobId)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteServiceJobAsync(jobId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(repo => repo.GetByIdAsync(jobId), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteAsync(jobId), Times.Once);
        }

        [Fact]
        public async Task DeleteServiceJobAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.GetByIdAsync(jobId)).ReturnsAsync((ServiceJob)null);

            // Act
            var result = await _service.DeleteServiceJobAsync(jobId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(repo => repo.GetByIdAsync(jobId), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteAsync(jobId), Times.Never);
        }

        [Fact]
        public async Task GetServiceJobsByRepairOrderIdAsync_ShouldReturnRelatedJobs()
        {
            // Arrange
            var repairOrderId = Guid.NewGuid();
            var expectedJobs = new List<ServiceJob>
            {
                new ServiceJob { Id = Guid.NewGuid(), Description = "Oil Change", RepairOrderId = repairOrderId },
                new ServiceJob { Id = Guid.NewGuid(), Description = "Tire Rotation", RepairOrderId = repairOrderId }
            };
            
            _mockRepository.Setup(repo => repo.GetByRepairOrderIdAsync(repairOrderId)).ReturnsAsync(expectedJobs);

            // Act
            var result = await _service.GetServiceJobsByRepairOrderIdAsync(repairOrderId);

            // Assert
            Assert.Equal(expectedJobs.Count, result.Count);
            Assert.All(result, job => Assert.Equal(repairOrderId, job.RepairOrderId));
            _mockRepository.Verify(repo => repo.GetByRepairOrderIdAsync(repairOrderId), Times.Once);
        }
    }
}
