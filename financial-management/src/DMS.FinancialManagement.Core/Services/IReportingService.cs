using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.DTOs;

namespace DMS.FinancialManagement.Core.Services
{
    /// <summary>
    /// Service interface for financial reports
    /// </summary>
    public interface IReportingService
    {
        /// <summary>
        /// Generates a journal entry balance report
        /// </summary>
        /// <param name="request">The report request parameters</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The journal entry balance report</returns>
        Task<JournalEntryBalanceReportResponseDto> GenerateJournalBalanceReportAsync(
            JournalEntryBalanceReportRequestDto request,
            CancellationToken cancellationToken = default);
            
        // Additional report methods will be added here as needed
    }
}
