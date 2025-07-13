using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;
using DMS.PartsManagement.Core.Repositories;
using DMS.PartsManagement.Core.Services;

namespace DMS.PartsManagement.Infrastructure.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IPartRepository _partRepository;

        public SupplierService(
            ISupplierRepository supplierRepository,
            IPartRepository partRepository)
        {
            _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
        }

        public async Task<IEnumerable<SupplierSummaryDto>> GetAllSuppliersAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var suppliers = await _supplierRepository.GetAllAsync(skip, take, cancellationToken);
            return suppliers.Select(s => MapToSummaryDto(s));
        }

        public async Task<SupplierDetailDto?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id, cancellationToken);
            if (supplier == null)
            {
                return null;
            }

            return MapToDetailDto(supplier);
        }

        public async Task<SupplierDetailDto> CreateSupplierAsync(CreateSupplierDto createSupplierDto, CancellationToken cancellationToken = default)
        {
            var supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = createSupplierDto.Name,
                Type = Enum.Parse<SupplierType>(createSupplierDto.Type),
                AccountNumber = createSupplierDto.AccountNumber,
                ContactPerson = createSupplierDto.ContactPerson,
                Email = createSupplierDto.Email,
                Phone = createSupplierDto.Phone,
                Address = new Address
                {
                    Street = createSupplierDto.Address?.Street ?? string.Empty,
                    City = createSupplierDto.Address?.City ?? string.Empty,
                    State = createSupplierDto.Address?.State ?? string.Empty,
                    Zip = createSupplierDto.Address?.ZipCode ?? string.Empty,
                    Country = createSupplierDto.Address?.Country ?? string.Empty
                },
                Website = createSupplierDto.Website,
                ShippingTerms = createSupplierDto.ShippingTerms,
                PaymentTerms = createSupplierDto.PaymentTerms,
                OrderMethods = createSupplierDto.OrderMethods?.ToList() ?? new List<string>(),
                LeadTime = createSupplierDto.LeadTime,
                IsActive = createSupplierDto.Status?.ToLower() == "active",
                CreatedAt = DateTime.UtcNow
            };

            var createdSupplier = await _supplierRepository.AddAsync(supplier, cancellationToken);
            return MapToDetailDto(createdSupplier);
        }

        public async Task<SupplierDetailDto?> UpdateSupplierAsync(Guid id, UpdateSupplierDto updateSupplierDto, CancellationToken cancellationToken = default)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id, cancellationToken);
            if (supplier == null)
            {
                return null;
            }

            // Update supplier properties
            if (!string.IsNullOrEmpty(updateSupplierDto.Name))
                supplier.Name = updateSupplierDto.Name;

            if (!string.IsNullOrEmpty(updateSupplierDto.Type))
                supplier.Type = Enum.Parse<SupplierType>(updateSupplierDto.Type);

            if (!string.IsNullOrEmpty(updateSupplierDto.AccountNumber))
                supplier.AccountNumber = updateSupplierDto.AccountNumber;

            if (!string.IsNullOrEmpty(updateSupplierDto.ContactPerson))
                supplier.ContactPerson = updateSupplierDto.ContactPerson;

            if (!string.IsNullOrEmpty(updateSupplierDto.Email))
                supplier.Email = updateSupplierDto.Email;

            if (!string.IsNullOrEmpty(updateSupplierDto.Phone))
                supplier.Phone = updateSupplierDto.Phone;

            if (updateSupplierDto.Address != null)
            {
                supplier.Address.Street = updateSupplierDto.Address.Street ?? supplier.Address.Street;
                supplier.Address.City = updateSupplierDto.Address.City ?? supplier.Address.City;
                supplier.Address.State = updateSupplierDto.Address.State ?? supplier.Address.State;
                supplier.Address.Zip = updateSupplierDto.Address.ZipCode ?? supplier.Address.Zip;
                supplier.Address.Country = updateSupplierDto.Address.Country ?? supplier.Address.Country;
            }

            if (!string.IsNullOrEmpty(updateSupplierDto.Website))
                supplier.Website = updateSupplierDto.Website;

            if (!string.IsNullOrEmpty(updateSupplierDto.ShippingTerms))
                supplier.ShippingTerms = updateSupplierDto.ShippingTerms;

            if (!string.IsNullOrEmpty(updateSupplierDto.PaymentTerms))
                supplier.PaymentTerms = updateSupplierDto.PaymentTerms;

            if (updateSupplierDto.OrderMethods != null)
                supplier.OrderMethods = updateSupplierDto.OrderMethods.ToList();

            if (updateSupplierDto.LeadTime > 0)
                supplier.LeadTime = updateSupplierDto.LeadTime;

            if (!string.IsNullOrEmpty(updateSupplierDto.Status))
                supplier.IsActive = updateSupplierDto.Status.ToLower() == "active";

            supplier.UpdatedAt = DateTime.UtcNow;

            var updatedSupplier = await _supplierRepository.UpdateAsync(supplier, cancellationToken);
            return MapToDetailDto(updatedSupplier);
        }

        public async Task<bool> DeleteSupplierAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _supplierRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<bool> SupplierExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _supplierRepository.ExistsAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<PartSummaryDto>> GetPartsBySupplierId(Guid supplierId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            // This would require a query to get parts that have a relationship with this supplier
            // For now, we're assuming a simplified approach
            
            // We'd need to implement this based on how the relationship is modeled in the database
            // Placeholder implementation, should be replaced with actual implementation
            var parts = await _partRepository.GetPartsBySupplierIdAsync(supplierId, skip, take, cancellationToken);
            
            return parts.Select(p => new PartSummaryDto
            {
                Id = p.Id,
                PartNumber = p.PartNumber,
                Description = p.Description,
                CategoryName = p.Category?.Name,
                RetailPrice = p.Pricing?.RetailPrice ?? 0,
                IsActive = p.IsActive
            });
        }

        private SupplierSummaryDto MapToSummaryDto(Supplier supplier)
        {
            return new SupplierSummaryDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Type = supplier.Type.ToString(),
                AccountNumber = supplier.AccountNumber,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Status = supplier.IsActive ? "Active" : "Inactive"
            };
        }

        private SupplierDetailDto MapToDetailDto(Supplier supplier)
        {
            return new SupplierDetailDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Type = supplier.Type.ToString(),
                AccountNumber = supplier.AccountNumber,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Status = supplier.IsActive ? "Active" : "Inactive",
                Address = new AddressDto
                {
                    Street = supplier.Address.Street,
                    City = supplier.Address.City,
                    State = supplier.Address.State,
                    ZipCode = supplier.Address.Zip,
                    Country = supplier.Address.Country
                },
                Website = supplier.Website,
                ShippingTerms = supplier.ShippingTerms,
                PaymentTerms = supplier.PaymentTerms,
                OrderMethods = supplier.OrderMethods.ToArray(),
                LeadTime = supplier.LeadTime,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt ?? supplier.CreatedAt
            };
        }
    }
}
