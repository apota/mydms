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
    public class OrderService : IOrderService
    {
        private readonly IPartOrderRepository _orderRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IPartRepository _partRepository;
        private readonly IPartInventoryRepository _inventoryRepository;

        public OrderService(
            IPartOrderRepository orderRepository,
            ISupplierRepository supplierRepository,
            IPartRepository partRepository,
            IPartInventoryRepository inventoryRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
        }

        public async Task<IEnumerable<PartOrderSummaryDto>> GetAllOrdersAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository.GetAllAsync(skip, take, cancellationToken);
            return orders.Select(o => MapToSummaryDto(o));
        }

        public async Task<PartOrderDetailDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
            {
                return null;
            }

            return await MapToDetailDtoAsync(order, cancellationToken);
        }

        public async Task<IEnumerable<PartOrderSummaryDto>> GetOrdersByStatusAsync(OrderStatus status, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository.GetByStatusAsync(status, skip, take, cancellationToken);
            return orders.Select(o => MapToSummaryDto(o));
        }

        public async Task<IEnumerable<PartOrderSummaryDto>> GetOrdersBySupplierIdAsync(Guid supplierId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository.GetBySupplierAsync(supplierId, skip, take, cancellationToken);
            return orders.Select(o => MapToSummaryDto(o));
        }

        public async Task<PartOrderDetailDto> CreateOrderAsync(CreatePartOrderDto createOrderDto, CancellationToken cancellationToken = default)
        {
            // Validate supplier exists
            var supplierExists = await _supplierRepository.ExistsAsync(createOrderDto.SupplierId, cancellationToken);
            if (!supplierExists)
            {
                throw new KeyNotFoundException($"Supplier with ID {createOrderDto.SupplierId} not found.");
            }

            // Create a unique order number
            string orderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            var order = new PartOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                SupplierId = createOrderDto.SupplierId,
                OrderDate = createOrderDto.OrderDate,
                ExpectedReceiveDate = createOrderDto.ExpectedReceiveDate,
                Status = OrderStatus.Draft,
                OrderType = Enum.Parse<OrderType>(createOrderDto.OrderType),
                RequestorId = createOrderDto.RequestorId,
                ShippingMethod = createOrderDto.ShippingMethod,
                TrackingNumber = createOrderDto.TrackingNumber,
                ShippingCost = createOrderDto.ShippingCost,
                TaxAmount = createOrderDto.TaxAmount,
                Notes = createOrderDto.Notes,
                CreatedAt = DateTime.UtcNow,
                OrderLines = new List<PartOrderLine>()
            };

            // Process order lines
            decimal subtotal = 0;
            foreach (var line in createOrderDto.OrderLines)
            {
                var part = await _partRepository.GetByIdAsync(line.PartId, cancellationToken);
                if (part == null)
                {
                    throw new KeyNotFoundException($"Part with ID {line.PartId} not found.");
                }

                decimal unitCost = line.UnitCost ?? part.Pricing?.CostPrice ?? 0;
                decimal extendedCost = unitCost * line.Quantity;
                subtotal += extendedCost;

                var orderLine = new PartOrderLine
                {
                    Id = Guid.NewGuid(),
                    PartOrderId = order.Id,
                    PartId = line.PartId,
                    Quantity = line.Quantity,
                    UnitCost = unitCost,
                    ExtendedCost = extendedCost,
                    Status = OrderLineStatus.Ordered,
                    ReceivedQuantity = 0,
                    CreatedAt = DateTime.UtcNow
                };

                if (line.Allocation != null)
                {
                    orderLine.Allocation = new AllocationInfo
                    {
                        Type = Enum.Parse<AllocationType>(line.Allocation.Type),
                        ReferenceId = line.Allocation.ReferenceId,
                        ReferenceType = line.Allocation.ReferenceType
                    };
                }

                order.OrderLines.Add(orderLine);
            }

            order.Subtotal = subtotal;
            order.TotalAmount = subtotal + order.ShippingCost + order.TaxAmount;

            var createdOrder = await _orderRepository.AddAsync(order, cancellationToken);
            return await MapToDetailDtoAsync(createdOrder, cancellationToken);
        }

        public async Task<PartOrderDetailDto?> UpdateOrderAsync(Guid id, UpdatePartOrderDto updateOrderDto, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
            {
                return null;
            }

            // Can't update submitted or completed orders
            if (order.Status != OrderStatus.Draft)
            {
                throw new InvalidOperationException($"Cannot update order {id} with status {order.Status}. Only draft orders can be updated.");
            }

            if (updateOrderDto.ExpectedReceiveDate.HasValue)
                order.ExpectedReceiveDate = updateOrderDto.ExpectedReceiveDate;

            if (!string.IsNullOrWhiteSpace(updateOrderDto.ShippingMethod))
                order.ShippingMethod = updateOrderDto.ShippingMethod;

            if (!string.IsNullOrWhiteSpace(updateOrderDto.TrackingNumber))
                order.TrackingNumber = updateOrderDto.TrackingNumber;

            if (updateOrderDto.ShippingCost.HasValue)
                order.ShippingCost = updateOrderDto.ShippingCost.Value;

            if (updateOrderDto.TaxAmount.HasValue)
                order.TaxAmount = updateOrderDto.TaxAmount.Value;

            if (!string.IsNullOrWhiteSpace(updateOrderDto.Notes))
                order.Notes = updateOrderDto.Notes;

            // Update order lines if provided
            if (updateOrderDto.OrderLines != null && updateOrderDto.OrderLines.Count > 0)
            {
                decimal subtotal = 0;

                foreach (var updateLine in updateOrderDto.OrderLines)
                {
                    // Find the existing line or ignore if not found
                    var orderLine = order.OrderLines.FirstOrDefault(ol => ol.Id == updateLine.Id);
                    if (orderLine == null) continue;

                    if (updateLine.Quantity.HasValue)
                        orderLine.Quantity = updateLine.Quantity.Value;

                    if (updateLine.UnitCost.HasValue)
                        orderLine.UnitCost = updateLine.UnitCost.Value;

                    orderLine.ExtendedCost = orderLine.UnitCost * orderLine.Quantity;
                    
                    if (updateLine.Allocation != null)
                    {
                        orderLine.Allocation ??= new AllocationInfo();
                        orderLine.Allocation.Type = Enum.Parse<AllocationType>(updateLine.Allocation.Type);
                        orderLine.Allocation.ReferenceId = updateLine.Allocation.ReferenceId;
                        orderLine.Allocation.ReferenceType = updateLine.Allocation.ReferenceType;
                    }

                    orderLine.UpdatedAt = DateTime.UtcNow;
                }

                // Recalculate subtotal
                subtotal = order.OrderLines.Sum(ol => ol.ExtendedCost);
                order.Subtotal = subtotal;
                order.TotalAmount = subtotal + order.ShippingCost + order.TaxAmount;
            }

            order.UpdatedAt = DateTime.UtcNow;

            var updatedOrder = await _orderRepository.UpdateAsync(order, cancellationToken);
            return await MapToDetailDtoAsync(updatedOrder, cancellationToken);
        }

        public async Task<bool> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
            {
                return false;
            }

            // Can only delete draft orders
            if (order.Status != OrderStatus.Draft)
            {
                throw new InvalidOperationException($"Cannot delete order {id} with status {order.Status}. Only draft orders can be deleted.");
            }

            return await _orderRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<bool> OrderExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _orderRepository.ExistsAsync(id, cancellationToken);
        }

        public async Task<bool> SubmitOrderAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
            {
                return false;
            }

            // Can only submit draft orders
            if (order.Status != OrderStatus.Draft)
            {
                throw new InvalidOperationException($"Cannot submit order {id} with status {order.Status}. Only draft orders can be submitted.");
            }

            // Ensure order has lines
            if (order.OrderLines == null || !order.OrderLines.Any())
            {
                throw new InvalidOperationException($"Cannot submit order {id} with no order lines.");
            }

            order.Status = OrderStatus.Submitted;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order, cancellationToken);
            return true;
        }

        public async Task<PartOrderReceiptDto> ReceiveOrderAsync(PartOrderReceiveDto receiveDto, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(receiveDto.OrderId, cancellationToken);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {receiveDto.OrderId} not found.");
            }

            // Can only receive submitted orders
            if (order.Status != OrderStatus.Submitted && order.Status != OrderStatus.Partial)
            {
                throw new InvalidOperationException($"Cannot receive order {receiveDto.OrderId} with status {order.Status}. Only submitted or partially received orders can be received.");
            }

            var receiptDate = DateTime.UtcNow;
            var receiptLines = new List<ReceiptLineDto>();
            
            foreach (var receiveLine in receiveDto.ReceivedLines)
            {
                var orderLine = order.OrderLines.FirstOrDefault(ol => ol.Id == receiveLine.OrderLineId);
                if (orderLine == null)
                {
                    throw new KeyNotFoundException($"Order line with ID {receiveLine.OrderLineId} not found in order {receiveDto.OrderId}.");
                }

                // Can't receive more than ordered
                if (orderLine.ReceivedQuantity + receiveLine.ReceivedQuantity > orderLine.Quantity)
                {
                    throw new InvalidOperationException($"Cannot receive {receiveLine.ReceivedQuantity} of part {orderLine.Part?.PartNumber ?? orderLine.PartId.ToString()} when only {orderLine.Quantity - orderLine.ReceivedQuantity} remain to be received.");
                }

                // Update the received quantity
                orderLine.ReceivedQuantity += receiveLine.ReceivedQuantity;
                orderLine.ReceivedDate = receiptDate;
                
                // Update status based on received quantity
                if (orderLine.ReceivedQuantity == orderLine.Quantity)
                {
                    orderLine.Status = OrderLineStatus.Received;
                }
                else if (orderLine.ReceivedQuantity > 0)
                {
                    orderLine.Status = OrderLineStatus.Backordered;
                }

                // Update inventory
                await UpdateInventoryOnReceiptAsync(orderLine.PartId, receiveLine.ReceivedQuantity, cancellationToken);
                
                receiptLines.Add(new ReceiptLineDto
                {
                    OrderLineId = orderLine.Id,
                    PartId = orderLine.PartId,
                    PartNumber = orderLine.Part?.PartNumber ?? string.Empty,
                    ReceivedQuantity = receiveLine.ReceivedQuantity,
                    BackorderedQuantity = orderLine.Quantity - orderLine.ReceivedQuantity,
                    Status = orderLine.Status.ToString()
                });

                orderLine.UpdatedAt = receiptDate;
            }

            // Update the order status
            bool allLinesReceived = order.OrderLines.All(ol => ol.Status == OrderLineStatus.Received || ol.Status == OrderLineStatus.Cancelled);
            bool anyLinesReceived = order.OrderLines.Any(ol => ol.ReceivedQuantity > 0);
            
            if (allLinesReceived)
            {
                order.Status = OrderStatus.Complete;
            }
            else if (anyLinesReceived)
            {
                order.Status = OrderStatus.Partial;
            }

            order.UpdatedAt = receiptDate;
            await _orderRepository.UpdateAsync(order, cancellationToken);

            return new PartOrderReceiptDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                ReceiveDate = receiptDate,
                Status = order.Status.ToString(),
                ReceivedLines = receiptLines
            };
        }

        public async Task<IEnumerable<PartOrderLineDto>> GetOrderLinesAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order == null)
            {
                return Enumerable.Empty<PartOrderLineDto>();
            }

            return order.OrderLines.Select(ol => MapToOrderLineDto(ol));
        }

        public async Task<IEnumerable<ReorderRecommendationDto>> GenerateReorderRecommendationsAsync(CancellationToken cancellationToken = default)
        {
            // Get all inventory items below reorder point
            var inventoryItems = await _inventoryRepository.GetBelowReorderPointAsync(cancellationToken);
            var recommendations = new List<ReorderRecommendationDto>();

            foreach (var inventory in inventoryItems)
            {
                var part = await _partRepository.GetByIdAsync(inventory.PartId, cancellationToken);
                if (part == null || !part.IsActive) continue;

                // Determine the preferred supplier (in a real system, this would be more sophisticated)
                Guid preferredSupplierId = Guid.Empty;
                string supplierName = "Unknown";
                int leadTime = 7; // Default lead time
                
                // In a real implementation, determine the best supplier based on lead time, cost, etc.
                // For now, just use a placeholder
                
                recommendations.Add(new ReorderRecommendationDto
                {
                    PartId = part.Id,
                    PartNumber = part.PartNumber,
                    Description = part.Description,
                    CurrentStock = inventory.QuantityOnHand,
                    ReorderPoint = inventory.ReorderPoint,
                    RecommendedOrderQuantity = inventory.ReorderQuantity,
                    EstimatedCost = inventory.ReorderQuantity * (part.Pricing?.CostPrice ?? 0),
                    PreferredSupplierId = preferredSupplierId,
                    SupplierName = supplierName,
                    EstimatedLeadTime = leadTime
                });
            }

            return recommendations;
        }

        public async Task<IEnumerable<PartOrderSummaryDto>> GetSpecialOrdersAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            // Get all orders of type 'Special'
            var orders = await _orderRepository.GetAllAsync(skip, take, cancellationToken);
            return orders
                .Where(o => o.OrderType == OrderType.Special)
                .Select(o => MapToSummaryDto(o));
        }

        private async Task UpdateInventoryOnReceiptAsync(Guid partId, int quantity, CancellationToken cancellationToken)
        {
            // In a real implementation, this would update the inventory records
            // For now, this is just a placeholder
            var inventoryRecord = await _inventoryRepository.GetByPartIdAsync(partId, cancellationToken);
            if (inventoryRecord != null)
            {
                inventoryRecord.QuantityOnHand += quantity;
                await _inventoryRepository.UpdateAsync(inventoryRecord, cancellationToken);
            }
        }

        private PartOrderSummaryDto MapToSummaryDto(PartOrder order)
        {
            return new PartOrderSummaryDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                SupplierId = order.SupplierId,
                SupplierName = order.Supplier?.Name ?? string.Empty,
                OrderDate = order.OrderDate,
                ExpectedReceiveDate = order.ExpectedReceiveDate ?? DateTime.MinValue,
                Status = order.Status.ToString(),
                OrderType = order.OrderType.ToString(),
                TotalAmount = order.TotalAmount
            };
        }

        private async Task<PartOrderDetailDto> MapToDetailDtoAsync(PartOrder order, CancellationToken cancellationToken)
        {
            // Get supplier info for the detailed view
            var supplier = await _supplierRepository.GetByIdAsync(order.SupplierId, cancellationToken);
            
            return new PartOrderDetailDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                SupplierId = order.SupplierId,
                SupplierName = supplier?.Name ?? string.Empty,
                OrderDate = order.OrderDate,
                ExpectedReceiveDate = order.ExpectedReceiveDate ?? DateTime.MinValue,
                Status = order.Status.ToString(),
                OrderType = order.OrderType.ToString(),
                RequestorId = order.RequestorId ?? Guid.Empty,
                RequestorName = string.Empty, // In a real system, we'd look up the requestor's name
                ShippingMethod = order.ShippingMethod,
                TrackingNumber = order.TrackingNumber ?? string.Empty,
                Subtotal = order.Subtotal,
                ShippingCost = order.ShippingCost,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                Notes = order.Notes,
                OrderLines = order.OrderLines.Select(ol => MapToOrderLineDto(ol)).ToList(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt ?? order.CreatedAt
            };
        }

        private PartOrderLineDto MapToOrderLineDto(PartOrderLine line)
        {
            AllocationDto? allocationDto = null;
            if (line.Allocation != null)
            {
                allocationDto = new AllocationDto
                {
                    Type = line.Allocation.Type.ToString(),
                    ReferenceId = line.Allocation.ReferenceId,
                    ReferenceType = line.Allocation.ReferenceType ?? string.Empty
                };
            }

            return new PartOrderLineDto
            {
                Id = line.Id,
                PartId = line.PartId,
                PartNumber = line.Part?.PartNumber ?? string.Empty,
                Description = line.Part?.Description ?? string.Empty,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                ExtendedCost = line.ExtendedCost,
                Status = line.Status.ToString(),
                ReceivedQuantity = line.ReceivedQuantity,
                ReceivedDate = line.ReceivedDate,
                Allocation = allocationDto
            };
        }
    }
}
