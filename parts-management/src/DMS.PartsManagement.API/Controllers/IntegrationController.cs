using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Services;

namespace DMS.PartsManagement.API.Controllers
{
    [ApiController]
    [Route("api/integration")]
    public class IntegrationController : ControllerBase
    {
        private readonly ILogger<IntegrationController> _logger;
        private readonly IIntegrationService _integrationService;

        public IntegrationController(
            ILogger<IntegrationController> logger,
            IIntegrationService integrationService)
        {
            _logger = logger;
            _integrationService = integrationService;
        }

        #region CRM Integration

        /// <summary>
        /// Get customer information from CRM
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Customer information</returns>
        [HttpGet("crm/customers/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetCustomerInfo(
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting customer information for customer ID: {CustomerId}", customerId);
            var customer = await _integrationService.GetCustomerInfoFromCrmAsync(customerId, cancellationToken);

            if (customer == null)
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found", customerId);
                return NotFound();
            }

            return Ok(customer);
        }

        #endregion

        #region Financial Integration

        /// <summary>
        /// Get pricing information for a part
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Pricing information</returns>
        [HttpGet("financial/pricing/{partId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartPricingDto>> GetPartPricing(
            Guid partId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting pricing information for part ID: {PartId}", partId);
            var pricing = await _integrationService.GetPartPricingFromFinancialAsync(partId, cancellationToken);

            if (pricing == null)
            {
                _logger.LogWarning("Pricing for part with ID {PartId} not found", partId);
                return NotFound();
            }

            return Ok(pricing);
        }

        /// <summary>
        /// Create invoice for parts
        /// </summary>
        /// <param name="createInvoiceDto">Invoice creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created invoice</returns>
        [HttpPost("financial/invoices")]
        [Authorize(Roles = "PartsManager,PartsSales,Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InvoiceDto>> CreateInvoiceForParts(
            CreateInvoiceDto createInvoiceDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating invoice for parts");

            try
            {
                var invoice = await _integrationService.CreateInvoiceInFinancialAsync(createInvoiceDto, cancellationToken);
                return CreatedAtAction(nameof(GetInvoice), new { invoiceId = invoice.Id }, invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for parts");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Invoice information</returns>
        [HttpGet("financial/invoices/{invoiceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(
            Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting invoice with ID: {InvoiceId}", invoiceId);
            var invoice = await _integrationService.GetInvoiceFromFinancialAsync(invoiceId, cancellationToken);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice with ID {InvoiceId} not found", invoiceId);
                return NotFound();
            }

            return Ok(invoice);
        }

        #endregion

        #region Service Integration

        /// <summary>
        /// Get parts assigned to a service order
        /// </summary>
        /// <param name="serviceOrderId">Service order ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of assigned parts</returns>
        [HttpGet("service/orders/{serviceOrderId}/parts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ServiceOrderPartDto>>> GetServiceOrderParts(
            Guid serviceOrderId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting parts for service order ID: {ServiceOrderId}", serviceOrderId);
            var parts = await _integrationService.GetPartsForServiceOrderAsync(serviceOrderId, cancellationToken);

            if (parts == null)
            {
                _logger.LogWarning("Service order with ID {ServiceOrderId} not found", serviceOrderId);
                return NotFound();
            }

            return Ok(parts);
        }

        /// <summary>
        /// Assign parts to a service order
        /// </summary>
        /// <param name="serviceOrderId">Service order ID</param>
        /// <param name="assignPartsDto">Parts assignment data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of assigned parts</returns>
        [HttpPost("service/orders/{serviceOrderId}/parts")]
        [Authorize(Roles = "PartsManager,PartsSales,ServiceAdvisor,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ServiceOrderPartDto>>> AssignPartsToServiceOrder(
            Guid serviceOrderId,
            AssignServiceOrderPartsDto assignPartsDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Assigning parts to service order ID: {ServiceOrderId}", serviceOrderId);

            try
            {
                var parts = await _integrationService.AssignPartsToServiceOrderAsync(serviceOrderId, assignPartsDto, cancellationToken);
                return Ok(parts);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Service order with ID {ServiceOrderId} not found", serviceOrderId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning parts to service order");
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Sales Integration

        /// <summary>
        /// Get accessories for a vehicle
        /// </summary>
        /// <param name="vehicleId">Vehicle ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of accessories</returns>
        [HttpGet("sales/vehicles/{vehicleId}/accessories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<VehicleAccessoryDto>>> GetVehicleAccessories(
            Guid vehicleId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting accessories for vehicle ID: {VehicleId}", vehicleId);
            var accessories = await _integrationService.GetAccessoriesForVehicleAsync(vehicleId, cancellationToken);

            if (accessories == null)
            {
                _logger.LogWarning("Vehicle with ID {VehicleId} not found", vehicleId);
                return NotFound();
            }

            return Ok(accessories);
        }

        #endregion
    }
}
