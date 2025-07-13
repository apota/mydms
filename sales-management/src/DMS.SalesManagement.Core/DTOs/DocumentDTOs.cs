using System;
using System.Collections.Generic;

namespace DMS.SalesManagement.Core.DTOs
{
    public class SalesDocumentDto
    {
        public Guid Id { get; set; }
        public Guid DealId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Filename { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public List<RequiredSignature> RequiredSignatures { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    public class SalesDocumentCreateDto
    {
        public Guid DealId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile File { get; set; }
    }
    
    public class DocumentSignatureDto
    {
        public string Role { get; set; }
        public string Name { get; set; }
        public string SignatureData { get; set; }
    }
    
    public class DocumentTemplateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
