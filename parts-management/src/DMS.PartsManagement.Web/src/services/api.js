import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || '/api';

// Add auth token to requests if available
const getAuthHeaders = () => {
  const user = JSON.parse(localStorage.getItem('user') || '{}');
  return user.token ? { Authorization: `Bearer ${user.token}` } : {};
};

// Handle API errors
const handleApiError = (error) => {
  if (error.response) {
    // The request was made and the server responded with a status code
    if (error.response.status === 401) {
      // Unauthorized - clear user data and redirect to login
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error.response.data);
  } else if (error.request) {
    // The request was made but no response was received
    return Promise.reject({ message: 'No response from server. Please try again later.' });
  } else {
    // Something happened in setting up the request that triggered an Error
    return Promise.reject({ message: error.message });
  }
};

// Create axios instance with base configuration
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor to add auth token
apiClient.interceptors.request.use((config) => {
  const headers = getAuthHeaders();
  config.headers = { ...config.headers, ...headers };
  return config;
});

// Add response interceptor to handle errors
apiClient.interceptors.response.use(
  (response) => response,
  (error) => handleApiError(error)
);

// Parts API Service
export const PartsService = {
  // Get all parts with pagination
  getAllParts: async (skip = 0, take = 50) => {
    const response = await apiClient.get(`/parts?skip=${skip}&take=${take}`);
    return response.data;
  },

  // Get part by ID
  getPartById: async (id) => {
    const response = await apiClient.get(`/parts/${id}`);
    return response.data;
  },

  // Search parts
  searchParts: async (term, skip = 0, take = 50) => {
    const response = await apiClient.get(`/parts/search?term=${encodeURIComponent(term)}&skip=${skip}&take=${take}`);
    return response.data;
  },

  // Get part by part number
  getPartByPartNumber: async (partNumber) => {
    const response = await apiClient.get(`/parts/number/${encodeURIComponent(partNumber)}`);
    return response.data;
  },
  
  // Get parts by category
  getPartsByCategory: async (categoryId, skip = 0, take = 50) => {
    const response = await apiClient.get(`/parts/category/${categoryId}?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Get parts by manufacturer
  getPartsByManufacturer: async (manufacturerId, skip = 0, take = 50) => {
    const response = await apiClient.get(`/parts/manufacturer/${manufacturerId}?skip=${skip}&take=${take}`);
    return response.data;
  },

  // Get parts by vehicle fitment
  getPartsByFitment: async (year, make, model, trim, engine, skip = 0, take = 50) => {
    let url = `/parts/vehicle/fitment?year=${year}&make=${encodeURIComponent(make)}&model=${encodeURIComponent(model)}`;
    if (trim) url += `&trim=${encodeURIComponent(trim)}`;
    if (engine) url += `&engine=${encodeURIComponent(engine)}`;
    url += `&skip=${skip}&take=${take}`;
    
    const response = await apiClient.get(url);
    return response.data;
  },
  
  // Get similar parts
  getSimilarParts: async (partId, take = 10) => {
    const response = await apiClient.get(`/parts/${partId}/similar?take=${take}`);
    return response.data;
  },

  // Get supersession chain
  getSupersessionChain: async (id) => {
    const response = await apiClient.get(`/parts/${id}/supersessions`);
    return response.data;
  },
  
  // Create a new part
  createPart: async (partData) => {
    const response = await apiClient.post('/parts', partData);
    return response.data;
  },
  
  // Update an existing part
  updatePart: async (id, partData) => {
    const response = await apiClient.put(`/parts/${id}`, partData);
    return response.data;
  },
  
  // Delete a part
  deletePart: async (id) => {
    await apiClient.delete(`/parts/${id}`);
    return true;
  },
  
  // Get total count of parts
  getPartsCount: async () => {
    const response = await apiClient.get('/parts/count');
    return response.data;
  }
};

// Inventory API Service
export const InventoryService = {
  // Get inventory for a specific part
  getInventoryByPart: async (partId) => {
    const response = await apiClient.get(`/inventory/part/${partId}`);
    return response.data;
  },
  
  // Get inventory for a specific location
  getInventoryByLocation: async (locationId, skip = 0, take = 50) => {
    const response = await apiClient.get(`/inventory/location/${locationId}?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Get low stock inventory items
  getLowStockInventory: async (skip = 0, take = 50) => {
    const response = await apiClient.get(`/inventory/low-stock?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Update inventory settings
  updateInventorySettings: async (inventoryId, settingsData) => {
    const response = await apiClient.put(`/inventory/${inventoryId}`, settingsData);
    return response.data;
  },
  
  // Adjust inventory
  adjustInventory: async (adjustmentData) => {
    const response = await apiClient.post('/inventory/adjust', adjustmentData);
    return response.data;
  },
  
  // Create part inventory record
  createInventory: async (inventoryData) => {
    const response = await apiClient.post('/inventory', inventoryData);
    return response.data;
  }
};

// Suppliers API Service
export const SuppliersService = {
  // Get all suppliers with pagination
  getAllSuppliers: async (skip = 0, take = 50) => {
    const response = await apiClient.get(`/suppliers?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Get supplier by ID
  getSupplierById: async (id) => {
    const response = await apiClient.get(`/suppliers/${id}`);
    return response.data;
  },
  
  // Get parts supplied by a specific supplier
  getSupplierParts: async (supplierId, skip = 0, take = 50) => {
    const response = await apiClient.get(`/suppliers/${supplierId}/parts?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Search suppliers by term
  searchSuppliers: async (term, skip = 0, take = 50) => {
    const response = await apiClient.get(`/suppliers/search?term=${encodeURIComponent(term)}&skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Create a new supplier
  createSupplier: async (supplierData) => {
    const response = await apiClient.post('/suppliers', supplierData);
    return response.data;
  },
  
  // Update an existing supplier
  updateSupplier: async (id, supplierData) => {
    const response = await apiClient.put(`/suppliers/${id}`, supplierData);
    return response.data;
  },
  
  // Delete a supplier
  deleteSupplier: async (id) => {
    await apiClient.delete(`/suppliers/${id}`);
    return true;
  }
};

// Orders API Service
export const OrdersService = {
  // Get all orders with pagination
  getAllOrders: async (skip = 0, take = 50) => {
    const response = await apiClient.get(`/orders?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Get order by ID
  getOrderById: async (id) => {
    const response = await apiClient.get(`/orders/${id}`);
    return response.data;
  },
  
  // Get orders by status
  getOrdersByStatus: async (status, skip = 0, take = 50) => {
    const response = await apiClient.get(`/orders/status/${status}?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Get orders by supplier
  getOrdersBySupplier: async (supplierId, skip = 0, take = 50) => {
    const response = await apiClient.get(`/orders/supplier/${supplierId}?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Create a new order
  createOrder: async (orderData) => {
    const response = await apiClient.post('/orders', orderData);
    return response.data;
  },
  
  // Update an existing order
  updateOrder: async (id, orderData) => {
    const response = await apiClient.put(`/orders/${id}`, orderData);
    return response.data;
  },
  
  // Delete an order
  deleteOrder: async (id) => {
    await apiClient.delete(`/orders/${id}`);
    return true;
  },
  
  // Submit an order to supplier
  submitOrder: async (id) => {
    const response = await apiClient.post(`/orders/${id}/submit`);
    return response.data;
  },
  
  // Receive order items
  receiveOrder: async (receiveData) => {
    const response = await apiClient.post('/orders/receive', receiveData);
    return response.data;
  },
  
  // Get order lines by order ID
  getOrderLines: async (orderId) => {
    const response = await apiClient.get(`/orders/${orderId}/lines`);
    return response.data;
  },
  
  // Generate reorder recommendations
  getReorderRecommendations: async () => {
    const response = await apiClient.get('/orders/recommend');
    return response.data;
  },
  
  // Get special orders
  getSpecialOrders: async (skip = 0, take = 50) => {
    const response = await apiClient.get(`/orders/special?skip=${skip}&take=${take}`);
    return response.data;
  }
};

// Transactions API Service
export const TransactionsService = {
  // Get all transactions with pagination and optional date filtering
  getAllTransactions: async (skip = 0, take = 50, startDate = null, endDate = null) => {
    let url = `/transactions?skip=${skip}&take=${take}`;
    if (startDate) url += `&startDate=${startDate.toISOString()}`;
    if (endDate) url += `&endDate=${endDate.toISOString()}`;
    
    const response = await apiClient.get(url);
    return response.data;
  },
  
  // Get transaction by ID
  getTransactionById: async (id) => {
    const response = await apiClient.get(`/transactions/${id}`);
    return response.data;
  },
  
  // Get transactions by part ID
  getTransactionsByPart: async (partId, skip = 0, take = 50) => {
    const response = await apiClient.get(`/transactions/part/${partId}?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Get transactions by type
  getTransactionsByType: async (type, skip = 0, take = 50) => {
    const response = await apiClient.get(`/transactions/type/${type}?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Issue parts
  issueParts: async (issueData) => {
    const response = await apiClient.post('/transactions/issue', issueData);
    return response.data;
  },
  
  // Return parts
  returnParts: async (returnData) => {
    const response = await apiClient.post('/transactions/return', returnData);
    return response.data;
  },
  
  // Adjust inventory
  adjustInventory: async (adjustData) => {
    const response = await apiClient.post('/transactions/adjust', adjustData);
    return response.data;
  },
  
  // Transfer parts between locations
  transferParts: async (transferData) => {
    const response = await apiClient.post('/transactions/transfer', transferData);
    return response.data;
  },
  
  // Get transaction history for a part within date range
  getPartTransactionHistory: async (partId, startDate, endDate) => {
    const url = `/transactions/history/${partId}?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`;
    const response = await apiClient.get(url);
    return response.data;
  }
};

// Core Tracking API Service
export const CoreTrackingService = {
  // Get all core tracking records with pagination
  getAllCores: async (skip = 0, take = 50) => {
    const response = await apiClient.get(`/cores?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Get core tracking record by ID
  getCoreById: async (id) => {
    const response = await apiClient.get(`/cores/${id}`);
    return response.data;
  },
  
  // Get core tracking records by part ID
  getCoresByPart: async (partId, skip = 0, take = 50) => {
    const response = await apiClient.get(`/cores/part/${partId}?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Get core tracking records by status
  getCoresByStatus: async (status, skip = 0, take = 50) => {
    const response = await apiClient.get(`/cores/status/${status}?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  // Create a new core tracking record
  createCoreTracking: async (coreData) => {
    const response = await apiClient.post('/cores/track', coreData);
    return response.data;
  },
  
  // Process core return
  processCoreReturn: async (id, returnData) => {
    const response = await apiClient.put(`/cores/${id}/return`, returnData);
    return response.data;
  },
  
  // Apply credit for returned core
  applyCoreCredit: async (id, creditData) => {
    const response = await apiClient.put(`/cores/${id}/credit`, creditData);
    return response.data;
  },
  
  // Get total outstanding core value
  getOutstandingCoreValue: async () => {
    const response = await apiClient.get('/cores/outstanding-value');
    return response.data;
  }
};

// Integration Services
export const IntegrationService = {
  // Create a cache for integration responses
  _cache: new Map(),
  _cacheTimeout: 5 * 60 * 1000, // 5 minutes in milliseconds
  
  // Cache handling methods
  _getCached: (key) => {
    const cached = IntegrationService._cache.get(key);
    if (cached && Date.now() - cached.timestamp < IntegrationService._cacheTimeout) {
      return cached.data;
    }
    return null;
  },
  
  _setCache: (key, data) => {
    IntegrationService._cache.set(key, { 
      data, 
      timestamp: Date.now() 
    });
    return data;
  },
  
  _clearCache: () => {
    IntegrationService._cache.clear();
  },
  
  _invalidateCache: (key) => {
    if (key) {
      IntegrationService._cache.delete(key);
    }
  },
  
  // CRM Integration
  getCustomerInfo: async (customerId) => {
    const cacheKey = `crm_customer_${customerId}`;
    const cached = IntegrationService._getCached(cacheKey);
    if (cached) return cached;
    
    try {
      const response = await apiClient.get(`/integration/crm/customers/${customerId}`);
      return IntegrationService._setCache(cacheKey, response.data);
    } catch (error) {
      console.error('Error fetching customer info:', error);
      throw error;
    }
  },
  
  // Financial Integration
  getPricing: async (partId) => {
    const cacheKey = `financial_pricing_${partId}`;
    const cached = IntegrationService._getCached(cacheKey);
    if (cached) return cached;
    
    try {
      const response = await apiClient.get(`/integration/financial/pricing/${partId}`);
      return IntegrationService._setCache(cacheKey, response.data);
    } catch (error) {
      console.error('Error fetching pricing:', error);
      throw error;
    }
  },

  getBulkPricing: async (partIds) => {
    try {
      const response = await apiClient.post(`/integration/financial/bulk-pricing`, { partIds });
      return response.data;
    } catch (error) {
      console.error('Error fetching bulk pricing:', error);
      throw error;
    }
  },

  createInvoiceForParts: async (invoiceData) => {
    try {
      const response = await apiClient.post(`/integration/financial/invoices`, invoiceData);
      return response.data;
    } catch (error) {
      console.error('Error creating invoice:', error);
      throw error;
    }
  },
  
  getFinancialReportForParts: async (startDate, endDate) => {
    try {
      const url = `/integration/financial/reports/parts?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`;
      const response = await apiClient.get(url);
      return response.data;
    } catch (error) {
      console.error('Error fetching financial report:', error);
      throw error;
    }
  },
  
  // Inventory Management Integration
  getInventorySync: async (locationId = null) => {
    try {
      const url = locationId ? 
        `/integration/inventory/sync?locationId=${locationId}` :
        `/integration/inventory/sync`;
      const response = await apiClient.get(url);
      return response.data;
    } catch (error) {
      console.error('Error syncing inventory data:', error);
      throw error;
    }
  },

  pushInventoryUpdates: async (updates) => {
    try {
      const response = await apiClient.post(`/integration/inventory/updates`, updates);
      return response.data;
    } catch (error) {
      console.error('Error pushing inventory updates:', error);
      throw error;
    }
  },
  
  // Service Integration
  getServiceOrderParts: async (serviceOrderId) => {
    const cacheKey = `service_order_parts_${serviceOrderId}`;
    const cached = IntegrationService._getCached(cacheKey);
    if (cached) return cached;
    
    try {
      const response = await apiClient.get(`/integration/service/orders/${serviceOrderId}/parts`);
      return IntegrationService._setCache(cacheKey, response.data);
    } catch (error) {
      console.error('Error fetching service order parts:', error);
      throw error;
    }
  },
  
  assignPartsToServiceOrder: async (serviceOrderId, partsData) => {
    try {
      const response = await apiClient.post(`/integration/service/orders/${serviceOrderId}/parts`, partsData);
      // Invalidate cache after update
      IntegrationService._invalidateCache(`service_order_parts_${serviceOrderId}`);
      return response.data;
    } catch (error) {
      console.error('Error assigning parts to service order:', error);
      throw error;
    }
  },
  
  getServicePartsAvailability: async (partIds) => {
    try {
      const response = await apiClient.post(`/integration/service/parts/availability`, { partIds });
      return response.data;
    } catch (error) {
      console.error('Error checking service parts availability:', error);
      throw error;
    }
  },
  
  getServicePartsUsageReport: async (startDate, endDate) => {
    try {
      const url = `/integration/service/reports/parts-usage?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`;
      const response = await apiClient.get(url);
      return response.data;
    } catch (error) {
      console.error('Error fetching service parts usage report:', error);
      throw error;
    }
  },
  
  // Sales Integration - Fixed typo from "Accesories" to "Accessories"
  getVehicleAccessories: async (vehicleId) => {
    const cacheKey = `vehicle_accessories_${vehicleId}`;
    const cached = IntegrationService._getCached(cacheKey);
    if (cached) return cached;
    
    try {
      const response = await apiClient.get(`/integration/sales/vehicles/${vehicleId}/accessories`);
      return IntegrationService._setCache(cacheKey, response.data);
    } catch (error) {
      console.error('Error fetching vehicle accessories:', error);
      throw error;
    }
  },
  
  // Add the missing implemented endpoints from SalesIntegrationController
  getCompatibleParts: async (vehicleId) => {
    const cacheKey = `compatible_parts_${vehicleId}`;
    const cached = IntegrationService._getCached(cacheKey);
    if (cached) return cached;
    
    try {
      const response = await apiClient.get(`/integration/sales/vehicles/${vehicleId}/compatible-parts`);
      return IntegrationService._setCache(cacheKey, response.data);
    } catch (error) {
      console.error('Error fetching compatible parts:', error);
      throw error;
    }
  },
  
  reservePartsForDeal: async (dealId, reservationRequest) => {
    try {
      const response = await apiClient.post(`/integration/sales/deals/${dealId}/reserve-parts`, reservationRequest);
      return response.data;
    } catch (error) {
      console.error('Error reserving parts for deal:', error);
      throw error;
    }
  },
  
  getDealPartsOrdersStatus: async (dealId) => {
    try {
      const response = await apiClient.get(`/integration/sales/deals/${dealId}/parts-orders`);
      return response.data;
    } catch (error) {
      console.error('Error fetching deal parts orders status:', error);
      throw error;
    }
  },
  
  getInstalledVehicleParts: async (vehicleId) => {
    const cacheKey = `installed_parts_${vehicleId}`;
    const cached = IntegrationService._getCached(cacheKey);
    if (cached) return cached;
    
    try {
      const response = await apiClient.get(`/integration/sales/vehicles/${vehicleId}/installed-parts`);
      return IntegrationService._setCache(cacheKey, response.data);
    } catch (error) {
      console.error('Error fetching installed vehicle parts:', error);
      throw error;
    }
  },
  
  getAccessoryInstallationEstimate: async (installationRequest) => {
    try {
      const response = await apiClient.post(`/integration/sales/accessories/installation-estimate`, installationRequest);
      return response.data;
    } catch (error) {
      console.error('Error fetching accessory installation estimate:', error);
      throw error;
    }
  },
  
  // Batch operations for better performance
  batchGetVehicleData: async (vehicleId) => {
    try {
      const [accessories, compatibleParts, installedParts] = await Promise.all([
        IntegrationService.getVehicleAccessories(vehicleId),
        IntegrationService.getCompatibleParts(vehicleId),
        IntegrationService.getInstalledVehicleParts(vehicleId)
      ]);
      
      return {
        accessories,
        compatibleParts,
        installedParts
      };
    } catch (error) {
      console.error('Error batch fetching vehicle data:', error);
      throw error;
    }
  },
  
  // Reporting Analytics Integration
  getPartsAnalyticsData: async (startDate, endDate, filters = {}) => {
    try {
      let url = `/integration/reporting/analytics/parts?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`;
      
      // Add any filters
      if (filters.categories) url += `&categories=${filters.categories.join(',')}`;
      if (filters.manufacturers) url += `&manufacturers=${filters.manufacturers.join(',')}`;
      
      const response = await apiClient.get(url);
      return response.data;
    } catch (error) {
      console.error('Error fetching parts analytics data:', error);
      throw error;
    }
  },
  
  getPartsMovementReport: async (startDate, endDate) => {
    try {
      const url = `/integration/reporting/parts-movement?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`;
      const response = await apiClient.get(url);
      return response.data;
    } catch (error) {
      console.error('Error fetching parts movement report:', error);
      throw error;
    }
  },
  
  getAIRecommendations: async (customerId, vehicleId = null) => {
    try {
      let url = `/integration/reporting/ai-recommendations?customerId=${customerId}`;
      if (vehicleId) url += `&vehicleId=${vehicleId}`;
      
      const response = await apiClient.get(url);
      return response.data;
    } catch (error) {
      console.error('Error fetching AI recommendations:', error);
      throw error;
    }
  }
};

// Error Handling and Logging Service
export const ErrorHandlingService = {
  // Severity levels
  SEVERITY: {
    INFO: 'info',
    WARNING: 'warning',
    ERROR: 'error',
    CRITICAL: 'critical'
  },

  // Error types
  ERROR_TYPES: {
    API: 'api_error',
    NETWORK: 'network_error',
    VALIDATION: 'validation_error',
    AUTH: 'authentication_error',
    INTEGRATION: 'integration_error',
    UNKNOWN: 'unknown_error'
  },
  
  // Log error with additional context
  logError: (error, context = {}) => {
    const timestamp = new Date().toISOString();
    const errorType = ErrorHandlingService._determineErrorType(error);
    const severity = ErrorHandlingService._determineSeverity(error, errorType);
    
    const errorLog = {
      timestamp,
      type: errorType,
      severity,
      message: error.message || 'Unknown error occurred',
      stack: error.stack,
      context: {
        ...context,
        url: window.location.href,
        userAgent: navigator.userAgent
      }
    };
    
    // Log to console in development
    if (process.env.NODE_ENV !== 'production') {
      console.error('[ErrorHandlingService]', errorLog);
    }
    
    // In production, send to error tracking service
    if (process.env.NODE_ENV === 'production') {
      // Send to error tracking system (could be implemented to use a service like Sentry)
      ErrorHandlingService._sendToErrorTrackingService(errorLog);
    }
    
    return errorLog;
  },
  
  // Handle API error with standard approach
  handleApiError: (error, userFriendlyMessage = null) => {
    // Log the error
    ErrorHandlingService.logError(error, { source: 'api_call' });
    
    // Extract error details
    const errorDetails = ErrorHandlingService._extractApiErrorDetails(error);
    
    // Handle special error cases
    if (errorDetails.status === 401) {
      // Unauthorized - redirect to login
      localStorage.removeItem('user');
      window.location.href = '/login';
      return { message: 'Your session has expired. Please log in again.' };
    }
    
    if (errorDetails.status === 403) {
      // Forbidden - user doesn't have permission
      return { message: 'You do not have permission to perform this action.' };
    }
    
    // Return user-friendly message or the extracted error message
    return {
      message: userFriendlyMessage || errorDetails.message || 'An error occurred. Please try again later.',
      details: process.env.NODE_ENV !== 'production' ? errorDetails : null
    };
  },
  
  // Determine error type based on error object
  _determineErrorType: (error) => {
    if (error.response) {
      return ErrorHandlingService.ERROR_TYPES.API;
    } else if (error.request) {
      return ErrorHandlingService.ERROR_TYPES.NETWORK;
    } else if (error.validationErrors) {
      return ErrorHandlingService.ERROR_TYPES.VALIDATION;
    } else if (error.status === 401 || error.status === 403) {
      return ErrorHandlingService.ERROR_TYPES.AUTH;
    } else if (error.isIntegrationError) {
      return ErrorHandlingService.ERROR_TYPES.INTEGRATION;
    }
    return ErrorHandlingService.ERROR_TYPES.UNKNOWN;
  },
  
  // Determine severity based on error
  _determineSeverity: (error, errorType) => {
    if (errorType === ErrorHandlingService.ERROR_TYPES.NETWORK || 
        errorType === ErrorHandlingService.ERROR_TYPES.CRITICAL) {
      return ErrorHandlingService.SEVERITY.CRITICAL;
    }
    
    if (errorType === ErrorHandlingService.ERROR_TYPES.API) {
      const status = error.response?.status;
      if (status >= 500) return ErrorHandlingService.SEVERITY.ERROR;
      if (status === 401 || status === 403) return ErrorHandlingService.SEVERITY.WARNING;
      return ErrorHandlingService.SEVERITY.INFO;
    }
    
    if (errorType === ErrorHandlingService.ERROR_TYPES.VALIDATION) {
      return ErrorHandlingService.SEVERITY.INFO;
    }
    
    return ErrorHandlingService.SEVERITY.ERROR;
  },
  
  // Extract useful details from API error
  _extractApiErrorDetails: (error) => {
    if (error.response) {
      // The request was made and the server responded with a status code outside of 2xx
      return {
        status: error.response.status,
        statusText: error.response.statusText,
        data: error.response.data,
        message: error.response.data?.message || error.message
      };
    } else if (error.request) {
      // The request was made but no response was received
      return {
        status: 0,
        message: 'No response received from server. Please check your connection.'
      };
    } else {
      // Something happened in setting up the request
      return {
        status: 0,
        message: error.message || 'An unknown error occurred.'
      };
    }
  },
  
  // Send error to error tracking service
  _sendToErrorTrackingService: (errorLog) => {
    // This would be implemented to use a service like Sentry
    // For now, just log to console that we would send this
    console.log('[ErrorTracking] Would send to error tracking service:', errorLog);
  },
  
  // Create a wrapped API function with error handling
  createSafeApiCall: (apiFunction, errorMessage = null) => {
    return async (...args) => {
      try {
        return await apiFunction(...args);
      } catch (error) {
        throw ErrorHandlingService.handleApiError(error, errorMessage);
      }
    };
  }
};

// Advanced Cache Service for API optimization
export const CacheService = {
  // Cache storage
  _storage: new Map(),
  
  // Default cache configuration
  _defaultConfig: {
    ttl: 5 * 60 * 1000, // 5 minutes TTL
    staleWhileRevalidate: true,
    cacheBustParam: '_t'
  },
  
  // Initialize cache with custom config
  init: (config = {}) => {
    CacheService._defaultConfig = {
      ...CacheService._defaultConfig,
      ...config
    };
    return CacheService;
  },
  
  // Get item from cache
  get: (key) => {
    const item = CacheService._storage.get(key);
    if (!item) return null;
    
    const now = Date.now();
    if (now - item.timestamp < item.config.ttl) {
      return item.data;
    }
    
    // Handle stale-while-revalidate
    if (item.config.staleWhileRevalidate) {
      setTimeout(() => {
        CacheService._storage.delete(key);
      }, 0);
      return item.data; // Return stale data while revalidation happens in background
    }
    
    CacheService._storage.delete(key);
    return null;
  },
  
  // Set item in cache
  set: (key, data, customConfig = {}) => {
    const config = { ...CacheService._defaultConfig, ...customConfig };
    CacheService._storage.set(key, {
      data,
      timestamp: Date.now(),
      config
    });
    return data;
  },
  
  // Remove item from cache
  remove: (key) => {
    return CacheService._storage.delete(key);
  },
  
  // Clear all cache
  clear: () => {
    CacheService._storage.clear();
  },
  
  // Extend CacheService with additional methods
  ...{
    // Get cache stats
    getStats: () => {
      const now = Date.now();
      let validItems = 0;
      let expiredItems = 0;
      
      CacheService._storage.forEach(item => {
        if (now - item.timestamp < item.config.ttl) {
          validItems++;
        } else {
          expiredItems++;
        }
      });
      
      return {
        totalItems: CacheService._storage.size,
        validItems,
        expiredItems
      };
    },
    
    // Create a cached version of any API function
    createCachedFunction: (fn, keyPrefix, customConfig = {}) => {
      return async (...args) => {
        const cacheKey = `${keyPrefix}:${JSON.stringify(args)}`;
        const cached = CacheService.get(cacheKey);
        
        if (cached) return cached;
        
        const result = await fn(...args);
        return CacheService.set(cacheKey, result, customConfig);
      };
    }
  }
};

// Create pre-configured cached versions of common API functions
export const CachedPartsService = {
  getAllParts: CacheService.createCachedFunction(PartsService.getAllParts, 'parts:getAll', { ttl: 2 * 60 * 1000 }),
  getPartsByCategory: CacheService.createCachedFunction(PartsService.getPartsByCategory, 'parts:byCategory', { ttl: 5 * 60 * 1000 }),
  getPartsByManufacturer: CacheService.createCachedFunction(PartsService.getPartsByManufacturer, 'parts:byManufacturer', { ttl: 5 * 60 * 1000 }),
  getPartsByFitment: CacheService.createCachedFunction(PartsService.getPartsByFitment, 'parts:byFitment', { ttl: 10 * 60 * 1000 })
};

// Create pre-configured cached versions of integration functions
export const CachedIntegrationService = {
  getVehicleAccessories: CacheService.createCachedFunction(
    IntegrationService.getVehicleAccessories, 
    'integration:vehicleAccessories', 
    { ttl: 15 * 60 * 1000 }
  ),
  getCompatibleParts: CacheService.createCachedFunction(
    IntegrationService.getCompatibleParts, 
    'integration:compatibleParts', 
    { ttl: 15 * 60 * 1000 }
  ),
  getInstalledVehicleParts: CacheService.createCachedFunction(
    IntegrationService.getInstalledVehicleParts, 
    'integration:installedParts', 
    { ttl: 30 * 60 * 1000 }
  ),
  getCustomerInfo: CacheService.createCachedFunction(
    IntegrationService.getCustomerInfo,
    'integration:customerInfo',
    { ttl: 10 * 60 * 1000 }
  )
};
