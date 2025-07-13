using DMS.FinancialManagement.Core.Models;
using DMS.FinancialManagement.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.FinancialManagement.API.Controllers
{
    [ApiController]
    [Route("api/finance")]
    [Authorize]
    public class AccountsPayableController : ControllerBase
    {
        private readonly IAccountsPayableService _accountsPayableService;
        private readonly ILogger<AccountsPayableController> _logger;

        public AccountsPayableController(
            IAccountsPayableService accountsPayableService,
            ILogger<AccountsPayableController> logger)
        {
            _accountsPayableService = accountsPayableService ?? throw new ArgumentNullException(nameof(accountsPayableService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("bills")]
        [SwaggerOperation(Summary = "Get all bills", Description = "Retrieves a list of bills with optional filtering")]
        [SwaggerResponse(200, "Bills retrieved successfully", typeof(IEnumerable<Bill>))]
        public async Task<ActionResult<IEnumerable<Bill>>> GetBills(
            [FromQuery] string status = null,
            [FromQuery] Guid? vendorId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var bills = await _accountsPayableService.GetBillsAsync(status, vendorId, fromDate, toDate);
                return Ok(bills);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bills");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving bills");
            }
        }

        [HttpGet("bills/{id:guid}")]
        [SwaggerOperation(Summary = "Get bill by ID", Description = "Retrieves a specific bill by its ID")]
        [SwaggerResponse(200, "Bill retrieved successfully", typeof(Bill))]
        [SwaggerResponse(404, "Bill not found")]
        public async Task<ActionResult<Bill>> GetBillById(Guid id)
        {
            try
            {
                var bill = await _accountsPayableService.GetBillByIdAsync(id);
                if (bill == null)
                {
                    return NotFound($"Bill with ID {id} not found");
                }
                return Ok(bill);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bill with ID: {BillId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving bill");
            }
        }

        [HttpPost("bills")]
        [SwaggerOperation(Summary = "Create bill", Description = "Creates a new bill")]
        [SwaggerResponse(201, "Bill created successfully", typeof(Bill))]
        [SwaggerResponse(400, "Invalid bill data")]
        public async Task<ActionResult<Bill>> CreateBill([FromBody] Bill bill)
        {
            try
            {
                if (bill == null)
                {
                    return BadRequest("Bill data is null");
                }

                var createdBill = await _accountsPayableService.CreateBillAsync(bill);
                return CreatedAtAction(nameof(GetBillById), new { id = createdBill.Id }, createdBill);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bill");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating bill");
            }
        }

        [HttpPut("bills/{id:guid}")]
        [SwaggerOperation(Summary = "Update bill", Description = "Updates an existing bill")]
        [SwaggerResponse(200, "Bill updated successfully", typeof(Bill))]
        [SwaggerResponse(404, "Bill not found")]
        [SwaggerResponse(400, "Invalid bill data")]
        public async Task<ActionResult<Bill>> UpdateBill(Guid id, [FromBody] Bill bill)
        {
            try
            {
                if (bill == null)
                {
                    return BadRequest("Bill data is null");
                }

                if (id != bill.Id)
                {
                    return BadRequest("Bill ID mismatch");
                }

                var updatedBill = await _accountsPayableService.UpdateBillAsync(bill);
                return Ok(updatedBill);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bill with ID: {BillId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating bill");
            }
        }

        [HttpPost("bills/{id:guid}/pay")]
        [SwaggerOperation(Summary = "Pay bill", Description = "Pays a bill")]
        [SwaggerResponse(200, "Bill paid successfully", typeof(Bill))]
        [SwaggerResponse(404, "Bill not found")]
        [SwaggerResponse(400, "Invalid payment data")]
        public async Task<ActionResult<Bill>> PayBill(Guid id, [FromBody] Payment payment)
        {
            try
            {
                if (payment == null)
                {
                    return BadRequest("Payment data is null");
                }

                var paidBill = await _accountsPayableService.PayBillAsync(id, payment);
                return Ok(paidBill);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error paying bill with ID: {BillId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error paying bill");
            }
        }

        [HttpGet("vendor-aging")]
        [SwaggerOperation(Summary = "Get vendor aging report", Description = "Generates a vendor aging report")]
        [SwaggerResponse(200, "Vendor aging report generated successfully", typeof(VendorAgingReport))]
        public async Task<ActionResult<VendorAgingReport>> GetVendorAgingReport([FromQuery] DateTime? asOfDate = null)
        {
            try
            {
                var agingReport = await _accountsPayableService.GetVendorAgingReportAsync(asOfDate);
                return Ok(agingReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating vendor aging report");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error generating vendor aging report");
            }
        }
        
        [HttpGet("billable-purchase-orders")]
        [SwaggerOperation(Summary = "Get billable purchase orders", Description = "Retrieves a list of purchase orders that can be billed")]
        [SwaggerResponse(200, "Billable purchase orders retrieved successfully", typeof(IEnumerable<PurchaseOrder>))]
        public async Task<ActionResult<IEnumerable<PurchaseOrder>>> GetBillablePurchaseOrders([FromQuery] Guid? vendorId = null)
        {
            try
            {
                var purchaseOrders = await _accountsPayableService.GetBillablePurchaseOrdersAsync(vendorId);
                return Ok(purchaseOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving billable purchase orders");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving billable purchase orders");
            }
        }
        
        [HttpPost("bills/from-purchase-order/{purchaseOrderId:guid}")]
        [SwaggerOperation(Summary = "Create bill from purchase order", Description = "Creates a new bill from a purchase order")]
        [SwaggerResponse(201, "Bill created successfully", typeof(Bill))]
        [SwaggerResponse(400, "Invalid purchase order ID")]
        [SwaggerResponse(404, "Purchase order not found")]
        public async Task<ActionResult<Bill>> CreateBillFromPurchaseOrder(Guid purchaseOrderId)
        {
            try
            {
                var createdBill = await _accountsPayableService.CreateBillFromPurchaseOrderAsync(purchaseOrderId);
                return CreatedAtAction(nameof(GetBillById), new { id = createdBill.Id }, createdBill);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bill from purchase order with ID: {PurchaseOrderId}", purchaseOrderId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating bill from purchase order");
            }
        }
    }
}
