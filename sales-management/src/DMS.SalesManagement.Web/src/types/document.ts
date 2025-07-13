// @ts-nocheck 
export interface Document {
  id: string;
  title: string;
  description?: string;
  fileUrl: string;
  fileType: string;
  fileSize: number;
  category: DocumentCategory;
  entityId?: string;
  entityType?: EntityType;
  tags?: string[];
  createdBy: string;
  createdAt: string;
  updatedAt: string;
  status: DocumentStatus;
  expiryDate?: string;
}

export enum DocumentCategory {
  Contract = 'Contract',
  Insurance = 'Insurance',
  Registration = 'Registration',
  Finance = 'Finance',
  Trade = 'Trade',
  Service = 'Service',
  Other = 'Other'
}

export enum DocumentStatus {
  Draft = 'Draft',
  PendingSignature = 'Pending Signature',
  Signed = 'Signed',
  Expired = 'Expired',
  Completed = 'Completed',
  Archived = 'Archived'
}

export enum EntityType {
  Deal = 'Deal',
  Customer = 'Customer',
  Vehicle = 'Vehicle',
  Employee = 'Employee'
}

export interface CreateDocumentRequest {
  title: string;
  description?: string;
  category: DocumentCategory;
  entityId?: string;
  entityType?: EntityType;
  tags?: string[];
  expiryDate?: string;
  file: File;
}

export interface UpdateDocumentRequest {
  title?: string;
  description?: string;
  category?: DocumentCategory;
  tags?: string[];
  status?: DocumentStatus;
  expiryDate?: string;
}
