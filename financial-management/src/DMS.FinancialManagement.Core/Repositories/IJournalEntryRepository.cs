using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.FinancialManagement.Core.Repositories
{
    /// <summary>
    /// Repository interface for JournalEntry entities
    /// </summary>
    public interface IJournalEntryRepository : IRepository<JournalEntry>
    {
        /// <summary>
        /// Gets journal entries by date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="skip">The number of entries to skip</param>
        /// <param name="take">The number of entries to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of journal entries in the specified date range</returns>
        Task<IEnumerable<JournalEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets journal entries by financial period ID
        /// </summary>
        /// <param name="financialPeriodId">The financial period ID</param>
        /// <param name="skip">The number of entries to skip</param>
        /// <param name="take">The number of entries to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of journal entries in the specified financial period</returns>
        Task<IEnumerable<JournalEntry>> GetByFinancialPeriodIdAsync(Guid financialPeriodId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets journal entries by reference
        /// </summary>
        /// <param name="reference">The reference</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of journal entries with the specified reference</returns>
        Task<IEnumerable<JournalEntry>> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets journal entries by entry number
        /// </summary>
        /// <param name="entryNumber">The entry number</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The journal entry with the specified entry number</returns>
        Task<JournalEntry?> GetByEntryNumberAsync(string entryNumber, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Posts a journal entry
        /// </summary>
        /// <param name="id">The journal entry ID</param>
        /// <param name="postingDate">The posting date</param>
        /// <param name="postedBy">The user who posted the entry</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The posted journal entry</returns>
        Task<JournalEntry?> PostJournalEntryAsync(Guid id, DateTime postingDate, string postedBy, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Reverses a journal entry
        /// </summary>
        /// <param name="id">The journal entry ID to reverse</param>
        /// <param name="reversalDate">The date of the reversal</param>
        /// <param name="description">The description of the reversal</param>
        /// <param name="reversedBy">The user who reversed the entry</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The reversal journal entry</returns>
        Task<JournalEntry?> ReverseJournalEntryAsync(Guid id, DateTime reversalDate, string description, string reversedBy, CancellationToken cancellationToken = default);
    }
}
