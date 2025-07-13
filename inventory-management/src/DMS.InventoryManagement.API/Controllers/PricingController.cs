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
    public class PricingController : ControllerBase
    {
        private readonly IVehiclePricingRepository _pricingRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<PricingController> _logger;

        public PricingController(
            IVehiclePricingRepository pricingRepository,
            IVehicleRepository vehicleRepository, 
            ILogger<PricingController> logger)
        {
            _pricingRepository = pricingRepository;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets pricing for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The pricing information</returns>
        [HttpGet("vehicle/{vehicleId:guid}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(VehiclePricingDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehiclePricingDto>> GetByVehicleId(Guid vehicleId)
        {
            var pricing = await _pricingRepository.GetByVehicleIdAsync(vehicleId);
            if (pricing == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(pricing));
        }

        /// <summary>
        /// Creates pricing for a vehicle
        /// </summary>
        /// <param name="createDto">The pricing information</param>
        /// <returns>The created pricing information</returns>
        [HttpPost]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(typeof(VehiclePricingDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehiclePricingDto>> Create(CreateVehiclePricingDto createDto)
        {
            // Check if vehicle exists
            var vehicle = await _vehicleRepository.GetByIdAsync(createDto.VehicleId);
            if (vehicle == null)
            {
                return NotFound($"Vehicle with ID {createDto.VehicleId} not found");
            }

            // Check if pricing already exists
            var existingPricing = await _pricingRepository.GetByVehicleIdAsync(createDto.VehicleId);
            if (existingPricing != null)
            {
                return BadRequest($"Pricing already exists for vehicle {createDto.VehicleId}");
            }

            // Create pricing
            var pricing = new VehiclePricing
            {
                Id = Guid.NewGuid(),
                VehicleId = createDto.VehicleId,
                MSRP = createDto.MSRP,
                InternetPrice = createDto.InternetPrice,
                StickingPrice = createDto.StickingPrice,
                FloorPrice = createDto.FloorPrice,
                SpecialPrice = createDto.SpecialPrice,
                SpecialStartDate = createDto.SpecialStartDate,
                SpecialEndDate = createDto.SpecialEndDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name
            };

            // Add first price history entry
            pricing.PriceHistory.Add(new PriceHistoryEntry
            {
                Id = Guid.NewGuid(),
                Price = createDto.InternetPrice,
                Date = DateTime.UtcNow,
                Reason = "Initial price",
                UserId = User.Identity?.Name,
                VehiclePricingId = pricing.Id
            });

            await _pricingRepository.AddAsync(pricing);
            await (_pricingRepository as IUnitOfWork).SaveChangesAsync();

            return CreatedAtAction(nameof(GetByVehicleId), new { vehicleId = pricing.VehicleId }, MapToDto(pricing));
        }

        /// <summary>
        /// Updates pricing for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="updateDto">The updated pricing information</param>
        /// <returns>No content</returns>
        [HttpPut("vehicle/{vehicleId:guid}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(Guid vehicleId, UpdateVehiclePricingDto updateDto)
        {
            var pricing = await _pricingRepository.GetByVehicleIdAsync(vehicleId);
            if (pricing == null)
            {
                return NotFound();
            }

            // Check if price changed
            bool internetPriceChanged = pricing.InternetPrice != updateDto.InternetPrice;

            // Update properties
            pricing.MSRP = updateDto.MSRP;
            pricing.InternetPrice = updateDto.InternetPrice;
            pricing.StickingPrice = updateDto.StickingPrice;
            pricing.FloorPrice = updateDto.FloorPrice;
            pricing.SpecialPrice = updateDto.SpecialPrice;
            pricing.SpecialStartDate = updateDto.SpecialStartDate;
            pricing.SpecialEndDate = updateDto.SpecialEndDate;
            pricing.UpdatedAt = DateTime.UtcNow;
            pricing.UpdatedBy = User.Identity?.Name;

            // Add price history entry if internet price changed
            if (internetPriceChanged)
            {
                pricing.PriceHistory.Add(new PriceHistoryEntry
                {
                    Id = Guid.NewGuid(),
                    Price = updateDto.InternetPrice,
                    Date = DateTime.UtcNow,
                    Reason = "Price update",
                    UserId = User.Identity?.Name,
                    VehiclePricingId = pricing.Id
                });
            }

            await _pricingRepository.UpdateAsync(pricing);
            await (_pricingRepository as IUnitOfWork).SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Adds a price history entry to a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="entryDto">The price history entry</param>
        /// <returns>The updated pricing information</returns>
        [HttpPost("vehicle/{vehicleId:guid}/price-history")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(typeof(VehiclePricingDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehiclePricingDto>> AddPriceHistory(Guid vehicleId, AddPriceHistoryEntryDto entryDto)
        {
            var pricing = await _pricingRepository.GetByVehicleIdAsync(vehicleId);
            if (pricing == null)
            {
                return NotFound();
            }

            // Update the internet price
            pricing.InternetPrice = entryDto.Price;
            
            // Add price history entry
            pricing.PriceHistory.Add(new PriceHistoryEntry
            {
                Id = Guid.NewGuid(),
                Price = entryDto.Price,
                Date = DateTime.UtcNow,
                Reason = entryDto.Reason,
                UserId = User.Identity?.Name,
                VehiclePricingId = pricing.Id
            });

            pricing.UpdatedAt = DateTime.UtcNow;
            pricing.UpdatedBy = User.Identity?.Name;

            await _pricingRepository.UpdateAsync(pricing);
            await (_pricingRepository as IUnitOfWork).SaveChangesAsync();

            // Update vehicle aging info to record the price reduction
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                if (vehicle?.AgingInfo != null)
                {
                    vehicle.AgingInfo.LastPriceReductionDate = DateTime.UtcNow;
                    await _vehicleRepository.UpdateAsync(vehicle);
                    await (_vehicleRepository as IUnitOfWork).SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle aging info for price reduction");
            }

            return Ok(MapToDto(pricing));
        }

        /// <summary>
        /// Gets vehicles with special pricing
        /// </summary>
        /// <returns>A list of vehicles with special pricing</returns>
        [HttpGet("special-pricing")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehiclePricingDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehiclePricingDto>>> GetSpecialPricing()
        {
            var vehicles = await _pricingRepository.GetVehiclesWithSpecialPricingAsync();
            return Ok(vehicles.Select(MapToDto));
        }

        /// <summary>
        /// Gets vehicles with price changes in a date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <returns>A list of vehicles with price changes</returns>
        [HttpGet("price-changes")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehiclePricingDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehiclePricingDto>>> GetPriceChanges(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            var vehicles = await _pricingRepository.GetVehiclesWithPriceChangesAsync(startDate, endDate);
            return Ok(vehicles.Select(MapToDto));
        }

        #region Helper Methods

        private static VehiclePricingDto MapToDto(VehiclePricing pricing)
        {
            return new VehiclePricingDto
            {
                Id = pricing.Id,
                VehicleId = pricing.VehicleId,
                MSRP = pricing.MSRP,
                InternetPrice = pricing.InternetPrice,
                StickingPrice = pricing.StickingPrice,
                FloorPrice = pricing.FloorPrice,
                SpecialPrice = pricing.SpecialPrice,
                SpecialStartDate = pricing.SpecialStartDate,
                SpecialEndDate = pricing.SpecialEndDate,
                PriceHistory = pricing.PriceHistory.Select(ph => new PriceHistoryEntryDto
                {
                    Id = ph.Id,
                    Price = ph.Price,
                    Date = ph.Date,
                    Reason = ph.Reason,
                    UserId = ph.UserId
                }).OrderByDescending(ph => ph.Date).ToList(),
                CreatedAt = pricing.CreatedAt,
                CreatedBy = pricing.CreatedBy,
                UpdatedAt = pricing.UpdatedAt,
                UpdatedBy = pricing.UpdatedBy
            };
        }

        #endregion
    }
}
