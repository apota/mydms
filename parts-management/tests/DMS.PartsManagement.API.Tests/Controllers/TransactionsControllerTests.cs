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
    public class TransactionsControllerTests
    {
        private readonly Mock<ITransactionService> _mockTransactionService;
        private readonly Mock<ILogger<TransactionsController>> _mockLogger;
        private readonly TransactionsController _controller;

        public TransactionsControllerTests()
        {
            _mockTransactionService = new Mock<ITransactionService>();
            _mockLogger = new Mock<ILogger<TransactionsController>>();
            _controller = new TransactionsController(_mockTransactionService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllTransactions_ReturnsOkResult_WithTransactions()
        {
            // Arrange
            var skip = 0;
            var take = 50;
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            var expectedTransactions = new List<PartTransactionDto>
            {
                new PartTransactionDto
                {
                    Id = Guid.NewGuid(),
                    PartId = Guid.NewGuid(),
                    PartNumber = "P12345",
                    TransactionType = TransactionType.Receive.ToString(),
                    Quantity = 10,
                    TransactionDate = DateTime.UtcNow.AddDays(-3)
                },
                new PartTransactionDto
                {
                    Id = Guid.NewGuid(),
                    PartId = Guid.NewGuid(),
                    PartNumber = "P67890",
                    TransactionType = TransactionType.Issue.ToString(),
                    Quantity = 5,
                    TransactionDate = DateTime.UtcNow.AddDays(-2)
                }
            };

            _mockTransactionService.Setup(s => s.GetAllTransactionsAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedTransactions);

            // Act
            var result = await _controller.GetAllTransactions(skip, take, startDate, endDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransactions = Assert.IsAssignableFrom<IEnumerable<PartTransactionDto>>(okResult.Value);
            Assert.Equal(2, returnedTransactions.Count());
            Assert.Equal(expectedTransactions, returnedTransactions);
        }

        [Fact]
        public async Task GetTransactionById_WithValidId_ReturnsOkResult_WithTransaction()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var expectedTransaction = new PartTransactionDto
            {
                Id = transactionId,
                PartId = Guid.NewGuid(),
                PartNumber = "P12345",
                PartDescription = "Test Part",
                TransactionType = TransactionType.Receive.ToString(),
                Quantity = 10,
                TransactionDate = DateTime.UtcNow.AddDays(-1)
            };

            _mockTransactionService.Setup(s => s.GetTransactionByIdAsync(
                It.Is<Guid>(id => id == transactionId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedTransaction);

            // Act
            var result = await _controller.GetTransactionById(transactionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransaction = Assert.IsType<PartTransactionDto>(okResult.Value);
            Assert.Equal(transactionId, returnedTransaction.Id);
            Assert.Equal(expectedTransaction.PartNumber, returnedTransaction.PartNumber);
            Assert.Equal(expectedTransaction.TransactionType, returnedTransaction.TransactionType);
        }

        [Fact]
        public async Task GetTransactionById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            _mockTransactionService.Setup(s => s.GetTransactionByIdAsync(
                It.Is<Guid>(id => id == transactionId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartTransactionDto)null);

            // Act
            var result = await _controller.GetTransactionById(transactionId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetTransactionsByPartId_ReturnsOkResult_WithTransactions()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var expectedTransactions = new List<PartTransactionDto>
            {
                new PartTransactionDto
                {
                    Id = Guid.NewGuid(),
                    PartId = partId,
                    PartNumber = "P12345",
                    TransactionType = TransactionType.Receive.ToString(),
                    Quantity = 10,
                    TransactionDate = DateTime.UtcNow.AddDays(-3)
                },
                new PartTransactionDto
                {
                    Id = Guid.NewGuid(),
                    PartId = partId,
                    PartNumber = "P12345",
                    TransactionType = TransactionType.Issue.ToString(),
                    Quantity = 5,
                    TransactionDate = DateTime.UtcNow.AddDays(-2)
                }
            };

            _mockTransactionService.Setup(s => s.GetTransactionsByPartIdAsync(
                It.Is<Guid>(id => id == partId), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedTransactions);

            // Act
            var result = await _controller.GetTransactionsByPartId(partId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransactions = Assert.IsAssignableFrom<IEnumerable<PartTransactionDto>>(okResult.Value);
            Assert.Equal(2, returnedTransactions.Count());
            Assert.All(returnedTransactions, t => Assert.Equal(partId, t.PartId));
        }

        [Fact]
        public async Task IssueParts_WithValidDto_ReturnsOkResult_WithTransaction()
        {
            // Arrange
            var issuePartsDto = new IssuePartsDto
            {
                PartId = Guid.NewGuid(),
                Quantity = 5,
                LocationId = Guid.NewGuid(),
                ReferenceType = "ServiceOrder",
                ReferenceId = Guid.NewGuid(),
                ReferenceNumber = "SO-12345"
            };

            var createdTransaction = new PartTransactionDto
            {
                Id = Guid.NewGuid(),
                PartId = issuePartsDto.PartId,
                PartNumber = "P12345",
                TransactionType = TransactionType.Issue.ToString(),
                Quantity = issuePartsDto.Quantity,
                SourceLocationId = issuePartsDto.LocationId,
                ReferenceType = issuePartsDto.ReferenceType,
                ReferenceId = issuePartsDto.ReferenceId,
                ReferenceNumber = issuePartsDto.ReferenceNumber,
                TransactionDate = DateTime.UtcNow
            };

            _mockTransactionService.Setup(s => s.IssuePartsAsync(
                It.IsAny<IssuePartsDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTransaction);

            // Act
            var result = await _controller.IssueParts(issuePartsDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransaction = Assert.IsType<PartTransactionDto>(okResult.Value);
            Assert.Equal(issuePartsDto.PartId, returnedTransaction.PartId);
            Assert.Equal(TransactionType.Issue.ToString(), returnedTransaction.TransactionType);
            Assert.Equal(issuePartsDto.Quantity, returnedTransaction.Quantity);
        }

        [Fact]
        public async Task ReturnParts_WithValidDto_ReturnsOkResult_WithTransaction()
        {
            // Arrange
            var returnPartsDto = new ReturnPartsDto
            {
                PartId = Guid.NewGuid(),
                Quantity = 3,
                LocationId = Guid.NewGuid(),
                ReferenceType = "ServiceOrder",
                ReferenceId = Guid.NewGuid(),
                ReferenceNumber = "SO-12345"
            };

            var createdTransaction = new PartTransactionDto
            {
                Id = Guid.NewGuid(),
                PartId = returnPartsDto.PartId,
                PartNumber = "P12345",
                TransactionType = TransactionType.Return.ToString(),
                Quantity = returnPartsDto.Quantity,
                DestinationLocationId = returnPartsDto.LocationId,
                ReferenceType = returnPartsDto.ReferenceType,
                ReferenceId = returnPartsDto.ReferenceId,
                ReferenceNumber = returnPartsDto.ReferenceNumber,
                TransactionDate = DateTime.UtcNow
            };

            _mockTransactionService.Setup(s => s.ReturnPartsAsync(
                It.IsAny<ReturnPartsDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTransaction);

            // Act
            var result = await _controller.ReturnParts(returnPartsDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransaction = Assert.IsType<PartTransactionDto>(okResult.Value);
            Assert.Equal(returnPartsDto.PartId, returnedTransaction.PartId);
            Assert.Equal(TransactionType.Return.ToString(), returnedTransaction.TransactionType);
            Assert.Equal(returnPartsDto.Quantity, returnedTransaction.Quantity);
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
                AdjustmentReason = "Inventory count"
            };

            var createdTransaction = new PartTransactionDto
            {
                Id = Guid.NewGuid(),
                PartId = adjustInventoryDto.PartId,
                PartNumber = "P12345",
                TransactionType = TransactionType.Adjustment.ToString(),
                Quantity = adjustInventoryDto.QuantityAdjustment,
                DestinationLocationId = adjustInventoryDto.LocationId,
                Notes = adjustInventoryDto.AdjustmentReason,
                TransactionDate = DateTime.UtcNow
            };

            _mockTransactionService.Setup(s => s.AdjustInventoryAsync(
                It.IsAny<AdjustInventoryDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTransaction);

            // Act
            var result = await _controller.AdjustInventory(adjustInventoryDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransaction = Assert.IsType<PartTransactionDto>(okResult.Value);
            Assert.Equal(adjustInventoryDto.PartId, returnedTransaction.PartId);
            Assert.Equal(TransactionType.Adjustment.ToString(), returnedTransaction.TransactionType);
            Assert.Equal(adjustInventoryDto.QuantityAdjustment, returnedTransaction.Quantity);
        }

        [Fact]
        public async Task TransferParts_WithValidDto_ReturnsOkResult_WithTransaction()
        {
            // Arrange
            var transferPartsDto = new TransferPartsDto
            {
                PartId = Guid.NewGuid(),
                Quantity = 15,
                SourceLocationId = Guid.NewGuid(),
                DestinationLocationId = Guid.NewGuid()
            };

            var createdTransaction = new PartTransactionDto
            {
                Id = Guid.NewGuid(),
                PartId = transferPartsDto.PartId,
                PartNumber = "P12345",
                TransactionType = TransactionType.Transfer.ToString(),
                Quantity = transferPartsDto.Quantity,
                SourceLocationId = transferPartsDto.SourceLocationId,
                DestinationLocationId = transferPartsDto.DestinationLocationId,
                TransactionDate = DateTime.UtcNow
            };

            _mockTransactionService.Setup(s => s.TransferPartsAsync(
                It.IsAny<TransferPartsDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTransaction);

            // Act
            var result = await _controller.TransferParts(transferPartsDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransaction = Assert.IsType<PartTransactionDto>(okResult.Value);
            Assert.Equal(transferPartsDto.PartId, returnedTransaction.PartId);
            Assert.Equal(TransactionType.Transfer.ToString(), returnedTransaction.TransactionType);
            Assert.Equal(transferPartsDto.Quantity, returnedTransaction.Quantity);
            Assert.Equal(transferPartsDto.SourceLocationId, returnedTransaction.SourceLocationId);
            Assert.Equal(transferPartsDto.DestinationLocationId, returnedTransaction.DestinationLocationId);
        }

        [Fact]
        public async Task GetTransactionHistory_ReturnsOkResult_WithTransactionSummaries()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;

            var expectedHistory = new List<PartTransactionSummaryDto>
            {
                new PartTransactionSummaryDto
                {
                    Id = Guid.NewGuid(),
                    TransactionType = TransactionType.Receive.ToString(),
                    Quantity = 50,
                    LocationName = "Main Warehouse",
                    ReferenceNumber = "PO-12345",
                    TransactionDate = DateTime.UtcNow.AddDays(-25)
                },
                new PartTransactionSummaryDto
                {
                    Id = Guid.NewGuid(),
                    TransactionType = TransactionType.Issue.ToString(),
                    Quantity = 10,
                    LocationName = "Main Warehouse",
                    ReferenceNumber = "SO-56789",
                    TransactionDate = DateTime.UtcNow.AddDays(-18)
                },
                new PartTransactionSummaryDto
                {
                    Id = Guid.NewGuid(),
                    TransactionType = TransactionType.Return.ToString(),
                    Quantity = 2,
                    LocationName = "Main Warehouse",
                    ReferenceNumber = "SO-56789",
                    TransactionDate = DateTime.UtcNow.AddDays(-15)
                }
            };

            _mockTransactionService.Setup(s => s.GetTransactionHistoryAsync(
                It.Is<Guid>(id => id == partId), 
                It.IsAny<DateTime>(), 
                It.IsAny<DateTime>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHistory);

            // Act
            var result = await _controller.GetTransactionHistory(partId, startDate, endDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedHistory = Assert.IsAssignableFrom<IEnumerable<PartTransactionSummaryDto>>(okResult.Value);
            Assert.Equal(3, returnedHistory.Count());
            Assert.Equal(expectedHistory, returnedHistory);
        }
    }
}
