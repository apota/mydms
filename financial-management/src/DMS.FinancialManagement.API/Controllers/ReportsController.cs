using System;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.DTOs;
using DMS.FinancialManagement.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DMS.FinancialManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService _reportingService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportingService reportingService, ILogger<ReportsController> logger)
        {
            _reportingService = reportingService;
            _logger = logger;
        }
        
        /// <summary>
        /// Generates a journal entry balance report
        /// </summary>
        /// <param name="request">The report request parameters</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The journal entry balance report</returns>
        [HttpPost("journal-balance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<JournalEntryBalanceReportResponseDto>> GenerateJournalBalanceReport(
            JournalEntryBalanceReportRequestDto request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generating journal balance report from {StartDate} to {EndDate}", 
                request.StartDate, request.EndDate);
                
            try
            {
                var report = await _reportingService.GenerateJournalBalanceReportAsync(request, cancellationToken);
                return Ok(report);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when generating journal balance report");
                return BadRequest(ex.Message);
            }
        }
    }
}
