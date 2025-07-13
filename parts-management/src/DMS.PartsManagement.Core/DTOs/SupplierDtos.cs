using System;
using System.ComponentModel.DataAnnotations;

namespace DMS.PartsManagement.Core.DTOs
{
    public class SupplierSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string AccountNumber { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
    }

    public class SupplierDetailDto : SupplierSummaryDto
    {
        public AddressDto Address { get; set; }
        public string Website { get; set; }
        public string ShippingTerms { get; set; }
        public string PaymentTerms { get; set; }
        public string[] OrderMethods { get; set; }
        public int LeadTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AddressDto
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }

    public class CreateSupplierDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [Required]
        public string Type { get; set; }
        
        [MaxLength(50)]
        public string AccountNumber { get; set; }
        
        [MaxLength(100)]
        public string ContactPerson { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string Phone { get; set; }
        
        public AddressDto Address { get; set; }
        
        [Url]
        public string Website { get; set; }
        
        public string ShippingTerms { get; set; }
        
        public string PaymentTerms { get; set; }
        
        public string[] OrderMethods { get; set; }
        
        public int LeadTime { get; set; }
        
        public string Status { get; set; } = "Active";
    }

    public class UpdateSupplierDto
    {
        [MaxLength(100)]
        public string Name { get; set; }
        
        public string Type { get; set; }
        
        [MaxLength(50)]
        public string AccountNumber { get; set; }
        
        [MaxLength(100)]
        public string ContactPerson { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string Phone { get; set; }
        
        public AddressDto Address { get; set; }
        
        [Url]
        public string Website { get; set; }
        
        public string ShippingTerms { get; set; }
        
        public string PaymentTerms { get; set; }
        
        public string[] OrderMethods { get; set; }
        
        public int LeadTime { get; set; }
        
        public string Status { get; set; }
    }
}
