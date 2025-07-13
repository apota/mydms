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
    public class CostsController : ControllerBase
    {
        private readonly IVehicleCostRepository _costRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<CostsController> _logger;

        public CostsController(
            IVehicleCostRepository costRepository,
            IVehicleRepository vehicleRepository,
            ILogger<CostsController> logger)
        {
            _costRepository = costRepository;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets cost details for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The cost details</returns>
        [HttpGet("vehicle/{vehicleId:guid}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(VehicleCostDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehicleCostDto>> GetByVehicleId(Guid vehicleId)
        {
            var vehicleCost = await _costRepository.GetByVehicleIdAsync(vehicleId);
            if (vehicleCost == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(vehicleCost));
        }

        /// <summary>
        /// Creates cost details for a vehicle
        /// </summary>
        /// <param name="createDto">The cost details</param>
        /// <returns>The created cost details</returns>
        [HttpPost]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(typeof(VehicleCostDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehicleCostDto>> Create(CreateVehicleCostDto createDto)
        {
            // Check if vehicle exists
            var vehicle = await _vehicleRepository.GetByIdAsync(createDto.VehicleId);
            if (vehicle == null)
            {
                return NotFound($"Vehicle with ID {createDto.VehicleId} not found");
            }

            // Check if cost details already exist
            var existingCost = await _costRepository.GetByVehicleIdAsync(createDto.VehicleId);
            if (existingCost != null)
            {
                return BadRequest($"Cost details already exist for vehicle {createDto.VehicleId}");
            }

            // Create cost details
            var vehicleCost = new VehicleCost
            {
                Id = Guid.NewGuid(),
                VehicleId = createDto.VehicleId,
                AcquisitionCost = createDto.AcquisitionCost,
                TransportCost = createDto.TransportCost,
                ReconditioningCost = createDto.ReconditioningCost,
                CertificationCost = createDto.CertificationCost,
                TargetGrossProfit = createDto.TargetGrossProfit,
                AdditionalCosts = createDto.AdditionalCosts.Select(ac => new AdditionalCost
                {
                    Id = Guid.NewGuid(),
                    Description = ac.Description,
                    Amount = ac.Amount,
                    Date = ac.Date
                }).ToList(),
                TotalCost = createDto.AcquisitionCost + createDto.TransportCost + createDto.ReconditioningCost + 
                            createDto.CertificationCost + createDto.AdditionalCosts.Sum(ac => ac.Amount),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name
            };

            await _costRepository.AddAsync(vehicleCost);
            await (_costRepository as IUnitOfWork).SaveChangesAsync();

            return CreatedAtAction(nameof(GetByVehicleId), new { vehicleId = vehicleCost.VehicleId }, MapToDto(vehicleCost));
        }

        /// <summary>
        /// Updates cost details for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="updateDto">The updated cost details</param>
        /// <returns>No content</returns>
        [HttpPut("vehicle/{vehicleId:guid}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(Guid vehicleId, UpdateVehicleCostDto updateDto)
        {
            var vehicleCost = await _costRepository.GetByVehicleIdAsync(vehicleId);
            if (vehicleCost == null)
            {
                return NotFound();
            }

            // Update properties
            vehicleCost.AcquisitionCost = updateDto.AcquisitionCost;
            vehicleCost.TransportCost = updateDto.TransportCost;
            vehicleCost.ReconditioningCost = updateDto.ReconditioningCost;
            vehicleCost.CertificationCost = updateDto.CertificationCost;
            vehicleCost.TargetGrossProfit = updateDto.TargetGrossProfit;
            
            // Recalculate total cost
            vehicleCost.TotalCost = updateDto.AcquisitionCost + updateDto.TransportCost + 
                                   updateDto.ReconditioningCost + updateDto.CertificationCost + 
                                   vehicleCost.AdditionalCosts.Sum(ac => ac.Amount);
            
            vehicleCost.UpdatedAt = DateTime.UtcNow;
            vehicleCost.UpdatedBy = User.Identity?.Name;

            await _costRepository.UpdateAsync(vehicleCost);
            await (_costRepository as IUnitOfWork).SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Adds an additional cost to a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="additionalCostDto">The additional cost</param>
        /// <returns>The updated cost details</returns>
        [HttpPost("vehicle/{vehicleId:guid}/additional-cost")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(typeof(VehicleCostDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehicleCostDto>> AddAdditionalCost(Guid vehicleId, CreateAdditionalCostDto additionalCostDto)
        {
            var vehicleCost = await _costRepository.GetByVehicleIdAsync(vehicleId);
            if (vehicleCost == null)
            {
                return NotFound();
            }

            var additionalCost = new AdditionalCost
            {
                Id = Guid.NewGuid(),
                VehicleCostId = vehicleCost.Id,
                Description = additionalCostDto.Description,
                Amount = additionalCostDto.Amount,
                Date = additionalCostDto.Date
            };

            vehicleCost.AdditionalCosts.Add(additionalCost);
            vehicleCost.TotalCost += additionalCost.Amount;
            vehicleCost.UpdatedAt = DateTime.UtcNow;
            vehicleCost.UpdatedBy = User.Identity?.Name;

            await _costRepository.UpdateAsync(vehicleCost);
            await (_costRepository as IUnitOfWork).SaveChangesAsync();

            return Ok(MapToDto(vehicleCost));
        }

        /// <summary>
        /// Gets the total inventory investment
        /// </summary>
        /// <returns>The total inventory investment</returns>
        [HttpGet("total-investment")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(decimal), 200)]
        public async Task<ActionResult<decimal>> GetTotalInvestment()
        {
            var totalInvestment = await _costRepository.GetTotalInventoryInvestmentAsync();
            return Ok(totalInvestment);
        }

        /// <summary>
        /// Gets vehicles above a cost threshold
        /// </summary>
        /// <param name="threshold">The cost threshold</param>
        /// <returns>A list of vehicle costs</returns>
        [HttpGet("above-threshold/{threshold:decimal}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleCostDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehicleCostDto>>> GetAboveThreshold(decimal threshold)
        {
            var vehicleCosts = await _costRepository.GetVehiclesAboveCostThresholdAsync(threshold);
            return Ok(vehicleCosts.Select(MapToDto));
        }

        #region Helper Methods

        private static VehicleCostDto MapToDto(VehicleCost vehicleCost)
        {
            return new VehicleCostDto
            {
                Id = vehicleCost.Id,
                VehicleId = vehicleCost.VehicleId,
                AcquisitionCost = vehicleCost.AcquisitionCost,
                TransportCost = vehicleCost.TransportCost,
                ReconditioningCost = vehicleCost.ReconditioningCost,
                CertificationCost = vehicleCost.CertificationCost,
                AdditionalCosts = vehicleCost.AdditionalCosts.Select(ac => new AdditionalCostDto
                {
                    Id = ac.Id,
                    Description = ac.Description,
                    Amount = ac.Amount,
                    Date = ac.Date
                }).ToList(),
                TotalCost = vehicleCost.TotalCost,
                TargetGrossProfit = vehicleCost.TargetGrossProfit,
                CreatedAt = vehicleCost.CreatedAt,
                CreatedBy = vehicleCost.CreatedBy,
                UpdatedAt = vehicleCost.UpdatedAt,
                UpdatedBy = vehicleCost.UpdatedBy
            };
        }

        #endregion
    }
}
