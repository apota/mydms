import axios from 'axios';
import { AxiosResponse } from 'axios';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

// Types definition
export enum InspectionResult {
    Pass = 'PASS',
    Fail = 'FAIL',
    Warning = 'WARNING',
    NotApplicable = 'N/A'
}

export enum ServiceUrgency {
    Critical = 'CRITICAL',
    Soon = 'SOON',
    Future = 'FUTURE'
}

export interface InspectionPoint {
    id: string;
    name: string;
    category: string;
    result: InspectionResult;
    notes: string;
    imageUrls: string[];
}

export interface RecommendedService {
    id: string;
    description: string;
    urgency: ServiceUrgency;
    estimatedPrice: number;
}

export interface ServiceInspection {
    id?: string;
    repairOrderId: string;
    vehicleId: string;
    technicianId: string;
    type: string;
    status: string;
    startTime: string;
    endTime: string;
    notes: string;
    inspectionPoints: InspectionPoint[];
    recommendedServices: RecommendedService[];
}

// API functions
/**
 * Create a new inspection
 */
export const createInspection = async (inspection: ServiceInspection): Promise<ServiceInspection> => {
    try {
        const response: AxiosResponse<ServiceInspection> = await axios.post(`${API_URL}/inspections`, inspection);
        return response.data;
    } catch (error) {
        console.error('Error creating inspection:', error);
        throw error;
    }
};

/**
 * Get inspection by ID
 */
export const getInspection = async (inspectionId: string): Promise<ServiceInspection> => {
    try {
        const response: AxiosResponse<ServiceInspection> = await axios.get(`${API_URL}/inspections/${inspectionId}`);
        return response.data;
    } catch (error) {
        console.error(`Error fetching inspection ${inspectionId}:`, error);
        throw error;
    }
};

/**
 * Get inspections by repair order ID
 */
export const getInspectionsByRepairOrder = async (repairOrderId: string): Promise<ServiceInspection[]> => {
    try {
        const response: AxiosResponse<ServiceInspection[]> = await axios.get(
            `${API_URL}/inspections/repair-order/${repairOrderId}`
        );
        return response.data;
    } catch (error) {
        console.error(`Error fetching inspections for repair order ${repairOrderId}:`, error);
        throw error;
    }
};

/**
 * Update an existing inspection
 */
export const updateInspection = async (
    inspectionId: string, 
    inspection: Partial<ServiceInspection>
): Promise<ServiceInspection> => {
    try {
        const response: AxiosResponse<ServiceInspection> = await axios.put(
            `${API_URL}/inspections/${inspectionId}`, 
            inspection
        );
        return response.data;
    } catch (error) {
        console.error(`Error updating inspection ${inspectionId}:`, error);
        throw error;
    }
};

/**
 * Upload an inspection image
 * Note: In a real app, this would use FormData to upload the file
 */
export const uploadInspectionImage = async (
    inspectionId: string,
    pointId: string,
    file: File
): Promise<string> => {
    try {
        const formData = new FormData();
        formData.append('file', file);
        
        const response: AxiosResponse<{imageUrl: string}> = await axios.post(
            `${API_URL}/inspections/${inspectionId}/points/${pointId}/images`,
            formData,
            {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            }
        );
        
        return response.data.imageUrl;
    } catch (error) {
        console.error('Error uploading inspection image:', error);
        throw error;
    }
};
