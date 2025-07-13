using System.ComponentModel.DataAnnotations;
using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.API.Models
{
    /// <summary>
    /// Data transfer object for vehicle information
    /// </summary>
    public class VehicleDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the vehicle
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Vehicle Identification Number
        /// </summary>
        public string VIN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle stock number
        /// </summary>
        public string StockNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle make
        /// </summary>
        public string Make { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle model
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle year
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the vehicle trim level
        /// </summary>
        public string? Trim { get; set; }

        /// <summary>
        /// Gets or sets the vehicle exterior color
        /// </summary>
        public string? ExteriorColor { get; set; }

        /// <summary>
        /// Gets or sets the vehicle interior color
        /// </summary>
        public string? InteriorColor { get; set; }

        /// <summary>
        /// Gets or sets the vehicle mileage
        /// </summary>
        public int Mileage { get; set; }

        /// <summary>
        /// Gets or sets the vehicle type (New, Used, Certified Pre-Owned)
        /// </summary>
        public VehicleType VehicleType { get; set; }

        /// <summary>
        /// Gets or sets the vehicle status
        /// </summary>
        public VehicleStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the vehicle acquisition cost
        /// </summary>
        public decimal AcquisitionCost { get; set; }

        /// <summary>
        /// Gets or sets the vehicle listing price
        /// </summary>
        public decimal ListPrice { get; set; }

        /// <summary>
        /// Gets or sets the vehicle invoice price (for new vehicles)
        /// </summary>
        public decimal? InvoicePrice { get; set; }

        /// <summary>
        /// Gets or sets the vehicle MSRP (for new vehicles)
        /// </summary>
        public decimal? MSRP { get; set; }

        /// <summary>
        /// Gets or sets the date when the vehicle was acquired
        /// </summary>
        public DateTime AcquisitionDate { get; set; }

        /// <summary>
        /// Gets or sets the source of the vehicle acquisition
        /// </summary>
        public string? AcquisitionSource { get; set; }

        /// <summary>
        /// Gets or sets the physical location of the vehicle on the lot
        /// </summary>
        public string? LotLocation { get; set; }

        /// <summary>
        /// Gets or sets the list of vehicle features
        /// </summary>
        public List<VehicleFeatureDto> Features { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of vehicle images
        /// </summary>
        public List<VehicleImageDto> Images { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of reconditioning records
        /// </summary>
        public List<ReconditioningRecordDto> ReconditioningRecords { get; set; } = new();

        /// <summary>
        /// Gets or sets the date the vehicle was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who created the vehicle
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date the vehicle was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who last updated the vehicle
        /// </summary>
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating a new vehicle
    /// </summary>
    public class CreateVehicleDto
    {
        /// <summary>
        /// Gets or sets the Vehicle Identification Number
        /// </summary>
        [Required]
        [StringLength(17, MinimumLength = 17)]
        public string VIN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle stock number
        /// </summary>
        [Required]
        [StringLength(50)]
        public string StockNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle make
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle model
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle year
        /// </summary>
        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the vehicle trim level
        /// </summary>
        [StringLength(50)]
        public string? Trim { get; set; }

        /// <summary>
        /// Gets or sets the vehicle exterior color
        /// </summary>
        [StringLength(50)]
        public string? ExteriorColor { get; set; }

        /// <summary>
        /// Gets or sets the vehicle interior color
        /// </summary>
        [StringLength(50)]
        public string? InteriorColor { get; set; }

        /// <summary>
        /// Gets or sets the vehicle mileage
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }

        /// <summary>
        /// Gets or sets the vehicle type (New, Used, Certified Pre-Owned)
        /// </summary>
        [Required]
        public VehicleType VehicleType { get; set; }

        /// <summary>
        /// Gets or sets the vehicle status
        /// </summary>
        [Required]
        public VehicleStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the vehicle acquisition cost
        /// </summary>
        [Required]
        public decimal AcquisitionCost { get; set; }

        /// <summary>
        /// Gets or sets the vehicle listing price
        /// </summary>
        [Required]
        public decimal ListPrice { get; set; }

        /// <summary>
        /// Gets or sets the vehicle invoice price (for new vehicles)
        /// </summary>
        public decimal? InvoicePrice { get; set; }

        /// <summary>
        /// Gets or sets the vehicle MSRP (for new vehicles)
        /// </summary>
        public decimal? MSRP { get; set; }

        /// <summary>
        /// Gets or sets the date when the vehicle was acquired
        /// </summary>
        [Required]
        public DateTime AcquisitionDate { get; set; }

        /// <summary>
        /// Gets or sets the source of the vehicle acquisition
        /// </summary>
        [StringLength(100)]
        public string? AcquisitionSource { get; set; }

        /// <summary>
        /// Gets or sets the physical location of the vehicle on the lot
        /// </summary>
        [StringLength(100)]
        public string? LotLocation { get; set; }
    }

    /// <summary>
    /// Data transfer object for updating an existing vehicle
    /// </summary>
    public class UpdateVehicleDto
    {
        /// <summary>
        /// Gets or sets the Vehicle Identification Number
        /// </summary>
        [Required]
        [StringLength(17, MinimumLength = 17)]
        public string VIN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle stock number
        /// </summary>
        [Required]
        [StringLength(50)]
        public string StockNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle make
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle model
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle year
        /// </summary>
        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the vehicle trim level
        /// </summary>
        [StringLength(50)]
        public string? Trim { get; set; }

        /// <summary>
        /// Gets or sets the vehicle exterior color
        /// </summary>
        [StringLength(50)]
        public string? ExteriorColor { get; set; }

        /// <summary>
        /// Gets or sets the vehicle interior color
        /// </summary>
        [StringLength(50)]
        public string? InteriorColor { get; set; }

        /// <summary>
        /// Gets or sets the vehicle mileage
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }

        /// <summary>
        /// Gets or sets the vehicle type (New, Used, Certified Pre-Owned)
        /// </summary>
        [Required]
        public VehicleType VehicleType { get; set; }

        /// <summary>
        /// Gets or sets the vehicle status
        /// </summary>
        [Required]
        public VehicleStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the vehicle listing price
        /// </summary>
        [Required]
        public decimal ListPrice { get; set; }

        /// <summary>
        /// Gets or sets the vehicle invoice price (for new vehicles)
        /// </summary>
        public decimal? InvoicePrice { get; set; }

        /// <summary>
        /// Gets or sets the vehicle MSRP (for new vehicles)
        /// </summary>
        public decimal? MSRP { get; set; }

        /// <summary>
        /// Gets or sets the physical location of the vehicle on the lot
        /// </summary>
        [StringLength(100)]
        public string? LotLocation { get; set; }
    }
}
