using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using DMS.InventoryManagement.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the document storage service using AWS S3
    /// </summary>
    public class S3DocumentStorageService : IDocumentStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _baseFolder;
        private readonly ILogger<S3DocumentStorageService> _logger;

        public S3DocumentStorageService(
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<S3DocumentStorageService> logger)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:S3:DocumentBucketName"] ?? throw new ArgumentNullException("S3 bucket name is not configured");
            _baseFolder = configuration["AWS:S3:DocumentBaseFolder"] ?? "documents";
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<string> UploadDocumentAsync(IFormFile file, string fileName)
        {
            try
            {
                // Ensure the file name doesn't contain invalid characters
                fileName = SanitizeFileName(fileName);
                
                // Generate the S3 key/path
                var key = $"{_baseFolder}/{DateTime.UtcNow:yyyy/MM/dd}/{fileName}";
                
                using var fileStream = file.OpenReadStream();
                var transferUtility = new TransferUtility(_s3Client);
                
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = fileStream,
                    BucketName = _bucketName,
                    Key = key,
                    ContentType = file.ContentType
                };

                await transferUtility.UploadAsync(uploadRequest);
                _logger.LogInformation("Uploaded document {FileName} to S3 path {Key}", fileName, key);
                
                return key;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document {FileName} to S3", fileName);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteDocumentAsync(string filePath)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = filePath
                };
                
                await _s3Client.DeleteObjectAsync(deleteRequest);
                _logger.LogInformation("Deleted document from S3 path {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document from S3 path {FilePath}", filePath);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> GetDocumentUrlAsync(string filePath, int expirationMinutes = 60)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = filePath,
                    Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
                };
                
                var url = await Task.FromResult(_s3Client.GetPreSignedURL(request));
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating pre-signed URL for document {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Sanitizes a filename for S3 storage
        /// </summary>
        /// <param name="fileName">The input filename</param>
        /// <returns>A sanitized filename</returns>
        private static string SanitizeFileName(string fileName)
        {
            // Replace invalid characters with underscores
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }

            // Replace spaces with dashes
            fileName = fileName.Replace(' ', '-');
            
            // Remove consecutive dashes or underscores
            while (fileName.Contains("--"))
            {
                fileName = fileName.Replace("--", "-");
            }
            
            while (fileName.Contains("__"))
            {
                fileName = fileName.Replace("__", "_");
            }

            return fileName;
        }
    }
}
