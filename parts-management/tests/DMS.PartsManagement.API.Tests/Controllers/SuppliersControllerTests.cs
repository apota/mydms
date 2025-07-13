using DMS.PartsManagement.API.Controllers;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DMS.PartsManagement.API.Tests.Controllers
{
    public class SuppliersControllerTests
    {
        private readonly Mock<ISupplierService> _mockSupplierService;
        private readonly Mock<IPartService> _mockPartService;
        private readonly Mock<ILogger<SuppliersController>> _mockLogger;
        private readonly SuppliersController _controller;

        public SuppliersControllerTests()
        {
            _mockSupplierService = new Mock<ISupplierService>();
            _mockPartService = new Mock<IPartService>();
            _mockLogger = new Mock<ILogger<SuppliersController>>();
            _controller = new SuppliersController(_mockSupplierService.Object, _mockPartService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllSuppliers_ReturnsOkResult_WithSuppliers()
        {
            // Arrange
            var expectedSuppliers = new List<SupplierSummaryDto>
            {
                new SupplierSummaryDto
                {
                    Id = Guid.NewGuid(),
                    Name = "ABC Auto Parts",
                    ContactName = "John Smith",
                    PhoneNumber = "123-456-7890"
                },
                new SupplierSummaryDto
                {
                    Id = Guid.NewGuid(),
                    Name = "XYZ Parts Distributors",
                    ContactName = "Jane Doe",
                    PhoneNumber = "987-654-3210"
                }
            };

            _mockSupplierService.Setup(s => s.GetAllSuppliersAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSuppliers);

            // Act
            var result = await _controller.GetAllSuppliers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSuppliers = Assert.IsAssignableFrom<IEnumerable<SupplierSummaryDto>>(okResult.Value);
            Assert.Equal(2, returnedSuppliers.Count());
            Assert.Equal(expectedSuppliers, returnedSuppliers);
        }

        [Fact]
        public async Task GetSupplierById_WithValidId_ReturnsOkResult_WithSupplier()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var expectedSupplier = new SupplierDetailDto
            {
                Id = supplierId,
                Name = "ABC Auto Parts",
                ContactName = "John Smith",
                PhoneNumber = "123-456-7890",
                Email = "john@abcautoparts.com",
                Address = "123 Main St, Anytown, USA",
                Website = "www.abcautoparts.com",
                AccountNumber = "ABCP1234",
                Notes = "Preferred supplier for brake components"
            };

            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(
                It.Is<Guid>(id => id == supplierId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSupplier);

            // Act
            var result = await _controller.GetSupplierById(supplierId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSupplier = Assert.IsType<SupplierDetailDto>(okResult.Value);
            Assert.Equal(supplierId, returnedSupplier.Id);
            Assert.Equal(expectedSupplier.Name, returnedSupplier.Name);
            Assert.Equal(expectedSupplier.ContactName, returnedSupplier.ContactName);
        }

        [Fact]
        public async Task GetSupplierById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(
                It.Is<Guid>(id => id == supplierId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SupplierDetailDto)null);

            // Act
            var result = await _controller.GetSupplierById(supplierId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPartsBySupplier_WithValidId_ReturnsOkResult_WithParts()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var expectedParts = new List<PartSummaryDto>
            {
                new PartSummaryDto
                {
                    Id = Guid.NewGuid(),
                    PartNumber = "ABC123",
                    Description = "Brake Pad"
                },
                new PartSummaryDto
                {
                    Id = Guid.NewGuid(),
                    PartNumber = "DEF456",
                    Description = "Air Filter"
                }
            };

            _mockSupplierService.Setup(s => s.SupplierExistsAsync(
                It.Is<Guid>(id => id == supplierId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockPartService.Setup(s => s.GetPartsBySupplierIdAsync(
                It.Is<Guid>(id => id == supplierId),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedParts);

            // Act
            var result = await _controller.GetPartsBySupplier(supplierId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedParts = Assert.IsAssignableFrom<IEnumerable<PartSummaryDto>>(okResult.Value);
            Assert.Equal(2, returnedParts.Count());
            Assert.Equal(expectedParts, returnedParts);
        }

        [Fact]
        public async Task GetPartsBySupplier_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            _mockSupplierService.Setup(s => s.SupplierExistsAsync(
                It.Is<Guid>(id => id == supplierId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.GetPartsBySupplier(supplierId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateSupplier_WithValidDto_ReturnsCreatedAtAction()
        {
            // Arrange
            var createSupplierDto = new CreateSupplierDto
            {
                Name = "New Supplier Inc.",
                ContactName = "Bob Johnson",
                PhoneNumber = "555-123-4567",
                Email = "bob@newsupplier.com",
                Address = "456 Business Ave, Commerce City, USA",
                AccountNumber = "NS789"
            };

            var createdSupplier = new SupplierDetailDto
            {
                Id = Guid.NewGuid(),
                Name = createSupplierDto.Name,
                ContactName = createSupplierDto.ContactName,
                PhoneNumber = createSupplierDto.PhoneNumber,
                Email = createSupplierDto.Email,
                Address = createSupplierDto.Address,
                AccountNumber = createSupplierDto.AccountNumber
            };

            _mockSupplierService.Setup(s => s.CreateSupplierAsync(
                It.IsAny<CreateSupplierDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdSupplier);

            // Act
            var result = await _controller.CreateSupplier(createSupplierDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(SuppliersController.GetSupplierById), createdAtActionResult.ActionName);
            Assert.Equal(createdSupplier.Id, createdAtActionResult.RouteValues["id"]);
            var returnedSupplier = Assert.IsType<SupplierDetailDto>(createdAtActionResult.Value);
            Assert.Equal(createSupplierDto.Name, returnedSupplier.Name);
        }

        [Fact]
        public async Task UpdateSupplier_WithValidData_ReturnsOkResult_WithUpdatedSupplier()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var updateSupplierDto = new UpdateSupplierDto
            {
                ContactName = "Updated Contact",
                Email = "new@email.com",
                Notes = "Updated supplier notes"
            };

            var updatedSupplier = new SupplierDetailDto
            {
                Id = supplierId,
                Name = "ABC Auto Parts",
                ContactName = updateSupplierDto.ContactName,
                Email = updateSupplierDto.Email,
                Notes = updateSupplierDto.Notes
            };

            _mockSupplierService.Setup(s => s.SupplierExistsAsync(
                It.Is<Guid>(id => id == supplierId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockSupplierService.Setup(s => s.UpdateSupplierAsync(
                It.Is<Guid>(id => id == supplierId), 
                It.IsAny<UpdateSupplierDto>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedSupplier);

            // Act
            var result = await _controller.UpdateSupplier(supplierId, updateSupplierDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSupplier = Assert.IsType<SupplierDetailDto>(okResult.Value);
            Assert.Equal(supplierId, returnedSupplier.Id);
            Assert.Equal(updateSupplierDto.ContactName, returnedSupplier.ContactName);
            Assert.Equal(updateSupplierDto.Email, returnedSupplier.Email);
            Assert.Equal(updateSupplierDto.Notes, returnedSupplier.Notes);
        }

        [Fact]
        public async Task DeleteSupplier_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            _mockSupplierService.Setup(s => s.SupplierExistsAsync(
                It.Is<Guid>(id => id == supplierId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockSupplierService.Setup(s => s.DeleteSupplierAsync(
                It.Is<Guid>(id => id == supplierId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteSupplier(supplierId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task SearchSuppliers_ReturnsOkResult_WithMatchingSuppliers()
        {
            // Arrange
            var searchTerm = "auto";
            var expectedSuppliers = new List<SupplierSummaryDto>
            {
                new SupplierSummaryDto
                {
                    Id = Guid.NewGuid(),
                    Name = "ABC Auto Parts",
                    ContactName = "John Smith",
                    PhoneNumber = "123-456-7890"
                },
                new SupplierSummaryDto
                {
                    Id = Guid.NewGuid(),
                    Name = "East Coast Auto Supply",
                    ContactName = "Mike Brown",
                    PhoneNumber = "555-987-6543"
                }
            };

            _mockSupplierService.Setup(s => s.SearchSuppliersAsync(
                It.Is<string>(term => term == searchTerm), 
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSuppliers);

            // Act
            var result = await _controller.SearchSuppliers(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSuppliers = Assert.IsAssignableFrom<IEnumerable<SupplierSummaryDto>>(okResult.Value);
            Assert.Equal(2, returnedSuppliers.Count());
        }
    }
}
