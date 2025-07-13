using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.DTOs;
using DMS.FinancialManagement.Core.Repositories;
using DMS.FinancialManagement.Core.Services;
using Microsoft.EntityFrameworkCore;
using DMS.FinancialManagement.Infrastructure.Data;

namespace DMS.FinancialManagement.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for financial reports
    /// </summary>
    public class ReportingService : IReportingService
    {
        private readonly IJournalEntryRepository _journalEntryRepository;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly FinancialDbContext _dbContext;

        public ReportingService(
            IJournalEntryRepository journalEntryRepository,
            IChartOfAccountRepository chartOfAccountRepository,
            FinancialDbContext dbContext)
        {
            _journalEntryRepository = journalEntryRepository;
            _chartOfAccountRepository = chartOfAccountRepository;
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<JournalEntryBalanceReportResponseDto> GenerateJournalBalanceReportAsync(
            JournalEntryBalanceReportRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Validate request dates
            if (request.StartDate > request.EndDate)
            {
                throw new InvalidOperationException("Start date must be before or equal to end date");
            }

            // Query to get the journal entries based on request parameters
            var journalEntriesQuery = _dbContext.JournalEntries
                .Include(je => je.LineItems)
                    .ThenInclude(li => li.Account)
                .AsQueryable();

            // Apply date range filter
            journalEntriesQuery = journalEntriesQuery.Where(je => 
                je.EntryDate >= request.StartDate && 
                je.EntryDate <= request.EndDate);

            // Apply posted status filter
            if (!request.IncludeUnposted)
            {
                journalEntriesQuery = journalEntriesQuery.Where(je => je.IsPosted);
            }

            // Apply financial period filter if specified
            if (request.FinancialPeriodId.HasValue)
            {
                journalEntriesQuery = journalEntriesQuery.Where(je => 
                    je.FinancialPeriodId == request.FinancialPeriodId.Value);
            }

            // Retrieve the journal entries
            var journalEntries = await journalEntriesQuery.ToListAsync(cancellationToken);
            
            // Get all accounts
            var accounts = await _chartOfAccountRepository.GetAllAsync(0, int.MaxValue, cancellationToken);
            var accountsDict = accounts.ToDictionary(a => a.Id);

            // Process line items for the report
            var reportLinesByAccount = new Dictionary<Guid, JournalEntryBalanceReportLineDto>();
            
            foreach (var entry in journalEntries)
            {
                foreach (var lineItem in entry.LineItems)
                {
                    // Apply account filter if specified
                    if (request.ChartOfAccountId.HasValue && 
                        lineItem.AccountId != request.ChartOfAccountId.Value)
                    {
                        continue;
                    }

                    // Apply department filter if specified
                    if (request.DepartmentId.HasValue && 
                        lineItem.DepartmentId != request.DepartmentId.Value)
                    {
                        continue;
                    }

                    if (!reportLinesByAccount.TryGetValue(lineItem.AccountId, out var reportLine))
                    {
                        var account = accountsDict.GetValueOrDefault(lineItem.AccountId);
                        
                        reportLine = new JournalEntryBalanceReportLineDto
                        {
                            AccountId = lineItem.AccountId,
                            AccountCode = account?.AccountCode ?? "Unknown",
                            AccountName = account?.Name ?? "Unknown Account",
                            AccountTypeName = account?.AccountType.ToString() ?? "Unknown",
                            TotalDebits = 0,
                            TotalCredits = 0,
                            Balance = 0,
                            EntryCount = 0
                        };
                        
                        reportLinesByAccount[lineItem.AccountId] = reportLine;
                    }

                    // Update report line with this line item's data
                    reportLine = reportLinesByAccount[lineItem.AccountId] with
                    {
                        TotalDebits = reportLine.TotalDebits + lineItem.DebitAmount,
                        TotalCredits = reportLine.TotalCredits + lineItem.CreditAmount,
                        Balance = reportLine.Balance + (lineItem.DebitAmount - lineItem.CreditAmount),
                        EntryCount = reportLine.EntryCount + 1
                    };
                    
                    reportLinesByAccount[lineItem.AccountId] = reportLine;
                }
            }

            // Build the report response
            var reportLines = reportLinesByAccount.Values.ToList();
            
            var totalDebits = reportLines.Sum(r => r.TotalDebits);
            var totalCredits = reportLines.Sum(r => r.TotalCredits);
            var netBalance = totalDebits - totalCredits;
            
            var report = new JournalEntryBalanceReportResponseDto
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = "system", // This should be replaced with actual user info from auth context
                Lines = reportLines.OrderBy(r => r.AccountCode).ToList(),
                TotalDebits = totalDebits,
                TotalCredits = totalCredits,
                NetBalance = netBalance
            };
            
            return report;
        }
    }
}
