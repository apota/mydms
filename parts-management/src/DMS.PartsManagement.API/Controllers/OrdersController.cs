using DMS.PartsManagement.Core.Services;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DMS.PartsManagement.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all orders with pagination
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of orders</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartOrderSummaryDto>>> GetAllOrders(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all orders with skip: {Skip}, take: {Take}", skip, take);
            var orders = await _orderService.GetAllOrdersAsync(skip, take, cancellationToken);
            return Ok(orders);
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Order details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartOrderDetailDto>> GetOrderById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting order by ID: {Id}", id);
            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {Id} not found", id);
                return NotFound();
            }

            return Ok(order);
        }

        /// <summary>
        /// Get orders by status
        /// </summary>
        /// <param name="status">Order status</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of orders with the specified status</returns>
        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartOrderSummaryDto>>> GetOrdersByStatus(
            OrderStatus status,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting orders by status: {Status}", status);
            var orders = await _orderService.GetOrdersByStatusAsync(status, skip, take, cancellationToken);
            return Ok(orders);
        }

        /// <summary>
        /// Get orders by supplier
        /// </summary>
        /// <param name="supplierId">Supplier ID</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of orders from the specified supplier</returns>
        [HttpGet("supplier/{supplierId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartOrderSummaryDto>>> GetOrdersBySupplier(
            Guid supplierId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting orders by supplier ID: {SupplierId}", supplierId);
            var orders = await _orderService.GetOrdersBySupplierIdAsync(supplierId, skip, take, cancellationToken);
            return Ok(orders);
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        /// <param name="createOrderDto">Order creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created order</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, PartsSales, Admin")]
        public async Task<ActionResult<PartOrderDetailDto>> CreateOrder(
            CreatePartOrderDto createOrderDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating order for supplier ID: {SupplierId}", createOrderDto.SupplierId);
                
                var order = await _orderService.CreateOrderAsync(createOrderDto, cancellationToken);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing order
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="updateOrderDto">Order update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated order</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, PartsSales, Admin")]
        public async Task<ActionResult<PartOrderDetailDto>> UpdateOrder(
            Guid id,
            UpdatePartOrderDto updateOrderDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating order with ID: {Id}", id);
                
                if (!await _orderService.OrderExistsAsync(id, cancellationToken))
                {
                    _logger.LogWarning("Order with ID {Id} not found", id);
                    return NotFound();
                }
                
                var order = await _orderService.UpdateOrderAsync(id, updateOrderDto, cancellationToken);
                
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {Id} could not be updated", id);
                    return NotFound();
                }
                
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order with ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete an order
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<IActionResult> DeleteOrder(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting order with ID: {Id}", id);
                
                if (!await _orderService.OrderExistsAsync(id, cancellationToken))
                {
                    _logger.LogWarning("Order with ID {Id} not found", id);
                    return NotFound();
                }
                
                var result = await _orderService.DeleteOrderAsync(id, cancellationToken);
                
                if (!result)
                {
                    _logger.LogWarning("Order with ID {Id} could not be deleted", id);
                    return BadRequest("Order could not be deleted. It may have related records or be in a state that cannot be deleted.");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order with ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Submit an order to supplier
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>OK if successful</returns>
        [HttpPost("{id}/submit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<IActionResult> SubmitOrder(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Submitting order with ID: {Id}", id);
                
                if (!await _orderService.OrderExistsAsync(id, cancellationToken))
                {
                    _logger.LogWarning("Order with ID {Id} not found", id);
                    return NotFound();
                }
                
                var result = await _orderService.SubmitOrderAsync(id, cancellationToken);
                
                if (!result)
                {
                    _logger.LogWarning("Order with ID {Id} could not be submitted", id);
                    return BadRequest("Order could not be submitted. It may already be submitted or have invalid data.");
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting order with ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Receive order items
        /// </summary>
        /// <param name="receiveDto">Order receive data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Receipt details</returns>
        [HttpPost("receive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, PartsSales, Admin")]
        public async Task<ActionResult<PartOrderReceiptDto>> ReceiveOrder(
            PartOrderReceiveDto receiveDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Receiving order with ID: {OrderId}", receiveDto.OrderId);
                
                if (!await _orderService.OrderExistsAsync(receiveDto.OrderId, cancellationToken))
                {
                    _logger.LogWarning("Order with ID {Id} not found", receiveDto.OrderId);
                    return NotFound();
                }
                
                var receipt = await _orderService.ReceiveOrderAsync(receiveDto, cancellationToken);
                return Ok(receipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving order with ID: {OrderId}", receiveDto.OrderId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get order lines by order ID
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of order lines</returns>
        [HttpGet("{orderId}/lines")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PartOrderLineDto>>> GetOrderLines(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting order lines for order ID: {OrderId}", orderId);
            
            if (!await _orderService.OrderExistsAsync(orderId, cancellationToken))
            {
                _logger.LogWarning("Order with ID {Id} not found", orderId);
                return NotFound();
            }
            
            var orderLines = await _orderService.GetOrderLinesAsync(orderId, cancellationToken);
            return Ok(orderLines);
        }

        /// <summary>
        /// Generate reorder recommendations
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of recommended parts to reorder</returns>
        [HttpGet("recommend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<ActionResult<IEnumerable<ReorderRecommendationDto>>> GetReorderRecommendations(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generating reorder recommendations");
            var recommendations = await _orderService.GenerateReorderRecommendationsAsync(cancellationToken);
            return Ok(recommendations);
        }

        /// <summary>
        /// Get special orders
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of special orders</returns>
        [HttpGet("special")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartOrderSummaryDto>>> GetSpecialOrders(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting special orders with skip: {Skip}, take: {Take}", skip, take);
            var orders = await _orderService.GetSpecialOrdersAsync(skip, take, cancellationToken);
            return Ok(orders);
        }
    }
}
