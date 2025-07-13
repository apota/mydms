// @ts-nocheck 
export enum DealStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  FinancingRequired = 'Financing Required',
  FinancingApproved = 'Financing Approved',
  FinancingRejected = 'Financing Rejected',
  DepositPaid = 'Deposit Paid',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum DealType {
  New = 'New',
  Used = 'Used',
  Lease = 'Lease'
}

export interface DealVehicle {
  id: string;
  make: string;
  model: string;
  year: number;
  vin: string;
  stockNumber: string;
  listPrice: number;
  imageUrl?: string;
}

export interface DealCustomer {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
}

export interface DealAddOn {
  id: string;
  name: string;
  type: string;
  price: number;
  description?: string;
  isSelected: boolean;
}

export interface DealPayment {
  id?: string;
  type: string;
  amount: number;
  date: string;
  reference?: string;
}

export interface DealFinancing {
  lender?: string;
  amount?: number;
  term?: number;
  rate?: number;
  monthlyPayment?: number;
  approved?: boolean;
  applicationDate?: string;
  approvalDate?: string;
}

export interface Deal {
  id: string;
  dealNumber: string;
  status: DealStatus;
  type: DealType;
  vehicle: DealVehicle;
  customer: DealCustomer;
  salesRepId: string;
  addOns: DealAddOn[];
  payments: DealPayment[];
  financing?: DealFinancing;
  subtotal: number;
  taxes: number;
  total: number;
  deposit: number;
  balance: number;
  tradeIn?: {
    make: string;
    model: string;
    year: number;
    value: number;
  };
  createdAt: string;
  updatedAt: string;
  closedAt?: string;
  notes?: string;
}

export interface CreateDealRequest {
  customerId: string;
  vehicleId: string;
  salesRepId: string;
  type: DealType;
  tradeIn?: {
    make: string;
    model: string;
    year: number;
    value: number;
  };
  notes?: string;
}

export interface UpdateDealRequest {
  status?: DealStatus;
  addOns?: string[];
  notes?: string;
  deposit?: number;
  salesRepId?: string;
}
