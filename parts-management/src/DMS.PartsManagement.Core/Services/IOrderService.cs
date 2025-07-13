using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<PartOrderSummaryDto>> GetAllOrdersAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartOrderDetailDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartOrderSummaryDto>> GetOrdersByStatusAsync(OrderStatus status, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartOrderSummaryDto>> GetOrdersBySupplierIdAsync(Guid supplierId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartOrderDetailDto> CreateOrderAsync(CreatePartOrderDto createOrderDto, CancellationToken cancellationToken = default);
        Task<PartOrderDetailDto?> UpdateOrderAsync(Guid id, UpdatePartOrderDto updateOrderDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> OrderExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> SubmitOrderAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PartOrderReceiptDto> ReceiveOrderAsync(PartOrderReceiveDto receiveDto, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartOrderLineDto>> GetOrderLinesAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ReorderRecommendationDto>> GenerateReorderRecommendationsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<PartOrderSummaryDto>> GetSpecialOrdersAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    }
}
