using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Interface for blob storage operations
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads a blob to storage
        /// </summary>
        /// <param name="containerName">The container name</param>
        /// <param name="blobName">The blob name</param>
        /// <param name="content">The content stream</param>
        /// <param name="contentType">The content type</param>
        /// <returns>The URL of the uploaded blob</returns>
        Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType);
        
        /// <summary>
        /// Downloads a blob from storage
        /// </summary>
        /// <param name="containerName">The container name</param>
        /// <param name="blobName">The blob name</param>
        /// <returns>The blob content stream</returns>
        Task<Stream> DownloadAsync(string containerName, string blobName);
        
        /// <summary>
        /// Deletes a blob from storage
        /// </summary>
        /// <param name="containerName">The container name</param>
        /// <param name="blobName">The blob name</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteAsync(string containerName, string blobName);
        
        /// <summary>
        /// Checks if a blob exists
        /// </summary>
        /// <param name="containerName">The container name</param>
        /// <param name="blobName">The blob name</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> ExistsAsync(string containerName, string blobName);
        
        /// <summary>
        /// Gets blob metadata
        /// </summary>
        /// <param name="containerName">The container name</param>
        /// <param name="blobName">The blob name</param>
        /// <returns>Dictionary of metadata key-value pairs</returns>
        Task<Dictionary<string, string>> GetMetadataAsync(string containerName, string blobName);
        
        /// <summary>
        /// Updates blob metadata
        /// </summary>
        /// <param name="containerName">The container name</param>
        /// <param name="blobName">The blob name</param>
        /// <param name="metadata">Dictionary of metadata key-value pairs</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SetMetadataAsync(string containerName, string blobName, Dictionary<string, string> metadata);
        
        /// <summary>
        /// Gets a signed URL for temporary access to a blob
        /// </summary>
        /// <param name="containerName">The container name</param>
        /// <param name="blobName">The blob name</param>
        /// <param name="expiryTime">The expiry time for the signed URL</param>
        /// <returns>The signed URL</returns>
        Task<string> GetSignedUrlAsync(string containerName, string blobName, TimeSpan expiryTime);
        
        /// <summary>
        /// Creates a container if it doesn't exist
        /// </summary>
        /// <param name="containerName">The container name</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> CreateContainerIfNotExistsAsync(string containerName);
    }
}
