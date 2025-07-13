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
    [Route("api/finance/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IAccountsReceivableService _accountsReceivableService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IAccountsReceivableService accountsReceivableService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _accountsReceivableService = accountsReceivableService ?? throw new ArgumentNullException(nameof(accountsReceivableService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all payments", Description = "Retrieves a list of payments with optional filtering")]
        [SwaggerResponse(200, "Payments retrieved successfully", typeof(IEnumerable<Payment>))]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments(
            [FromQuery] string status = null,
            [FromQuery] Guid? entityId = null,
            [FromQuery] string entityType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsAsync(status, entityId, entityType, fromDate, toDate);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving payments");
            }
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get payment by ID", Description = "Retrieves a specific payment by its ID")]
        [SwaggerResponse(200, "Payment retrieved successfully", typeof(Payment))]
        [SwaggerResponse(404, "Payment not found")]
        public async Task<ActionResult<Payment>> GetPaymentById(Guid id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound($"Payment with ID {id} not found");
                }
                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment with ID: {PaymentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving payment");
            }
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create payment", Description = "Creates a new standalone payment")]
        [SwaggerResponse(201, "Payment created successfully", typeof(Payment))]
        [SwaggerResponse(400, "Invalid payment data")]
        public async Task<ActionResult<Payment>> CreatePayment([FromBody] Payment payment)
        {
            try
            {
                if (payment == null)
                {
                    return BadRequest("Payment data is null");
                }

                var createdPayment = await _paymentService.CreatePaymentAsync(payment);
                return CreatedAtAction(nameof(GetPaymentById), new { id = createdPayment.Id }, createdPayment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating payment");
            }
        }

        [HttpPost("invoice-payment")]
        [SwaggerOperation(Summary = "Record invoice payment", Description = "Records a payment for one or more invoices")]
        [SwaggerResponse(201, "Payment recorded successfully", typeof(Payment))]
        [SwaggerResponse(400, "Invalid payment data")]
        public async Task<ActionResult<Payment>> RecordInvoicePayment(
            [FromBody] PaymentRequest paymentRequest)
        {
            try
            {
                if (paymentRequest == null || paymentRequest.Payment == null || paymentRequest.InvoiceIds == null || paymentRequest.InvoiceIds.Count == 0)
                {
                    return BadRequest("Invalid payment request");
                }

                var payment = await _accountsReceivableService.RecordPaymentAsync(paymentRequest.Payment, paymentRequest.InvoiceIds);
                return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, payment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording invoice payment");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error recording invoice payment");
            }
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Update payment", Description = "Updates an existing payment")]
        [SwaggerResponse(200, "Payment updated successfully", typeof(Payment))]
        [SwaggerResponse(404, "Payment not found")]
        [SwaggerResponse(400, "Invalid payment data")]
        public async Task<ActionResult<Payment>> UpdatePayment(Guid id, [FromBody] Payment payment)
        {
            try
            {
                if (payment == null)
                {
                    return BadRequest("Payment data is null");
                }

                if (id != payment.Id)
                {
                    return BadRequest("Payment ID mismatch");
                }

                var updatedPayment = await _paymentService.UpdatePaymentAsync(payment);
                return Ok(updatedPayment);
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
                _logger.LogError(ex, "Error updating payment with ID: {PaymentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating payment");
            }
        }

        [HttpPost("{id:guid}/void")]
        [SwaggerOperation(Summary = "Void payment", Description = "Voids an existing payment")]
        [SwaggerResponse(200, "Payment voided successfully")]
        [SwaggerResponse(404, "Payment not found")]
        public async Task<ActionResult> VoidPayment(Guid id, [FromBody] VoidRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest("Void reason is required");
                }

                var result = await _paymentService.VoidPaymentAsync(id, request.Reason);
                if (!result)
                {
                    return NotFound($"Payment with ID {id} not found");
                }

                return Ok(new { Message = "Payment voided successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voiding payment with ID: {PaymentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error voiding payment");
            }
        }

        [HttpGet("bank-reconciliation/{bankAccountId:guid}")]
        [SwaggerOperation(Summary = "Get bank reconciliation data", Description = "Retrieves bank reconciliation data for a specific account")]
        [SwaggerResponse(200, "Bank reconciliation data retrieved successfully", typeof(BankReconciliation))]
        [SwaggerResponse(404, "Bank account not found")]
        public async Task<ActionResult<BankReconciliation>> GetBankReconciliationData(
            Guid bankAccountId,
            [FromQuery] DateTime statementDate)
        {
            try
            {
                var reconciliation = await _paymentService.GetBankReconciliationDataAsync(bankAccountId, statementDate);
                return Ok(reconciliation);
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
                _logger.LogError(ex, "Error retrieving bank reconciliation data for account ID: {BankAccountId}", bankAccountId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving bank reconciliation data");
            }
        }

        [HttpPost("bank-reconciliation")]
        [SwaggerOperation(Summary = "Process bank reconciliation", Description = "Processes a bank reconciliation")]
        [SwaggerResponse(200, "Bank reconciliation processed successfully", typeof(BankReconciliation))]
        [SwaggerResponse(400, "Invalid reconciliation data")]
        public async Task<ActionResult<BankReconciliation>> ProcessBankReconciliation([FromBody] BankReconciliation reconciliation)
        {
            try
            {
                if (reconciliation == null)
                {
                    return BadRequest("Reconciliation data is null");
                }

                var processedReconciliation = await _paymentService.ProcessBankReconciliationAsync(reconciliation);
                return Ok(processedReconciliation);
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
                _logger.LogError(ex, "Error processing bank reconciliation");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing bank reconciliation");
            }
        }
    }

    public class PaymentRequest
    {
        public Payment Payment { get; set; }
        public List<Guid> InvoiceIds { get; set; }
    }

    public class VoidRequest
    {
        public string Reason { get; set; }
    }
}
