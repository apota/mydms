using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents a location where vehicles can be stored
    /// </summary>
    public class VehicleLocation : IAuditableEntity
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
        public Address Address { get; set; } = new();

        /// <summary>
        /// Gets or sets the geographic coordinates
        /// </summary>
        public GeoCoordinates? Coordinates { get; set; }

        /// <summary>
        /// Gets or sets the zones within this location
        /// </summary>
        public List<LocationZone> Zones { get; set; } = new();

        /// <summary>
        /// Gets or sets the vehicles at this location
        /// </summary>
        public List<Vehicle> Vehicles { get; set; } = new();

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
    /// Represents a zone within a location
    /// </summary>
    public class LocationZone : IEntity
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

        /// <summary>
        /// Gets or sets the location ID this zone belongs to
        /// </summary>
        public Guid VehicleLocationId { get; set; }
    }

    /// <summary>
    /// Represents an address
    /// </summary>
    public class Address
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
    /// Represents geographic coordinates
    /// </summary>
    public class GeoCoordinates
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
    /// Type of vehicle location
    /// </summary>
    public enum LocationType
    {
        Lot,
        Showroom,
        Service,
        Storage,
        Transit,
        Other
    }
}
