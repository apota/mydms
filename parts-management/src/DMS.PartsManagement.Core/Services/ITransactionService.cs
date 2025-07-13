using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<PartTransactionDto>> GetAllTransactionsAsync(int skip = 0, int take = 50, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<PartTransactionDto?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartTransactionDto>> GetTransactionsByPartIdAsync(Guid partId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartTransactionDto>> GetTransactionsByTypeAsync(TransactionType type, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartTransactionDto> IssuePartsAsync(IssuePartsDto issuePartsDto, CancellationToken cancellationToken = default);
        Task<PartTransactionDto> ReturnPartsAsync(ReturnPartsDto returnPartsDto, CancellationToken cancellationToken = default);
        Task<PartTransactionDto> AdjustInventoryAsync(AdjustInventoryDto adjustInventoryDto, CancellationToken cancellationToken = default);
        Task<PartTransactionDto> TransferPartsAsync(TransferPartsDto transferPartsDto, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartTransactionSummaryDto>> GetTransactionHistoryAsync(Guid partId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
