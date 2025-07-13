# Parts Management API Reference

## Parts API

### GET /api/parts
Get all parts with pagination.

**Parameters:**
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartSummaryDto` objects.

### GET /api/parts/{id}
Get part by ID.

**Parameters:**
- `id` (path, required): Part ID.

**Returns:**
- 200 OK with `PartDetailDto` object.
- 404 Not Found if part does not exist.

### GET /api/parts/search
Search parts by term.

**Parameters:**
- `term` (query, required): Search term (part number, description, etc.).
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartSummaryDto` objects.

### GET /api/parts/number/{partNumber}
Get part by part number.

**Parameters:**
- `partNumber` (path, required): Part number.

**Returns:**
- 200 OK with `PartDetailDto` object.
- 404 Not Found if part does not exist.

### GET /api/parts/category/{categoryId}
Get parts by category.

**Parameters:**
- `categoryId` (path, required): Category ID.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartSummaryDto` objects.

### GET /api/parts/manufacturer/{manufacturerId}
Get parts by manufacturer.

**Parameters:**
- `manufacturerId` (path, required): Manufacturer ID.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartSummaryDto` objects.

### GET /api/parts/vehicle/fitment
Find parts by vehicle fitment.

**Parameters:**
- `year` (query, required): Vehicle year.
- `make` (query, required): Vehicle make.
- `model` (query, required): Vehicle model.
- `trim` (query, optional): Vehicle trim.
- `engine` (query, optional): Vehicle engine.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:**
- 200 OK with list of `PartSummaryDto` objects.
- 400 Bad Request if parameters are invalid.

### GET /api/parts/{partId}/similar
Get similar parts that can be cross-sold.

**Parameters:**
- `partId` (path, required): Part ID.
- `take` (query, optional): Number of similar parts to return. Default: 10.

**Returns:**
- 200 OK with list of `PartSummaryDto` objects.
- 404 Not Found if part does not exist.

### GET /api/parts/{id}/supersessions
Get supersession chain for a part.

**Parameters:**
- `id` (path, required): Part ID.

**Returns:**
- 200 OK with list of `PartSummaryDto` objects.
- 404 Not Found if part does not exist.

### POST /api/parts
Create a new part.

**Parameters:**
- `createPartDto` (body, required): Part creation data.

**Returns:**
- 201 Created with `PartDetailDto` object.
- 400 Bad Request if data is invalid or part number already exists.

**Authorization:** Requires PartsManager or Admin role.

### PUT /api/parts/{id}
Update an existing part.

**Parameters:**
- `id` (path, required): Part ID.
- `updatePartDto` (body, required): Part update data.

**Returns:**
- 200 OK with `PartDetailDto` object.
- 404 Not Found if part does not exist.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager or Admin role.

### DELETE /api/parts/{id}
Delete a part.

**Parameters:**
- `id` (path, required): Part ID.

**Returns:**
- 204 No Content if successful.
- 404 Not Found if part does not exist.
- 400 Bad Request if part cannot be deleted due to existing references.

**Authorization:** Requires PartsManager or Admin role.

### GET /api/parts/count
Get total count of parts in the system.

**Returns:** 200 OK with total count as integer.

## Inventory API

### GET /api/inventory/part/{partId}
Get inventory for a specific part.

**Parameters:**
- `partId` (path, required): Part ID.

**Returns:**
- 200 OK with list of `PartInventoryDto` objects.
- 404 Not Found if part does not exist.

### GET /api/inventory/location/{locationId}
Get inventory for a specific location.

**Parameters:**
- `locationId` (path, required): Location ID.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartInventoryDto` objects.

### GET /api/inventory/low-stock
Get low stock inventory items.

**Parameters:**
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartInventoryDto` objects.

### PUT /api/inventory/{inventoryId}
Update inventory settings.

**Parameters:**
- `inventoryId` (path, required): Inventory ID.
- `updateInventorySettingsDto` (body, required): Inventory settings update data.

**Returns:**
- 200 OK with `PartInventoryDto` object.
- 404 Not Found if inventory does not exist.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager or Admin role.

### POST /api/inventory/adjust
Adjust inventory.

**Parameters:**
- `adjustInventoryDto` (body, required): Inventory adjustment data.

**Returns:**
- 200 OK with `PartTransactionDto` object.
- 404 Not Found if part does not exist.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager or Admin role.

### POST /api/inventory
Create part inventory record.

**Parameters:**
- `createPartInventoryDto` (body, required): Inventory creation data.

**Returns:**
- 201 Created with `PartInventoryDto` object.
- 404 Not Found if part does not exist.
- 400 Bad Request if data is invalid or inventory already exists.

**Authorization:** Requires PartsManager or Admin role.

## Suppliers API

### GET /api/suppliers
Get all suppliers with pagination.

**Parameters:**
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `SupplierSummaryDto` objects.

### GET /api/suppliers/{id}
Get supplier by ID.

**Parameters:**
- `id` (path, required): Supplier ID.

**Returns:**
- 200 OK with `SupplierDetailDto` object.
- 404 Not Found if supplier does not exist.

### GET /api/suppliers/{supplierId}/parts
Get parts supplied by a specific supplier.

**Parameters:**
- `supplierId` (path, required): Supplier ID.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:**
- 200 OK with list of `PartSummaryDto` objects.
- 404 Not Found if supplier does not exist.

### GET /api/suppliers/search
Search suppliers by term.

**Parameters:**
- `term` (query, required): Search term.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `SupplierSummaryDto` objects.

### POST /api/suppliers
Create a new supplier.

**Parameters:**
- `createSupplierDto` (body, required): Supplier creation data.

**Returns:**
- 201 Created with `SupplierDetailDto` object.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager or Admin role.

### PUT /api/suppliers/{id}
Update an existing supplier.

**Parameters:**
- `id` (path, required): Supplier ID.
- `updateSupplierDto` (body, required): Supplier update data.

**Returns:**
- 200 OK with `SupplierDetailDto` object.
- 404 Not Found if supplier does not exist.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager or Admin role.

### DELETE /api/suppliers/{id}
Delete a supplier.

**Parameters:**
- `id` (path, required): Supplier ID.

**Returns:**
- 204 No Content if successful.
- 404 Not Found if supplier does not exist.
- 400 Bad Request if supplier cannot be deleted due to existing references.

**Authorization:** Requires PartsManager or Admin role.

## Orders API

### GET /api/orders
Get all orders with pagination.

**Parameters:**
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartOrderSummaryDto` objects.

### GET /api/orders/{id}
Get order by ID.

**Parameters:**
- `id` (path, required): Order ID.

**Returns:**
- 200 OK with `PartOrderDetailDto` object.
- 404 Not Found if order does not exist.

### GET /api/orders/status/{status}
Get orders by status.

**Parameters:**
- `status` (path, required): Order status.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartOrderSummaryDto` objects.

### GET /api/orders/supplier/{supplierId}
Get orders by supplier.

**Parameters:**
- `supplierId` (path, required): Supplier ID.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartOrderSummaryDto` objects.

### POST /api/orders
Create a new order.

**Parameters:**
- `createOrderDto` (body, required): Order creation data.

**Returns:**
- 201 Created with `PartOrderDetailDto` object.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager, PartsSales, or Admin role.

### PUT /api/orders/{id}
Update an existing order.

**Parameters:**
- `id` (path, required): Order ID.
- `updateOrderDto` (body, required): Order update data.

**Returns:**
- 200 OK with `PartOrderDetailDto` object.
- 404 Not Found if order does not exist.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager, PartsSales, or Admin role.

### DELETE /api/orders/{id}
Delete an order.

**Parameters:**
- `id` (path, required): Order ID.

**Returns:**
- 204 No Content if successful.
- 404 Not Found if order does not exist.
- 400 Bad Request if order cannot be deleted.

**Authorization:** Requires PartsManager or Admin role.

### POST /api/orders/{id}/submit
Submit an order to supplier.

**Parameters:**
- `id` (path, required): Order ID.

**Returns:**
- 200 OK if successful.
- 404 Not Found if order does not exist.
- 400 Bad Request if order cannot be submitted.

**Authorization:** Requires PartsManager or Admin role.

### POST /api/orders/receive
Receive order items.

**Parameters:**
- `receiveDto` (body, required): Order receive data.

**Returns:**
- 200 OK with `PartOrderReceiptDto` object.
- 404 Not Found if order does not exist.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager, PartsSales, or Admin role.

### GET /api/orders/{orderId}/lines
Get order lines by order ID.

**Parameters:**
- `orderId` (path, required): Order ID.

**Returns:**
- 200 OK with list of `PartOrderLineDto` objects.
- 404 Not Found if order does not exist.

### GET /api/orders/recommend
Generate reorder recommendations.

**Returns:** 200 OK with list of `ReorderRecommendationDto` objects.

**Authorization:** Requires PartsManager or Admin role.

### GET /api/orders/special
Get special orders.

**Parameters:**
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartOrderSummaryDto` objects.

## Transactions API

### GET /api/transactions
Get all transactions with pagination and optional date filtering.

**Parameters:**
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.
- `startDate` (query, optional): Optional start date filter.
- `endDate` (query, optional): Optional end date filter.

**Returns:** 200 OK with list of `PartTransactionDto` objects.

### GET /api/transactions/{id}
Get transaction by ID.

**Parameters:**
- `id` (path, required): Transaction ID.

**Returns:**
- 200 OK with `PartTransactionDto` object.
- 404 Not Found if transaction does not exist.

### GET /api/transactions/part/{partId}
Get transactions by part ID.

**Parameters:**
- `partId` (path, required): Part ID.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartTransactionDto` objects.

### GET /api/transactions/type/{type}
Get transactions by type.

**Parameters:**
- `type` (path, required): Transaction type.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `PartTransactionDto` objects.

### POST /api/transactions/issue
Issue parts.

**Parameters:**
- `issuePartsDto` (body, required): Parts issue data.

**Returns:**
- 200 OK with `PartTransactionDto` object.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager, PartsSales, ServiceAdvisor, or Admin role.

### POST /api/transactions/return
Return parts.

**Parameters:**
- `returnPartsDto` (body, required): Parts return data.

**Returns:**
- 200 OK with `PartTransactionDto` object.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager, PartsSales, ServiceAdvisor, or Admin role.

### POST /api/transactions/adjust
Adjust inventory.

**Parameters:**
- `adjustInventoryDto` (body, required): Inventory adjustment data.

**Returns:**
- 200 OK with `PartTransactionDto` object.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager or Admin role.

### POST /api/transactions/transfer
Transfer parts between locations.

**Parameters:**
- `transferPartsDto` (body, required): Parts transfer data.

**Returns:**
- 200 OK with `PartTransactionDto` object.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager or Admin role.

### GET /api/transactions/history/{partId}
Get transaction history for a part within date range.

**Parameters:**
- `partId` (path, required): Part ID.
- `startDate` (query, required): Start date.
- `endDate` (query, required): End date.

**Returns:** 200 OK with list of `PartTransactionSummaryDto` objects.

## Cores API

### GET /api/cores
Get all core tracking records with pagination.

**Parameters:**
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `CoreTrackingDto` objects.

### GET /api/cores/{id}
Get core tracking record by ID.

**Parameters:**
- `id` (path, required): Core tracking ID.

**Returns:**
- 200 OK with `CoreTrackingDto` object.
- 404 Not Found if core tracking record does not exist.

### GET /api/cores/part/{partId}
Get core tracking records by part ID.

**Parameters:**
- `partId` (path, required): Part ID.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `CoreTrackingDto` objects.

### GET /api/cores/status/{status}
Get core tracking records by status.

**Parameters:**
- `status` (path, required): Core tracking status.
- `skip` (query, optional): Number of records to skip. Default: 0.
- `take` (query, optional): Number of records to take. Default: 50.

**Returns:** 200 OK with list of `CoreTrackingDto` objects.

### POST /api/cores/track
Create a new core tracking record.

**Parameters:**
- `createCoreTrackingDto` (body, required): Core tracking creation data.

**Returns:**
- 201 Created with `CoreTrackingDto` object.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager, PartsSales, or Admin role.

### PUT /api/cores/{id}/return
Process core return.

**Parameters:**
- `id` (path, required): Core tracking ID.
- `processReturnDto` (body, required): Core return processing data.

**Returns:**
- 200 OK with `CoreTrackingDto` object.
- 404 Not Found if core tracking record does not exist.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager, PartsSales, or Admin role.

### PUT /api/cores/{id}/credit
Apply credit for returned core.

**Parameters:**
- `id` (path, required): Core tracking ID.
- `applyCreditDto` (body, required): Credit application data.

**Returns:**
- 200 OK with `CoreTrackingDto` object.
- 404 Not Found if core tracking record does not exist.
- 400 Bad Request if data is invalid.

**Authorization:** Requires PartsManager or Admin role.

### GET /api/cores/outstanding-value
Get total outstanding core value.

**Returns:** 200 OK with total outstanding core value as decimal.
