// @ts-nocheck
import { DocumentStatus, DocumentType } from '../types/document';

// API endpoints
const API_URL = '/api';

// Sign document and update its status
export const signDocument = async (
  documentId: string,
  signatureData: string,
  signerId: string = null
): Promise<boolean> => {
  try {
    const response = await fetch(`${API_URL}/documents/${documentId}/sign`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        signatureData,
        signerId,
        timestamp: new Date().toISOString()
      }),
    });
    
    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`);
    }
    
    return true;
  } catch (error) {
    console.error('Error signing document:', error);
    throw error;
  }
};

// Function to verify if a document has been signed
export const verifyDocumentSignature = async (documentId: string): Promise<{
  isSigned: boolean;
  signedBy?: string;
  signedAt?: Date;
  isValid?: boolean;
}> => {
  try {
    const response = await fetch(`${API_URL}/documents/${documentId}/verify`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });
    
    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`);
    }
    
    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Error verifying document signature:', error);
    throw error;
  }
};

// Create a new signed document
export const createSignedDocument = async (
  dealId: string,
  documentType: DocumentType,
  fileData: File,
  signatureData: string
): Promise<any> => {
  try {
    // First upload the document
    const formData = new FormData();
    formData.append('file', fileData);
    formData.append('dealId', dealId);
    formData.append('type', documentType);
    formData.append('status', DocumentStatus.Signed);
    
    const uploadResponse = await fetch(`${API_URL}/documents`, {
      method: 'POST',
      body: formData,
    });
    
    if (!uploadResponse.ok) {
      throw new Error(`Error ${uploadResponse.status}: ${uploadResponse.statusText}`);
    }
    
    const document = await uploadResponse.json();
    
    // Now sign the document
    await signDocument(document.id, signatureData);
    
    return document;
  } catch (error) {
    console.error('Error creating and signing document:', error);
    throw error;
  }
};
