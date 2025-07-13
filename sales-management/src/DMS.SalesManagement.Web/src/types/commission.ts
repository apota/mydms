// @ts-nocheck 
export enum CommissionType {
  Sales = 'Sales',
  Finance = 'Finance',
  Service = 'Service',
  Referral = 'Referral',
  Bonus = 'Bonus'
}

export enum CommissionStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Paid = 'Paid',
  Adjusted = 'Adjusted',
  Denied = 'Denied'
}

export interface Commission {
  id: string;
  salesPersonId: string;
  dealId: string;
  type: CommissionType;
  amount: number;
  baseAmount: number;
  percentage: number;
  status: CommissionStatus;
  payDate?: string;
  adjustmentReason?: string;
  createdAt: string;
  updatedAt: string;
  notes?: string;
}

export interface CommissionSummary {
  salesPersonId: string;
  salesPersonName: string;
  pending: number;
  approved: number;
  paid: number;
  totalDeals: number;
  averageCommission: number;
}

export interface CreateCommissionRequest {
  salesPersonId: string;
  dealId: string;
  type: CommissionType;
  amount: number;
  baseAmount: number;
  percentage: number;
  notes?: string;
}

export interface UpdateCommissionRequest {
  status?: CommissionStatus;
  amount?: number;
  adjustmentReason?: string;
  payDate?: string;
  notes?: string;
}
