using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Service for managing vehicle images
    /// </summary>
    public class VehicleImageService : IVehicleImageService
    {
        private readonly IRepository<VehicleImage> _imageRepository;
        private readonly IRepository<Vehicle> _vehicleRepository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VehicleImageService> _logger;
        private readonly string _containerName;

        public VehicleImageService(
            IRepository<VehicleImage> imageRepository,
            IRepository<Vehicle> vehicleRepository,
            IBlobStorageService blobStorageService,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<VehicleImageService> logger)
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _containerName = configuration["Storage:VehicleImages:ContainerName"] ?? "vehicle-images";
        }

        /// <inheritdoc />
        public async Task<List<VehicleImageResult>> UploadVehicleImagesAsync(Guid vehicleId, IFormFileCollection images)
        {
            _logger.LogInformation("Uploading {Count} images for vehicle {VehicleId}", images.Count, vehicleId);
            
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
            {
                throw new Exception($"Vehicle with ID {vehicleId} not found");
            }

            var results = new List<VehicleImageResult>();
            var existingImages = await _imageRepository.FindAsync(img => img.VehicleId == vehicleId);
            var isPrimaryNeeded = !existingImages.Any();

            foreach (var image in images)
            {
                try
                {
                    // Generate unique filename
                    var extension = Path.GetExtension(image.FileName);
                    var uniqueFileName = $"{vehicleId}_{Guid.NewGuid()}{extension}";
                    
                    // Upload original image
                    using var stream = image.OpenReadStream();
                    var blobUrl = await _blobStorageService.UploadAsync(_containerName, uniqueFileName, stream, image.ContentType);
                    
                    // Generate and upload thumbnail
                    var thumbnailUrl = await GenerateAndUploadThumbnail(image, vehicleId, uniqueFileName);
                    
                    // Create database record
                    var imageEntity = new VehicleImage
                    {
                        Id = Guid.NewGuid(),
                        VehicleId = vehicleId,
                        FileName = image.FileName,
                        ContentType = image.ContentType,
                        FileSize = image.Length,
                        Url = blobUrl,
                        ThumbnailUrl = thumbnailUrl,
                        IsPrimary = isPrimaryNeeded, // First image is primary if no images exist
                        UploadDate = DateTime.UtcNow,
                        Metadata = new Dictionary<string, string>
                        {
                            { "OriginalFileName", image.FileName },
                            { "UploadedBy", "System" } // Would normally get from authenticated user
                        }
                    };
                    
                    await _imageRepository.AddAsync(imageEntity);
                    
                    // Only the first image should be primary if we needed one
                    isPrimaryNeeded = false;
                    
                    results.Add(new VehicleImageResult
                    {
                        ImageId = imageEntity.Id,
                        Url = blobUrl,
                        ThumbnailUrl = thumbnailUrl,
                        FileName = image.FileName,
                        FileSize = image.Length,
                        ContentType = image.ContentType,
                        UploadDate = imageEntity.UploadDate,
                        IsPrimary = imageEntity.IsPrimary
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image {FileName} for vehicle {VehicleId}", image.FileName, vehicleId);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return results;
        }

        /// <inheritdoc />
        public async Task<List<VehicleImage>> GetVehicleImagesAsync(Guid vehicleId)
        {
            _logger.LogInformation("Getting images for vehicle {VehicleId}", vehicleId);
            
            var images = await _imageRepository.FindAsync(img => img.VehicleId == vehicleId);
            return images.OrderByDescending(img => img.IsPrimary)
                        .ThenBy(img => img.UploadDate)
                        .ToList();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteVehicleImageAsync(Guid vehicleId, Guid imageId)
        {
            _logger.LogInformation("Deleting image {ImageId} for vehicle {VehicleId}", imageId, vehicleId);
            
            var image = await _imageRepository.GetByIdAsync(imageId);
            
            if (image == null || image.VehicleId != vehicleId)
            {
                return false;
            }

            // Delete from blob storage
            var blobName = Path.GetFileName(image.Url);
            await _blobStorageService.DeleteAsync(_containerName, blobName);
            
            // Delete thumbnail if it exists
            if (!string.IsNullOrEmpty(image.ThumbnailUrl))
            {
                var thumbnailBlobName = Path.GetFileName(image.ThumbnailUrl);
                await _blobStorageService.DeleteAsync($"{_containerName}-thumbnails", thumbnailBlobName);
            }

            // Delete database record
            await _imageRepository.DeleteAsync(image);
            await _unitOfWork.SaveChangesAsync();

            // If this was the primary image, set a new one
            if (image.IsPrimary)
            {
                await SetNewPrimaryImageAsync(vehicleId);
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> SetPrimaryImageAsync(Guid vehicleId, Guid imageId)
        {
            _logger.LogInformation("Setting image {ImageId} as primary for vehicle {VehicleId}", imageId, vehicleId);
            
            var image = await _imageRepository.GetByIdAsync(imageId);
            
            if (image == null || image.VehicleId != vehicleId)
            {
                return false;
            }

            // Unset primary flag on all images for this vehicle
            var allImages = await _imageRepository.FindAsync(img => img.VehicleId == vehicleId);
            foreach (var img in allImages)
            {
                img.IsPrimary = false;
            }

            // Set primary flag on the selected image
            image.IsPrimary = true;
            
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc />
        public async Task<VehicleImage> UpdateImageMetadataAsync(Guid vehicleId, Guid imageId, Dictionary<string, string> metadata)
        {
            _logger.LogInformation("Updating metadata for image {ImageId}", imageId);
            
            var image = await _imageRepository.GetByIdAsync(imageId);
            
            if (image == null || image.VehicleId != vehicleId)
            {
                return null;
            }

            // Update metadata
            image.Metadata = metadata;
            
            await _unitOfWork.SaveChangesAsync();
            return image;
        }

        /// <summary>
        /// Generates a thumbnail from an image and uploads it to blob storage
        /// </summary>
        private async Task<string> GenerateAndUploadThumbnail(IFormFile image, Guid vehicleId, string uniqueFileName)
        {
            try
            {
                // Load the image using ImageSharp
                using var imageStream = image.OpenReadStream();
                using var imageSharp = await Image.LoadAsync(imageStream);
                
                // Resize image to create thumbnail (max 200px height or width while maintaining aspect ratio)
                imageSharp.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(200, 200),
                    Mode = ResizeMode.Max
                }));

                // Save the thumbnail to a memory stream
                using var thumbnailStream = new MemoryStream();
                await imageSharp.SaveAsync(thumbnailStream, imageSharp.Metadata.DecodedImageFormat);
                thumbnailStream.Position = 0;

                // Upload the thumbnail to blob storage
                var thumbnailFileName = $"thumb_{uniqueFileName}";
                return await _blobStorageService.UploadAsync(
                    $"{_containerName}-thumbnails",
                    thumbnailFileName,
                    thumbnailStream,
                    image.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thumbnail for image {FileName}", image.FileName);
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets a new primary image after the current one is deleted
        /// </summary>
        private async Task SetNewPrimaryImageAsync(Guid vehicleId)
        {
            var images = await _imageRepository.FindAsync(img => img.VehicleId == vehicleId);
            if (images.Any())
            {
                var firstImage = images.OrderBy(img => img.UploadDate).First();
                firstImage.IsPrimary = true;
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
