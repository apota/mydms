import axios from 'axios';

// Always use relative URL so nginx proxy works in Docker and local
const API_URL = '/api';

// Add request interceptor to include auth token
axios.interceptors.request.use(config => {
  const token = localStorage.getItem('auth_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Customer API calls
export const CustomerService = {
  getAll: async (skip = 0, take = 50) => {
    const response = await axios.get(`${API_URL}/customers?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  getById: async (id) => {
    const response = await axios.get(`${API_URL}/customers/${id}`);
    return response.data;
  },
  
  create: async (customerData) => {
    const response = await axios.post(`${API_URL}/customers`, customerData);
    return response.data;
  },
  
  update: async (id, customerData) => {
    const response = await axios.put(`${API_URL}/customers/${id}`, customerData);
    return response.data;
  },
  
  delete: async (id) => {
    const response = await axios.delete(`${API_URL}/customers/${id}`);
    return response.status === 204;
  },
  
  getInteractions: async (customerId, skip = 0, take = 50) => {
    const response = await axios.get(`${API_URL}/customers/${customerId}/interactions?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  getVehicles: async (customerId, skip = 0, take = 50) => {
    const response = await axios.get(`${API_URL}/customers/${customerId}/vehicles?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  search: async (query, skip = 0, take = 50) => {
    const response = await axios.get(`${API_URL}/customers/search?query=${query}&skip=${skip}&take=${take}`);
    return response.data;
  }
};

// Integration API calls
export const IntegrationService = {
  getCustomer360: async (customerId) => {
    const response = await axios.get(`${API_URL}/integration/customer360/${customerId}`);
    return response.data;
  },
  
  synchronizeCustomer: async (customerId) => {
    const response = await axios.post(`${API_URL}/integration/synchronize/${customerId}`);
    return response.data;
  },
  
  getCustomerInteractions: async (customerId, startDate, endDate) => {
    const response = await axios.get(
      `${API_URL}/integration/interactions/${customerId}?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`
    );
    return response.data;
  },
  
  propagateCustomerChanges: async (customerId) => {
    const response = await axios.post(`${API_URL}/integration/propagate/${customerId}`);
    return response.data;
  }
};

// Loyalty API calls
export const LoyaltyService = {
  // Get customer loyalty status and dashboard information
  getCustomerLoyaltyStatus: async (customerId) => {
    const response = await axios.get(`${API_URL}/loyalty/customers/${customerId}/status`);
    return response.data;
  },
  
  // Add loyalty points to customer account
  addLoyaltyPoints: async (customerId, pointsRequest) => {
    const response = await axios.post(`${API_URL}/loyalty/customers/${customerId}/points`, pointsRequest);
    return response.data;
  },
  
  // Redeem loyalty points for a reward
  redeemPoints: async (customerId, redeemRequest) => {
    const response = await axios.post(`${API_URL}/loyalty/customers/${customerId}/redeem`, redeemRequest);
    return response.data;
  },
  
  // Get loyalty point transaction history
  getPointsHistory: async (customerId) => {
    const response = await axios.get(`${API_URL}/loyalty/customers/${customerId}/transactions`);
    return response.data;
  },
  
  // Get available rewards (optionally filtered by tier)
  getAvailableRewards: async (tierLevel = null) => {
    const url = tierLevel ? 
      `${API_URL}/loyalty/rewards?tierLevel=${tierLevel}` : 
      `${API_URL}/loyalty/rewards`;
    const response = await axios.get(url);
    return response.data;
  },
  
  // Get redeemed rewards for a customer
  getRedeemedRewards: async (customerId) => {
    const response = await axios.get(`${API_URL}/loyalty/customers/${customerId}/redemptions`);
    return response.data;
  },
  
  // Update customer loyalty tier (admin function)
  updateLoyaltyTier: async (customerId, tierUpdateRequest) => {
    const response = await axios.put(`${API_URL}/loyalty/customers/${customerId}/tier`, tierUpdateRequest);
    return response.data;
  },
  
  // Calculate points for a transaction
  calculatePoints: async (calculateRequest) => {
    const response = await axios.post(`${API_URL}/loyalty/calculate-points`, calculateRequest);
    return response.data;
  },
  
  // Get loyalty dashboard data
  getDashboardData: async () => {
    const response = await axios.get(`${API_URL}/loyalty/dashboard`);
    return response.data;
  },

  // Legacy methods for backwards compatibility (marked as deprecated)
  /** @deprecated Use getCustomerLoyaltyStatus instead */
  getCustomerLoyaltyStatus_Old: async (customerId) => {
    const response = await axios.get(`${API_URL}/loyalty/status/${customerId}`);
    return response.data;
  },
  
  /** @deprecated Use addLoyaltyPoints instead */
  awardLoyaltyPoints: async (customerId, pointsDto) => {
    const response = await axios.post(`${API_URL}/loyalty/award-points/${customerId}`, pointsDto);
    return response.data;
  },
  
  /** @deprecated Use redeemPoints instead */
  redeemReward: async (customerId, rewardDto) => {
    const response = await axios.post(`${API_URL}/loyalty/redeem/${customerId}`, rewardDto);
    return response.data;
  }
};

// Communication API calls 
export const CommunicationService = {
  getCustomerCommunications: async (customerId, skip = 0, take = 50) => {
    const response = await axios.get(`${API_URL}/communications/${customerId}?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  sendCustomerMessage: async (message) => {
    const response = await axios.post(`${API_URL}/communications/send`, message);
    return response.data;
  },
  
  getTemplates: async () => {
    const response = await axios.get(`${API_URL}/communications/templates`);
    return response.data;
  },
  
  getCommunicationAnalytics: async (customerId) => {
    const response = await axios.get(`${API_URL}/communications/analytics/${customerId}`);
    return response.data;
  }
};

// Customer Journey API calls
export const CustomerJourneyService = {
  getCustomerJourney: async (customerId) => {
    const response = await axios.get(`${API_URL}/journey/${customerId}`);
    return response.data;
  },
  
  updateJourneyStage: async (customerId, stageUpdate) => {
    const response = await axios.put(`${API_URL}/journey/${customerId}/stage`, stageUpdate);
    return response.data;
  },
  
  getTouchpointAnalytics: async (customerId) => {
    const response = await axios.get(`${API_URL}/journey/${customerId}/touchpoints`);
    return response.data;
  }
};

// Campaign API calls expanded
export const CampaignService = {
  getAll: async (skip = 0, take = 50) => {
    const response = await axios.get(`${API_URL}/campaigns?skip=${skip}&take=${take}`);
    return response.data;
  },
  
  getById: async (id) => {
    const response = await axios.get(`${API_URL}/campaigns/${id}`);
    return response.data;
  },
  
  create: async (campaignData) => {
    const response = await axios.post(`${API_URL}/campaigns`, campaignData);
    return response.data;
  },
  
  update: async (id, campaignData) => {
    const response = await axios.put(`${API_URL}/campaigns/${id}`, campaignData);
    return response.data;
  },
  
  delete: async (id) => {
    const response = await axios.delete(`${API_URL}/campaigns/${id}`);
    return response.status === 204;
  },
  
  getPerformanceMetrics: async (campaignId) => {
    const response = await axios.get(`${API_URL}/campaigns/${campaignId}/metrics`);
    return response.data;
  },
  
  addCustomersToCampaign: async (campaignId, customerIds) => {
    const response = await axios.post(`${API_URL}/campaigns/${campaignId}/customers`, { customerIds });
    return response.data;
  }
};

// AI Analytics API calls
export const AIAnalyticsService = {
  analyzeSentiment: async (text) => {
    const response = await axios.post(`${API_URL}/analytics/sentiment`, { text });
    return response.data;
  },
  
  analyzeSentimentBatch: async (texts) => {
    const response = await axios.post(`${API_URL}/analytics/sentiment/batch`, { texts });
    return response.data;
  },
  
  extractTopics: async (text) => {
    const response = await axios.post(`${API_URL}/analytics/topics`, { text });
    return response.data;
  },
  
  predictCustomerChurn: async (customerId) => {
    const response = await axios.get(`${API_URL}/analytics/churn-prediction/${customerId}`);
    return response.data;
  },
  
  getNextBestAction: async (customerId) => {
    const response = await axios.get(`${API_URL}/analytics/next-best-action/${customerId}`);
    return response.data;
  },
  
  getContentRecommendations: async (customerId, context = 'email') => {
    const response = await axios.get(`${API_URL}/analytics/content-recommendations/${customerId}?context=${context}`);
    return response.data;
  },
  
  getOptimalContactTime: async (customerId, channel) => {
    const response = await axios.get(`${API_URL}/analytics/optimal-contact-time/${customerId}?channel=${channel}`);
    return response.data;
  },
  
  getCampaignOptimization: async (campaignId) => {
    const response = await axios.get(`${API_URL}/analytics/campaign-optimization/${campaignId}`);
    return response.data;
  },
  
  discoverCustomerSegments: async () => {
    const response = await axios.get(`${API_URL}/analytics/discover-segments`);
    return response.data;
  }
};

// Customer Survey Service
export const CustomerSurveyService = {
  getAnalytics: async (startDate, endDate) => {
    const response = await axios.get(`${API_URL}/surveys/analytics?startDate=${startDate}&endDate=${endDate}`);
    return response.data;
  },
  
  getSatisfactionTrend: async (startDate, endDate, timeInterval) => {
    const response = await axios.get(`${API_URL}/surveys/satisfaction-trend?startDate=${startDate}&endDate=${endDate}&interval=${timeInterval}`);
    return response.data;
  }
};
