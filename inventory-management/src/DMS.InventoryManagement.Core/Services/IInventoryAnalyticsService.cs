using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.InventoryManagement.API.Models;

namespace DMS.InventoryManagement.Core.Services
{
    public interface IInventoryAnalyticsService
    {
        Task<AgingAnalyticsDto> GetAgingAnalyticsAsync(
            List<Guid>? locationIds,
            List<string>? vehicleTypes,
            DateRangeDto? dateRange);

        Task<InventoryValuationDto> GetInventoryValuationAsync(
            string groupBy,
            List<Guid>? locationIds,
            bool includeSold);

        Task<TurnoverMetricsDto> GetTurnoverMetricsAsync(
            string timePeriod,
            List<string>? vehicleTypes,
            List<Guid>? locationIds);

        Task<InventoryMixAnalysisDto> GetInventoryMixAnalysisAsync(
            string groupingFactor,
            string comparisonPeriod);

        Task<PriceCompetitivenessDto> GetPriceCompetitivenessAsync(
            List<Guid>? vehicleIds,
            int marketRadius);
    }
}
