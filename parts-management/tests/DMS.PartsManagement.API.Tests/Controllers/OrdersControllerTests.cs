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
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<ILogger<OrdersController>> _mockLogger;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockLogger = new Mock<ILogger<OrdersController>>();
            _controller = new OrdersController(_mockOrderService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllOrders_ReturnsOkResult_WithOrders()
        {
            // Arrange
            var expectedOrders = new List<PartOrderSummaryDto>
            {
                new PartOrderSummaryDto
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = "ORD-001",
                    SupplierId = Guid.NewGuid(),
                    SupplierName = "Test Supplier",
                    OrderDate = DateTime.UtcNow.AddDays(-1),
                    Status = OrderStatus.Pending.ToString()
                },
                new PartOrderSummaryDto
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = "ORD-002",
                    SupplierId = Guid.NewGuid(),
                    SupplierName = "Another Supplier",
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Shipped.ToString()
                }
            };

            _mockOrderService.Setup(s => s.GetAllOrdersAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrders);

            // Act
            var result = await _controller.GetAllOrders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedOrders = Assert.IsAssignableFrom<IEnumerable<PartOrderSummaryDto>>(okResult.Value);
            Assert.Equal(2, returnedOrders.Count());
            Assert.Equal(expectedOrders, returnedOrders);
        }

        [Fact]
        public async Task GetOrderById_WithValidId_ReturnsOkResult_WithOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var expectedOrder = new PartOrderDetailDto
            {
                Id = orderId,
                OrderNumber = "ORD-001",
                SupplierId = Guid.NewGuid(),
                SupplierName = "Test Supplier",
                OrderDate = DateTime.UtcNow.AddDays(-1),
                Status = OrderStatus.Pending.ToString(),
                OrderLines = new List<PartOrderLineDto>
                {
                    new PartOrderLineDto
                    {
                        Id = Guid.NewGuid(),
                        PartId = Guid.NewGuid(),
                        PartNumber = "P12345",
                        Quantity = 5
                    }
                }
            };

            _mockOrderService.Setup(s => s.GetOrderByIdAsync(
                It.Is<Guid>(id => id == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrder);

            // Act
            var result = await _controller.GetOrderById(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedOrder = Assert.IsType<PartOrderDetailDto>(okResult.Value);
            Assert.Equal(orderId, returnedOrder.Id);
            Assert.Equal(expectedOrder.OrderNumber, returnedOrder.OrderNumber);
            Assert.Single(returnedOrder.OrderLines);
        }

        [Fact]
        public async Task GetOrderById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _mockOrderService.Setup(s => s.GetOrderByIdAsync(
                It.Is<Guid>(id => id == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartOrderDetailDto)null);

            // Act
            var result = await _controller.GetOrderById(orderId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateOrder_WithValidDto_ReturnsCreatedAtAction()
        {
            // Arrange
            var createOrderDto = new CreatePartOrderDto
            {
                SupplierId = Guid.NewGuid(),
                OrderDate = DateTime.UtcNow,
                OrderType = "Regular",
                OrderLines = new List<CreateOrderLineDto>
                {
                    new CreateOrderLineDto
                    {
                        PartId = Guid.NewGuid(),
                        Quantity = 5
                    }
                }
            };

            var createdOrder = new PartOrderDetailDto
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-001",
                SupplierId = createOrderDto.SupplierId,
                OrderDate = createOrderDto.OrderDate,
                Status = OrderStatus.Draft.ToString()
            };

            _mockOrderService.Setup(s => s.CreateOrderAsync(
                It.IsAny<CreatePartOrderDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _controller.CreateOrder(createOrderDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(OrdersController.GetOrderById), createdAtActionResult.ActionName);
            Assert.Equal(createdOrder.Id, createdAtActionResult.RouteValues["id"]);
            var returnedOrder = Assert.IsType<PartOrderDetailDto>(createdAtActionResult.Value);
            Assert.Equal(createdOrder.Id, returnedOrder.Id);
        }

        [Fact]
        public async Task CreateOrder_WhenServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var createOrderDto = new CreatePartOrderDto
            {
                SupplierId = Guid.NewGuid(),
                OrderDate = DateTime.UtcNow,
                OrderType = "Regular",
                OrderLines = new List<CreateOrderLineDto>
                {
                    new CreateOrderLineDto
                    {
                        PartId = Guid.NewGuid(),
                        Quantity = 5
                    }
                }
            };

            var errorMessage = "Invalid supplier";
            _mockOrderService.Setup(s => s.CreateOrderAsync(
                It.IsAny<CreatePartOrderDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await _controller.CreateOrder(createOrderDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOrder_WithValidData_ReturnsOkResult_WithUpdatedOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateOrderDto = new UpdatePartOrderDto
            {
                ExpectedReceiveDate = DateTime.UtcNow.AddDays(5),
                Notes = "Updated notes"
            };

            var updatedOrder = new PartOrderDetailDto
            {
                Id = orderId,
                OrderNumber = "ORD-001",
                ExpectedReceiveDate = updateOrderDto.ExpectedReceiveDate.Value,
                Notes = updateOrderDto.Notes,
                Status = OrderStatus.Pending.ToString()
            };

            _mockOrderService.Setup(s => s.OrderExistsAsync(
                It.Is<Guid>(id => id == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockOrderService.Setup(s => s.UpdateOrderAsync(
                It.Is<Guid>(id => id == orderId), 
                It.IsAny<UpdatePartOrderDto>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedOrder);

            // Act
            var result = await _controller.UpdateOrder(orderId, updateOrderDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedOrder = Assert.IsType<PartOrderDetailDto>(okResult.Value);
            Assert.Equal(orderId, returnedOrder.Id);
            Assert.Equal(updateOrderDto.ExpectedReceiveDate, returnedOrder.ExpectedReceiveDate);
            Assert.Equal(updateOrderDto.Notes, returnedOrder.Notes);
        }

        [Fact]
        public async Task DeleteOrder_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _mockOrderService.Setup(s => s.OrderExistsAsync(
                It.Is<Guid>(id => id == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockOrderService.Setup(s => s.DeleteOrderAsync(
                It.Is<Guid>(id => id == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteOrder(orderId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task SubmitOrder_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _mockOrderService.Setup(s => s.OrderExistsAsync(
                It.Is<Guid>(id => id == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockOrderService.Setup(s => s.SubmitOrderAsync(
                It.Is<Guid>(id => id == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SubmitOrder(orderId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ReceiveOrder_WithValidDto_ReturnsOkResult_WithReceipt()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var receiveDto = new PartOrderReceiveDto
            {
                OrderId = orderId,
                ReceivedLines = new List<ReceiveOrderLineDto>
                {
                    new ReceiveOrderLineDto
                    {
                        OrderLineId = Guid.NewGuid(),
                        ReceivedQuantity = 5
                    }
                }
            };

            var receipt = new PartOrderReceiptDto
            {
                OrderId = orderId,
                OrderNumber = "ORD-001",
                ReceiveDate = DateTime.UtcNow,
                Status = OrderStatus.PartiallyReceived.ToString(),
                ReceivedLines = new List<ReceiptLineDto>
                {
                    new ReceiptLineDto
                    {
                        OrderLineId = receiveDto.ReceivedLines[0].OrderLineId,
                        PartId = Guid.NewGuid(),
                        PartNumber = "P12345",
                        ReceivedQuantity = 5,
                        BackorderedQuantity = 0,
                        Status = "Received"
                    }
                }
            };

            _mockOrderService.Setup(s => s.OrderExistsAsync(
                It.Is<Guid>(id => id == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockOrderService.Setup(s => s.ReceiveOrderAsync(
                It.IsAny<PartOrderReceiveDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(receipt);

            // Act
            var result = await _controller.ReceiveOrder(receiveDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedReceipt = Assert.IsType<PartOrderReceiptDto>(okResult.Value);
            Assert.Equal(orderId, returnedReceipt.OrderId);
            Assert.Single(returnedReceipt.ReceivedLines);
        }
    }
}
