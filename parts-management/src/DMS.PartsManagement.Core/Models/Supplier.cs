using System;
using System.Collections.Generic;

namespace DMS.PartsManagement.Core.Models
{
    public enum SupplierType
    {
        Manufacturer,
        Distributor,
        Dealer,
        Aftermarket
    }

    public class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class Supplier
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SupplierType Type { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Address Address { get; set; } = new Address();
        public string Website { get; set; } = string.Empty;
        public string ShippingTerms { get; set; } = string.Empty;
        public string PaymentTerms { get; set; } = string.Empty;
        public List<string> OrderMethods { get; set; } = new List<string>();
        public int LeadTime { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<PartOrder> Orders { get; set; } = new List<PartOrder>();
    }
}
