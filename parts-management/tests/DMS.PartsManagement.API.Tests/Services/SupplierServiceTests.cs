using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;
using DMS.PartsManagement.Core.Repositories;
using DMS.PartsManagement.Infrastructure.Services;
using Moq;
using Xunit;

namespace DMS.PartsManagement.API.Tests.Services
{
    public class SupplierServiceTests
    {
        private readonly Mock<ISupplierRepository> _mockSupplierRepo;
        private readonly Mock<IPartRepository> _mockPartRepo;
        private readonly SupplierService _service;

        public SupplierServiceTests()
        {
            _mockSupplierRepo = new Mock<ISupplierRepository>();
            _mockPartRepo = new Mock<IPartRepository>();
            _service = new SupplierService(_mockSupplierRepo.Object, _mockPartRepo.Object);
        }

        [Fact]
        public async Task GetAllSuppliersAsync_ShouldReturnSuppliers()
        {
            // Arrange
            var suppliers = new List<Supplier>
            {
                new Supplier { Id = Guid.NewGuid(), Name = "Supplier 1", Type = SupplierType.Manufacturer },
                new Supplier { Id = Guid.NewGuid(), Name = "Supplier 2", Type = SupplierType.Distributor }
            };

            _mockSupplierRepo.Setup(repo => repo.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(suppliers);

            // Act
            var result = await _service.GetAllSuppliersAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, s => s.Name == "Supplier 1");
            Assert.Contains(result, s => s.Name == "Supplier 2");
        }

        [Fact]
        public async Task GetSupplierByIdAsync_WithValidId_ShouldReturnSupplier()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var supplier = new Supplier 
            { 
                Id = supplierId, 
                Name = "Test Supplier", 
                Type = SupplierType.Manufacturer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockSupplierRepo.Setup(repo => repo.GetByIdAsync(
                It.Is<Guid>(id => id == supplierId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(supplier);

            // Act
            var result = await _service.GetSupplierByIdAsync(supplierId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(supplierId, result.Id);
            Assert.Equal("Test Supplier", result.Name);
            Assert.Equal("Manufacturer", result.Type);
            Assert.Equal("Active", result.Status);
        }

        [Fact]
        public async Task CreateSupplierAsync_ShouldCreateAndReturnSupplier()
        {
            // Arrange
            var createDto = new CreateSupplierDto
            {
                Name = "New Supplier",
                Type = "Distributor",
                AccountNumber = "ABC123",
                Status = "Active"
            };

            _mockSupplierRepo.Setup(repo => repo.AddAsync(
                It.IsAny<Supplier>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Supplier s, CancellationToken _) => s);

            // Act
            var result = await _service.CreateSupplierAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Supplier", result.Name);
            Assert.Equal("Distributor", result.Type);
            Assert.Equal("ABC123", result.AccountNumber);
            Assert.Equal("Active", result.Status);

            _mockSupplierRepo.Verify(repo => repo.AddAsync(
                It.Is<Supplier>(s => 
                    s.Name == "New Supplier" && 
                    s.Type == SupplierType.Distributor && 
                    s.AccountNumber == "ABC123" && 
                    s.IsActive == true),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSupplierAsync_WithValidId_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            
            _mockSupplierRepo.Setup(repo => repo.DeleteAsync(
                It.Is<Guid>(id => id == supplierId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteSupplierAsync(supplierId);

            // Assert
            Assert.True(result);
            _mockSupplierRepo.Verify(repo => repo.DeleteAsync(supplierId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
