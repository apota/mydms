using DMS.PartsManagement.Core.Services;
using DMS.PartsManagement.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace DMS.PartsManagement.API.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            IInventoryService inventoryService,
            ITransactionService transactionService,
            ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all inventory
        /// </summary>
        /// <returns>List of inventory items</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartInventoryDto>>> GetAllInventory()
        {
            _logger.LogInformation("Getting all inventory");
            var inventory = await _inventoryService.GetAllInventoryAsync();
            return Ok(inventory);
        }

        /// <summary>
        /// Get inventory by part ID
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <returns>List of inventory items for the specified part</returns>
        [HttpGet("part/{partId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartInventoryDto>>> GetInventoryByPartId(int partId)
        {
            _logger.LogInformation("Getting inventory for part ID: {PartId}", partId);
            var inventory = await _inventoryService.GetInventoryByPartIdAsync(partId);
            return Ok(inventory);
        }

        /// <summary>
        /// Get inventory by location ID
        /// </summary>
        /// <param name="locationId">Location ID</param>
        /// <returns>List of inventory items for the specified location</returns>
        [HttpGet("location/{locationId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartInventoryDto>>> GetInventoryByLocationId(int locationId)
        {
            _logger.LogInformation("Getting inventory for location ID: {LocationId}", locationId);
            var inventory = await _inventoryService.GetInventoryByLocationIdAsync(locationId);
            return Ok(inventory);
        }

        /// <summary>
        /// Get inventory by part and location
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="locationId">Location ID</param>
        /// <returns>Inventory information for the specified part and location</returns>
        [HttpGet("{partId:int}/location/{locationId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartInventoryDto>> GetInventoryByPartAndLocation(int partId, int locationId)
        {
            _logger.LogInformation("Getting inventory for part ID: {PartId} at location ID: {LocationId}", partId, locationId);
            var inventory = await _inventoryService.GetInventoryByPartAndLocationAsync(partId, locationId);

            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found for part ID: {PartId} at location ID: {LocationId}", partId, locationId);
                return NotFound();
            }

            return Ok(inventory);
        }

        /// <summary>
        /// Get low stock inventory
        /// </summary>
        /// <param name="threshold">Low stock threshold (default: 5)</param>
        /// <returns>List of inventory items with quantity below the threshold</returns>
        [HttpGet("low-stock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartInventoryDto>>> GetLowStockInventory([FromQuery] int threshold = 5)
        {
            _logger.LogInformation("Getting low stock inventory with threshold: {Threshold}", threshold);
            var inventory = await _inventoryService.GetLowStockInventoryAsync(threshold);
            return Ok(inventory);
        }

        /// <summary>
        /// Create inventory record
        /// </summary>
        /// <param name="createInventoryDto">Inventory creation data</param>
        /// <returns>Created inventory ID</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CreateInventory(CreateInventoryDto createInventoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating inventory for part ID: {PartId} at location ID: {LocationId}",
                createInventoryDto.PartId, createInventoryDto.LocationId);

            try
            {
                var inventoryId = await _inventoryService.CreateInventoryAsync(createInventoryDto);
                return CreatedAtAction(nameof(GetInventoryByPartAndLocation), 
                    new { partId = createInventoryDto.PartId, locationId = createInventoryDto.LocationId }, 
                    inventoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory for part ID: {PartId}", createInventoryDto.PartId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update inventory record
        /// </summary>
        /// <param name="id">Inventory ID</param>
        /// <param name="updateInventoryDto">Inventory update data</param>
        /// <returns>No content</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateInventory(int id, UpdateInventoryDto updateInventoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Updating inventory with ID: {Id}", id);

            try
            {
                await _inventoryService.UpdateInventoryAsync(id, updateInventoryDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Inventory with ID {Id} not found during update", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory with ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update inventory quantity
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="locationId">Location ID</param>
        /// <param name="quantityChange">Quantity change (positive for addition, negative for subtraction)</param>
        /// <param name="notes">Notes about the quantity change</param>
        /// <returns>No content</returns>
        [HttpPatch("{partId:int}/location/{locationId:int}/quantity")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateQuantity(
            int partId, 
            int locationId,
            [FromQuery] int quantityChange,
            [FromQuery] string notes)
        {
            _logger.LogInformation("Updating quantity for part ID: {PartId} at location ID: {LocationId} by {QuantityChange}",
                partId, locationId, quantityChange);

            try
            {
                await _inventoryService.UpdateQuantityAsync(partId, locationId, quantityChange, notes);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity for part ID: {PartId} at location ID: {LocationId}", partId, locationId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get transaction history for a part
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <returns>List of transactions for the specified part</returns>
        [HttpGet("transactions/part/{partId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartTransactionDto>>> GetTransactionHistory(int partId)
        {
            _logger.LogInformation("Getting transaction history for part ID: {PartId}", partId);
            var transactions = await _inventoryService.GetTransactionHistoryAsync(partId);
            return Ok(transactions);
        }

        /// <summary>
        /// Record part receipt
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="locationId">Location ID</param>
        /// <param name="quantity">Quantity received</param>
        /// <param name="poNumber">Purchase order number</param>
        /// <param name="costPerUnit">Cost per unit</param>
        /// <returns>No content</returns>
        [HttpPost("transactions/receipt")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RecordPartReceipt(
            [FromQuery] int partId,
            [FromQuery] int locationId,
            [FromQuery] int quantity,
            [FromQuery] string poNumber,
            [FromQuery] decimal costPerUnit)
        {
            _logger.LogInformation("Recording part receipt for part ID: {PartId}, quantity: {Quantity}, PO: {PONumber}",
                partId, quantity, poNumber);

            try
            {
                await _inventoryService.CreatePartReceiptAsync(partId, locationId, quantity, poNumber, costPerUnit);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording part receipt for part ID: {PartId}", partId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Record part sale
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="locationId">Location ID</param>
        /// <param name="quantity">Quantity sold</param>
        /// <param name="orderNumber">Order number</param>
        /// <param name="customerInfo">Customer information</param>
        /// <returns>No content</returns>
        [HttpPost("transactions/sale")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RecordPartSale(
            [FromQuery] int partId,
            [FromQuery] int locationId,
            [FromQuery] int quantity,
            [FromQuery] string orderNumber,
            [FromQuery] string customerInfo)
        {
            _logger.LogInformation("Recording part sale for part ID: {PartId}, quantity: {Quantity}, order: {OrderNumber}",
                partId, quantity, orderNumber);

            try
            {
                await _inventoryService.CreatePartSaleAsync(partId, locationId, quantity, orderNumber, customerInfo);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Insufficient inventory for part ID: {PartId} at location ID: {LocationId}", partId, locationId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording part sale for part ID: {PartId}", partId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Record part transfer between locations
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="sourceLocationId">Source location ID</param>
        /// <param name="destinationLocationId">Destination location ID</param>
        /// <param name="quantity">Quantity transferred</param>
        /// <param name="notes">Notes about the transfer</param>
        /// <returns>No content</returns>
        [HttpPost("transactions/transfer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RecordPartTransfer(
            [FromQuery] int partId,
            [FromQuery] int sourceLocationId,
            [FromQuery] int destinationLocationId,
            [FromQuery] int quantity,
            [FromQuery] string notes)
        {
            _logger.LogInformation("Recording part transfer for part ID: {PartId}, from location: {SourceLocation} to location: {DestinationLocation}, quantity: {Quantity}",
                partId, sourceLocationId, destinationLocationId, quantity);

            try
            {
                await _inventoryService.CreatePartTransferAsync(partId, sourceLocationId, destinationLocationId, quantity, notes);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Insufficient inventory for transfer of part ID: {PartId} from location ID: {LocationId}", partId, sourceLocationId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording part transfer for part ID: {PartId}", partId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Record part return
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="locationId">Location ID</param>
        /// <param name="quantity">Quantity returned</param>
        /// <param name="returnReason">Reason for return</param>
        /// <param name="customerInfo">Customer information</param>
        /// <returns>No content</returns>
        [HttpPost("transactions/return")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RecordPartReturn(
            [FromQuery] int partId,
            [FromQuery] int locationId,
            [FromQuery] int quantity,
            [FromQuery] string returnReason,
            [FromQuery] string customerInfo)
        {
            _logger.LogInformation("Recording part return for part ID: {PartId}, quantity: {Quantity}, reason: {Reason}",
                partId, quantity, returnReason);

            try
            {
                await _inventoryService.CreatePartReturnAsync(partId, locationId, quantity, returnReason, customerInfo);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording part return for part ID: {PartId}", partId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adjust inventory with detailed tracking
        /// </summary>
        /// <param name="adjustInventoryDto">Details about the inventory adjustment</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transaction details of the adjustment</returns>
        [HttpPost("adjust")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<ActionResult<PartTransactionDto>> AdjustInventory(
            AdjustInventoryDto adjustInventoryDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Adjusting inventory for part ID: {PartId}, location: {LocationId}, quantity change: {Quantity}",
                    adjustInventoryDto.PartId, adjustInventoryDto.LocationId, adjustInventoryDto.QuantityAdjustment);

                var transaction = await _transactionService.AdjustInventoryAsync(adjustInventoryDto, cancellationToken);
                return Ok(transaction);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Part not found during inventory adjustment: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation during inventory adjustment: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting inventory for part ID: {PartId}", adjustInventoryDto.PartId);
                return BadRequest(ex.Message);
            }
        }
    }
}
