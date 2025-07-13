using DMS.CRM.Core.DTOs;

namespace DMS.CRM.Core.Services
{
    /// <summary>
    /// Service interface for dashboard data management
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Gets aggregated dashboard data including metrics and recent activities
        /// </summary>
        /// <returns>Dashboard data DTO</returns>
        Task<DashboardDataDto> GetDashboardDataAsync();

        /// <summary>
        /// Updates editable dashboard content and persists to database
        /// </summary>
        /// <param name="updateDto">Dashboard update data</param>
        /// <returns>Updated dashboard data</returns>
        Task<DashboardDataDto> UpdateDashboardDataAsync(DashboardUpdateDto updateDto);
    }
}
