// @ts-nocheck

// Accessories for vehicles
export interface AccessoryDto {
  id: string;
  name: string;
  description: string;
  price: number;
  category: string;
  isInstalled: boolean;
  imageUrl: string;
  installationTimeMinutes: number;
}

// Parts compatible with vehicles
export interface PartDto {
  id: string;
  partNumber: string;
  name: string;
  description: string;
  price: number;
  quantityAvailable: number;
  category: string;
  manufacturerName: string;
}

// Request to reserve parts for a deal
export interface ReservePartsRequestDto {
  parts: ReservePartItemDto[];
  requiredDate: string; // ISO date string
  notes: string;
}

export interface ReservePartItemDto {
  partId: string;
  quantity: number;
}

// Result of parts reservation
export interface PartsReservationDto {
  reservationId: string;
  dealId: string;
  reservedParts: ReservedPartDto[];
  reservationDate: string; // ISO date string
  expirationDate: string; // ISO date string
  totalPrice: number;
  allPartsAvailable: boolean;
}

export interface ReservedPartDto {
  partId: string;
  partNumber: string;
  name: string;
  quantity: number;
  unitPrice: number;
  isAvailable: boolean;
  estimatedAvailabilityDate?: string; // ISO date string
}

// Customer service history items
export interface ServiceHistoryDto {
  id: string;
  serviceOrderNumber: string;
  serviceDate: string; // ISO date string
  vehicleDescription: string;
  vin: string;
  totalCost: number;
  status: string;
  lineItems: ServiceLineItemDto[];
}

export interface ServiceLineItemDto {
  description: string;
  cost: number;
  type: string; // e.g., "Repair", "Maintenance", "Part"
}

// Financial quotes for deals
export interface FinancialQuoteDto {
  id: string;
  lenderName: string;
  productType: string; // e.g., "Loan", "Lease"
  amount: number;
  interestRate: number;
  termMonths: number;
  monthlyPayment: number;
  downPayment: number;
  expirationDate: string; // ISO date string
  isPromotional: boolean;
  promotionDescription: string;
  requirements: string[];
}

// Request for deal financing
export interface FinancingRequestDto {
  quoteId: string;
  lenderId: string;
  requestedAmount: number;
  termMonths: number;
  downPayment: number;
  coBuyerId?: string;
  additionalDocumentIds: string[];
}

// Result of financing application
export interface FinancingApplicationResultDto {
  applicationId: string;
  status: string; // "Approved", "Rejected", "PendingReview", "AdditionalInfoRequired"
  lenderName: string;
  approvedAmount: number;
  interestRate: number;
  termMonths: number;
  monthlyPayment: number;
  conditions: FinancingConditionDto[];
  comments: string;
  requiredDocuments: string[];
}

export interface FinancingConditionDto {
  type: string; // e.g., "DownPayment", "ProofOfIncome", "ProofOfResidence"
  description: string;
  isMandatory: boolean;
}

// Insurance quotes for deals
export interface InsuranceQuoteDto {
  id: string;
  providerName: string;
  policyType: string; // e.g., "Comprehensive", "Liability"
  annualPremium: number;
  monthlyPremium: number;
  deductible: number;
  coverage: number;
  inclusions: string[];
  exclusions: string[];
  expirationDate: string; // ISO date string
  contactInfo: string;
}

// DMV registration result
export interface DmvRegistrationResultDto {
  registrationId: string;
  status: string; // "Complete", "Pending", "Rejected"
  registrationDate: string; // ISO date string
  expirationDate: string; // ISO date string
  plateNumber: string;
  registrationFee: number;
  taxesPaid: number;
  requiredDocuments: string[];
  comments: string;
}

// Invoice for a deal
export interface InvoiceDto {
  invoiceNumber: string;
  invoiceDate: string; // ISO date string
  dealId: string;
  customerName: string;
  vehicleDescription: string;
  subtotal: number;
  taxRate: number;
  taxAmount: number;
  total: number;
  lineItems: InvoiceLineItemDto[];
  payments: InvoicePaymentDto[];
  balance: number;
  status: string; // "Paid", "PartiallyPaid", "Unpaid"
}

export interface InvoiceLineItemDto {
  description: string;
  unitPrice: number;
  quantity: number;
  extendedPrice: number;
  itemType: string; // "Vehicle", "Accessory", "Service", "Fee", "Tax"
}

export interface InvoicePaymentDto {
  paymentDate: string; // ISO date string
  paymentMethod: string;
  amount: number;
  referenceNumber: string;
}
