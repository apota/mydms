# Parts Management Module

## Overview
The Parts Management module provides complete control over parts inventory, ordering, pricing, and distribution. This module ensures optimal parts availability for service operations, maximizes parts sales profitability, and streamlines the entire parts supply chain from ordering to delivery.

## Implementation Status

✅ **COMPLETED** - June 21, 2025

The Parts Management module has been fully implemented with all the required functionality:

- Parts catalog and inventory management
- Supplier management
- Order management (creation, submission, receiving)
- Transaction tracking (issues, returns, adjustments, transfers)
- Core tracking (sold, returned, credited)
- Integration with other modules in the DMS

## Documentation

- [Implementation Guide](./IMPLEMENTATION-GUIDE.md) - Overview of architecture, components, and workflows
- [API Reference](./API-REFERENCE.md) - Complete reference of all REST APIs
- [README-IMPLEMENTATION.md](./README-IMPLEMENTATION.md) - Implementation details and development guidelines

## Architecture

The module follows a clean architecture approach with:

1. **Core Layer** - Domain models, DTOs, interfaces, and service contracts
2. **Infrastructure Layer** - Service and repository implementations
3. **API Layer** - REST API controllers
4. **Tests** - Unit tests for services and controllers

## Key Features

### Parts Management
- Create, update, and delete parts
- Search by various criteria (part number, description, etc.)
- Vehicle fitment lookup
- Supersession tracking
- Cross-reference management

### Inventory Management
- Track inventory across multiple locations
- Manage minimum and maximum levels
- Bin location management
- Low stock alerts
- Inventory adjustments and transfers

### Order Management
- Create and manage purchase orders
- Process special orders
- Track order status
- Receive complete and partial shipments
- Reorder recommendations

### Transaction Management
- Issue parts to service orders
- Process returns
- Inventory adjustments
- Location transfers
- Transaction history

### Core Tracking
- Track cores through their lifecycle
- Process core returns
- Apply credits
- Track outstanding core value

## Data Model

### Core Entities

#### Part
```json
{
  "id": "UUID",
  "partNumber": "String",
  "manufacturerId": "Reference to Manufacturer",
  "manufacturerPartNumber": "String",
  "description": "String",
  "categoryId": "Reference to PartCategory",
  "weight": "Decimal",
  "dimensions": {
    "length": "Decimal",
    "width": "Decimal",
    "height": "Decimal",
    "unitOfMeasure": "String"
  },
  "replacementFor": ["Array of Part Numbers"],
  "crossReferences": [
    {
      "manufacturerId": "Reference to Manufacturer",
      "partNumber": "String"
    }
  ],
  "fitmentData": {
    "years": ["Array of Integers"],
    "makes": ["Array of Strings"],
    "models": ["Array of Strings"],
    "trims": ["Array of Strings"],
    "engines": ["Array of Strings"]
  },
  "isSerialized": "Boolean",
  "isSpecialOrder": "Boolean",
  "notes": "String",
  "status": "Enum (Active, Discontinued, Superseded)",
  "supercededBy": "String",
  "images": ["Array of Image URLs"],
  "createdAt": "DateTime",
  "updatedAt": "DateTime"
}
```

#### PartInventory
```json
{
  "id": "UUID",
  "partId": "Reference to Part",
  "locationId": "Reference to Location",
  "quantityOnHand": "Integer",
  "quantityAvailable": "Integer",
  "quantityAllocated": "Integer",
  "quantityOnOrder": "Integer",
  "minimumLevel": "Integer",
  "maximumLevel": "Integer",
  "reorderPoint": "Integer",
  "reorderQuantity": "Integer",
  "binLocation": "String",
  "lastCountDate": "DateTime",
  "lastReceiptDate": "DateTime",
  "lastIssuedDate": "DateTime",
  "movementClass": "Enum (Fast, Medium, Slow, Dead)",
  "createdAt": "DateTime",
  "updatedAt": "DateTime"
}
```

#### PartPricing
```json
{
  "id": "UUID",
  "partId": "Reference to Part",
  "cost": "Decimal",
  "retailPrice": "Decimal",
  "wholesalePrice": "Decimal",
  "specialPrice": "Decimal",
  "specialPriceStartDate": "DateTime",
  "specialPriceEndDate": "DateTime",
  "markup": "Decimal",
  "margin": "Decimal",
  "priceSource": "Enum (Manufacturer, Manual, Formula)",
  "priceHistory": [
    {
      "effectiveDate": "DateTime",
      "cost": "Decimal",
      "retailPrice": "Decimal",
      "reason": "String"
    }
  ],
  "createdAt": "DateTime",
  "updatedAt": "DateTime"
}
```

#### Supplier
```json
{
  "id": "UUID",
  "name": "String",
  "type": "Enum (Manufacturer, Distributor, Dealer, Aftermarket)",
  "accountNumber": "String",
  "contactPerson": "String",
  "email": "String",
  "phone": "String",
  "address": {
    "street": "String",
    "city": "String",
    "state": "String",
    "zip": "String",
    "country": "String"
  },
  "website": "String",
  "shippingTerms": "String",
  "paymentTerms": "String",
  "orderMethods": ["Array of Strings"],
  "leadTime": "Integer",
  "status": "Enum (Active, Inactive)",
  "createdAt": "DateTime",
  "updatedAt": "DateTime"
}
```

#### PartOrder
```json
{
  "id": "UUID",
  "orderNumber": "String",
  "supplierId": "Reference to Supplier",
  "orderDate": "DateTime",
  "expectedReceiveDate": "DateTime",
  "status": "Enum (Draft, Submitted, Partial, Complete, Cancelled)",
  "orderType": "Enum (Stock, Special, Emergency)",
  "requestorId": "Reference to User",
  "shippingMethod": "String",
  "trackingNumber": "String",
  "subtotal": "Decimal",
  "shippingCost": "Decimal",
  "taxAmount": "Decimal",
  "totalAmount": "Decimal",
  "notes": "String",
  "createdAt": "DateTime",
  "updatedAt": "DateTime"
}
```

#### PartOrderLine
```json
{
  "id": "UUID",
  "partOrderId": "Reference to PartOrder",
  "partId": "Reference to Part",
  "quantity": "Integer",
  "unitCost": "Decimal",
  "extendedCost": "Decimal",
  "status": "Enum (Ordered, Backordered, Received, Cancelled)",
  "receivedQuantity": "Integer",
  "receivedDate": "DateTime",
  "allocation": {
    "type": "Enum (Stock, Service, Customer, Wholesale)",
    "referenceId": "UUID",
    "referenceType": "String"
  },
  "createdAt": "DateTime",
  "updatedAt": "DateTime"
}
```

#### PartTransaction
```json
{
  "id": "UUID",
  "transactionType": "Enum (Receipt, Issue, Return, Adjustment, Transfer)",
  "partId": "Reference to Part",
  "quantity": "Integer",
  "sourceLocationId": "Reference to Location",
  "destinationLocationId": "Reference to Location",
  "referenceType": "String",
  "referenceId": "UUID",
  "userId": "Reference to User",
  "notes": "String",
  "unitCost": "Decimal",
  "extendedCost": "Decimal",
  "unitPrice": "Decimal",
  "extendedPrice": "Decimal",
  "transactionDate": "DateTime",
  "createdAt": "DateTime"
}
```

#### PartCoreTracking
```json
{
  "id": "UUID",
  "partId": "Reference to Part",
  "corePartNumber": "String",
  "coreValue": "Decimal",
  "status": "Enum (Sold, Returned, Credited)",
  "soldDate": "DateTime",
  "soldReferenceId": "UUID",
  "returnedDate": "DateTime",
  "returnReferenceId": "UUID",
  "creditedDate": "DateTime",
  "creditAmount": "Decimal",
  "notes": "String",
  "createdAt": "DateTime",
  "updatedAt": "DateTime"
}
```

## API Endpoints

### Parts Inventory Management
- `GET /api/parts` - List parts with filtering
- `GET /api/parts/{id}` - Get part details
- `POST /api/parts` - Create new part
- `PUT /api/parts/{id}` - Update part
- `GET /api/parts/inventory` - Get inventory levels
- `PUT /api/parts/inventory/{partId}` - Update inventory levels
- `POST /api/parts/inventory/count` - Record inventory count

### Parts Catalog
- `GET /api/parts/catalog/search` - Search parts catalog
- `GET /api/parts/catalog/vehicle` - Search by vehicle fitment
- `GET /api/parts/catalog/supersessions` - Get part supersession chain
- `GET /api/parts/catalog/interchange` - Get interchange information

### Parts Ordering
- `GET /api/parts/orders` - List orders
- `GET /api/parts/orders/{id}` - Get order details
- `POST /api/parts/orders` - Create new order
- `PUT /api/parts/orders/{id}` - Update order
- `POST /api/parts/orders/{id}/submit` - Submit order to supplier
- `POST /api/parts/orders/{id}/receive` - Receive order
- `POST /api/parts/orders/recommend` - Generate reorder recommendations

### Parts Transaction
- `POST /api/parts/transactions/issue` - Issue parts
- `POST /api/parts/transactions/return` - Return parts
- `POST /api/parts/transactions/adjust` - Adjust inventory
- `POST /api/parts/transactions/transfer` - Transfer between locations
- `GET /api/parts/transactions/history/{partId}` - Get transaction history

### Core Tracking
- `GET /api/parts/cores` - List core transactions
- `POST /api/parts/cores/track` - Record core sale
- `PUT /api/parts/cores/{id}/return` - Process core return
- `PUT /api/parts/cores/{id}/credit` - Apply core credit

## Integration Points

### Internal Integrations
- **Service Management Module** - For parts usage in repair orders
- **Sales Management Module** - For parts sales and accessories
- **Financial Management Module** - For costs, pricing, and accounting
- **Inventory Management Module** - For shared inventory concepts
- **Reporting Module** - For parts department analytics

### External Integrations
- **Manufacturer Parts Catalogs** - For OEM parts information
- **Parts Distributors** - For ordering and availability checking
- **Aftermarket Suppliers** - For alternate parts sources
- **Shipping Providers** - For tracking and delivery information
- **Pricing Services** - For competitive pricing analysis

## User Interface Design

### Parts Dashboard
- Inventory status overview
- Parts on order status
- Special order tracking
- Fast-moving/slow-moving parts analysis
- Low stock alerts

### Parts Search and Lookup
- Multi-criteria search interface
- Vehicle-specific parts finder
- Visual parts catalog navigation
- Interchange and supersession display
- Pricing and availability information

### Inventory Management
- Bin location management
- Cycle counting interface
- Inventory adjustments
- Stock level optimization tools
- Physical inventory reconciliation

### Parts Counter Sales
- Fast part lookup interface
- Customer information capture
- Price level selection
- Invoicing and payment processing
- Special order processing

## Workflows

### Parts Ordering Workflow
1. Identify parts needing reorder (manual or automatic)
2. Generate purchase order
3. Review and adjust quantities
4. Submit order to supplier
5. Receive order confirmation
6. Track shipping status
7. Receive parts into inventory
8. Reconcile received parts with order
9. Process any discrepancies
10. Update inventory levels

### Parts Counter Sale Workflow
1. Customer requests part
2. Look up part(s) by vehicle or part number
3. Check availability and pricing
4. Reserve parts for customer
5. Create sales transaction
6. Process payment
7. Issue parts to customer
8. Track core charge if applicable
9. Update inventory

### Special Order Parts Workflow
1. Identify need for special order part
2. Look up part information in catalog
3. Check availability with suppliers
4. Provide customer with price and ETA
5. Create special order with customer information
6. Submit order to supplier
7. Track order status
8. Notify customer upon receipt
9. Process customer pickup/delivery

### Parts for Service Workflow
1. Service technician requests parts
2. Parts department locates requested parts
3. Reserve parts for specific repair order
4. Stage parts for technician pickup
5. Record parts issued to repair order
6. Process any returned unused parts
7. Update inventory

## Security Considerations
- Role-based access for parts personnel
- Separation of duties for ordering and receiving
- Audit trails for inventory adjustments
- Secure supplier connection for ordering
- Physical security for high-value parts

## Performance Requirements
- Support for dealerships with 50,000+ parts in inventory
- Fast part lookup response time (< 1 second)
- Efficient handling of daily transaction volume (1,000+ transactions)
- Robust search capabilities for parts catalog
- Responsive UI even with slow connections

## Compliance Requirements
- Manufacturer-specific parts return policies
- Hazardous materials handling and documentation
- Core return tracking and accounting
- Warranty parts retention requirements
- Taxation rules for parts sales

## Future Enhancements
- Barcode/RFID scanning for inventory management
- Predictive ordering based on service demand
- Automated bin location optimization
- 3D parts visualization and diagrams
- Integration with 3D printing for rare parts
- AI-powered parts identification from images

## Technical Implementation Notes
- Efficient indexing for parts catalog search
- Caching strategy for frequently accessed parts data
- Batch processing for bulk inventory updates
- Real-time inventory level tracking
- Support for offline operation during connectivity issues
