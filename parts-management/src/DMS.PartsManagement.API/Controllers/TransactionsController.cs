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
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all transactions with pagination and optional date filtering
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of transactions</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartTransactionDto>>> GetAllTransactions(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all transactions with skip: {Skip}, take: {Take}, startDate: {StartDate}, endDate: {EndDate}", 
                skip, take, startDate, endDate);
            var transactions = await _transactionService.GetAllTransactionsAsync(skip, take, startDate, endDate, cancellationToken);
            return Ok(transactions);
        }

        /// <summary>
        /// Get transaction by ID
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transaction details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartTransactionDto>> GetTransactionById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting transaction by ID: {Id}", id);
            var transaction = await _transactionService.GetTransactionByIdAsync(id, cancellationToken);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction with ID {Id} not found", id);
                return NotFound();
            }

            return Ok(transaction);
        }

        /// <summary>
        /// Get transactions by part ID
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of transactions for the specified part</returns>
        [HttpGet("part/{partId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartTransactionDto>>> GetTransactionsByPartId(
            Guid partId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting transactions for part ID: {PartId}", partId);
            var transactions = await _transactionService.GetTransactionsByPartIdAsync(partId, skip, take, cancellationToken);
            return Ok(transactions);
        }

        /// <summary>
        /// Get transactions by type
        /// </summary>
        /// <param name="type">Transaction type</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of transactions of the specified type</returns>
        [HttpGet("type/{type}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartTransactionDto>>> GetTransactionsByType(
            TransactionType type,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting transactions of type: {Type}", type);
            var transactions = await _transactionService.GetTransactionsByTypeAsync(type, skip, take, cancellationToken);
            return Ok(transactions);
        }

        /// <summary>
        /// Issue parts
        /// </summary>
        /// <param name="issuePartsDto">Parts issue data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transaction details</returns>
        [HttpPost("issue")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, PartsSales, ServiceAdvisor, Admin")]
        public async Task<ActionResult<PartTransactionDto>> IssueParts(
            IssuePartsDto issuePartsDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Issuing {Quantity} of part ID: {PartId}", issuePartsDto.Quantity, issuePartsDto.PartId);
                var transaction = await _transactionService.IssuePartsAsync(issuePartsDto, cancellationToken);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing parts");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Return parts
        /// </summary>
        /// <param name="returnPartsDto">Parts return data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transaction details</returns>
        [HttpPost("return")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, PartsSales, ServiceAdvisor, Admin")]
        public async Task<ActionResult<PartTransactionDto>> ReturnParts(
            ReturnPartsDto returnPartsDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Returning {Quantity} of part ID: {PartId}", returnPartsDto.Quantity, returnPartsDto.PartId);
                var transaction = await _transactionService.ReturnPartsAsync(returnPartsDto, cancellationToken);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning parts");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adjust inventory
        /// </summary>
        /// <param name="adjustInventoryDto">Inventory adjustment data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transaction details</returns>
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
                _logger.LogInformation("Adjusting inventory for part ID: {PartId} by {Quantity}", 
                    adjustInventoryDto.PartId, adjustInventoryDto.QuantityAdjustment);
                var transaction = await _transactionService.AdjustInventoryAsync(adjustInventoryDto, cancellationToken);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting inventory");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Transfer parts between locations
        /// </summary>
        /// <param name="transferPartsDto">Parts transfer data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transaction details</returns>
        [HttpPost("transfer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<ActionResult<PartTransactionDto>> TransferParts(
            TransferPartsDto transferPartsDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Transferring {Quantity} of part ID: {PartId} from location {SourceLocation} to {DestinationLocation}", 
                    transferPartsDto.Quantity, transferPartsDto.PartId, transferPartsDto.SourceLocationId, transferPartsDto.DestinationLocationId);
                var transaction = await _transactionService.TransferPartsAsync(transferPartsDto, cancellationToken);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring parts");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get transaction history for a part within date range
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of transaction summaries</returns>
        [HttpGet("history/{partId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartTransactionSummaryDto>>> GetTransactionHistory(
            Guid partId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting transaction history for part ID: {PartId} from {StartDate} to {EndDate}", 
                partId, startDate, endDate);
            var history = await _transactionService.GetTransactionHistoryAsync(partId, startDate, endDate, cancellationToken);
            return Ok(history);
        }
    }
}
