using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.DTOs;

namespace DMS.FinancialManagement.Core.Services
{
    /// <summary>
    /// Service interface for managing journal entries
    /// </summary>
    public interface IJournalEntryService
    {
        /// <summary>
        /// Gets all journal entries
        /// </summary>
        /// <param name="skip">The number of entries to skip</param>
        /// <param name="take">The number of entries to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of journal entries</returns>
        Task<IEnumerable<JournalEntrySummaryDto>> GetAllEntriesAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a journal entry by ID
        /// </summary>
        /// <param name="id">The journal entry ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The journal entry with the specified ID</returns>
        Task<JournalEntryDto?> GetEntryByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets journal entries by date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="skip">The number of entries to skip</param>
        /// <param name="take">The number of entries to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of journal entries in the specified date range</returns>
        Task<IEnumerable<JournalEntrySummaryDto>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets journal entries by financial period ID
        /// </summary>
        /// <param name="financialPeriodId">The financial period ID</param>
        /// <param name="skip">The number of entries to skip</param>
        /// <param name="take">The number of entries to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of journal entries in the specified financial period</returns>
        Task<IEnumerable<JournalEntrySummaryDto>> GetEntriesByFinancialPeriodAsync(Guid financialPeriodId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new journal entry
        /// </summary>
        /// <param name="entryDto">The journal entry data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created journal entry</returns>
        Task<JournalEntryDto> CreateEntryAsync(JournalEntryCreateDto entryDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing journal entry
        /// </summary>
        /// <param name="id">The journal entry ID</param>
        /// <param name="entryDto">The updated journal entry data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated journal entry</returns>
        Task<JournalEntryDto?> UpdateEntryAsync(Guid id, JournalEntryUpdateDto entryDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes a journal entry
        /// </summary>
        /// <param name="id">The journal entry ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>True if the journal entry was deleted, false otherwise</returns>
        Task<bool> DeleteEntryAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Posts a journal entry
        /// </summary>
        /// <param name="id">The journal entry ID</param>
        /// <param name="postDto">The posting data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The posted journal entry</returns>
        Task<JournalEntryDto?> PostEntryAsync(Guid id, JournalEntryPostDto postDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Reverses a journal entry
        /// </summary>
        /// <param name="id">The journal entry ID to reverse</param>
        /// <param name="reverseDto">The reversal data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The reversal journal entry</returns>
        Task<JournalEntryDto?> ReverseEntryAsync(Guid id, JournalEntryReverseDto reverseDto, CancellationToken cancellationToken = default);
    }
}
