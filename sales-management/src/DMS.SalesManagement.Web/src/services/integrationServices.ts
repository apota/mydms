// @ts-nocheck
import axios from 'axios';
import { Document, DocumentCategory, DocumentStatus } from '../types/document';

interface DocumentWithPreview extends Document {
  previewUrl?: string;
}

interface Vehicle {
  id: string;
  make: string;
  model: string;
  year: number;
  vin: string;
  stockNumber: string;
  price: number;
  type: string; // 'new' or 'used'
  imageUrl?: string;
  features?: string[];
}

interface Customer {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address?: {
    street: string;
    city: string;
    state: string;
    zip: string;
  };
}

// Create a generic error handling function for API calls
const handleApiError = (error: any, customMessage: string) => {
  console.error(`${customMessage}:`, error);
  if (axios.isAxiosError(error)) {
    throw new Error(error.response?.data?.message || error.message || 'An unknown error occurred');
  }
  throw error;
};

// Export individual functions for better testability
export const getVehiclesByType = async (type: string): Promise<Vehicle[]> => {
  try {
    const response = await axios.get(`/api/inventory/vehicles`, {
      params: { type }
    });
    return response.data;
  } catch (error) {
    console.error(`Error fetching vehicles by type ${type}:`, error);
    return [];
  }
};

export const getCrmCustomers = async (searchTerm?: string): Promise<Customer[]> => {
  try {
    const params = searchTerm ? { search: searchTerm } : {};
    const response = await axios.get('/api/crm/customers', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching CRM customers:', error);
    return [];
  }
}; 

// Default export for backward compatibility
export default {
  // API for fetching documents with integration to CRM module
  async getDocumentsForCustomer(customerId: string): Promise<Document[]> {
    try {
      const response = await axios.get(`/api/customers/${customerId}/documents`);
      return response.data;
    } catch (error) {
      handleApiError(error, `Error fetching documents for customer ${customerId}`);
    }
  },
  
  // API for fetching documents with integration to Inventory module
  async getDocumentsForVehicle(vehicleId: string): Promise<Document[]> {
    try {
      const response = await axios.get(`/api/vehicles/${vehicleId}/documents`);
      return response.data;
    } catch (error) {
      handleApiError(error, `Error fetching documents for vehicle ${vehicleId}`);
    }
  },
  
  // API for generating documents with integration to Financial Management module
  async generateFinancialDocument(dealId: string, templateId: string): Promise<Document> {
    try {
      const response = await axios.post('/api/financial-documents/generate', {
        dealId,
        templateId
      });
      return response.data;
    } catch (error) {
      handleApiError(error, 'Error generating financial document');
    }
  },
  
  // API for integration with third-party document signing services
  async sendDocumentForExternalSigning(documentId: string, signerEmail: string): Promise<{ 
    signingUrl: string, 
    signingRequestId: string 
  }> {
    try {
      const response = await axios.post(`/api/documents/${documentId}/external-signing`, {
        signerEmail
      });
      return response.data;
    } catch (error) {
      handleApiError(error, 'Error sending document for external signing');
    }
  },
  
  // Get document preview with caching
  async getDocumentPreview(documentId: string): Promise<DocumentWithPreview> {
    try {
      const response = await axios.get(`/api/documents/${documentId}/preview`);
      return {
        ...response.data.document,
        previewUrl: response.data.previewUrl
      };
    } catch (error) {
      handleApiError(error, `Error getting preview for document ${documentId}`);
    }
  }
};
