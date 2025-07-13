// @ts-nocheck 
import axios from 'axios';
import { Commission, CommissionStatus, CommissionType, CommissionSummary, CreateCommissionRequest, UpdateCommissionRequest } from '../types/commission';

const API_URL = '/api';

// Fetch all commissions with optional filters
export const fetchCommissions = async (
  salesPersonId?: string,
  dealId?: string,
  status?: CommissionStatus,
  startDate?: string,
  endDate?: string
): Promise<Commission[]> => {
  try {
    const params = { 
      ...(salesPersonId && { salesPersonId }),
      ...(dealId && { dealId }),
      ...(status && { status }),
      ...(startDate && { startDate }),
      ...(endDate && { endDate }),
    };
    const response = await axios.get(`${API_URL}/commissions`, { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching commissions:', error);
    throw error;
  }
};

// Fetch a single commission by ID
export const fetchCommission = async (id: string): Promise<Commission> => {
  try {
    const response = await axios.get(`${API_URL}/commissions/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching commission ${id}:`, error);
    throw error;
  }
};

// Create new commission
export const createCommission = async (commission: CreateCommissionRequest): Promise<Commission> => {
  try {
    const response = await axios.post(`${API_URL}/commissions`, commission);
    return response.data;
  } catch (error) {
    console.error('Error creating commission:', error);
    throw error;
  }
};

// Update existing commission
export const updateCommission = async (id: string, commission: UpdateCommissionRequest): Promise<Commission> => {
  try {
    const response = await axios.put(`${API_URL}/commissions/${id}`, commission);
    return response.data;
  } catch (error) {
    console.error(`Error updating commission ${id}:`, error);
    throw error;
  }
};

// Delete a commission
export const deleteCommission = async (id: string): Promise<void> => {
  try {
    await axios.delete(`${API_URL}/commissions/${id}`);
  } catch (error) {
    console.error(`Error deleting commission ${id}:`, error);
    throw error;
  }
};

// Approve a commission
export const approveCommission = async (id: string): Promise<Commission> => {
  try {
    const response = await axios.post(`${API_URL}/commissions/${id}/approve`);
    return response.data;
  } catch (error) {
    console.error(`Error approving commission ${id}:`, error);
    throw error;
  }
};

// Mark commission as paid
export const markCommissionPaid = async (id: string, payDate: string): Promise<Commission> => {
  try {
    const response = await axios.post(`${API_URL}/commissions/${id}/paid`, { payDate });
    return response.data;
  } catch (error) {
    console.error(`Error marking commission ${id} as paid:`, error);
    throw error;
  }
};

// Get summary of commissions by sales person
export const fetchCommissionSummaries = async (
  startDate?: string,
  endDate?: string
): Promise<CommissionSummary[]> => {
  try {
    const params = {
      ...(startDate && { startDate }),
      ...(endDate && { endDate }),
    };
    const response = await axios.get(`${API_URL}/commissions/summary`, { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching commission summaries:', error);
    throw error;
  }
};
