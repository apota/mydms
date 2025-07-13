// @ts-nocheck 
import axios from 'axios';
import { Lead, CreateLeadRequest, UpdateLeadRequest, LeadStatus, AddLeadActivityRequest } from '../types/lead';

const API_URL = '/api';

// Fetch all leads with optional status filter
export const fetchLeads = async (status?: LeadStatus): Promise<Lead[]> => {
  try {
    const params = status ? { status } : {};
    const response = await axios.get(`${API_URL}/leads`, { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching leads:', error);
    throw error;
  }
};

// Fetch a single lead by ID
export const fetchLead = async (id: string): Promise<Lead> => {
  try {
    const response = await axios.get(`${API_URL}/leads/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching lead ${id}:`, error);
    throw error;
  }
};

// Create new lead
export const createLead = async (lead: CreateLeadRequest): Promise<Lead> => {
  try {
    const response = await axios.post(`${API_URL}/leads`, lead);
    return response.data;
  } catch (error) {
    console.error('Error creating lead:', error);
    throw error;
  }
};

// Update existing lead
export const updateLead = async (id: string, lead: UpdateLeadRequest): Promise<Lead> => {
  try {
    const response = await axios.put(`${API_URL}/leads/${id}`, lead);
    return response.data;
  } catch (error) {
    console.error(`Error updating lead ${id}:`, error);
    throw error;
  }
};

// Delete a lead
export const deleteLead = async (id: string): Promise<void> => {
  try {
    await axios.delete(`${API_URL}/leads/${id}`);
  } catch (error) {
    console.error(`Error deleting lead ${id}:`, error);
    throw error;
  }
};

// Add activity to lead
export const addLeadActivity = async (
  leadId: string,
  activity: AddLeadActivityRequest
): Promise<Lead> => {
  try {
    const response = await axios.post(`${API_URL}/leads/${leadId}/activities`, activity);
    return response.data;
  } catch (error) {
    console.error(`Error adding activity to lead ${leadId}:`, error);
    throw error;
  }
};

// Convert lead to deal
export const convertLeadToDeal = async (
  leadId: string,
  dealData: { vehicleId: string; dealType: string }
): Promise<{ dealId: string }> => {
  try {
    const response = await axios.post(`${API_URL}/leads/${leadId}/convert`, dealData);
    return response.data;
  } catch (error) {
    console.error(`Error converting lead ${leadId} to deal:`, error);
    throw error;
  }
};
