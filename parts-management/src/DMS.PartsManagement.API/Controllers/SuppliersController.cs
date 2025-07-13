using DMS.PartsManagement.Core.Services;
using DMS.PartsManagement.Core.DTOs;
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
    [Route("api/suppliers")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(ISupplierService supplierService, ILogger<SuppliersController> logger)
        {
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all suppliers with pagination
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of suppliers</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SupplierSummaryDto>>> GetAllSuppliers(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all suppliers with skip: {Skip}, take: {Take}", skip, take);
            var suppliers = await _supplierService.GetAllSuppliersAsync(skip, take, cancellationToken);
            return Ok(suppliers);
        }

        /// <summary>
        /// Get supplier by ID
        /// </summary>
        /// <param name="id">Supplier ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Supplier details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SupplierDetailDto>> GetSupplierById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting supplier by ID: {Id}", id);
            var supplier = await _supplierService.GetSupplierByIdAsync(id, cancellationToken);

            if (supplier == null)
            {
                _logger.LogWarning("Supplier with ID {Id} not found", id);
                return NotFound();
            }

            return Ok(supplier);
        }

        /// <summary>
        /// Create a new supplier
        /// </summary>
        /// <param name="createSupplierDto">Supplier creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created supplier</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<ActionResult<SupplierDetailDto>> CreateSupplier(
            CreateSupplierDto createSupplierDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating supplier with name: {Name}", createSupplierDto.Name);
                
                var supplier = await _supplierService.CreateSupplierAsync(createSupplierDto, cancellationToken);
                return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.Id }, supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing supplier
        /// </summary>
        /// <param name="id">Supplier ID</param>
        /// <param name="updateSupplierDto">Supplier update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated supplier</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<ActionResult<SupplierDetailDto>> UpdateSupplier(
            Guid id,
            UpdateSupplierDto updateSupplierDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating supplier with ID: {Id}", id);
                
                if (!await _supplierService.SupplierExistsAsync(id, cancellationToken))
                {
                    _logger.LogWarning("Supplier with ID {Id} not found", id);
                    return NotFound();
                }
                
                var supplier = await _supplierService.UpdateSupplierAsync(id, updateSupplierDto, cancellationToken);
                
                if (supplier == null)
                {
                    _logger.LogWarning("Supplier with ID {Id} could not be updated", id);
                    return NotFound();
                }
                
                return Ok(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier with ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a supplier
        /// </summary>
        /// <param name="id">Supplier ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<IActionResult> DeleteSupplier(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting supplier with ID: {Id}", id);
                
                if (!await _supplierService.SupplierExistsAsync(id, cancellationToken))
                {
                    _logger.LogWarning("Supplier with ID {Id} not found", id);
                    return NotFound();
                }
                
                var result = await _supplierService.DeleteSupplierAsync(id, cancellationToken);
                
                if (!result)
                {
                    _logger.LogWarning("Supplier with ID {Id} could not be deleted", id);
                    return BadRequest("Supplier could not be deleted. It may have related records.");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier with ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get parts supplied by a supplier
        /// </summary>
        /// <param name="supplierId">Supplier ID</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of parts</returns>
        [HttpGet("{supplierId}/parts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PartSummaryDto>>> GetPartsBySupplier(
            Guid supplierId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting parts by supplier ID: {SupplierId}", supplierId);

            if (!await _supplierService.SupplierExistsAsync(supplierId, cancellationToken))
            {
                _logger.LogWarning("Supplier with ID {Id} not found", supplierId);
                return NotFound();
            }

            var parts = await _supplierService.GetPartsBySupplierId(supplierId, skip, take, cancellationToken);
            return Ok(parts);
        }
    }
}
