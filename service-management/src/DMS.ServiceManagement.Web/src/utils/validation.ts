// validation.ts - Utility for form validation

// Define ValidationError type
export type ValidationError<T> = Partial<Record<keyof T, string>>;

// Input validator types
type ValidatorFn<T> = (value: T) => string | null;

// Create a simple validation schema
export interface ValidationSchema<T> {
  [key: string]: ValidatorFn<T[keyof T]>[];
}

// Main validation function that runs all validators
export function validateForm<T extends Record<string, any>>(
  data: T, 
  schema: ValidationSchema<T>
): ValidationError<T> {
  const errors: Partial<Record<keyof T, string>> = {};
  
  // Loop through each field and apply all validators
  for (const field in schema) {
    const validators = schema[field];
    const value = data[field];
    
    for (const validator of validators) {
      const error = validator(value);
      if (error) {
        errors[field as keyof T] = error;
        break; // Stop at first error for this field
      }
    }
  }
  
  return errors as ValidationError<T>;
}

// Common validators
export const required = <T>(fieldName: string = 'This field') => 
  (value: T): string | null => {
    if (value === null || value === undefined || value === '') {
      return `${fieldName} is required`;
    }
    return null;
  };

export const minLength = (min: number, fieldName: string = 'Value') => 
  (value: string): string | null => {
    if (!value || value.length < min) {
      return `${fieldName} must be at least ${min} characters`;
    }
    return null;
  };

export const maxLength = (max: number, fieldName: string = 'Value') => 
  (value: string): string | null => {
    if (value && value.length > max) {
      return `${fieldName} must be at most ${max} characters`;
    }
    return null;
  };

export const isEmail = () => 
  (value: string): string | null => {
    if (!value) return null;
    
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(value)) {
      return 'Please enter a valid email address';
    }
    return null;
  };

export const isPhone = () => 
  (value: string): string | null => {
    if (!value) return null;
    
    // Allow formats like (123) 456-7890, 123-456-7890, etc.
    const phoneRegex = /^\(?\d{3}\)?[-\s.]?\d{3}[-\s.]?\d{4}$/;
    if (!phoneRegex.test(value)) {
      return 'Please enter a valid phone number';
    }
    return null;
  };

export const isDate = (fieldName: string = 'Date') => 
  (value: Date | null): string | null => {
    if (!value) return null;
    
    if (!(value instanceof Date) || isNaN(value.getTime())) {
      return `Please enter a valid ${fieldName.toLowerCase()}`;
    }
    return null;
  };

export const isInFuture = (fieldName: string = 'Date') => 
  (value: Date | null): string | null => {
    if (!value) return null;
    
    if (value < new Date()) {
      return `${fieldName} must be in the future`;
    }
    return null;
  };

export const isValidVIN = () => 
  (value: string): string | null => {
    if (!value) return null;
    
    // Basic VIN validation (17 alphanumeric chars except I, O, Q)
    const vinRegex = /^[A-HJ-NPR-Z0-9]{17}$/;
    if (!vinRegex.test(value)) {
      return 'Please enter a valid 17-character VIN';
    }
    return null;
  };

export const isNotWeekend = (fieldName: string = 'Date') => 
  (value: Date | null): string | null => {
    if (!value) return null;
    
    const day = value.getDay();
    if (day === 0 || day === 6) {
      return `${fieldName} cannot be on a weekend`;
    }
    return null;
  };

export const isNumber = (fieldName: string = 'Value') =>
  (value: any): string | null => {
    if (value === null || value === undefined || value === '') return null;
    
    if (isNaN(Number(value))) {
      return `${fieldName} must be a number`;
    }
    return null;
  };

export const isPositive = (fieldName: string = 'Value') =>
  (value: number): string | null => {
    if (value === null || value === undefined) return null;
    
    if (value <= 0) {
      return `${fieldName} must be a positive number`;
    }
    return null;
  };

export const isInRange = (min: number, max: number, fieldName: string = 'Value') =>
  (value: number): string | null => {
    if (value === null || value === undefined) return null;
    
    if (value < min || value > max) {
      return `${fieldName} must be between ${min} and ${max}`;
    }
    return null;
  };

export const isValidPrice = (fieldName: string = 'Price') =>
  (value: number): string | null => {
    if (value === null || value === undefined) return null;
    
    if (value < 0 || !Number.isFinite(value)) {
      return `${fieldName} must be a valid amount`;
    }
    return null;
  };

export const isValidMileage = () =>
  (value: number): string | null => {
    if (value === null || value === undefined) return null;
    
    if (value < 0 || !Number.isInteger(value)) {
      return 'Mileage must be a positive whole number';
    }
    return null;
  };

export const isValidHours = (fieldName: string = 'Hours') =>
  (value: number): string | null => {
    if (value === null || value === undefined) return null;
    
    if (value < 0 || value > 24) {
      return `${fieldName} must be between 0 and 24`;
    }
    return null;
  };

export const isValidImageType = () =>
  (value: File | null): string | null => {
    if (!value) return null;
    
    const validTypes = ['image/jpeg', 'image/png', 'image/gif'];
    if (!validTypes.includes(value.type)) {
      return 'Only JPG, PNG, or GIF images are allowed';
    }
    return null;
  };

// Form schema validators for specific forms
export const createAppointmentValidators = {
  customerId: [required('Customer')],
  vehicleId: [required('Vehicle')],
  appointmentType: [required('Service type')],
  date: [required('Date'), isDate(), isInFuture()],
  duration: [required('Duration')],
  customerConcerns: [minLength(10, 'Customer concerns'), maxLength(1000)],
  transportationType: [required('Transportation type')],
  contactPhone: [required('Contact phone'), isPhone()],
  contactEmail: [isEmail()]
};

// Inspection form validators
export const inspectionFormValidators = {
  repairOrderId: [required('Repair Order')],
  technicianId: [required('Technician')],
  inspectionItems: [(items) => items && items.length > 0 ? null : 'At least one inspection item is required'],
  notes: [maxLength(2000, 'Notes')]
};

// Recommended service validators
export const recommendedServiceValidators = {
  description: [required('Description'), minLength(5, 'Description'), maxLength(200)],
  urgency: [required('Urgency')],
  estimatedPrice: [required('Estimated price'), isNumber('Price'), isPositive('Price')]
};

// Service job validators
export const serviceJobValidators = {
  description: [required('Description'), minLength(5), maxLength(200)],
  jobType: [required('Job type')],
  laborOperationCode: [required('Labor operation code')],
  estimatedHours: [required('Estimated hours'), isNumber('Hours'), isValidHours()],
  technicianId: [(value) => value ? null : 'Technician assignment is recommended']
};

// Service part validators
export const servicePartValidators = {
  partId: [required('Part')],
  quantity: [required('Quantity'), isNumber('Quantity'), isPositive('Quantity')],
  unitPrice: [required('Unit price'), isNumber('Unit price'), isPositive('Unit price')]
};

// Job completion validators
export const jobCompletionValidators = {
  actualHours: [required('Actual hours'), isNumber('Actual hours'), isValidHours()],
  status: [required('Status')]
};
