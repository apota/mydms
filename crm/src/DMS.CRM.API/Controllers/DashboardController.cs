using Microsoft.AspNetCore.Mvc;
using DMS.CRM.Core.Services;
using DMS.CRM.Core.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace DMS.CRM.API.Controllers
{
    /// <summary>
    /// Dashboard controller for CRM dashboard data and operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Gets dashboard data including metrics and recent activities
        /// </summary>
        /// <returns>Dashboard data</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<DashboardDataDto>> GetDashboardData()
        {
            try
            {
                _logger.LogInformation("Getting dashboard data");
                var dashboardData = await _dashboardService.GetDashboardDataAsync();
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data");
                return StatusCode(500, "Internal server error while fetching dashboard data");
            }
        }

        /// <summary>
        /// Updates editable dashboard content
        /// </summary>
        /// <param name="updateDto">Dashboard update data</param>
        /// <returns>Updated dashboard data</returns>
        [HttpPut]
        [AllowAnonymous]
        public async Task<ActionResult<DashboardDataDto>> UpdateDashboardData([FromBody] DashboardUpdateDto updateDto)
        {
            try
            {
                if (updateDto == null)
                {
                    return BadRequest("Update data is required");
                }

                _logger.LogInformation("Updating dashboard data");
                var updatedData = await _dashboardService.UpdateDashboardDataAsync(updateDto);
                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard data");
                return StatusCode(500, "Internal server error while updating dashboard data");
            }
        }
    }
}