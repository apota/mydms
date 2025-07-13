using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DMS.ServiceManagement.API.Controllers
{
    [ApiController]
    [Route("api/service/repair-orders")]
    public class RepairOrdersController : ControllerBase
    {
        private readonly IRepairOrderService _repairOrderService;
        private readonly ILogger<RepairOrdersController> _logger;

        public RepairOrdersController(IRepairOrderService repairOrderService, ILogger<RepairOrdersController> logger)
        {
            _repairOrderService = repairOrderService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RepairOrder>>> GetAllRepairOrders()
        {
            try
            {
                var repairOrders = await _repairOrderService.GetAllRepairOrdersAsync();
                return Ok(repairOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all repair orders");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving repair orders");
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RepairOrder>> GetRepairOrderById(Guid id)
        {
            try
            {
                var repairOrder = await _repairOrderService.GetRepairOrderByIdAsync(id);
                if (repairOrder == null)
                {
                    return NotFound($"Repair Order with ID {id} not found");
                }
                return Ok(repairOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving repair order with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving repair order");
            }
        }

        [HttpGet("number/{number}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RepairOrder>> GetRepairOrderByNumber(string number)
        {
            try
            {
                var repairOrder = await _repairOrderService.GetRepairOrderByNumberAsync(number);
                if (repairOrder == null)
                {
                    return NotFound($"Repair Order with number {number} not found");
                }
                return Ok(repairOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving repair order with number: {number}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving repair order");
            }
        }

        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RepairOrder>>> GetRepairOrdersByCustomerId(Guid customerId)
        {
            try
            {
                var repairOrders = await _repairOrderService.GetRepairOrdersByCustomerIdAsync(customerId);
                return Ok(repairOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving repair orders for customer ID: {customerId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving repair orders");
            }
        }

        [HttpGet("vehicle/{vehicleId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RepairOrder>>> GetRepairOrdersByVehicleId(Guid vehicleId)
        {
            try
            {
                var repairOrders = await _repairOrderService.GetRepairOrdersByVehicleIdAsync(vehicleId);
                return Ok(repairOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving repair orders for vehicle ID: {vehicleId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving repair orders");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RepairOrder>> CreateRepairOrder([FromBody] RepairOrder repairOrder)
        {
            try
            {
                if (repairOrder == null)
                {
                    return BadRequest("Repair order data is null");
                }

                var createdRepairOrder = await _repairOrderService.CreateRepairOrderAsync(repairOrder);
                return CreatedAtAction(nameof(GetRepairOrderById), new { id = createdRepairOrder.Id }, createdRepairOrder);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid repair order data");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating repair order");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating repair order");
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RepairOrder>> UpdateRepairOrder(Guid id, [FromBody] RepairOrder repairOrder)
        {
            try
            {
                if (repairOrder == null)
                {
                    return BadRequest("Repair order data is null");
                }

                if (id != repairOrder.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var existingRepairOrder = await _repairOrderService.GetRepairOrderByIdAsync(id);
                if (existingRepairOrder == null)
                {
                    return NotFound($"Repair Order with ID {id} not found");
                }

                var updatedRepairOrder = await _repairOrderService.UpdateRepairOrderAsync(repairOrder);
                return Ok(updatedRepairOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Invalid repair order data for ID: {id}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating repair order with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating repair order");
            }
        }

        [HttpPost("{id:guid}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RepairOrder>> UpdateRepairOrderStatus(Guid id, [FromBody] RepairOrderStatusUpdateRequest request)
        {
            try
            {
                if (request == null || !Enum.IsDefined(typeof(RepairOrderStatus), request.Status))
                {
                    return BadRequest("Invalid status");
                }

                var updatedRepairOrder = await _repairOrderService.UpdateRepairOrderStatusAsync(id, request.Status);
                if (updatedRepairOrder == null)
                {
                    return NotFound($"Repair Order with ID {id} not found");
                }

                return Ok(updatedRepairOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for repair order with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating repair order status");
            }
        }

        [HttpPost("{id:guid}/close")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RepairOrder>> CloseRepairOrder(Guid id)
        {
            try
            {
                var closedRepairOrder = await _repairOrderService.CloseRepairOrderAsync(id);
                if (closedRepairOrder == null)
                {
                    return NotFound($"Repair Order with ID {id} not found");
                }

                return Ok(closedRepairOrder);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error closing repair order with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error closing repair order");
            }
        }
    }

    public class RepairOrderStatusUpdateRequest
    {
        public RepairOrderStatus Status { get; set; }
    }
}
