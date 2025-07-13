using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.API.Models
{
    /// <summary>
    /// Data transfer object for vehicle location
    /// </summary>
    public class VehicleLocationDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the location
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
        /// Gets or sets the vehicle count at this location
        /// </summary>
        public int VehicleCount { get; set; }

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
}
