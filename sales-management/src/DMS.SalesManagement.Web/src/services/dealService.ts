// @ts-nocheck 
import axios from 'axios';
import { Deal, DealStatus, CreateDealRequest, UpdateDealRequest } from '../types/deal';

const API_URL = '/api';

// Fetch all deals with optional status filter
export const fetchDeals = async (status?: DealStatus): Promise<Deal[]> => {
  try {
    const params = status ? { status } : {};
    const response = await axios.get(`${API_URL}/deals`, { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching deals:', error);
    throw error;
  }
};

// Fetch a single deal by ID
export const fetchDeal = async (id: string): Promise<Deal> => {
  try {
    const response = await axios.get(`${API_URL}/deals/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching deal ${id}:`, error);
    throw error;
  }
};

// Create new deal
export const createDeal = async (deal: CreateDealRequest): Promise<Deal> => {
  try {
    const response = await axios.post(`${API_URL}/deals`, deal);
    return response.data;
  } catch (error) {
    console.error('Error creating deal:', error);
    throw error;
  }
};

// Update existing deal
export const updateDeal = async (id: string, deal: UpdateDealRequest): Promise<Deal> => {
  try {
    const response = await axios.put(`${API_URL}/deals/${id}`, deal);
    return response.data;
  } catch (error) {
    console.error(`Error updating deal ${id}:`, error);
    throw error;
  }
};

// Add add-ons to a deal
export const addDealAddOns = async (id: string, addOnIds: string[]): Promise<Deal> => {
  try {
    const response = await axios.post(`${API_URL}/deals/${id}/addons`, { addOnIds });
    return response.data;
  } catch (error) {
    console.error(`Error adding add-ons to deal ${id}:`, error);
    throw error;
  }
};

// Remove add-ons from a deal
export const removeDealAddOns = async (id: string, addOnIds: string[]): Promise<Deal> => {
  try {
    const response = await axios.delete(`${API_URL}/deals/${id}/addons`, { 
      data: { addOnIds } 
    });
    return response.data;
  } catch (error) {
    console.error(`Error removing add-ons from deal ${id}:`, error);
    throw error;
  }
};

// Submit deal for financing
export const submitDealForFinancing = async (
  id: string,
  financingData: { lender: string; amount: number; term: number }
): Promise<Deal> => {
  try {
    const response = await axios.post(`${API_URL}/deals/${id}/financing`, financingData);
    return response.data;
  } catch (error) {
    console.error(`Error submitting deal ${id} for financing:`, error);
    throw error;
  }
};

// Record a payment for a deal
export const addDealPayment = async (
  id: string,
  paymentData: { type: string; amount: number; reference?: string }
): Promise<Deal> => {
  try {
    const response = await axios.post(`${API_URL}/deals/${id}/payments`, paymentData);
    return response.data;
  } catch (error) {
    console.error(`Error adding payment to deal ${id}:`, error);
    throw error;
  }
};

// Complete a deal
export const completeDeal = async (id: string): Promise<Deal> => {
  try {
    const response = await axios.post(`${API_URL}/deals/${id}/complete`);
    return response.data;
  } catch (error) {
    console.error(`Error completing deal ${id}:`, error);
    throw error;
  }
};

// Cancel a deal
export const cancelDeal = async (id: string, reason: string): Promise<Deal> => {
  try {
    const response = await axios.post(`${API_URL}/deals/${id}/cancel`, { reason });
    return response.data;
  } catch (error) {
    console.error(`Error cancelling deal ${id}:`, error);
    throw error;
  }
};
