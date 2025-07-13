// @ts-nocheck
import axios from 'axios';
import { 
  AccessoryDto, 
  PartDto, 
  ReservePartsRequestDto,
  PartsReservationDto,
  ServiceHistoryDto,
  FinancialQuoteDto,
  FinancingApplicationResultDto,
  DmvRegistrationResultDto,
  InsuranceQuoteDto
} from '../types/integration';

const API_URL = '/api/sales';

// Error handling function
const handleApiError = (error: any, customMessage: string) => {
  console.error(`${customMessage}:`, error);
  if (axios.isAxiosError(error)) {
    throw new Error(error.response?.data?.message || error.message || 'An unknown error occurred');
  }
  throw error;
};

export const getVehicleAccessories = async (vehicleId: string): Promise<AccessoryDto[]> => {
  try {
    const response = await axios.get(`${API_URL}/integration/vehicles/${vehicleId}/accessories`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching accessories for vehicle ${vehicleId}:`, error);
    return [];
  }
};

export const getCompatibleParts = async (vehicleId: string): Promise<PartDto[]> => {
  try {
    const response = await axios.get(`${API_URL}/integration/vehicles/${vehicleId}/parts`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching compatible parts for vehicle ${vehicleId}:`, error);
    return [];
  }
};

export const reservePartsForDeal = async (dealId: string, request: ReservePartsRequestDto): Promise<PartsReservationDto> => {
  try {
    const response = await axios.post(`${API_URL}/integration/deals/${dealId}/reserve-parts`, request);
    return response.data;
  } catch (error) {
    handleApiError(error, `Error reserving parts for deal ${dealId}`);
  }
};

export const getCustomerServiceHistory = async (customerId: string): Promise<ServiceHistoryDto[]> => {
  try {
    const response = await axios.get(`${API_URL}/integration/customers/${customerId}/service-history`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching service history for customer ${customerId}:`, error);
    return [];
  }
};

export const getFinancialQuotesForDeal = async (dealId: string): Promise<FinancialQuoteDto[]> => {
  try {
    const response = await axios.get(`${API_URL}/integration/deals/${dealId}/financial-quotes`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching financial quotes for deal ${dealId}:`, error);
    return [];
  }
};

export const submitDealForFinancing = async (
  dealId: string,
  financingRequest: any
): Promise<FinancingApplicationResultDto> => {
  try {
    const response = await axios.post(
      `${API_URL}/integration/deals/${dealId}/submit-financing`,
      financingRequest
    );
    return response.data;
  } catch (error) {
    handleApiError(error, `Error submitting financing for deal ${dealId}`);
  }
};

export const getInsuranceQuotes = async (dealId: string): Promise<InsuranceQuoteDto[]> => {
  try {
    const response = await axios.get(`${API_URL}/integration/deals/${dealId}/insurance-quotes`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching insurance quotes for deal ${dealId}:`, error);
    return [];
  }
};

export const registerVehicleWithDmv = async (dealId: string): Promise<DmvRegistrationResultDto> => {
  try {
    const response = await axios.post(`${API_URL}/integration/deals/${dealId}/dmv-registration`);
    return response.data;
  } catch (error) {
    handleApiError(error, `Error registering vehicle with DMV for deal ${dealId}`);
  }
};

export const createDealInvoice = async (dealId: string): Promise<any> => {
  try {
    const response = await axios.post(`${API_URL}/integration/deals/${dealId}/invoice`);
    return response.data;
  } catch (error) {
    handleApiError(error, `Error creating invoice for deal ${dealId}`);
  }
};

// Default export for backward compatibility
export default {
  getVehicleAccessories,
  getCompatibleParts,
  reservePartsForDeal,
  getCustomerServiceHistory,
  getFinancialQuotesForDeal,
  submitDealForFinancing,
  getInsuranceQuotes,
  registerVehicleWithDmv,
  createDealInvoice
};
