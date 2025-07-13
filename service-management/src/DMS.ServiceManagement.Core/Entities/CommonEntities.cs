using System;

namespace DMS.ServiceManagement.Core.Entities
{
    public class CustomerInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class VehicleInfo
    {
        public Guid Id { get; set; }
        public string VIN { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
    }

    public class PartAvailability
    {
        public string PartNumber { get; set; }
        public string Name { get; set; }
        public int QuantityAvailable { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable => QuantityAvailable > 0;
    }

    public class InvoiceResult
    {
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public bool Success { get; set; }
        public string InvoiceUrl { get; set; }
    }
}
