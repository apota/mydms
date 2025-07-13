using System;
using System.Collections.Generic;

namespace DMS.PartsManagement.Core.Models
{
    public enum PartStatus
    {
        Active,
        Discontinued,
        Superseded
    }

    public class Dimensions
    {
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string UnitOfMeasure { get; set; } = "mm";
    }

    public class CrossReference
    {
        public Guid ManufacturerId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
    }

    public class FitmentData
    {
        public List<int> Years { get; set; } = new List<int>();
        public List<string> Makes { get; set; } = new List<string>();
        public List<string> Models { get; set; } = new List<string>();
        public List<string> Trims { get; set; } = new List<string>();
        public List<string> Engines { get; set; } = new List<string>();
    }

    public class Part
    {
        public Guid Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public Guid ManufacturerId { get; set; }
        public string ManufacturerPartNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public decimal Weight { get; set; }
        public Dimensions? Dimensions { get; set; }
        public List<string> ReplacementFor { get; set; } = new List<string>();
        public List<CrossReference> CrossReferences { get; set; } = new List<CrossReference>();
        public FitmentData? FitmentData { get; set; }
        public bool IsSerialized { get; set; }
        public bool IsSpecialOrder { get; set; }
        public bool HasCore { get; set; }
        public decimal CoreValue { get; set; }
        public string Notes { get; set; } = string.Empty;
        public PartStatus Status { get; set; } = PartStatus.Active;
        public string? SupercededBy { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Manufacturer? Manufacturer { get; set; }
        public virtual PartCategory? Category { get; set; }
        public virtual ICollection<PartInventory> Inventories { get; set; } = new List<PartInventory>();
        public virtual PartPricing? Pricing { get; set; }
        public virtual ICollection<PartTransaction> Transactions { get; set; } = new List<PartTransaction>();
        public virtual ICollection<PartCoreTracking> CoreTrackings { get; set; } = new List<PartCoreTracking>();
    }
}
