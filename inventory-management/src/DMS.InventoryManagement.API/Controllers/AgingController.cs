using DMS.InventoryManagement.API.Models;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS.InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    [Authorize]
    public class AgingController : ControllerBase
    {
        private readonly IVehicleAgingRepository _agingRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<AgingController> _logger;

        public AgingController(
            IVehicleAgingRepository agingRepository,
            IVehicleRepository vehicleRepository,
            ILogger<AgingController> logger)
        {
            _agingRepository = agingRepository;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets aging information for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The aging information</returns>
        [HttpGet("vehicle/{vehicleId:guid}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(VehicleAgingDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehicleAgingDto>> GetByVehicleId(Guid vehicleId)
        {
            var aging = await _agingRepository.GetByVehicleIdAsync(vehicleId);
            if (aging == null)
            {
                return NotFound();
            }

            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            return Ok(MapToDto(aging, vehicle));
        }

        /// <summary>
        /// Gets vehicles with a specific alert level
        /// </summary>
        /// <param name="alertLevel">The alert level</param>
        /// <returns>A list of vehicles</returns>
        [HttpGet("alert-level/{alertLevel}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleAgingDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehicleAgingDto>>> GetByAlertLevel(AgingAlertLevel alertLevel)
        {
            var agingRecords = await _agingRepository.GetByAlertLevelAsync(alertLevel);
            return Ok(agingRecords.Select(a => MapToDto(a, a.Vehicle)));
        }

        /// <summary>
        /// Gets vehicles needing price reductions
        /// </summary>
        /// <returns>A list of vehicles needing price reductions</returns>
        [HttpGet("needs-price-reduction")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleAgingDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehicleAgingDto>>> GetNeedingPriceReduction()
        {
            var agingRecords = await _agingRepository.GetVehiclesNeedingPriceReductionAsync();
            return Ok(agingRecords.Select(a => MapToDto(a, a.Vehicle)));
        }

        /// <summary>
        /// Updates the aging threshold for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="updateDto">The updated threshold</param>
        /// <returns>No content</returns>
        [HttpPut("vehicle/{vehicleId:guid}/threshold")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateThreshold(Guid vehicleId, UpdateAgingThresholdDto updateDto)
        {
            var aging = await _agingRepository.GetByVehicleIdAsync(vehicleId);
            if (aging == null)
            {
                return NotFound();
            }

            aging.AgeThreshold = updateDto.AgeThreshold;

            // Re-evaluate alert level based on new threshold
            if (aging.DaysInInventory > aging.AgeThreshold * 1.5)
            {
                aging.AgingAlertLevel = AgingAlertLevel.Critical;
                aging.RecommendedAction = "Immediate price reduction or wholesale consideration";
            }
            else if (aging.DaysInInventory > aging.AgeThreshold)
            {
                aging.AgingAlertLevel = AgingAlertLevel.Warning;
                aging.RecommendedAction = "Consider price reduction";
            }
            else
            {
                aging.AgingAlertLevel = AgingAlertLevel.Normal;
                aging.RecommendedAction = null;
            }

            await _agingRepository.UpdateAsync(aging);
            await (_agingRepository as IUnitOfWork).SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Updates the days in inventory for all vehicles
        /// </summary>
        /// <returns>No content</returns>
        [HttpPost("update-inventory-days")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> UpdateInventoryDays()
        {
            await _agingRepository.UpdateDaysInInventoryAsync();
            return NoContent();
        }

        /// <summary>
        /// Gets aging stats summary
        /// </summary>
        /// <returns>Aging stats</returns>
        [HttpGet("summary")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(AgingSummaryDto), 200)]
        public async Task<ActionResult<AgingSummaryDto>> GetSummary()
        {
            var normalCount = (await _agingRepository.GetByAlertLevelAsync(AgingAlertLevel.Normal)).Count();
            var warningCount = (await _agingRepository.GetByAlertLevelAsync(AgingAlertLevel.Warning)).Count();
            var criticalCount = (await _agingRepository.GetByAlertLevelAsync(AgingAlertLevel.Critical)).Count();

            var summary = new AgingSummaryDto
            {
                NormalCount = normalCount,
                WarningCount = warningCount,
                CriticalCount = criticalCount,
                TotalVehicles = normalCount + warningCount + criticalCount,
                LastUpdated = DateTime.UtcNow
            };

            return Ok(summary);
        }

        #region Helper Methods

        private static VehicleAgingDto MapToDto(VehicleAging aging, Vehicle? vehicle)
        {
            var dto = new VehicleAgingDto
            {
                Id = aging.Id,
                VehicleId = aging.VehicleId,
                DaysInInventory = aging.DaysInInventory,
                AgeThreshold = aging.AgeThreshold,
                AgingAlertLevel = aging.AgingAlertLevel,
                LastPriceReductionDate = aging.LastPriceReductionDate,
                RecommendedAction = aging.RecommendedAction
            };

            if (vehicle != null)
            {
                dto.Vehicle = new VehicleSummaryDto
                {
                    Id = vehicle.Id,
                    StockNumber = vehicle.StockNumber,
                    VIN = vehicle.VIN,
                    Year = vehicle.Year,
                    Make = vehicle.Make,
                    Model = vehicle.Model,
                    ListPrice = vehicle.ListPrice
                };
            }

            return dto;
        }

        #endregion
    }

    /// <summary>
    /// Data transfer object for aging summary
    /// </summary>
    public class AgingSummaryDto
    {
        /// <summary>
        /// Gets or sets the number of vehicles with normal aging
        /// </summary>
        public int NormalCount { get; set; }

        /// <summary>
        /// Gets or sets the number of vehicles with warning aging
        /// </summary>
        public int WarningCount { get; set; }

        /// <summary>
        /// Gets or sets the number of vehicles with critical aging
        /// </summary>
        public int CriticalCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of vehicles
        /// </summary>
        public int TotalVehicles { get; set; }

        /// <summary>
        /// Gets or sets the last updated date
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
