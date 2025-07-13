using System;
using System.Collections.Generic;

namespace DMS.PartsManagement.Core.DTOs
{
    // Accessories for vehicles
    public class AccessoryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public bool IsInstalled { get; set; }
        public string ImageUrl { get; set; }
        public int InstallationTimeMinutes { get; set; }
    }

    // Request to reserve parts
    public class ReservePartsRequestDto
    {
        public List<ReservePartItemDto> Parts { get; set; } = new List<ReservePartItemDto>();
        public DateTime RequiredDate { get; set; }
        public string Notes { get; set; }
    }

    public class ReservePartItemDto
    {
        public string PartId { get; set; }
        public int Quantity { get; set; }
    }

    // Result of parts reservation
    public class PartsReservationDto
    {
        public string ReservationId { get; set; }
        public string DealId { get; set; }
        public List<ReservedPartDto> ReservedParts { get; set; } = new List<ReservedPartDto>();
        public DateTime ReservationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal TotalPrice { get; set; }
        public bool AllPartsAvailable { get; set; }
    }

    public class ReservedPartDto
    {
        public string PartId { get; set; }
        public string PartNumber { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? EstimatedAvailabilityDate { get; set; }
    }

    // Deal parts order status
    public class DealPartsOrderStatusDto
    {
        public string OrderId { get; set; }
        public string DealId { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public List<OrderItemStatusDto> Items { get; set; } = new List<OrderItemStatusDto>();
    }

    public class OrderItemStatusDto
    {
        public string PartId { get; set; }
        public string PartNumber { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int QuantityReceived { get; set; }
        public string Status { get; set; }
    }

    // Installed parts for vehicles
    public class InstalledPartDto
    {
        public string PartId { get; set; }
        public string PartNumber { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public DateTime InstallationDate { get; set; }
        public string InstallationTechnicianId { get; set; }
        public string InstallationLocation { get; set; }
        public string Condition { get; set; }
        public DateTime? WarrantyExpirationDate { get; set; }
    }

    // Accessory installation estimate
    public class AccessoryInstallationRequestDto
    {
        public string VehicleId { get; set; }
        public List<string> AccessoryIds { get; set; } = new List<string>();
    }

    public class InstallationEstimateDto
    {
        public int TotalTimeMinutes { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal TotalCost { get; set; }
        public List<AccessoryInstallationDetailDto> AccessoryDetails { get; set; } = new List<AccessoryInstallationDetailDto>();
    }

    public class AccessoryInstallationDetailDto
    {
        public string AccessoryId { get; set; }
        public string Name { get; set; }
        public int InstallationTimeMinutes { get; set; }
        public decimal Cost { get; set; }
    }
}
