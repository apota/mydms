using Microsoft.AspNetCore.Http;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Service for handling document storage
    /// </summary>
    public interface IDocumentStorageService
    {
        /// <summary>
        /// Uploads a document to storage
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="fileName">The name to save the file as</param>
        /// <returns>The file path in storage</returns>
        Task<string> UploadDocumentAsync(IFormFile file, string fileName);

        /// <summary>
        /// Deletes a document from storage
        /// </summary>
        /// <param name="filePath">The file path in storage</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task DeleteDocumentAsync(string filePath);

        /// <summary>
        /// Gets a document download URL
        /// </summary>
        /// <param name="filePath">The file path in storage</param>
        /// <param name="expirationMinutes">The expiration time in minutes</param>
        /// <returns>The download URL</returns>
        Task<string> GetDocumentUrlAsync(string filePath, int expirationMinutes = 60);
    }
}
