using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.API.Models
{
    /// <summary>
    /// Data transfer object for vehicle pricing
    /// </summary>
    public class VehiclePricingDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the pricing record
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }

        /// <summary>
        /// Gets or sets the MSRP (Manufacturer's Suggested Retail Price)
        /// </summary>
        public decimal MSRP { get; set; }

        /// <summary>
        /// Gets or sets the internet price
        /// </summary>
        public decimal InternetPrice { get; set; }

        /// <summary>
        /// Gets or sets the sticking price
        /// </summary>
        public decimal StickingPrice { get; set; }

        /// <summary>
        /// Gets or sets the floor price (minimum acceptable selling price)
        /// </summary>
        public decimal FloorPrice { get; set; }

        /// <summary>
        /// Gets or sets the special price
        /// </summary>
        public decimal? SpecialPrice { get; set; }

        /// <summary>
        /// Gets or sets the start date for the special price
        /// </summary>
        public DateTime? SpecialStartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date for the special price
        /// </summary>
        public DateTime? SpecialEndDate { get; set; }

        /// <summary>
        /// Gets or sets the price history
        /// </summary>
        public List<PriceHistoryEntryDto> PriceHistory { get; set; } = new();

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
    /// Data transfer object for price history entry
    /// </summary>
    public class PriceHistoryEntryDto
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the price value
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the date of the price change
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the reason for the price change
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Gets or sets the user ID who made the price change
        /// </summary>
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating vehicle pricing
    /// </summary>
    public class CreateVehiclePricingDto
    {
        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }

        /// <summary>
        /// Gets or sets the MSRP (Manufacturer's Suggested Retail Price)
        /// </summary>
        public decimal MSRP { get; set; }

        /// <summary>
        /// Gets or sets the internet price
        /// </summary>
        public decimal InternetPrice { get; set; }

        /// <summary>
        /// Gets or sets the sticking price
        /// </summary>
        public decimal StickingPrice { get; set; }

        /// <summary>
        /// Gets or sets the floor price (minimum acceptable selling price)
        /// </summary>
        public decimal FloorPrice { get; set; }

        /// <summary>
        /// Gets or sets the special price
        /// </summary>
        public decimal? SpecialPrice { get; set; }

        /// <summary>
        /// Gets or sets the start date for the special price
        /// </summary>
        public DateTime? SpecialStartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date for the special price
        /// </summary>
        public DateTime? SpecialEndDate { get; set; }
    }

    /// <summary>
    /// Data transfer object for updating vehicle pricing
    /// </summary>
    public class UpdateVehiclePricingDto
    {
        /// <summary>
        /// Gets or sets the MSRP (Manufacturer's Suggested Retail Price)
        /// </summary>
        public decimal MSRP { get; set; }

        /// <summary>
        /// Gets or sets the internet price
        /// </summary>
        public decimal InternetPrice { get; set; }

        /// <summary>
        /// Gets or sets the sticking price
        /// </summary>
        public decimal StickingPrice { get; set; }

        /// <summary>
        /// Gets or sets the floor price (minimum acceptable selling price)
        /// </summary>
        public decimal FloorPrice { get; set; }

        /// <summary>
        /// Gets or sets the special price
        /// </summary>
        public decimal? SpecialPrice { get; set; }

        /// <summary>
        /// Gets or sets the start date for the special price
        /// </summary>
        public DateTime? SpecialStartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date for the special price
        /// </summary>
        public DateTime? SpecialEndDate { get; set; }
    }

    /// <summary>
    /// Data transfer object for adding price history
    /// </summary>
    public class AddPriceHistoryEntryDto
    {
        /// <summary>
        /// Gets or sets the price value
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the reason for the price change
        /// </summary>
        public string? Reason { get; set; }
    }
}
