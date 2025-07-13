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
    public class OrderServiceTests
    {
        private readonly Mock<IPartOrderRepository> _mockOrderRepo;
        private readonly Mock<ISupplierRepository> _mockSupplierRepo;
        private readonly Mock<IPartRepository> _mockPartRepo;
        private readonly Mock<IPartInventoryRepository> _mockInventoryRepo;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _mockOrderRepo = new Mock<IPartOrderRepository>();
            _mockSupplierRepo = new Mock<ISupplierRepository>();
            _mockPartRepo = new Mock<IPartRepository>();
            _mockInventoryRepo = new Mock<IPartInventoryRepository>();
            
            _service = new OrderService(
                _mockOrderRepo.Object, 
                _mockSupplierRepo.Object,
                _mockPartRepo.Object,
                _mockInventoryRepo.Object);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnOrders()
        {
            // Arrange
            var supplier = new Supplier { Id = Guid.NewGuid(), Name = "Test Supplier" };
            var orders = new List<PartOrder>
            {
                new PartOrder 
                { 
                    Id = Guid.NewGuid(), 
                    OrderNumber = "PO-001", 
                    SupplierId = supplier.Id,
                    Supplier = supplier,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Draft
                },
                new PartOrder 
                { 
                    Id = Guid.NewGuid(), 
                    OrderNumber = "PO-002", 
                    SupplierId = supplier.Id,
                    Supplier = supplier,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Submitted
                }
            };

            _mockOrderRepo.Setup(repo => repo.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(orders);

            // Act
            var result = await _service.GetAllOrdersAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, o => o.OrderNumber == "PO-001" && o.Status == "Draft");
            Assert.Contains(result, o => o.OrderNumber == "PO-002" && o.Status == "Submitted");
        }

        [Fact]
        public async Task CreateOrderAsync_WithValidData_ShouldCreateOrder()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var partId = Guid.NewGuid();
            
            var supplier = new Supplier { Id = supplierId, Name = "Test Supplier" };
            var part = new Part 
            { 
                Id = partId, 
                PartNumber = "ABC123", 
                Description = "Test Part",
                Pricing = new PartPricing { CostPrice = 10.00m, RetailPrice = 20.00m }
            };
            
            _mockSupplierRepo.Setup(repo => repo.ExistsAsync(
                It.Is<Guid>(id => id == supplierId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
                
            _mockPartRepo.Setup(repo => repo.GetByIdAsync(
                It.Is<Guid>(id => id == partId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(part);
                
            _mockOrderRepo.Setup(repo => repo.AddAsync(
                It.IsAny<PartOrder>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartOrder order, CancellationToken _) => 
                {
                    order.Supplier = supplier;
                    return order;
                });
                
            var createOrderDto = new CreatePartOrderDto
            {
                SupplierId = supplierId,
                OrderDate = DateTime.UtcNow,
                OrderType = "Stock",
                OrderLines = new List<CreateOrderLineDto>
                {
                    new CreateOrderLineDto
                    {
                        PartId = partId,
                        Quantity = 5
                    }
                }
            };

            // Act
            var result = await _service.CreateOrderAsync(createOrderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(supplierId, result.SupplierId);
            Assert.Equal("Test Supplier", result.SupplierName);
            Assert.Equal("Stock", result.OrderType);
            Assert.Equal("Draft", result.Status);
            Assert.Equal(1, result.OrderLines.Count);
            
            var line = result.OrderLines.First();
            Assert.Equal(partId, line.PartId);
            Assert.Equal(5, line.Quantity);
            Assert.Equal(50.00m, result.TotalAmount); // 5 * $10.00 = $50.00
            
            _mockOrderRepo.Verify(repo => repo.AddAsync(
                It.Is<PartOrder>(o => 
                    o.SupplierId == supplierId && 
                    o.OrderType == OrderType.Stock && 
                    o.Status == OrderStatus.Draft &&
                    o.OrderLines.Count == 1),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SubmitOrderAsync_WithDraftOrder_ShouldSubmitOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new PartOrder
            {
                Id = orderId,
                OrderNumber = "PO-001",
                Status = OrderStatus.Draft,
                OrderLines = new List<PartOrderLine>
                {
                    new PartOrderLine
                    {
                        PartId = Guid.NewGuid(),
                        Quantity = 5
                    }
                }
            };

            _mockOrderRepo.Setup(repo => repo.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);
                
            _mockOrderRepo.Setup(repo => repo.UpdateAsync(
                It.IsAny<PartOrder>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartOrder o, CancellationToken _) => o);

            // Act
            var result = await _service.SubmitOrderAsync(orderId);

            // Assert
            Assert.True(result);
            _mockOrderRepo.Verify(repo => repo.UpdateAsync(
                It.Is<PartOrder>(o => o.Id == orderId && o.Status == OrderStatus.Submitted),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ReceiveOrderAsync_WithValidData_ShouldReceiveOrderAndUpdateInventory()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var partId = Guid.NewGuid();
            var orderLineId = Guid.NewGuid();
            var locationId = Guid.NewGuid();
            
            var part = new Part
            {
                Id = partId,
                PartNumber = "ABC123",
                Description = "Test Part"
            };
            
            var orderLine = new PartOrderLine
            {
                Id = orderLineId,
                PartId = partId,
                Part = part,
                Quantity = 10,
                ReceivedQuantity = 0,
                Status = OrderLineStatus.Ordered
            };
            
            var order = new PartOrder
            {
                Id = orderId,
                OrderNumber = "PO-001",
                Status = OrderStatus.Submitted,
                OrderLines = new List<PartOrderLine> { orderLine }
            };
            
            var inventory = new PartInventory
            {
                PartId = partId,
                LocationId = locationId,
                QuantityOnHand = 5
            };

            _mockOrderRepo.Setup(repo => repo.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);
                
            _mockInventoryRepo.Setup(repo => repo.GetByPartIdAsync(
                It.Is<Guid>(id => id == partId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);
                
            _mockInventoryRepo.Setup(repo => repo.UpdateAsync(
                It.IsAny<PartInventory>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartInventory inv, CancellationToken _) => inv);
                
            _mockOrderRepo.Setup(repo => repo.UpdateAsync(
                It.IsAny<PartOrder>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartOrder o, CancellationToken _) => o);
                
            var receiveDto = new PartOrderReceiveDto
            {
                OrderId = orderId,
                ReceivedLines = new List<ReceiveOrderLineDto>
                {
                    new ReceiveOrderLineDto
                    {
                        OrderLineId = orderLineId,
                        ReceivedQuantity = 5
                    }
                }
            };

            // Act
            var result = await _service.ReceiveOrderAsync(receiveDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.OrderId);
            Assert.Equal("PO-001", result.OrderNumber);
            Assert.Equal("Partial", result.Status);
            Assert.Equal(1, result.ReceivedLines.Count);
            
            var receivedLine = result.ReceivedLines.First();
            Assert.Equal(orderLineId, receivedLine.OrderLineId);
            Assert.Equal(5, receivedLine.ReceivedQuantity);
            Assert.Equal(5, receivedLine.BackorderedQuantity);
            
            _mockOrderRepo.Verify(repo => repo.UpdateAsync(
                It.Is<PartOrder>(o => 
                    o.Id == orderId && 
                    o.Status == OrderStatus.Partial &&
                    o.OrderLines.First().ReceivedQuantity == 5 &&
                    o.OrderLines.First().Status == OrderLineStatus.Backordered),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
