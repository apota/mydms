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
    public class AccountsReceivableController : ControllerBase
    {
        private readonly IAccountsReceivableService _accountsReceivableService;
        private readonly ILogger<AccountsReceivableController> _logger;

        public AccountsReceivableController(
            IAccountsReceivableService accountsReceivableService,
            ILogger<AccountsReceivableController> logger)
        {
            _accountsReceivableService = accountsReceivableService ?? throw new ArgumentNullException(nameof(accountsReceivableService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("invoices")]
        [SwaggerOperation(Summary = "Get all invoices", Description = "Retrieves a list of invoices with optional filtering")]
        [SwaggerResponse(200, "Invoices retrieved successfully", typeof(IEnumerable<Invoice>))]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices(
            [FromQuery] string status = null,
            [FromQuery] Guid? customerId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var invoices = await _accountsReceivableService.GetInvoicesAsync(status, customerId, fromDate, toDate);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving invoices");
            }
        }

        [HttpGet("invoices/{id:guid}")]
        [SwaggerOperation(Summary = "Get invoice by ID", Description = "Retrieves a specific invoice by its ID")]
        [SwaggerResponse(200, "Invoice retrieved successfully", typeof(Invoice))]
        [SwaggerResponse(404, "Invoice not found")]
        public async Task<ActionResult<Invoice>> GetInvoiceById(Guid id)
        {
            try
            {
                var invoice = await _accountsReceivableService.GetInvoiceByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound($"Invoice with ID {id} not found");
                }
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice with ID: {InvoiceId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving invoice");
            }
        }

        [HttpPost("invoices")]
        [SwaggerOperation(Summary = "Create invoice", Description = "Creates a new invoice")]
        [SwaggerResponse(201, "Invoice created successfully", typeof(Invoice))]
        [SwaggerResponse(400, "Invalid invoice data")]
        public async Task<ActionResult<Invoice>> CreateInvoice([FromBody] Invoice invoice)
        {
            try
            {
                if (invoice == null)
                {
                    return BadRequest("Invoice data is null");
                }

                var createdInvoice = await _accountsReceivableService.CreateInvoiceAsync(invoice);
                return CreatedAtAction(nameof(GetInvoiceById), new { id = createdInvoice.Id }, createdInvoice);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating invoice");
            }
        }

        [HttpPut("invoices/{id:guid}")]
        [SwaggerOperation(Summary = "Update invoice", Description = "Updates an existing invoice")]
        [SwaggerResponse(200, "Invoice updated successfully", typeof(Invoice))]
        [SwaggerResponse(404, "Invoice not found")]
        [SwaggerResponse(400, "Invalid invoice data")]
        public async Task<ActionResult<Invoice>> UpdateInvoice(Guid id, [FromBody] Invoice invoice)
        {
            try
            {
                if (invoice == null)
                {
                    return BadRequest("Invoice data is null");
                }

                if (id != invoice.Id)
                {
                    return BadRequest("Invoice ID mismatch");
                }

                var updatedInvoice = await _accountsReceivableService.UpdateInvoiceAsync(invoice);
                return Ok(updatedInvoice);
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
                _logger.LogError(ex, "Error updating invoice with ID: {InvoiceId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating invoice");
            }
        }

        [HttpPost("invoices/{id:guid}/send")]
        [SwaggerOperation(Summary = "Send invoice", Description = "Sends an invoice to the customer")]
        [SwaggerResponse(200, "Invoice sent successfully")]
        [SwaggerResponse(404, "Invoice not found")]
        public async Task<ActionResult> SendInvoice(Guid id)
        {
            try
            {
                var result = await _accountsReceivableService.SendInvoiceAsync(id);
                if (!result)
                {
                    return NotFound($"Invoice with ID {id} not found");
                }

                return Ok(new { Message = "Invoice sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invoice with ID: {InvoiceId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error sending invoice");
            }
        }

        [HttpGet("customer-statements")]
        [SwaggerOperation(Summary = "Generate customer statements", Description = "Generates customer statements")]
        [SwaggerResponse(200, "Customer statements generated successfully", typeof(IEnumerable<CustomerStatement>))]
        public async Task<ActionResult<IEnumerable<CustomerStatement>>> GenerateCustomerStatements(
            [FromQuery] Guid? customerId = null,
            [FromQuery] DateTime? asOfDate = null)
        {
            try
            {
                var statements = await _accountsReceivableService.GenerateCustomerStatementsAsync(customerId, asOfDate);
                return Ok(statements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer statements");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error generating customer statements");
            }
        }

        [HttpGet("aging")]
        [SwaggerOperation(Summary = "Get aging report", Description = "Generates an aging report for accounts receivable")]
        [SwaggerResponse(200, "Aging report generated successfully", typeof(ARAgingReport))]
        public async Task<ActionResult<ARAgingReport>> GetAgingReport([FromQuery] DateTime? asOfDate = null)
        {
            try
            {
                var agingReport = await _accountsReceivableService.GetAgingReportAsync(asOfDate);
                return Ok(agingReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating aging report");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error generating aging report");
            }
        }

        [HttpPost("credit-memos")]
        [SwaggerOperation(Summary = "Create credit memo", Description = "Creates a new credit memo")]
        [SwaggerResponse(201, "Credit memo created successfully", typeof(CreditMemo))]
        [SwaggerResponse(400, "Invalid credit memo data")]
        public async Task<ActionResult<CreditMemo>> CreateCreditMemo([FromBody] CreditMemo creditMemo)
        {
            try
            {
                if (creditMemo == null)
                {
                    return BadRequest("Credit memo data is null");
                }

                var createdCreditMemo = await _accountsReceivableService.CreateCreditMemoAsync(creditMemo);
                return CreatedAtAction(nameof(GetInvoiceById), new { id = createdCreditMemo.Id }, createdCreditMemo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating credit memo");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating credit memo");
            }
        }

        [HttpPost("credit-memos/{id:guid}/apply")]
        [SwaggerOperation(Summary = "Apply credit memo", Description = "Applies a credit memo to invoices")]
        [SwaggerResponse(200, "Credit memo applied successfully")]
        [SwaggerResponse(404, "Credit memo not found")]
        [SwaggerResponse(400, "Invalid invoice IDs")]
        public async Task<ActionResult> ApplyCreditMemo(Guid id, [FromBody] List<Guid> invoiceIds)
        {
            try
            {
                if (invoiceIds == null || invoiceIds.Count == 0)
                {
                    return BadRequest("No invoice IDs provided");
                }

                var result = await _accountsReceivableService.ApplyCreditMemoAsync(id, invoiceIds);
                return Ok(new { Message = "Credit memo applied successfully" });
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
                _logger.LogError(ex, "Error applying credit memo with ID: {CreditMemoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error applying credit memo");
            }
        }
    }
}
