using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.Services
{
    public interface ICoreTrackingService
    {
        Task<IEnumerable<CoreTrackingDto>> GetAllCoreTrackingAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<CoreTrackingDto?> GetCoreTrackingByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<CoreTrackingDto>> GetCoreTrackingByPartIdAsync(Guid partId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<CoreTrackingDto>> GetCoreTrackingByStatusAsync(CoreTrackingStatus status, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<CoreTrackingDto> CreateCoreTrackingAsync(CreateCoreTrackingDto coreTrackingDto, CancellationToken cancellationToken = default);
        Task<CoreTrackingDto?> ProcessCoreReturnAsync(Guid id, ProcessCoreReturnDto processReturnDto, CancellationToken cancellationToken = default);
        Task<CoreTrackingDto?> ApplyCreditAsync(Guid id, ApplyCreditDto applyCreditDto, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalOutstandingCoreValueAsync(CancellationToken cancellationToken = default);
    }
}
