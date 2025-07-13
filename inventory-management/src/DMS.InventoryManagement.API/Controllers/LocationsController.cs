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
    public class LocationsController : ControllerBase
    {
        private readonly IVehicleLocationRepository _locationRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<LocationsController> _logger;

        public LocationsController(
            IVehicleLocationRepository locationRepository,
            IVehicleRepository vehicleRepository,
            ILogger<LocationsController> logger)
        {
            _locationRepository = locationRepository;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets all locations
        /// </summary>
        /// <returns>A list of locations</returns>
        [HttpGet]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleLocationDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehicleLocationDto>>> GetAll()
        {
            var locations = await _locationRepository.GetAllAsync();
            return Ok(locations.Select(MapToDto));
        }

        /// <summary>
        /// Gets a location by ID
        /// </summary>
        /// <param name="id">The location ID</param>
        /// <returns>The location</returns>
        [HttpGet("{id:guid}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(VehicleLocationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehicleLocationDto>> GetById(Guid id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(location));
        }

        /// <summary>
        /// Gets locations by type
        /// </summary>
        /// <param name="type">The location type</param>
        /// <returns>A list of locations</returns>
        [HttpGet("type/{type}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleLocationDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehicleLocationDto>>> GetByType(LocationType type)
        {
            var locations = await _locationRepository.GetByTypeAsync(type);
            return Ok(locations.Select(MapToDto));
        }

        /// <summary>
        /// Gets locations with available capacity
        /// </summary>
        /// <returns>A list of locations</returns>
        [HttpGet("available")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleLocationDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehicleLocationDto>>> GetAvailable()
        {
            var locations = await _locationRepository.GetLocationsWithAvailableCapacityAsync();
            return Ok(locations.Select(MapToDto));
        }

        /// <summary>
        /// Creates a new location
        /// </summary>
        /// <param name="createDto">The location information</param>
        /// <returns>The created location</returns>
        [HttpPost]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(typeof(VehicleLocationDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<VehicleLocationDto>> Create(CreateVehicleLocationDto createDto)
        {
            var location = new VehicleLocation
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Type = createDto.Type,
                Address = new Address
                {
                    Street = createDto.Address.Street,
                    City = createDto.Address.City,
                    State = createDto.Address.State,
                    ZipCode = createDto.Address.ZipCode,
                    Country = createDto.Address.Country
                },
                Coordinates = createDto.Coordinates != null ? new GeoCoordinates
                {
                    Latitude = createDto.Coordinates.Latitude,
                    Longitude = createDto.Coordinates.Longitude
                } : null,
                Zones = createDto.Zones.Select(z => new LocationZone
                {
                    Id = Guid.NewGuid(),
                    Name = z.Name,
                    Capacity = z.Capacity
                }).ToList(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name
            };

            await _locationRepository.AddAsync(location);
            await (_locationRepository as IUnitOfWork).SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = location.Id }, MapToDto(location));
        }

        /// <summary>
        /// Updates a location
        /// </summary>
        /// <param name="id">The location ID</param>
        /// <param name="updateDto">The updated location information</param>
        /// <returns>No content</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(Guid id, UpdateVehicleLocationDto updateDto)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            location.Name = updateDto.Name;
            location.Type = updateDto.Type;
            location.Address = new Address
            {
                Street = updateDto.Address.Street,
                City = updateDto.Address.City,
                State = updateDto.Address.State,
                ZipCode = updateDto.Address.ZipCode,
                Country = updateDto.Address.Country
            };

            if (updateDto.Coordinates != null)
            {
                location.Coordinates = new GeoCoordinates
                {
                    Latitude = updateDto.Coordinates.Latitude,
                    Longitude = updateDto.Coordinates.Longitude
                };
            }

            location.UpdatedAt = DateTime.UtcNow;
            location.UpdatedBy = User.Identity?.Name;

            await _locationRepository.UpdateAsync(location);
            await (_locationRepository as IUnitOfWork).SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Adds a zone to a location
        /// </summary>
        /// <param name="locationId">The location ID</param>
        /// <param name="zoneDto">The zone information</param>
        /// <returns>The updated location</returns>
        [HttpPost("{locationId:guid}/zones")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(typeof(LocationZoneDto), 201)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<LocationZoneDto>> AddZone(Guid locationId, CreateLocationZoneDto zoneDto)
        {
            var location = await _locationRepository.GetByIdAsync(locationId);
            if (location == null)
            {
                return NotFound();
            }

            var zone = new LocationZone
            {
                Id = Guid.NewGuid(),
                Name = zoneDto.Name,
                Capacity = zoneDto.Capacity,
                VehicleLocationId = locationId
            };

            location.Zones.Add(zone);
            location.UpdatedAt = DateTime.UtcNow;
            location.UpdatedBy = User.Identity?.Name;

            await _locationRepository.UpdateAsync(location);
            await (_locationRepository as IUnitOfWork).SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = locationId }, MapZoneToDto(zone));
        }

        /// <summary>
        /// Transfers a vehicle to a new location
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="transferDto">The transfer information</param>
        /// <returns>No content</returns>
        [HttpPost("transfer")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> TransferVehicle(TransferVehicleDto transferDto)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(transferDto.VehicleId);
            if (vehicle == null)
            {
                return NotFound($"Vehicle with ID {transferDto.VehicleId} not found");
            }

            var location = await _locationRepository.GetByIdAsync(transferDto.LocationId);
            if (location == null)
            {
                return NotFound($"Location with ID {transferDto.LocationId} not found");
            }

            // Get vehicle counts for the location
            var vehicleCounts = await _locationRepository.GetVehicleCountsByLocationAsync();
            int currentCount = 0;
            if (vehicleCounts.ContainsKey(location.Id))
            {
                currentCount = vehicleCounts[location.Id];
            }

            // Check if location has available capacity
            int totalCapacity = location.Zones.Sum(z => z.Capacity);
            if (currentCount >= totalCapacity)
            {
                return BadRequest("Location is at full capacity");
            }

            // Update vehicle location
            vehicle.LocationId = location.Id;
            vehicle.LotLocation = transferDto.LotLocation;
            vehicle.UpdatedAt = DateTime.UtcNow;
            vehicle.UpdatedBy = User.Identity?.Name;

            await _vehicleRepository.UpdateAsync(vehicle);
            await (_vehicleRepository as IUnitOfWork).SaveChangesAsync();

            return NoContent();
        }

        #region Helper Methods

        private static VehicleLocationDto MapToDto(VehicleLocation location)
        {
            return new VehicleLocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Type = location.Type,
                Address = new AddressDto
                {
                    Street = location.Address.Street,
                    City = location.Address.City,
                    State = location.Address.State,
                    ZipCode = location.Address.ZipCode,
                    Country = location.Address.Country
                },
                Coordinates = location.Coordinates != null ? new GeoCoordinatesDto
                {
                    Latitude = location.Coordinates.Latitude,
                    Longitude = location.Coordinates.Longitude
                } : null,
                Zones = location.Zones.Select(z => MapZoneToDto(z)).ToList(),
                VehicleCount = location.Vehicles?.Count ?? 0,
                Capacity = location.Zones.Sum(z => z.Capacity),
                CreatedAt = location.CreatedAt,
                CreatedBy = location.CreatedBy,
                UpdatedAt = location.UpdatedAt,
                UpdatedBy = location.UpdatedBy
            };
        }

        private static LocationZoneDto MapZoneToDto(LocationZone zone)
        {
            return new LocationZoneDto
            {
                Id = zone.Id,
                Name = zone.Name,
                Capacity = zone.Capacity
            };
        }

        #endregion
    }

    /// <summary>
    /// Data transfer object for vehicle location
    /// </summary>
    public class VehicleLocationDto
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the location
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of location
        /// </summary>
        public LocationType Type { get; set; }

        /// <summary>
        /// Gets or sets the address
        /// </summary>
        public AddressDto Address { get; set; } = new();

        /// <summary>
        /// Gets or sets the geographic coordinates
        /// </summary>
        public GeoCoordinatesDto? Coordinates { get; set; }

        /// <summary>
        /// Gets or sets the zones within this location
        /// </summary>
        public List<LocationZoneDto> Zones { get; set; } = new();

        /// <summary>
        /// Gets or sets the number of vehicles at this location
        /// </summary>
        public int VehicleCount { get; set; }

        /// <summary>
        /// Gets or sets the total capacity of this location
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets the date the record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who created the record
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date the record was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who last updated the record
        /// </summary>
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Data transfer object for location zone
    /// </summary>
    public class LocationZoneDto
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the zone
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the capacity of the zone
        /// </summary>
        public int Capacity { get; set; }
    }

    /// <summary>
    /// Data transfer object for address
    /// </summary>
    public class AddressDto
    {
        /// <summary>
        /// Gets or sets the street
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the city
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state/province
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ZIP/postal code
        /// </summary>
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the country
        /// </summary>
        public string Country { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data transfer object for geographic coordinates
    /// </summary>
    public class GeoCoordinatesDto
    {
        /// <summary>
        /// Gets or sets the latitude
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude
        /// </summary>
        public double Longitude { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating a vehicle location
    /// </summary>
    public class CreateVehicleLocationDto
    {
        /// <summary>
        /// Gets or sets the name of the location
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of location
        /// </summary>
        public LocationType Type { get; set; }

        /// <summary>
        /// Gets or sets the address
        /// </summary>
        public AddressDto Address { get; set; } = new();

        /// <summary>
        /// Gets or sets the geographic coordinates
        /// </summary>
        public GeoCoordinatesDto? Coordinates { get; set; }

        /// <summary>
        /// Gets or sets the zones within this location
        /// </summary>
        public List<CreateLocationZoneDto> Zones { get; set; } = new();
    }

    /// <summary>
    /// Data transfer object for updating a vehicle location
    /// </summary>
    public class UpdateVehicleLocationDto
    {
        /// <summary>
        /// Gets or sets the name of the location
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of location
        /// </summary>
        public LocationType Type { get; set; }

        /// <summary>
        /// Gets or sets the address
        /// </summary>
        public AddressDto Address { get; set; } = new();

        /// <summary>
        /// Gets or sets the geographic coordinates
        /// </summary>
        public GeoCoordinatesDto? Coordinates { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating a location zone
    /// </summary>
    public class CreateLocationZoneDto
    {
        /// <summary>
        /// Gets or sets the name of the zone
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the capacity of the zone
        /// </summary>
        public int Capacity { get; set; }
    }

    /// <summary>
    /// Data transfer object for transferring a vehicle
    /// </summary>
    public class TransferVehicleDto
    {
        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }

        /// <summary>
        /// Gets or sets the location ID
        /// </summary>
        public Guid LocationId { get; set; }

        /// <summary>
        /// Gets or sets the specific lot location
        /// </summary>
        public string? LotLocation { get; set; }
    }
}
