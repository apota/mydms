export enum LeadStatus {
  New = 'New',
  Contacted = 'Contacted',
  Qualified = 'Qualified',
  Appointment = 'Appointment',
  Sold = 'Sold',
  Lost = 'Lost'
}

export enum LeadActivityType {
  Call = 'Call',
  Email = 'Email',
  Visit = 'Visit',
  TestDrive = 'TestDrive',
  Appointment = 'Appointment',
  Followup = 'Followup',
  Other = 'Other'
}

export enum InterestType {
  New = 'New',
  Used = 'Used',
  Service = 'Service'
}

export interface Address {
  street: string;
  city: string;
  state: string;
  zip: string;
}

export interface LeadActivity {
  id?: string;
  type: LeadActivityType;
  date: string;
  userId: string;
  notes: string;
}

export interface Lead {
  id: string;
  source: string;
  sourceId?: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address?: Address;
  status: LeadStatus;
  interestType: InterestType;
  interestVehicleId?: string;
  assignedSalesRepId?: string;
  comments?: string;
  createdAt: string;
  updatedAt: string;
  lastActivityDate?: string;
  followupDate?: string;
  activities: LeadActivity[];
}

export interface CreateLeadRequest {
  source: string;
  sourceId?: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address?: Address;
  interestType: InterestType;
  interestVehicleId?: string;
  comments?: string;
  followupDate?: string;
}

export interface UpdateLeadRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  address?: Address;
  status?: LeadStatus;
  interestType?: InterestType;
  interestVehicleId?: string;
  assignedSalesRepId?: string;
  comments?: string;
  followupDate?: string;
}

export interface AddLeadActivityRequest {
  type: LeadActivityType;
  notes: string;
  date?: string;
}

export interface ConvertLeadToDealRequest {
  vehicleId: string;
  dealType: string;
}
