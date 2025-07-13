using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;
using DMS.PartsManagement.Core.Repositories;
using DMS.PartsManagement.Infrastructure.Services;
using Moq;
using Xunit;

namespace DMS.PartsManagement.API.Tests.Services
{
    public class CoreTrackingServiceTests
    {
        private readonly Mock<IPartCoreTrackingRepository> _mockCoreTrackingRepo;
        private readonly Mock<IPartRepository> _mockPartRepo;
        private readonly CoreTrackingService _service;

        public CoreTrackingServiceTests()
        {
            _mockCoreTrackingRepo = new Mock<IPartCoreTrackingRepository>();
            _mockPartRepo = new Mock<IPartRepository>();
            
            _service = new CoreTrackingService(
                _mockCoreTrackingRepo.Object,
                _mockPartRepo.Object);
        }

        [Fact]
        public async Task GetAllCoreTrackingAsync_ShouldReturnCoreTrackingRecords()
        {
            // Arrange
            var coreTracking = new List<PartCoreTracking>
            {
                new PartCoreTracking 
                { 
                    Id = Guid.NewGuid(), 
                    PartId = Guid.NewGuid(),
                    CorePartNumber = "CORE123",
                    CoreValue = 100.00m,
                    Status = CoreStatus.Sold,
                    SoldDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                },
                new PartCoreTracking 
                { 
                    Id = Guid.NewGuid(), 
                    PartId = Guid.NewGuid(),
                    CorePartNumber = "CORE456",
                    CoreValue = 75.00m,
                    Status = CoreStatus.Returned,
                    SoldDate = DateTime.UtcNow.AddDays(-30),
                    ReturnedDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                }
            };

            _mockCoreTrackingRepo.Setup(repo => repo.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(coreTracking);

            // Act
            var result = await _service.GetAllCoreTrackingAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, c => c.CorePartNumber == "CORE123" && c.Status == "Sold" && c.CoreValue == 100.00m);
            Assert.Contains(result, c => c.CorePartNumber == "CORE456" && c.Status == "Returned" && c.CoreValue == 75.00m);
        }

        [Fact]
        public async Task CreateCoreTrackingAsync_WithValidData_ShouldCreateCoreTrackingRecord()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var soldReferenceId = Guid.NewGuid();
            
            var part = new Part 
            { 
                Id = partId, 
                PartNumber = "ABC123", 
                Description = "Test Part with Core",
                Pricing = new PartPricing { CostPrice = 100.00m, HasCore = true, CoreCharge = 50.00m }
            };
            
            _mockPartRepo.Setup(repo => repo.GetByIdAsync(
                It.Is<Guid>(id => id == partId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(part);
                
            _mockCoreTrackingRepo.Setup(repo => repo.AddAsync(
                It.IsAny<PartCoreTracking>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartCoreTracking ct, CancellationToken _) => 
                {
                    ct.Part = part;
                    return ct;
                });
                
            var createDto = new CreateCoreTrackingDto
            {
                PartId = partId,
                CorePartNumber = "CORE-ABC123",
                CoreValue = 50.00m,
                SoldReferenceId = soldReferenceId,
                SoldReferenceNumber = "SO-12345",
                Notes = "Core tracking for brake caliper"
            };

            // Act
            var result = await _service.CreateCoreTrackingAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(partId, result.PartId);
            Assert.Equal("CORE-ABC123", result.CorePartNumber);
            Assert.Equal(50.00m, result.CoreValue);
            Assert.Equal("Sold", result.Status);
            Assert.Equal(soldReferenceId, result.SoldReferenceId);
            Assert.Equal("SO-12345", result.SoldReferenceNumber);
            
            _mockCoreTrackingRepo.Verify(repo => repo.AddAsync(
                It.Is<PartCoreTracking>(ct => 
                    ct.PartId == partId &&
                    ct.CorePartNumber == "CORE-ABC123" &&
                    ct.CoreValue == 50.00m &&
                    ct.Status == CoreStatus.Sold &&
                    ct.SoldReferenceId == soldReferenceId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessCoreReturnAsync_WithValidData_ShouldUpdateCoreTrackingStatus()
        {
            // Arrange
            var coreId = Guid.NewGuid();
            var partId = Guid.NewGuid();
            var returnDate = DateTime.UtcNow;
            
            var part = new Part 
            { 
                Id = partId, 
                PartNumber = "ABC123", 
                Description = "Test Part with Core"
            };
            
            var coreTracking = new PartCoreTracking
            {
                Id = coreId,
                PartId = partId,
                Part = part,
                CorePartNumber = "CORE-ABC123",
                CoreValue = 50.00m,
                Status = CoreStatus.Sold,
                SoldDate = returnDate.AddDays(-30),
                Notes = "Original sale note",
                CreatedAt = returnDate.AddDays(-30)
            };
            
            _mockCoreTrackingRepo.Setup(repo => repo.GetByIdAsync(
                It.Is<Guid>(id => id == coreId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(coreTracking);
                
            _mockCoreTrackingRepo.Setup(repo => repo.UpdateAsync(
                It.IsAny<PartCoreTracking>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartCoreTracking ct, CancellationToken _) => ct);
                
            var processReturnDto = new ProcessCoreReturnDto
            {
                ReturnedDate = returnDate,
                ReturnReferenceId = Guid.NewGuid(),
                Notes = "Core returned with minor damage",
                IsDamaged = true,
                DamageDescription = "Rust on mounting bracket"
            };

            // Act
            var result = await _service.ProcessCoreReturnAsync(coreId, processReturnDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(coreId, result.Id);
            Assert.Equal("Returned", result.Status);
            Assert.Equal(returnDate, result.ReturnedDate);
            Assert.Equal(processReturnDto.ReturnReferenceId, result.ReturnReferenceId);
            Assert.Contains("DAMAGED: Rust on mounting bracket", result.Notes);
            
            _mockCoreTrackingRepo.Verify(repo => repo.UpdateAsync(
                It.Is<PartCoreTracking>(ct => 
                    ct.Id == coreId &&
                    ct.Status == CoreStatus.Returned &&
                    ct.ReturnedDate == returnDate),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ApplyCreditAsync_WithValidData_ShouldUpdateCoreTrackingStatus()
        {
            // Arrange
            var coreId = Guid.NewGuid();
            var partId = Guid.NewGuid();
            var creditDate = DateTime.UtcNow;
            
            var part = new Part 
            { 
                Id = partId, 
                PartNumber = "ABC123", 
                Description = "Test Part with Core"
            };
            
            var coreTracking = new PartCoreTracking
            {
                Id = coreId,
                PartId = partId,
                Part = part,
                CorePartNumber = "CORE-ABC123",
                CoreValue = 50.00m,
                Status = CoreStatus.Returned,
                SoldDate = creditDate.AddDays(-60),
                ReturnedDate = creditDate.AddDays(-5),
                Notes = "Core returned with minor damage",
                CreatedAt = creditDate.AddDays(-60)
            };
            
            _mockCoreTrackingRepo.Setup(repo => repo.GetByIdAsync(
                It.Is<Guid>(id => id == coreId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(coreTracking);
                
            _mockCoreTrackingRepo.Setup(repo => repo.UpdateAsync(
                It.IsAny<PartCoreTracking>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartCoreTracking ct, CancellationToken _) => ct);
                
            var applyCreditDto = new ApplyCreditDto
            {
                CreditedDate = creditDate,
                CreditAmount = 45.00m, // Reduced due to damage
                Notes = "Credit applied with 10% deduction for damage"
            };

            // Act
            var result = await _service.ApplyCreditAsync(coreId, applyCreditDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(coreId, result.Id);
            Assert.Equal("Credited", result.Status);
            Assert.Equal(creditDate, result.CreditedDate);
            Assert.Equal(45.00m, result.CreditAmount);
            Assert.Contains("CREDIT", result.Notes);
            Assert.Contains("10% deduction", result.Notes);
            
            _mockCoreTrackingRepo.Verify(repo => repo.UpdateAsync(
                It.Is<PartCoreTracking>(ct => 
                    ct.Id == coreId &&
                    ct.Status == CoreStatus.Credited &&
                    ct.CreditedDate == creditDate &&
                    ct.CreditAmount == 45.00m),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTotalOutstandingCoreValueAsync_ShouldReturnSumOfSoldCores()
        {
            // Arrange
            _mockCoreTrackingRepo.Setup(repo => repo.GetTotalCoreValueByStatusAsync(
                It.Is<CoreStatus>(s => s == CoreStatus.Sold),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(1250.75m);

            // Act
            var result = await _service.GetTotalOutstandingCoreValueAsync();

            // Assert
            Assert.Equal(1250.75m, result);
            
            _mockCoreTrackingRepo.Verify(repo => repo.GetTotalCoreValueByStatusAsync(
                CoreStatus.Sold,
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
