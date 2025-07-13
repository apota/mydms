import axios from 'axios';

// API base URL - this should match your actual API base
const API_URL = '/api';

// Helper function to handle API errors
const handleError = (error) => {
  console.error('API Error:', error);
  if (error.response) {
    // The request was made and the server responded with a status code
    // that falls out of the range of 2xx
    console.error('Response data:', error.response.data);
    console.error('Response status:', error.response.status);
    throw new Error(error.response.data.message || 'An error occurred with the API request');
  } else if (error.request) {
    // The request was made but no response was received
    throw new Error('No response received from server. Please check your connection.');
  } else {
    // Something happened in setting up the request that triggered an Error
    throw new Error(error.message || 'An error occurred with the API request');
  }
};

// Vehicle API calls
const getVehicles = async (params = {}) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/vehicles`, { params });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getVehicleById = async (id) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/vehicles/${id}`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const createVehicle = async (vehicleData) => {
  try {
    const response = await axios.post(`${API_URL}/inventory/vehicles`, vehicleData);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const updateVehicle = async (id, vehicleData) => {
  try {
    const response = await axios.put(`${API_URL}/inventory/vehicles/${id}`, vehicleData);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const deleteVehicle = async (id) => {
  try {
    const response = await axios.delete(`${API_URL}/inventory/vehicles/${id}`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

// Vehicle Cost API calls
const getVehicleCosts = async (vehicleId) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/costs/${vehicleId}`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const addVehicleCost = async (costData) => {
  try {
    const response = await axios.post(`${API_URL}/inventory/costs`, costData);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const updateVehicleCost = async (id, costData) => {
  try {
    const response = await axios.put(`${API_URL}/inventory/costs/${id}`, costData);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const deleteVehicleCost = async (id) => {
  try {
    const response = await axios.delete(`${API_URL}/inventory/costs/${id}`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

// Vehicle Operations API calls
const transferVehicle = async (vehicleId, transferData) => {
  try {
    const response = await axios.post(`${API_URL}/inventory/operations/vehicles/${vehicleId}/transfer`, transferData);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const updateVehicleStatus = async (vehicleId, statusData) => {
  try {
    const response = await axios.post(`${API_URL}/inventory/operations/vehicles/${vehicleId}/status`, statusData);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

// Vehicle Image API calls
const uploadVehicleImages = async (vehicleId, formData, progressCallback = null) => {
  try {
    const config = {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    };
    
    if (progressCallback) {
      config.onUploadProgress = (progressEvent) => {
        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
        progressCallback(percentCompleted);
      };
    }
    
    const response = await axios.post(
      `${API_URL}/inventory/operations/vehicles/${vehicleId}/images`, 
      formData,
      config
    );
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getVehicleImages = async (vehicleId) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/vehicles/${vehicleId}/images`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const setVehiclePrimaryImage = async (vehicleId, imageId) => {
  try {
    const response = await axios.put(`${API_URL}/inventory/vehicles/${vehicleId}/images/${imageId}/primary`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const deleteVehicleImage = async (vehicleId, imageId) => {
  try {
    const response = await axios.delete(`${API_URL}/inventory/vehicles/${vehicleId}/images/${imageId}`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

// Vehicle Document API calls
const uploadVehicleDocuments = async (vehicleId, formData) => {
  try {
    const response = await axios.post(`${API_URL}/inventory/operations/vehicles/${vehicleId}/documents`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getVehicleDocuments = async (vehicleId) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/vehicles/${vehicleId}/documents`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const deleteVehicleDocument = async (vehicleId, documentId) => {
  try {
    const response = await axios.delete(`${API_URL}/inventory/vehicles/${vehicleId}/documents/${documentId}`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

// Location API calls
const getLocations = async () => {
  try {
    const response = await axios.get(`${API_URL}/inventory/locations`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

// Inventory Stats and Analytics API calls
const getInventoryStats = async () => {
  try {
    const response = await axios.get(`${API_URL}/inventory/stats`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getAgingReport = async (params = {}) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/analytics/aging`, { params });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getValuationReport = async (params = {}) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/analytics/valuation`, { params });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getTurnoverMetrics = async (params = {}) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/analytics/turnover`, { params });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getPriceCompetitiveness = async (params = {}) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/analytics/price-competitiveness`, { params });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getInventoryMixAnalysis = async (params = {}) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/analytics/inventory-mix`, { params });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

// Import/Export Operations
const importVehicles = async (formData) => {
  try {
    const response = await axios.post(`${API_URL}/inventory/operations/import`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getImportStatus = async (importId) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/operations/import/${importId}/status`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const exportVehicles = async (params = {}) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/operations/export`, { 
      params,
      responseType: 'blob'
    });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

// Marketplace API calls
const getAvailableMarketplaces = async () => {
  try {
    const response = await axios.get(`${API_URL}/marketplace/available`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getListableVehicles = async () => {
  try {
    const response = await axios.get(`${API_URL}/inventory/vehicles/listable`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const listVehicleOnMarketplaces = async (vehicleId, marketplaceIds) => {
  try {
    const response = await axios.post(`${API_URL}/marketplace/vehicle/${vehicleId}/list`, {
      marketplaceIds
    });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const updateVehicleOnMarketplaces = async (vehicleId, marketplaceIds) => {
  try {
    const response = await axios.put(`${API_URL}/marketplace/vehicle/${vehicleId}/update`, {
      marketplaceIds
    });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const removeVehicleFromMarketplaces = async (vehicleId, marketplaceIds) => {
  try {
    const response = await axios.delete(`${API_URL}/marketplace/vehicle/${vehicleId}/remove`, {
      data: { marketplaceIds }
    });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getVehicleListingStatus = async (vehicleId) => {
  try {
    const response = await axios.get(`${API_URL}/marketplace/vehicle/${vehicleId}/status`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getVehicleListingStats = async (vehicleId) => {
  try {
    const response = await axios.get(`${API_URL}/marketplace/vehicle/${vehicleId}/stats`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const verifyMarketplaceCredentials = async (marketplaceId, credentials) => {
  try {
    const response = await axios.post(`${API_URL}/marketplace/verify-credentials`, {
      marketplaceId,
      credentials
    });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const synchronizeInventory = async () => {
  try {
    const response = await axios.post(`${API_URL}/marketplace/sync`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

// Advanced search methods
const searchVehicles = async (searchCriteria) => {
  try {
    const response = await axios.post(`${API_URL}/search/vehicles`, searchCriteria);
    return {
      vehicles: response.data,
      totalCount: parseInt(response.headers['x-total-count'] || response.data.length)
    };
  } catch (error) {
    return handleError(error);
  }
};

const findSimilarVehicles = async (vehicleId, maxResults = 5) => {
  try {
    const response = await axios.get(`${API_URL}/search/vehicles/${vehicleId}/similar`, {
      params: { maxResults }
    });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getRecommendedVehicles = async (customerPreferences, maxResults = 10) => {
  try {
    const response = await axios.post(`${API_URL}/search/vehicles/recommended`, customerPreferences, {
      params: { maxResults }
    });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getAvailableMakes = async () => {
  try {
    const response = await axios.get(`${API_URL}/inventory/makes`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getAvailableModelsByMake = async (make) => {
  try {
    const response = await axios.get(`${API_URL}/inventory/models`, {
      params: { make }
    });
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getPopularFeatures = async () => {
  try {
    const response = await axios.get(`${API_URL}/inventory/features/popular`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

// Import/Export methods
const getMappingTemplates = async () => {
  try {
    const response = await axios.get(`${API_URL}/inventory/import/templates`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getManufacturerCodes = async () => {
  try {
    const response = await axios.get(`${API_URL}/inventory/import/manufacturers`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const getAuctionCodes = async () => {
  try {
    const response = await axios.get(`${API_URL}/inventory/import/auctions`);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const importVehiclesFromCsv = async (file, templateName) => {
  try {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('templateName', templateName);
    
    const response = await axios.post(`${API_URL}/inventory/import/csv`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const importVehiclesFromManufacturer = async (manufacturerCode, options) => {
  try {
    const response = await axios.post(`${API_URL}/inventory/import/manufacturer/${manufacturerCode}`, options);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

const importVehiclesFromAuction = async (auctionCode, options) => {
  try {
    const response = await axios.post(`${API_URL}/inventory/import/auction/${auctionCode}`, options);
    return response.data;
  } catch (error) {
    return handleError(error);
  }
};

// Create an object with all the service functions
const inventoryService = {
  getVehicles,
  getVehicleById,
  createVehicle,
  updateVehicle,
  deleteVehicle,
  getVehicleCosts,
  addVehicleCost,
  updateVehicleCost,
  deleteVehicleCost,
  transferVehicle,
  updateVehicleStatus,
  uploadVehicleImages,
  getVehicleImages,
  setVehiclePrimaryImage,
  deleteVehicleImage,
  uploadVehicleDocuments,
  getVehicleDocuments,
  deleteVehicleDocument,
  getLocations,
  getInventoryStats,
  getAgingReport,
  getValuationReport,
  getTurnoverMetrics,
  getPriceCompetitiveness,
  getInventoryMixAnalysis,
  importVehicles,
  getImportStatus,
  exportVehicles,
  getAvailableMarketplaces,
  getListableVehicles,
  listVehicleOnMarketplaces,
  updateVehicleOnMarketplaces,
  removeVehicleFromMarketplaces,
  getVehicleListingStatus,
  getVehicleListingStats,
  verifyMarketplaceCredentials,
  synchronizeInventory,
  
  // Workflow API calls
  getWorkflowDefinitions: async (includeInactive = false) => {
    try {
      const response = await axios.get(`${API_URL}/workflows/definitions`, {
        params: { includeInactive }
      });
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  getWorkflowDefinition: async (id) => {
    try {
      const response = await axios.get(`${API_URL}/workflows/definitions/${id}`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  getVehicleWorkflows: async (vehicleId) => {
    try {
      const response = await axios.get(`${API_URL}/workflows/vehicle/${vehicleId}`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  getWorkflowInstance: async (id) => {
    try {
      const response = await axios.get(`${API_URL}/workflows/instances/${id}`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  createWorkflowInstance: async (data) => {
    try {
      const response = await axios.post(`${API_URL}/workflows/instances`, data);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  advanceWorkflow: async (id) => {
    try {
      const response = await axios.post(`${API_URL}/workflows/instances/${id}/advance`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  // Reconditioning workflow API calls
  getVehiclesInReconditioning: async () => {
    try {
      const response = await axios.get(`${API_URL}/workflows/reconditioning/vehicles`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  createReconditioningWorkflow: async (vehicleId) => {
    try {
      const response = await axios.post(`${API_URL}/workflows/reconditioning/${vehicleId}`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  // Aging workflow API calls
  getAgingVehicles: async (daysThreshold) => {
    try {
      const response = await axios.get(`${API_URL}/workflows/aging/vehicles`, {
        params: { daysThreshold }
      });
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  createAgingWorkflow: async (vehicleId) => {
    try {
      const response = await axios.post(`${API_URL}/workflows/aging/${vehicleId}`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  // Acquisition workflow API calls
  getVehiclesInAcquisition: async () => {
    try {
      const response = await axios.get(`${API_URL}/workflows/acquisition/vehicles`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  createAcquisitionWorkflow: async (vehicleId) => {
    try {
      const response = await axios.post(`${API_URL}/workflows/acquisition/${vehicleId}`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  getAcquisitionStatistics: async (startDate, endDate) => {
    try {
      const response = await axios.get(`${API_URL}/workflows/acquisition/statistics`, {
        params: { startDate, endDate }
      });
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  recordVehicleInspection: async (inspection) => {
    try {
      const response = await axios.post(`${API_URL}/workflows/acquisition/inspections`, inspection);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  getVehicleInspection: async (vehicleId) => {
    try {
      const response = await axios.get(`${API_URL}/workflows/acquisition/${vehicleId}/inspection`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  updateAcquisitionDocuments: async (vehicleId, documentIds) => {
    try {
      const response = await axios.post(`${API_URL}/workflows/acquisition/${vehicleId}/documents`, {
        documentIds
      });
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  completeVehicleIntake: async (vehicleId, notes) => {
    try {
      const response = await axios.post(`${API_URL}/workflows/acquisition/${vehicleId}/complete`, {
        notes
      });
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  // Market Analysis
  getMarketAnalysis: async (vehicleId) => {
    try {
      const response = await axios.get(`${API_URL}/inventory/market-analysis/${vehicleId}`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  getMarketTrends: async (year, make, model, months = 6) => {
    try {
      const response = await axios.get(`${API_URL}/inventory/market-trends`, {
        params: { year, make, model, months }
      });
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  getCompetitiveListings: async (vehicleId, radius = 50) => {
    try {
      const response = await axios.get(`${API_URL}/inventory/competitive-listings/${vehicleId}`, {
        params: { radius }
      });
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
  
  getPriceRecommendations: async (vehicleId) => {
    try {
      const response = await axios.get(`${API_URL}/inventory/price-recommendations/${vehicleId}`);
      return response.data;
    } catch (error) {
      return handleError(error);
    }
  },
};

export default inventoryService;
