using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;
using DMS.PartsManagement.Core.Repositories;
using DMS.PartsManagement.Core.Services;

namespace DMS.PartsManagement.Infrastructure.Services
{
    public class CoreTrackingService : ICoreTrackingService
    {
        private readonly IPartCoreTrackingRepository _coreTrackingRepository;
        private readonly IPartRepository _partRepository;

        public CoreTrackingService(
            IPartCoreTrackingRepository coreTrackingRepository,
            IPartRepository partRepository)
        {
            _coreTrackingRepository = coreTrackingRepository ?? throw new ArgumentNullException(nameof(coreTrackingRepository));
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
        }

        public async Task<IEnumerable<CoreTrackingDto>> GetAllCoreTrackingAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var coreTracking = await _coreTrackingRepository.GetAllAsync(skip, take, cancellationToken);
            return coreTracking.Select(ct => MapToDto(ct));
        }

        public async Task<CoreTrackingDto?> GetCoreTrackingByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var coreTracking = await _coreTrackingRepository.GetByIdAsync(id, cancellationToken);
            if (coreTracking == null)
            {
                return null;
            }

            return MapToDto(coreTracking);
        }

        public async Task<IEnumerable<CoreTrackingDto>> GetCoreTrackingByPartIdAsync(Guid partId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var coreTracking = await _coreTrackingRepository.GetByPartIdAsync(partId, skip, take, cancellationToken);
            return coreTracking.Select(ct => MapToDto(ct));
        }

        public async Task<IEnumerable<CoreTrackingDto>> GetCoreTrackingByStatusAsync(CoreTrackingStatus status, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            // Map from the API status enum to the internal status enum
            var coreStatus = status switch
            {
                CoreTrackingStatus.Sold => CoreStatus.Sold,
                CoreTrackingStatus.Returned => CoreStatus.Returned,
                CoreTrackingStatus.Credited => CoreStatus.Credited,
                _ => throw new ArgumentOutOfRangeException(nameof(status))
            };

            var coreTracking = await _coreTrackingRepository.GetByStatusAsync(coreStatus, skip, take, cancellationToken);
            return coreTracking.Select(ct => MapToDto(ct));
        }

        public async Task<CoreTrackingDto> CreateCoreTrackingAsync(CreateCoreTrackingDto coreTrackingDto, CancellationToken cancellationToken = default)
        {
            // Verify part exists
            var part = await _partRepository.GetByIdAsync(coreTrackingDto.PartId, cancellationToken);
            if (part == null)
            {
                throw new KeyNotFoundException($"Part with ID {coreTrackingDto.PartId} not found.");
            }

            // Verify part has core charge
            if (part.Pricing == null || !part.Pricing.HasCore)
            {
                throw new InvalidOperationException($"Part {part.PartNumber} does not have a core charge.");
            }

            var coreTracking = new PartCoreTracking
            {
                Id = Guid.NewGuid(),
                PartId = coreTrackingDto.PartId,
                CorePartNumber = coreTrackingDto.CorePartNumber,
                CoreValue = coreTrackingDto.CoreValue,
                Status = CoreStatus.Sold,
                SoldDate = DateTime.UtcNow,
                SoldReferenceId = coreTrackingDto.SoldReferenceId,
                Notes = coreTrackingDto.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            var createdCoreTracking = await _coreTrackingRepository.AddAsync(coreTracking, cancellationToken);
            return MapToDto(createdCoreTracking);
        }

        public async Task<CoreTrackingDto?> ProcessCoreReturnAsync(Guid id, ProcessCoreReturnDto processReturnDto, CancellationToken cancellationToken = default)
        {
            var coreTracking = await _coreTrackingRepository.GetByIdAsync(id, cancellationToken);
            if (coreTracking == null)
            {
                return null;
            }

            // Can only process cores that are in 'Sold' status
            if (coreTracking.Status != CoreStatus.Sold)
            {
                throw new InvalidOperationException($"Core with ID {id} is not in 'Sold' status. Current status: {coreTracking.Status}.");
            }

            // Update core status and details
            coreTracking.Status = CoreStatus.Returned;
            coreTracking.ReturnedDate = processReturnDto.ReturnedDate;
            coreTracking.ReturnReferenceId = processReturnDto.ReturnReferenceId;
            
            // Append notes about the return
            var returnNotes = processReturnDto.Notes ?? string.Empty;
            if (processReturnDto.IsDamaged && !string.IsNullOrWhiteSpace(processReturnDto.DamageDescription))
            {
                returnNotes += $" DAMAGED: {processReturnDto.DamageDescription}";
            }
            
            if (!string.IsNullOrWhiteSpace(returnNotes))
            {
                coreTracking.Notes += $"\nRETURN ({processReturnDto.ReturnedDate:yyyy-MM-dd}): {returnNotes}";
            }
            
            coreTracking.UpdatedAt = DateTime.UtcNow;

            var updatedCoreTracking = await _coreTrackingRepository.UpdateAsync(coreTracking, cancellationToken);
            return MapToDto(updatedCoreTracking);
        }

        public async Task<CoreTrackingDto?> ApplyCreditAsync(Guid id, ApplyCreditDto applyCreditDto, CancellationToken cancellationToken = default)
        {
            var coreTracking = await _coreTrackingRepository.GetByIdAsync(id, cancellationToken);
            if (coreTracking == null)
            {
                return null;
            }

            // Can only apply credit to cores that are in 'Returned' status
            if (coreTracking.Status != CoreStatus.Returned)
            {
                throw new InvalidOperationException($"Core with ID {id} is not in 'Returned' status. Current status: {coreTracking.Status}.");
            }

            // Update core status and details
            coreTracking.Status = CoreStatus.Credited;
            coreTracking.CreditedDate = applyCreditDto.CreditedDate;
            coreTracking.CreditAmount = applyCreditDto.CreditAmount;
            
            if (!string.IsNullOrWhiteSpace(applyCreditDto.Notes))
            {
                coreTracking.Notes += $"\nCREDIT ({applyCreditDto.CreditedDate:yyyy-MM-dd}): {applyCreditDto.Notes}";
            }
            
            coreTracking.UpdatedAt = DateTime.UtcNow;

            var updatedCoreTracking = await _coreTrackingRepository.UpdateAsync(coreTracking, cancellationToken);
            return MapToDto(updatedCoreTracking);
        }

        public async Task<decimal> GetTotalOutstandingCoreValueAsync(CancellationToken cancellationToken = default)
        {
            // Get the sum of core values for all cores in 'Sold' status
            return await _coreTrackingRepository.GetTotalCoreValueByStatusAsync(CoreStatus.Sold, cancellationToken);
        }

        private CoreTrackingDto MapToDto(PartCoreTracking coreTracking)
        {
            return new CoreTrackingDto
            {
                Id = coreTracking.Id,
                PartId = coreTracking.PartId,
                PartNumber = coreTracking.Part?.PartNumber ?? string.Empty,
                PartDescription = coreTracking.Part?.Description ?? string.Empty,
                CorePartNumber = coreTracking.CorePartNumber,
                CoreValue = coreTracking.CoreValue,
                Status = coreTracking.Status.ToString(),
                SoldDate = coreTracking.SoldDate,
                SoldReferenceId = coreTracking.SoldReferenceId,
                SoldReferenceNumber = string.Empty, // In a real system, we'd look up the reference number
                ReturnedDate = coreTracking.ReturnedDate,
                ReturnReferenceId = coreTracking.ReturnReferenceId,
                CreditedDate = coreTracking.CreditedDate,
                CreditAmount = coreTracking.CreditAmount,
                Notes = coreTracking.Notes,
                CreatedAt = coreTracking.CreatedAt,
                UpdatedAt = coreTracking.UpdatedAt ?? coreTracking.CreatedAt
            };
        }
    }

    // API-facing enum to be used in the interface
    public enum CoreTrackingStatus
    {
        Sold,
        Returned,
        Credited
    }
}
