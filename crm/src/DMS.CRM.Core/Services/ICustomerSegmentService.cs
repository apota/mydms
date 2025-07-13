using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;

namespace DMS.CRM.Core.Services
{
    public interface ICustomerSegmentService
    {
        Task<IEnumerable<CustomerSegmentDto>> GetAllSegmentsAsync(int skip = 0, int take = 50);
        Task<CustomerSegmentDto> GetSegmentByIdAsync(Guid id);
        Task<CustomerSegmentDto> CreateSegmentAsync(CustomerSegmentCreateDto segmentDto);
        Task<CustomerSegmentDto> UpdateSegmentAsync(Guid id, CustomerSegmentUpdateDto segmentDto);
        Task<bool> DeleteSegmentAsync(Guid id);
        Task<int> CalculateSegmentSizeAsync(Guid id);
        Task<IEnumerable<CustomerDto>> GetCustomersInSegmentAsync(Guid segmentId, int skip = 0, int take = 50);
        Task<bool> AddCustomerToSegmentAsync(Guid segmentId, Guid customerId);
        Task<bool> RemoveCustomerFromSegmentAsync(Guid segmentId, Guid customerId);
        Task<IEnumerable<CustomerSegmentDto>> GetSegmentsByCriteria(Dictionary<string, object> criteria);
        Task<IEnumerable<CampaignDto>> GetCampaignsBySegmentAsync(Guid segmentId);
    }
}
