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
    public class TransactionServiceTests
    {
        private readonly Mock<IPartTransactionRepository> _mockTransactionRepo;
        private readonly Mock<IPartRepository> _mockPartRepo;
        private readonly Mock<IPartInventoryRepository> _mockInventoryRepo;
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            _mockTransactionRepo = new Mock<IPartTransactionRepository>();
            _mockPartRepo = new Mock<IPartRepository>();
            _mockInventoryRepo = new Mock<IPartInventoryRepository>();
            
            _service = new TransactionService(
                _mockTransactionRepo.Object,
                _mockPartRepo.Object,
                _mockInventoryRepo.Object);
        }

        [Fact]
        public async Task GetAllTransactionsAsync_ShouldReturnTransactions()
        {
            // Arrange
            var transactions = new List<PartTransaction>
            {
                new PartTransaction 
                { 
                    Id = Guid.NewGuid(), 
                    TransactionType = TransactionType.Receipt, 
                    PartId = Guid.NewGuid(),
                    Quantity = 10,
                    TransactionDate = DateTime.UtcNow
                },
                new PartTransaction 
                { 
                    Id = Guid.NewGuid(), 
                    TransactionType = TransactionType.Issue, 
                    PartId = Guid.NewGuid(),
                    Quantity = 5,
                    TransactionDate = DateTime.UtcNow
                }
            };

            _mockTransactionRepo.Setup(repo => repo.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            // Act
            var result = await _service.GetAllTransactionsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, t => t.TransactionType == "Receipt" && t.Quantity == 10);
            Assert.Contains(result, t => t.TransactionType == "Issue" && t.Quantity == 5);
        }

        [Fact]
        public async Task IssuePartsAsync_WithValidData_ShouldIssuePartsAndUpdateInventory()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            
            var part = new Part 
            { 
                Id = partId, 
                PartNumber = "ABC123", 
                Description = "Test Part",
                Pricing = new PartPricing { CostPrice = 10.00m, RetailPrice = 20.00m }
            };
            
            var inventory = new PartInventory
            {
                PartId = partId,
                LocationId = locationId,
                QuantityOnHand = 10
            };
            
            _mockPartRepo.Setup(repo => repo.GetByIdAsync(
                It.Is<Guid>(id => id == partId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(part);
                
            _mockInventoryRepo.Setup(repo => repo.GetByPartIdAndLocationAsync(
                It.Is<Guid>(id => id == partId),
                It.Is<Guid>(id => id == locationId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);
                
            _mockInventoryRepo.Setup(repo => repo.UpdateAsync(
                It.IsAny<PartInventory>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartInventory inv, CancellationToken _) => inv);
                
            _mockTransactionRepo.Setup(repo => repo.AddAsync(
                It.IsAny<PartTransaction>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartTransaction t, CancellationToken _) => 
                {
                    t.Part = part;
                    return t;
                });
                
            var issueDto = new IssuePartsDto
            {
                PartId = partId,
                LocationId = locationId,
                Quantity = 5,
                ReferenceType = "ServiceOrder",
                UnitPrice = 22.50m
            };

            // Act
            var result = await _service.IssuePartsAsync(issueDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(partId, result.PartId);
            Assert.Equal("ABC123", result.PartNumber);
            Assert.Equal("Issue", result.TransactionType);
            Assert.Equal(5, result.Quantity);
            Assert.Equal(22.50m, result.UnitPrice);
            Assert.Equal(112.50m, result.ExtendedPrice); // 5 * $22.50 = $112.50
            
            _mockInventoryRepo.Verify(repo => repo.UpdateAsync(
                It.Is<PartInventory>(i => i.QuantityOnHand == 5), // 10 - 5 = 5
                It.IsAny<CancellationToken>()),
                Times.Once);
                
            _mockTransactionRepo.Verify(repo => repo.AddAsync(
                It.Is<PartTransaction>(t => 
                    t.PartId == partId &&
                    t.Quantity == 5 && 
                    t.TransactionType == TransactionType.Issue &&
                    t.SourceLocationId == locationId &&
                    t.ReferenceType == "ServiceOrder"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AdjustInventoryAsync_WithPositiveAdjustment_ShouldUpdateInventory()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            
            var part = new Part 
            { 
                Id = partId, 
                PartNumber = "ABC123", 
                Description = "Test Part",
                Pricing = new PartPricing { CostPrice = 10.00m }
            };
            
            var inventory = new PartInventory
            {
                PartId = partId,
                LocationId = locationId,
                QuantityOnHand = 5
            };
            
            _mockPartRepo.Setup(repo => repo.GetByIdAsync(
                It.Is<Guid>(id => id == partId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(part);
                
            _mockInventoryRepo.Setup(repo => repo.GetByPartIdAndLocationAsync(
                It.Is<Guid>(id => id == partId),
                It.Is<Guid>(id => id == locationId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);
                
            _mockInventoryRepo.Setup(repo => repo.UpdateAsync(
                It.IsAny<PartInventory>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartInventory inv, CancellationToken _) => inv);
                
            _mockTransactionRepo.Setup(repo => repo.AddAsync(
                It.IsAny<PartTransaction>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartTransaction t, CancellationToken _) => 
                {
                    t.Part = part;
                    return t;
                });
                
            var adjustDto = new AdjustInventoryDto
            {
                PartId = partId,
                LocationId = locationId,
                QuantityAdjustment = 3,
                AdjustmentReason = "Count adjustment"
            };

            // Act
            var result = await _service.AdjustInventoryAsync(adjustDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(partId, result.PartId);
            Assert.Equal("ABC123", result.PartNumber);
            Assert.Equal("Adjustment", result.TransactionType);
            Assert.Equal(3, result.Quantity);
            Assert.Equal(locationId, result.DestinationLocationId);
            Assert.Equal("Count adjustment", result.ReferenceType);
            
            _mockInventoryRepo.Verify(repo => repo.UpdateAsync(
                It.Is<PartInventory>(i => i.QuantityOnHand == 8), // 5 + 3 = 8
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TransferPartsAsync_WithValidData_ShouldTransferPartsAndUpdateInventory()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var sourceLocationId = Guid.NewGuid();
            var destLocationId = Guid.NewGuid();
            
            var part = new Part 
            { 
                Id = partId, 
                PartNumber = "ABC123", 
                Description = "Test Part",
                Pricing = new PartPricing { CostPrice = 10.00m }
            };
            
            var sourceInventory = new PartInventory
            {
                PartId = partId,
                LocationId = sourceLocationId,
                QuantityOnHand = 10
            };
            
            var destInventory = new PartInventory
            {
                PartId = partId,
                LocationId = destLocationId,
                QuantityOnHand = 2
            };
            
            _mockPartRepo.Setup(repo => repo.GetByIdAsync(
                It.Is<Guid>(id => id == partId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(part);
                
            _mockInventoryRepo.Setup(repo => repo.GetByPartIdAndLocationAsync(
                It.Is<Guid>(id => id == partId),
                It.Is<Guid>(id => id == sourceLocationId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(sourceInventory);
                
            _mockInventoryRepo.Setup(repo => repo.GetByPartIdAndLocationAsync(
                It.Is<Guid>(id => id == partId),
                It.Is<Guid>(id => id == destLocationId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(destInventory);
                
            _mockInventoryRepo.Setup(repo => repo.UpdateAsync(
                It.IsAny<PartInventory>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartInventory inv, CancellationToken _) => inv);
                
            _mockTransactionRepo.Setup(repo => repo.AddAsync(
                It.IsAny<PartTransaction>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartTransaction t, CancellationToken _) => 
                {
                    t.Part = part;
                    return t;
                });
                
            var transferDto = new TransferPartsDto
            {
                PartId = partId,
                SourceLocationId = sourceLocationId,
                DestinationLocationId = destLocationId,
                Quantity = 5,
                Notes = "Transfer to store location"
            };

            // Act
            var result = await _service.TransferPartsAsync(transferDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(partId, result.PartId);
            Assert.Equal("ABC123", result.PartNumber);
            Assert.Equal("Transfer", result.TransactionType);
            Assert.Equal(5, result.Quantity);
            Assert.Equal(sourceLocationId, result.SourceLocationId);
            Assert.Equal(destLocationId, result.DestinationLocationId);
            
            _mockInventoryRepo.Verify(repo => repo.UpdateAsync(
                It.Is<PartInventory>(i => i.LocationId == sourceLocationId && i.QuantityOnHand == 5), // 10 - 5 = 5
                It.IsAny<CancellationToken>()),
                Times.Once);
                
            _mockInventoryRepo.Verify(repo => repo.UpdateAsync(
                It.Is<PartInventory>(i => i.LocationId == destLocationId && i.QuantityOnHand == 7), // 2 + 5 = 7
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
