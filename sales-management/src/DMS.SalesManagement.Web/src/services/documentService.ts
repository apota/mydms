// @ts-nocheck 
import axios from 'axios';
import { Document, DocumentCategory, DocumentStatus, CreateDocumentRequest, UpdateDocumentRequest } from '../types/document';

const API_URL = '/api';

// Fetch all documents with optional filters
export const fetchDocuments = async (
  category?: DocumentCategory,
  entityType?: string,
  entityId?: string
): Promise<Document[]> => {
  try {
    const params = { ...(category && { category }), ...(entityType && { entityType }), ...(entityId && { entityId }) };
    const response = await axios.get(`${API_URL}/documents`, { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching documents:', error);
    throw error;
  }
};

// Fetch a single document by ID
export const fetchDocument = async (id: string): Promise<Document> => {
  try {
    const response = await axios.get(`${API_URL}/documents/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching document ${id}:`, error);
    throw error;
  }
};

// Upload a new document
export const createDocument = async (document: CreateDocumentRequest): Promise<Document> => {
  try {
    const formData = new FormData();
    formData.append('file', document.file);
    formData.append('title', document.title);
    
    if (document.description) formData.append('description', document.description);
    formData.append('category', document.category);
    if (document.entityId) formData.append('entityId', document.entityId);
    if (document.entityType) formData.append('entityType', document.entityType);
    if (document.tags && document.tags.length) formData.append('tags', JSON.stringify(document.tags));
    if (document.expiryDate) formData.append('expiryDate', document.expiryDate);

    const response = await axios.post(`${API_URL}/documents`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  } catch (error) {
    console.error('Error creating document:', error);
    throw error;
  }
};

// Update existing document
export const updateDocument = async (id: string, document: UpdateDocumentRequest): Promise<Document> => {
  try {
    const response = await axios.put(`${API_URL}/documents/${id}`, document);
    return response.data;
  } catch (error) {
    console.error(`Error updating document ${id}:`, error);
    throw error;
  }
};

// Delete a document
export const deleteDocument = async (id: string): Promise<void> => {
  try {
    await axios.delete(`${API_URL}/documents/${id}`);
  } catch (error) {
    console.error(`Error deleting document ${id}:`, error);
    throw error;
  }
};

// Generate a signed document URL for viewing/downloading
export const getSignedDocumentUrl = async (id: string): Promise<{ url: string; expiresAt: string }> => {
  try {
    const response = await axios.get(`${API_URL}/documents/${id}/url`);
    return response.data;
  } catch (error) {
    console.error(`Error getting signed URL for document ${id}:`, error);
    throw error;
  }
};

// Change document status
export const updateDocumentStatus = async (id: string, status: DocumentStatus): Promise<Document> => {
  try {
    const response = await axios.put(`${API_URL}/documents/${id}/status`, { status });
    return response.data;
  } catch (error) {
    console.error(`Error updating document status for ${id}:`, error);
    throw error;
  }
};
