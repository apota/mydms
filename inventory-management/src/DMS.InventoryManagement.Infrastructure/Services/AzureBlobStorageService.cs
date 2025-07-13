using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using DMS.InventoryManagement.Core.Exceptions;
using DMS.InventoryManagement.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Azure Blob Storage implementation of IBlobStorageService
    /// </summary>
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly string _baseUrl;

        public AzureBlobStorageService(
            IConfiguration configuration,
            ILogger<AzureBlobStorageService> logger)
        {
            _connectionString = configuration["Storage:ConnectionString"]
                ?? throw new ConfigurationException("Azure Storage connection string is not configured");
                
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _baseUrl = configuration["Storage:BaseUrl"];
        }

        /// <inheritdoc />
        public async Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType)
        {
            try
            {
                var containerClient = new BlobContainerClient(_connectionString, containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobClient = containerClient.GetBlobClient(blobName);
                content.Position = 0;
                
                await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });
                
                // Return the full URL to the blob
                if (!string.IsNullOrEmpty(_baseUrl))
                {
                    return $"{_baseUrl}/{containerName}/{blobName}";
                }
                
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading blob {BlobName} to container {ContainerName}", blobName, containerName);
                throw new Exception($"Error uploading blob: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<Stream> DownloadAsync(string containerName, string blobName)
        {
            try
            {
                var containerClient = new BlobContainerClient(_connectionString, containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning("Blob {BlobName} in container {ContainerName} not found", blobName, containerName);
                    return null;
                }
                
                var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;
                
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading blob {BlobName} from container {ContainerName}", blobName, containerName);
                throw new Exception($"Error downloading blob: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string containerName, string blobName)
        {
            try
            {
                var containerClient = new BlobContainerClient(_connectionString, containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blob {BlobName} from container {ContainerName}", blobName, containerName);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string containerName, string blobName)
        {
            try
            {
                var containerClient = new BlobContainerClient(_connectionString, containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                return await blobClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of blob {BlobName} in container {ContainerName}", blobName, containerName);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, string>> GetMetadataAsync(string containerName, string blobName)
        {
            try
            {
                var containerClient = new BlobContainerClient(_connectionString, containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                var properties = await blobClient.GetPropertiesAsync();
                return new Dictionary<string, string>(properties.Value.Metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata for blob {BlobName} in container {ContainerName}", blobName, containerName);
                throw new Exception($"Error getting blob metadata: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> SetMetadataAsync(string containerName, string blobName, Dictionary<string, string> metadata)
        {
            try
            {
                var containerClient = new BlobContainerClient(_connectionString, containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                await blobClient.SetMetadataAsync(metadata);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting metadata for blob {BlobName} in container {ContainerName}", blobName, containerName);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<string> GetSignedUrlAsync(string containerName, string blobName, TimeSpan expiryTime)
        {
            try
            {
                var containerClient = new BlobContainerClient(_connectionString, containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning("Blob {BlobName} in container {ContainerName} not found", blobName, containerName);
                    return string.Empty;
                }
                
                // Create a SAS token with read permissions that expires in the specified time
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = blobName,
                    Resource = "b", // b for blob
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiryTime)
                };
                
                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
                {
                    Sas = sasBuilder.ToSasQueryParameters(new Azure.Storage.StorageSharedKeyCredential(
                        accountName: containerClient.AccountName,
                        accountKey: _connectionString.Split(';')
                            .First(s => s.StartsWith("AccountKey="))
                            .Substring("AccountKey=".Length)))
                };
                
                return blobUriBuilder.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating signed URL for blob {BlobName} in container {ContainerName}", blobName, containerName);
                throw new Exception($"Error generating signed URL: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> CreateContainerIfNotExistsAsync(string containerName)
        {
            try
            {
                var containerClient = new BlobContainerClient(_connectionString, containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating container {ContainerName}", containerName);
                return false;
            }
        }
    }
}
