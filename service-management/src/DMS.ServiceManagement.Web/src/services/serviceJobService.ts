import axios from 'axios';

export interface ServiceJobPart {
  id: string;
  partId: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  totalAmount: number;
  status: string;
}

export interface ServiceJob {
  id: string;
  repairOrderId: string;
  description: string;
  jobType: string;
  status: 'NotStarted' | 'InProgress' | 'Completed' | 'WaitingParts' | 'OnHold';
  laborOperationCode: string;
  laborOperationDescription?: string;
  estimatedHours: number;
  actualHours: number;
  startTime?: Date | string;
  endTime?: Date | string;
  technicianId?: string;
  notes?: string;
  parts?: ServiceJobPart[];
}

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

const serviceJobService = {
    getAllJobs: async (): Promise<ServiceJob[]> => {
        try {
            const response = await axios.get(`${API_URL}/api/service/jobs`);
            return response.data;
        } catch (error) {
            console.error('Error fetching all jobs:', error);
            throw error;
        }
    },

    getJobById: async (id: string): Promise<ServiceJob> => {
        try {
            const response = await axios.get(`${API_URL}/api/service/jobs/${id}`);
            return response.data;
        } catch (error) {
            console.error(`Error fetching job with ID ${id}:`, error);
            throw error;
        }
    },

    getJobsByRepairOrderId: async (repairOrderId: string): Promise<ServiceJob[]> => {
        try {
            const response = await axios.get(`${API_URL}/api/service/jobs/repair-order/${repairOrderId}`);
            return response.data;
        } catch (error) {
            console.error(`Error fetching jobs for repair order ID ${repairOrderId}:`, error);
            throw error;
        }
    },

    getJobsByTechnicianId: async (technicianId: string): Promise<ServiceJob[]> => {
        try {
            const response = await axios.get(`${API_URL}/api/service/jobs/technician/${technicianId}`);
            return response.data;
        } catch (error) {
            console.error(`Error fetching jobs for technician ID ${technicianId}:`, error);
            throw error;
        }
    },

    createJob: async (job: Partial<ServiceJob>): Promise<ServiceJob> => {
        try {
            const response = await axios.post(`${API_URL}/api/service/jobs`, job);
            return response.data;
        } catch (error) {
            console.error('Error creating job:', error);
            throw error;
        }
    },

    updateJob: async (id: string, job: Partial<ServiceJob>): Promise<ServiceJob> => {
        try {
            const response = await axios.put(`${API_URL}/api/service/jobs/${id}`, job);
            return response.data;
        } catch (error) {
            console.error(`Error updating job with ID ${id}:`, error);
            throw error;
        }
    },

    assignTechnician: async (jobId: string, technicianId: string): Promise<ServiceJob> => {
        try {
            const response = await axios.post(`${API_URL}/api/service/jobs/${jobId}/assign`, { technicianId });
            return response.data;
        } catch (error) {
            console.error(`Error assigning technician to job ID ${jobId}:`, error);
            throw error;
        }
    },

    startJob: async (jobId: string): Promise<ServiceJob> => {
        try {
            const response = await axios.post(`${API_URL}/api/service/jobs/${jobId}/start`);
            return response.data;
        } catch (error) {
            console.error(`Error starting job with ID ${jobId}:`, error);
            throw error;
        }
    },

    completeJob: async (jobId: string, job?: Partial<ServiceJob>): Promise<ServiceJob> => {
        try {
            const response = await axios.post(`${API_URL}/api/service/jobs/${jobId}/complete`, job);
            return response.data;
        } catch (error) {
            console.error(`Error completing job with ID ${jobId}:`, error);
            throw error;
        }
    },

    addPartsToJob: async (jobId: string, parts: ServiceJobPart[]): Promise<ServiceJob> => {
        try {
            const response = await axios.post(`${API_URL}/api/service/jobs/${jobId}/parts`, parts);
            return response.data;
        } catch (error) {
            console.error(`Error adding parts to job ID ${jobId}:`, error);
            throw error;
        }
    }
};

export default serviceJobService;
