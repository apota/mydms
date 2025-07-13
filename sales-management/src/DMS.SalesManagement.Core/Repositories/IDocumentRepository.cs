using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.SalesManagement.Core.DTOs;
using DMS.SalesManagement.Core.Models;
using Microsoft.AspNetCore.Http;

namespace DMS.SalesManagement.Core.Repositories
{
    public interface IDocumentRepository
    {
        Task<SalesDocument> GetByIdAsync(Guid id);
        Task<IEnumerable<SalesDocument>> GetByDealIdAsync(Guid dealId);
        Task<IEnumerable<SalesDocument>> GetByTypeAsync(DocumentType type);
        Task AddAsync(SalesDocument document);
        Task UpdateAsync(SalesDocument document);
        Task DeleteAsync(Guid id);
        Task<DocumentUploadResult> UploadDocumentAsync(SalesDocument document, IFormFile file);
        Task<bool> ProcessSignatureAsync(SalesDocument document, DocumentSignatureDto signatureDto);
        Task<IEnumerable<DocumentTemplateDto>> GetTemplatesAsync();
    }

    public class DocumentUploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FileUrl { get; set; }
    }
}
