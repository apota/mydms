using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DMS.SalesManagement.Core.DTOs;
using DMS.SalesManagement.Core.Models;
using DMS.SalesManagement.Core.Repositories;

namespace DMS.SalesManagement.API.Controllers
{
    [ApiController]
    [Route("api/sales/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentRepository _documentRepository;

        public DocumentsController(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        /// <summary>
        /// Gets all documents for a deal
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <returns>A collection of documents</returns>
        [HttpGet("{dealId}")]
        public async Task<ActionResult<IEnumerable<SalesDocumentDto>>> GetDealDocuments(Guid dealId)
        {
            var documents = await _documentRepository.GetByDealIdAsync(dealId);
            
            var documentDtos = new List<SalesDocumentDto>();
            foreach (var doc in documents)
            {
                documentDtos.Add(new SalesDocumentDto
                {
                    Id = doc.Id,
                    DealId = doc.DealId,
                    Type = doc.Type.ToString(),
                    Name = doc.Name,
                    Filename = doc.Filename,
                    Location = doc.Location,
                    Status = doc.Status.ToString(),
                    RequiredSignatures = doc.RequiredSignatures,
                    CreatedAt = doc.CreatedAt,
                    UpdatedAt = doc.UpdatedAt
                });
            }

            return documentDtos;
        }

        /// <summary>
        /// Uploads a new document
        /// </summary>
        /// <param name="documentDto">The document data</param>
        /// <returns>The created document</returns>
        [HttpPost]
        public async Task<ActionResult<SalesDocumentDto>> UploadDocument([FromForm] SalesDocumentCreateDto documentDto)
        {
            if (documentDto.File == null || documentDto.File.Length == 0)
                return BadRequest("File is required");

            var document = new SalesDocument
            {
                Id = Guid.NewGuid(),
                DealId = documentDto.DealId,
                Type = Enum.Parse<DocumentType>(documentDto.Type),
                Name = documentDto.Name,
                Filename = documentDto.File.FileName,
                Status = DocumentStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Upload file to S3 and save metadata
            var uploadResult = await _documentRepository.UploadDocumentAsync(document, documentDto.File);
            
            if (!uploadResult.Success)
                return StatusCode(500, "Failed to upload document: " + uploadResult.Message);

            document.Location = uploadResult.FileUrl;
            await _documentRepository.AddAsync(document);

            var resultDto = new SalesDocumentDto
            {
                Id = document.Id,
                DealId = document.DealId,
                Type = document.Type.ToString(),
                Name = document.Name,
                Filename = document.Filename,
                Location = document.Location,
                Status = document.Status.ToString(),
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt
            };

            return CreatedAtAction(nameof(GetDealDocuments), new { dealId = document.DealId }, resultDto);
        }

        /// <summary>
        /// Process document signature
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="signatureDto">Signature information</param>
        /// <returns>Updated document</returns>
        [HttpPost("{id}/sign")]
        public async Task<ActionResult<SalesDocumentDto>> SignDocument(Guid id, DocumentSignatureDto signatureDto)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
                return NotFound();

            var signatureResult = await _documentRepository.ProcessSignatureAsync(document, signatureDto);
            if (!signatureResult)
                return BadRequest("Failed to process signature");

            var resultDto = new SalesDocumentDto
            {
                Id = document.Id,
                DealId = document.DealId,
                Type = document.Type.ToString(),
                Name = document.Name,
                Filename = document.Filename,
                Location = document.Location,
                Status = document.Status.ToString(),
                RequiredSignatures = document.RequiredSignatures,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt
            };

            return resultDto;
        }

        /// <summary>
        /// Gets available document templates
        /// </summary>
        /// <returns>List of available templates</returns>
        [HttpGet("templates")]
        public async Task<ActionResult<IEnumerable<DocumentTemplateDto>>> GetDocumentTemplates()
        {
            var templates = await _documentRepository.GetTemplatesAsync();
            return Ok(templates);
        }
    }
}
