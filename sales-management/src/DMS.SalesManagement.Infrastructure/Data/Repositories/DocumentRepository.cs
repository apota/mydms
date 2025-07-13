using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using DMS.SalesManagement.Core.DTOs;
using DMS.SalesManagement.Core.Models;
using DMS.SalesManagement.Core.Repositories;
using DMS.SalesManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DMS.SalesManagement.Infrastructure.Data.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly SalesDbContext _context;
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        
        public DocumentRepository(SalesDbContext context, IAmazonS3 s3Client, IConfiguration configuration)
        {
            _context = context;
            _s3Client = s3Client;
            _bucketName = configuration.GetValue<string>("AWS:DocumentBucketName");
        }

        public async Task<SalesDocument> GetByIdAsync(Guid id)
        {
            return await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<SalesDocument>> GetByDealIdAsync(Guid dealId)
        {
            return await _context.Documents
                .Where(d => d.DealId == dealId)
                .ToListAsync();
        }

        public async Task<IEnumerable<SalesDocument>> GetByTypeAsync(DocumentType type)
        {
            return await _context.Documents
                .Where(d => d.Type == type)
                .ToListAsync();
        }

        public async Task AddAsync(SalesDocument document)
        {
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SalesDocument document)
        {
            _context.Entry(document).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DocumentUploadResult> UploadDocumentAsync(SalesDocument document, IFormFile file)
        {
            try
            {
                var fileKey = $"{document.DealId}/{document.Type}/{Guid.NewGuid()}_{file.FileName}";
                
                using var fileStream = file.OpenReadStream();
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey,
                    InputStream = fileStream,
                    ContentType = file.ContentType
                };
                
                // Add metadata
                putRequest.Metadata.Add("dealId", document.DealId.ToString());
                putRequest.Metadata.Add("documentType", document.Type.ToString());
                
                await _s3Client.PutObjectAsync(putRequest);
                
                var fileUrl = $"https://{_bucketName}.s3.amazonaws.com/{fileKey}";
                
                return new DocumentUploadResult
                {
                    Success = true,
                    FileUrl = fileUrl
                };
            }
            catch (Exception ex)
            {
                return new DocumentUploadResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<bool> ProcessSignatureAsync(SalesDocument document, DocumentSignatureDto signatureDto)
        {
            try
            {
                var signature = document.RequiredSignatures?.FirstOrDefault(s => s.Role == signatureDto.Role);
                
                if (signature == null)
                {
                    if (document.RequiredSignatures == null)
                        document.RequiredSignatures = new List<RequiredSignature>();
                    
                    document.RequiredSignatures.Add(new RequiredSignature
                    {
                        Role = signatureDto.Role,
                        Name = signatureDto.Name,
                        Status = SignatureStatus.Signed,
                        SignedDate = DateTime.UtcNow
                    });
                }
                else
                {
                    signature.Status = SignatureStatus.Signed;
                    signature.SignedDate = DateTime.UtcNow;
                }
                
                // Check if all signatures are complete
                if (document.RequiredSignatures?.All(s => s.Status == SignatureStatus.Signed) ?? false)
                {
                    document.Status = DocumentStatus.Complete;
                }
                
                document.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(document);
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<DocumentTemplateDto>> GetTemplatesAsync()
        {
            // In a real implementation, this would likely be stored in the database
            // or retrieved from a template service
            return await Task.FromResult(new List<DocumentTemplateDto>
            {
                new DocumentTemplateDto
                {
                    Id = "sales-agreement",
                    Name = "Sales Agreement",
                    Type = DocumentType.PurchaseOrder.ToString(),
                    Description = "Standard vehicle sales agreement"
                },
                new DocumentTemplateDto
                {
                    Id = "finance-application",
                    Name = "Finance Application",
                    Type = DocumentType.FinanceApplication.ToString(),
                    Description = "Vehicle financing application form"
                },
                new DocumentTemplateDto
                {
                    Id = "insurance-verification",
                    Name = "Insurance Verification",
                    Type = DocumentType.Insurance.ToString(),
                    Description = "Vehicle insurance verification form"
                },
                new DocumentTemplateDto
                {
                    Id = "delivery-checklist",
                    Name = "Delivery Checklist",
                    Type = DocumentType.Other.ToString(),
                    Description = "Vehicle delivery inspection checklist"
                }
            });
        }
    }
}
