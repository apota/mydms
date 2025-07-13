// types.ts - Common TypeScript interfaces for the application

// Error type for form validation
export type ValidationError<T> = Partial<Record<keyof T, string>>;

// Appointment related types
export interface AppointmentFormData {
  customerId: string;
  vehicleId: string;
  appointmentType: string;
  date: Date | null;
  time: string;
  duration: number;
  customerConcerns: string;
  transportationType: string;
  contactPhone: string;
  contactEmail: string;
}

// Customer related types
export interface Customer {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  address?: string;
}

// Vehicle related types
export interface Vehicle {
  id: string;
  customerId: string;
  make: string;
  model: string;
  year: number;
  vin: string;
  licensePlate?: string;
  color?: string;
  mileage?: number;
}

// Service inspection related types
export interface InspectionFormData {
  repairOrderId: string;
  technicianId: string;
  type: string;  
  notes: string;
  inspectionItems: InspectionItem[];
  recommendations: ServiceRecommendation[];
}

export interface InspectionItem {
  id: string;
  name: string;
  category: string;
  status: 'pass' | 'fail' | 'warning' | '';
  notes?: string;
  images?: string[];
}

export interface ServiceRecommendation {
  id?: string;
  description: string;
  urgency: 'Critical' | 'Soon' | 'Future';
  estimatedPrice: number;
}

// API response types
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  validation?: Record<string, string>;
}

// Service Job related types
export interface ServiceJob {
  id: string;
  repairOrderId: string;
  description: string;
  jobType: string;
  status: ServiceJobStatus;
  laborOperationCode: string;
  laborOperationDescription?: string;
  estimatedHours: number;
  actualHours: number;
  startTime?: Date;
  endTime?: Date;
  technicianId?: string;
  technicianName?: string;
  notes?: string;
  parts?: ServiceJobPart[];
}

export type ServiceJobStatus = 'NotStarted' | 'InProgress' | 'Completed' | 'WaitingParts' | 'OnHold';

export interface ServiceJobPart {
  id?: string;
  partId: string;
  partNumber?: string;
  description: string;
  quantity: number;
  unitPrice: number;
  totalAmount: number;
  status: 'Available' | 'Ordered' | 'BackOrdered' | 'Used';
}

export interface JobCompletionData {
  jobId: string;
  actualHours: number;
  notes?: string;
  completionTime?: Date;
  status: ServiceJobStatus;
  technicianSignature?: string;
}

// Repair Order types
export interface RepairOrder {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName: string;
  vehicleId: string;
  vehicleInfo: string;
  createdDate: Date;
  status: RepairOrderStatus;
  estimatedCompletionDate: Date;
  actualCompletionDate?: Date;
  serviceTotalAmount: number;
  partsTotalAmount: number;
  totalAmount: number;
  notes?: string;
  serviceJobs: ServiceJob[];
  inspections: InspectionSummary[];
}

export type RepairOrderStatus = 'Created' | 'InProgress' | 'WaitingParts' | 'WaitingCustomerApproval' | 'Completed' | 'Invoiced' | 'Paid' | 'Cancelled';

export interface InspectionSummary {
  id: string;
  type: string;
  status: string;
  technicianId: string;
  technicianName: string;
  completedDate?: Date;
  recommendationCount: number;
}

// Integration with other modules
export interface CrmCustomer {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  address: {
    street: string;
    city: string;
    state: string;
    zipCode: string;
  };
  loyaltyLevel?: string;
  lastVisitDate?: Date;
}

export interface InventoryVehicle {
  id: string;
  customerId: string;
  make: string;
  model: string;
  year: number;
  vin: string;
  licensePlate: string;
  color: string;
  mileage: number;
  lastServiceDate?: Date;
  serviceHistory: ServiceHistoryItem[];
}

export interface ServiceHistoryItem {
  serviceDate: Date;
  description: string;
  mileage: number;
  type: string;
  technicianNotes?: string;
}

export interface PartInfo {
  id: string;
  partNumber: string;
  description: string;
  category: string;
  price: number;
  taxRate: number;
  quantityInStock: number;
  location: string;
  isBackOrdered: boolean;
  estimatedArrivalDate?: Date;
}

export interface PartsOrderRequest {
  repairOrderId: string;
  jobId?: string;
  parts: {
    partId: string;
    partNumber?: string;
    quantity: number;
    isUrgent: boolean;
  }[];
  notes?: string;
}

export interface TechnicianInfo {
  id: string;
  name: string;
  specializations: string[];
  certifications: string[];
  isAvailable: boolean;
  nextAvailableTime?: Date;
}

export interface ServiceBayInfo {
  id: string;
  name: string;
  type: string;
  isAvailable: boolean;
  currentJobId?: string;
  nextAvailableTime?: Date;
}

export interface InvoiceRequest {
  repairOrderId: string;
  customerId: string;
  items: InvoiceItem[];
  taxRate: number;
  discountAmount?: number;
  notes?: string;
}

export interface InvoiceItem {
  description: string;
  quantity: number;
  unitPrice: number;
  itemType: 'Labor' | 'Parts' | 'Other';
  taxable: boolean;
}

export interface PaymentProcessRequest {
  invoiceId: string;
  amount: number;
  paymentMethod: 'CreditCard' | 'Cash' | 'Check' | 'Financing';
  paymentReference?: string;
}
