using System;
using System.Collections.Generic;

namespace DMS.PartsManagement.Core.DTOs
{
    #region CRM Integration DTOs

    /// <summary>
    /// Customer information DTO
    /// </summary>
    public class CustomerDto
    {
        /// <summary>
        /// Customer ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Customer number
        /// </summary>
        public string CustomerNumber { get; set; }

        /// <summary>
        /// Customer name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Customer email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Customer phone number
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Customer address
        /// </summary>
        public AddressDto Address { get; set; }

        /// <summary>
        /// Customer type
        /// </summary>
        public string CustomerType { get; set; }

        /// <summary>
        /// Tax exempt status
        /// </summary>
        public bool IsTaxExempt { get; set; }

        /// <summary>
        /// Tax exemption number
        /// </summary>
        public string TaxExemptionNumber { get; set; }
    }

    /// <summary>
    /// Address DTO
    /// </summary>
    public class AddressDto
    {
        /// <summary>
        /// Street line 1
        /// </summary>
        public string Line1 { get; set; }

        /// <summary>
        /// Street line 2
        /// </summary>
        public string Line2 { get; set; }

        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// State/Province
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Postal/Zip code
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; }
    }

    #endregion

    #region Financial Integration DTOs

    /// <summary>
    /// Part pricing DTO
    /// </summary>
    public class PartPricingDto
    {
        /// <summary>
        /// Part ID
        /// </summary>
        public Guid PartId { get; set; }

        /// <summary>
        /// Part number
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// List price
        /// </summary>
        public decimal ListPrice { get; set; }

        /// <summary>
        /// Cost
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Markup percentage
        /// </summary>
        public decimal MarkupPercentage { get; set; }

        /// <summary>
        /// Price tiers
        /// </summary>
        public List<PriceTierDto> PriceTiers { get; set; }

        /// <summary>
        /// Special pricing
        /// </summary>
        public List<SpecialPricingDto> SpecialPricing { get; set; }

        /// <summary>
        /// Tax rate
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Core charge
        /// </summary>
        public decimal? CoreCharge { get; set; }

        /// <summary>
        /// Core charge tax rate
        /// </summary>
        public decimal? CoreChargeTaxRate { get; set; }
    }

    /// <summary>
    /// Price tier DTO
    /// </summary>
    public class PriceTierDto
    {
        /// <summary>
        /// Tier name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Minimum quantity
        /// </summary>
        public int MinQuantity { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Discount percentage
        /// </summary>
        public decimal DiscountPercentage { get; set; }
    }

    /// <summary>
    /// Special pricing DTO
    /// </summary>
    public class SpecialPricingDto
    {
        /// <summary>
        /// Customer ID
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Customer name
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Discount percentage
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Invoice DTO
    /// </summary>
    public class InvoiceDto
    {
        /// <summary>
        /// Invoice ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Invoice number
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Customer ID
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Customer name
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Due date
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Invoice items
        /// </summary>
        public List<InvoiceItemDto> Items { get; set; }

        /// <summary>
        /// Subtotal
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Tax
        /// </summary>
        public decimal Tax { get; set; }

        /// <summary>
        /// Total
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// Invoice item DTO
    /// </summary>
    public class InvoiceItemDto
    {
        /// <summary>
        /// Item ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Part ID
        /// </summary>
        public Guid? PartId { get; set; }

        /// <summary>
        /// Part number
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Extended price
        /// </summary>
        public decimal ExtendedPrice { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Core charge
        /// </summary>
        public decimal? CoreCharge { get; set; }

        /// <summary>
        /// Core charge tax
        /// </summary>
        public decimal? CoreChargeTax { get; set; }

        /// <summary>
        /// Total
        /// </summary>
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Create invoice DTO
    /// </summary>
    public class CreateInvoiceDto
    {
        /// <summary>
        /// Customer ID
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Due date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Invoice items
        /// </summary>
        public List<CreateInvoiceItemDto> Items { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// Create invoice item DTO
    /// </summary>
    public class CreateInvoiceItemDto
    {
        /// <summary>
        /// Part ID
        /// </summary>
        public Guid? PartId { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Tax exempt
        /// </summary>
        public bool IsTaxExempt { get; set; }

        /// <summary>
        /// Core charge
        /// </summary>
        public decimal? CoreCharge { get; set; }
    }

    #endregion

    #region Service Integration DTOs

    /// <summary>
    /// Service order part DTO
    /// </summary>
    public class ServiceOrderPartDto
    {
        /// <summary>
        /// Line ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Service order ID
        /// </summary>
        public Guid ServiceOrderId { get; set; }

        /// <summary>
        /// Service order number
        /// </summary>
        public string ServiceOrderNumber { get; set; }

        /// <summary>
        /// Part ID
        /// </summary>
        public Guid PartId { get; set; }

        /// <summary>
        /// Part number
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Extended price
        /// </summary>
        public decimal ExtendedPrice { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Core charge
        /// </summary>
        public decimal? CoreCharge { get; set; }

        /// <summary>
        /// Assigned by
        /// </summary>
        public string AssignedBy { get; set; }

        /// <summary>
        /// Assigned date
        /// </summary>
        public DateTime AssignedDate { get; set; }
    }

    /// <summary>
    /// Assign service order parts DTO
    /// </summary>
    public class AssignServiceOrderPartsDto
    {
        /// <summary>
        /// Parts to assign
        /// </summary>
        public List<AssignServiceOrderPartItemDto> Parts { get; set; }
    }

    /// <summary>
    /// Assign service order part item DTO
    /// </summary>
    public class AssignServiceOrderPartItemDto
    {
        /// <summary>
        /// Part ID
        /// </summary>
        public Guid PartId { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        public string Notes { get; set; }
    }

    #endregion

    #region Sales Integration DTOs

    /// <summary>
    /// Vehicle accessory DTO
    /// </summary>
    public class VehicleAccessoryDto
    {
        /// <summary>
        /// Part ID
        /// </summary>
        public Guid PartId { get; set; }

        /// <summary>
        /// Part number
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List price
        /// </summary>
        public decimal ListPrice { get; set; }

        /// <summary>
        /// Installation time (hours)
        /// </summary>
        public decimal InstallationTime { get; set; }

        /// <summary>
        /// Installation price
        /// </summary>
        public decimal InstallationPrice { get; set; }

        /// <summary>
        /// Category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Is popular
        /// </summary>
        public bool IsPopular { get; set; }

        /// <summary>
        /// Image URL
        /// </summary>
        public string ImageUrl { get; set; }
    }

    #endregion
}
