import axios from 'axios';
import { format } from 'date-fns';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api/service';

// Types
export interface Appointment {
  id: string;
  customerId: string;
  vehicleId: string;
  appointmentType: string;
  status: string;
  scheduledStartTime: string;
  scheduledEndTime: string;
  actualStartTime?: string;
  actualEndTime?: string;
  advisorId?: string;
  bayId?: string;
  transportationType: string;
  customerConcerns?: string;
  appointmentNotes?: string;
  confirmationStatus: string;
  confirmationTime?: string;
  duration?: number; // Duration in minutes
  startTime?: string; // For available slots
  available?: boolean; // For available slots
}

export interface TimeSlot {
  id: string;
  startTime: string;
  endTime: string;
  available: boolean;
  duration: number;
}

export interface AppointmentRequest {
  customerId: string;
  vehicleId: string;
  serviceType: string;
  notes?: string;
  startTime: string;
  duration: number;
  transportationType: string;
  contactPhone?: string;
  contactEmail?: string;
}

// Service functions
export const getAppointments = async (): Promise<Appointment[]> => {
  try {
    const response = await axios.get(`${API_URL}/appointments`);
    return response.data;
  } catch (error) {
    console.error('Error fetching appointments:', error);
    throw error;
  }
};

export const getAppointmentById = async (id: string): Promise<Appointment> => {
  try {
    const response = await axios.get(`${API_URL}/appointments/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching appointment with ID ${id}:`, error);
    throw error;
  }
};

export const getAppointmentsByCustomerId = async (customerId: string): Promise<Appointment[]> => {
  try {
    const response = await axios.get(`${API_URL}/appointments/customer/${customerId}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching appointments for customer ID ${customerId}:`, error);
    throw error;
  }
};

export const getAvailableSlots = async (
  date: Date, 
  appointmentType?: string,
  duration: number = 60
): Promise<TimeSlot[]> => {
  try {
    const formattedDate = format(date, 'yyyy-MM-dd');
    
    const response = await axios.get(`${API_URL}/appointments/availability`, {
      params: {
        date: formattedDate,
        appointmentType,
        duration
      }
    });
    
    return response.data;
  } catch (error) {
    console.error(`Error fetching available slots:`, error);
    throw error;
  }
};

export const createAppointment = async (appointmentRequest: AppointmentRequest): Promise<Appointment> => {
  try {
    const response = await axios.post(`${API_URL}/appointments`, appointmentRequest);
    return response.data;
  } catch (error: any) {
    console.error('Error creating appointment:', error);
    
    // Extract validation errors if they exist in the response
    if (error.response && error.response.data && error.response.data.errors) {
      const apiError = new Error(error.response.data.message || 'Validation failed');
      (apiError as any).validation = error.response.data.errors;
      throw apiError;
    }
    
    throw error;
  }
};

export const updateAppointment = async (appointment: Partial<Appointment>): Promise<Appointment> => {
  try {
    if (!appointment.id) {
      throw new Error('Appointment ID is required for updates');
    }
    
    const response = await axios.put(`${API_URL}/appointments/${appointment.id}`, appointment);
    return response.data;
  } catch (error) {
    console.error(`Error updating appointment:`, error);
    throw error;
  }
};

export const cancelAppointment = async (id: string, reason?: string): Promise<void> => {
  try {
    await axios.post(`${API_URL}/appointments/${id}/cancel`, { reason });
  } catch (error) {
    console.error(`Error canceling appointment with ID ${id}:`, error);
    throw error;
  }
};

export const confirmAppointment = async (id: string): Promise<Appointment> => {
  try {
    const response = await axios.post(`${API_URL}/appointments/${id}/confirm`);
    return response.data;
  } catch (error) {
    console.error(`Error confirming appointment with ID ${id}:`, error);
    throw error;
  }
};

export const rescheduleAppointment = async (
  id: string, 
  newDate: Date,
  newTime: string
): Promise<Appointment> => {
  try {
    const dateObj = new Date(newDate);
    const [hours, minutes] = newTime.split(':').map(Number);
    dateObj.setHours(hours, minutes, 0, 0);
    
    const response = await axios.post(`${API_URL}/appointments/${id}/reschedule`, {
      newDateTime: dateObj.toISOString()
    });
    
    return response.data;
  } catch (error) {
    console.error(`Error rescheduling appointment:`, error);
    throw error;
  }
};
