// Appointment Scheduler Page - For booking new service appointments
import React, { useState, useEffect } from 'react';
import { 
  Box, 
  Typography,
  Paper,
  Button,
  Grid,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Stepper,
  Step,
  StepLabel,
  CircularProgress,
  FormHelperText,
  Alert,
  Stack
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { format, addMinutes, isValid } from 'date-fns';
import { Appointment, getAvailableSlots, createAppointment } from '../services/appointmentService';
import { validateForm, createAppointmentValidators, ValidationError } from '../utils/validation';
import { AppointmentFormData } from '../utils/types';

// Appointment types
const APPOINTMENT_TYPES = [
  { value: 'Maintenance', label: 'Routine Maintenance' },
  { value: 'Repair', label: 'Repair' },
  { value: 'Recall', label: 'Recall Service' },
  { value: 'Diagnostic', label: 'Diagnostic' },
  { value: 'Inspection', label: 'Inspection' }
];

// Transportation types
const TRANSPORTATION_TYPES = [
  { value: 'Self', label: 'Self (Wait)' },
  { value: 'Pickup', label: 'Pickup Service' },
  { value: 'Loaner', label: 'Loaner Vehicle' },
  { value: 'Shuttle', label: 'Shuttle Service' }
];

// Duration options in minutes
const DURATION_OPTIONS = [
  { value: 30, label: '30 minutes' },
  { value: 60, label: '1 hour' },
  { value: 90, label: '1.5 hours' },
  { value: 120, label: '2 hours' },
  { value: 180, label: '3 hours' },
  { value: 240, label: '4 hours' }
];

const AppointmentScheduler: React.FC = () => {
  const [activeStep, setActiveStep] = useState<number>(0);
  const [formData, setFormData] = useState<AppointmentFormData>({
    customerId: '',
    vehicleId: '',
    appointmentType: '',
    date: null,
    time: '',
    duration: 60,
    customerConcerns: '',
    transportationType: 'Self',
    contactPhone: '',
    contactEmail: ''
  });
  
  const [availableSlots, setAvailableSlots] = useState<Appointment[]>([]);
  const [selectedSlot, setSelectedSlot] = useState<Appointment | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [confirmationOpen, setConfirmationOpen] = useState<boolean>(false);
  const [errors, setErrors] = useState<ValidationError<AppointmentFormData>>({});
  const [submitted, setSubmitted] = useState<boolean>(false);
  const [appointmentSuccess, setAppointmentSuccess] = useState<boolean>(false);
  
  const steps = ['Select Customer & Vehicle', 'Choose Service Type', 'Select Date & Time', 'Confirm Appointment'];
  
  // Handle form field changes
  const handleInputChange = (field: keyof AppointmentFormData) => (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement> | null,
    value?: any
  ) => {
    // Clear field-specific error when the field is edited
    if (errors[field]) {
      setErrors({ ...errors, [field]: undefined });
    }
    
    // Handle date picker which returns the value directly
    if (field === 'date' && e === null) {
      setFormData({ ...formData, [field]: value });
      return;
    }
    
    // Handle regular input changes
    const inputValue = e?.target.value ?? value;
    setFormData({ ...formData, [field]: inputValue });
  };
  
  // Validate the current step
  const validateCurrentStep = (): boolean => {
    let fieldsToValidate: (keyof AppointmentFormData)[] = [];
    
    switch (activeStep) {
      case 0:
        fieldsToValidate = ['customerId', 'vehicleId'];
        break;
      case 1:
        fieldsToValidate = ['appointmentType', 'customerConcerns', 'transportationType'];
        break;
      case 2:
        fieldsToValidate = ['date', 'time', 'duration'];
        break;
      case 3:
        fieldsToValidate = ['contactPhone', 'contactEmail'];
        break;
    }
    
    // Create a subset of validators for current step fields
    const stepValidators: any = {};
    fieldsToValidate.forEach(field => {
      if (createAppointmentValidators[field]) {
        stepValidators[field] = createAppointmentValidators[field];
      }
    });
    
    // Validate only the fields for current step
    const validationErrors = validateForm(formData, stepValidators);
    setErrors(validationErrors);
    
    // Return true if no errors
    return Object.keys(validationErrors).length === 0;
  };
  
  // Handle next step button
  const handleNext = () => {
    const isValid = validateCurrentStep();
    
    if (!isValid) {
      return;
    }
    
    if (activeStep === 2) {
      // Fetch available slots if moving to date selection
      fetchAvailableSlots();
    }
    
    setActiveStep((prevStep) => prevStep + 1);
  };
  
  // Handle back button
  const handleBack = () => {
    setActiveStep((prevStep) => prevStep - 1);
  };
    // Fetch available slots based on selected date and service type
  const fetchAvailableSlots = async () => {
    if (!formData.date) {
      setErrors({ ...errors, date: 'Please select a date' });
      return;
    }
    
    setLoading(true);
    setError(null);
    
    try {
      const slots = await getAvailableSlots(
        formData.date, 
        formData.appointmentType, 
        formData.duration
      );
      
      // Convert TimeSlot[] to Appointment[] to match expected type
      const appointmentSlots = slots.map(slot => ({
        id: slot.id,
        customerId: '',
        vehicleId: '',
        appointmentType: formData.appointmentType,
        status: 'Available',
        scheduledStartTime: slot.startTime,
        scheduledEndTime: slot.endTime,
        transportationType: '',
        confirmationStatus: 'Pending',
        duration: slot.duration,
        startTime: slot.startTime, // For available slots
        available: slot.available
      }));
      
      setAvailableSlots(appointmentSlots);
      
      // Clear any selected slot when date changes
      setSelectedSlot(null);
      setFormData({ ...formData, time: '' });
    } catch (err) {
      console.error('Error fetching available slots:', err);
      setError('Failed to fetch available time slots. Please try again.');
    } finally {
      setLoading(false);
    }
  };
  
  // Handle slot selection
  const handleSelectSlot = (slot: Appointment) => {
    setSelectedSlot(slot);
    setFormData({ ...formData, time: format(new Date(slot.startTime), 'HH:mm') });
    
    // Clear time error if present
    if (errors.time) {
      setErrors({ ...errors, time: undefined });
    }
  };
  
  // Submit the appointment
  const handleSubmit = async () => {
    // Final validation of all fields
    const allValidators = createAppointmentValidators;
    const validationErrors = validateForm(formData, allValidators);
    setErrors(validationErrors);
    
    if (Object.keys(validationErrors).length > 0) {
      return;
    }
    
    setLoading(true);
    setError(null);
    
    try {
      // Combine date and time for API request
      const appointmentDateTime = formData.date;
      if (appointmentDateTime && formData.time) {
        const [hours, minutes] = formData.time.split(':').map(Number);
        appointmentDateTime.setHours(hours, minutes);
      }
      
      const appointmentRequest = {
        customerId: formData.customerId,
        vehicleId: formData.vehicleId,
        serviceType: formData.appointmentType,
        notes: formData.customerConcerns,
        startTime: appointmentDateTime?.toISOString(),
        duration: formData.duration,
        transportationType: formData.transportationType,
        contactPhone: formData.contactPhone,
        contactEmail: formData.contactEmail
      };
      
      const response = await createAppointment(appointmentRequest);
      
      setAppointmentSuccess(true);
      setConfirmationOpen(false);
      
      // Reset form after successful submission
      setFormData({
        customerId: '',
        vehicleId: '',
        appointmentType: '',
        date: null,
        time: '',
        duration: 60,
        customerConcerns: '',
        transportationType: 'Self',
        contactPhone: '',
        contactEmail: ''
      });
      
      setActiveStep(0);
    } catch (err: any) {
      console.error('Error creating appointment:', err);
      setError(err.message || 'Failed to create appointment. Please try again.');
      
      // Handle validation errors from API if available
      if (err.validation) {
        setErrors(err.validation);
      }
    } finally {
      setLoading(false);
      setSubmitted(true);
    }
  };
  
  // Render different step content
  const renderStepContent = () => {
    switch (activeStep) {
      case 0:
        return renderCustomerVehicleStep();
      case 1:
        return renderServiceTypeStep();
      case 2:
        return renderDateTimeStep();
      case 3:
        return renderConfirmStep();
      default:
        return null;
    }
  };
  
  // Step 1: Customer & Vehicle selection
  const renderCustomerVehicleStep = () => {
    // Mock data for demonstration
    const customers = [
      { id: 'cust1', name: 'John Doe' },
      { id: 'cust2', name: 'Jane Smith' },
      { id: 'cust3', name: 'Mike Johnson' }
    ];
    
    const vehicles = [
      { id: 'veh1', customerId: 'cust1', name: '2020 Toyota Camry', vin: '1HGBH41JXMN109186' },
      { id: 'veh2', customerId: 'cust1', name: '2018 Honda Accord', vin: 'JH4KA3240KC023341' },
      { id: 'veh3', customerId: 'cust2', name: '2021 Ford F-150', vin: '1FTFW1ET5BFC13001' }
    ];
    
    // Filter vehicles by selected customer
    const customerVehicles = vehicles.filter(v => v.customerId === formData.customerId);
    
    return (
      <Box sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>Select Customer and Vehicle</Typography>
        
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <FormControl fullWidth error={!!errors.customerId}>
              <InputLabel id="customer-select-label">Customer</InputLabel>
              <Select
                labelId="customer-select-label"
                id="customer-select"
                value={formData.customerId}
                onChange={handleInputChange('customerId') as any}
                label="Customer"
              >
                {customers.map(customer => (
                  <MenuItem key={customer.id} value={customer.id}>{customer.name}</MenuItem>
                ))}
              </Select>
              {errors.customerId && <FormHelperText>{errors.customerId}</FormHelperText>}
            </FormControl>
          </Grid>
          
          <Grid item xs={12}>
            <FormControl fullWidth error={!!errors.vehicleId}>
              <InputLabel id="vehicle-select-label">Vehicle</InputLabel>
              <Select
                labelId="vehicle-select-label"
                id="vehicle-select"
                value={formData.vehicleId}
                onChange={handleInputChange('vehicleId') as any}
                label="Vehicle"
                disabled={!formData.customerId}
              >
                {customerVehicles.map(vehicle => (
                  <MenuItem key={vehicle.id} value={vehicle.id}>
                    {vehicle.name} (VIN: {vehicle.vin.substring(vehicle.vin.length - 6)})
                  </MenuItem>
                ))}
              </Select>
              {errors.vehicleId && <FormHelperText>{errors.vehicleId}</FormHelperText>}
              {formData.customerId && customerVehicles.length === 0 && 
                <FormHelperText>No vehicles found for this customer</FormHelperText>
              }
            </FormControl>
          </Grid>
        </Grid>
      </Box>
    );
  };
  
  // Step 2: Service Type selection
  const renderServiceTypeStep = () => {
    return (
      <Box sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>Choose Service Type</Typography>
        
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <FormControl fullWidth error={!!errors.appointmentType}>
              <InputLabel id="appointment-type-label">Service Type</InputLabel>
              <Select
                labelId="appointment-type-label"
                id="appointment-type"
                value={formData.appointmentType}
                onChange={handleInputChange('appointmentType') as any}
                label="Service Type"
              >
                {APPOINTMENT_TYPES.map(type => (
                  <MenuItem key={type.value} value={type.value}>{type.label}</MenuItem>
                ))}
              </Select>
              {errors.appointmentType && <FormHelperText>{errors.appointmentType}</FormHelperText>}
            </FormControl>
          </Grid>
          
          <Grid item xs={12}>
            <FormControl fullWidth error={!!errors.transportationType}>
              <InputLabel id="transportation-type-label">Transportation</InputLabel>
              <Select
                labelId="transportation-type-label"
                id="transportation-type"
                value={formData.transportationType}
                onChange={handleInputChange('transportationType') as any}
                label="Transportation"
              >
                {TRANSPORTATION_TYPES.map(type => (
                  <MenuItem key={type.value} value={type.value}>{type.label}</MenuItem>
                ))}
              </Select>
              {errors.transportationType && <FormHelperText>{errors.transportationType}</FormHelperText>}
            </FormControl>
          </Grid>
          
          <Grid item xs={12}>
            <TextField
              id="customer-concerns"
              label="Customer Concerns / Service Requests"
              multiline
              rows={4}
              fullWidth
              value={formData.customerConcerns}
              onChange={handleInputChange('customerConcerns')}
              error={!!errors.customerConcerns}
              helperText={errors.customerConcerns || 'Please describe the issue or service needed'}
            />
          </Grid>
          
          <Grid item xs={12}>
            <FormControl fullWidth>
              <InputLabel id="duration-label">Estimated Duration</InputLabel>
              <Select
                labelId="duration-label"
                id="duration"
                value={formData.duration}
                onChange={handleInputChange('duration') as any}
                label="Estimated Duration"
              >
                {DURATION_OPTIONS.map(option => (
                  <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </Box>
    );
  };
  
  // Step 3: Date & Time selection
  const renderDateTimeStep = () => {
    return (
      <Box sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>Select Date & Time</Typography>
        
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <LocalizationProvider dateAdapter={AdapterDateFns}>
              <DatePicker
                label="Appointment Date"
                value={formData.date}
                onChange={(newDate) => {
                  handleInputChange('date')(null, newDate);
                  
                  // Clear available slots when date changes
                  setAvailableSlots([]);
                  setSelectedSlot(null);
                  
                  if (newDate && isValid(newDate)) {
                    fetchAvailableSlots();
                  }
                }}
                slotProps={{
                  textField: {
                    fullWidth: true,
                    error: !!errors.date,
                    helperText: errors.date
                  }
                }}
              />
            </LocalizationProvider>
          </Grid>
          
          <Grid item xs={12}>
            {loading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', my: 4 }}>
                <CircularProgress />
              </Box>
            ) : (
              <>
                {formData.date && (
                  <>
                    <Typography variant="subtitle1" gutterBottom>
                      Available Time Slots
                    </Typography>
                    
                    {error && (
                      <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>
                    )}
                    
                    {availableSlots.length === 0 ? (
                      <Alert severity="info">
                        No available slots for the selected date and service type. 
                        Please try another date.
                      </Alert>
                    ) : (
                      <Grid container spacing={2}>
                        {availableSlots.map((slot) => {
                          const startTime = new Date(slot.startTime);
                          const endTime = addMinutes(startTime, slot.duration);
                          const timeDisplay = `${format(startTime, 'h:mm a')} - ${format(endTime, 'h:mm a')}`;
                          const isSelected = selectedSlot?.id === slot.id;
                          
                          return (
                            <Grid item xs={6} sm={4} md={3} key={slot.id}>
                              <Button 
                                variant={isSelected ? "contained" : "outlined"}
                                fullWidth
                                onClick={() => handleSelectSlot(slot)}
                                sx={{ py: 2 }}
                              >
                                {timeDisplay}
                              </Button>
                            </Grid>
                          );
                        })}
                      </Grid>
                    )}
                    
                    {errors.time && (
                      <FormHelperText error>{errors.time}</FormHelperText>
                    )}
                  </>
                )}
              </>
            )}
          </Grid>
        </Grid>
      </Box>
    );
  };
  
  // Step 4: Confirmation step
  const renderConfirmStep = () => {
    // Get display values
    const selectedCustomer = { id: 'cust1', name: 'John Doe' }; // Mock data
    const selectedVehicle = { id: 'veh1', name: '2020 Toyota Camry' }; // Mock data
    
    const appointmentTypeLabel = APPOINTMENT_TYPES.find(t => t.value === formData.appointmentType)?.label || '';
    const transportationTypeLabel = TRANSPORTATION_TYPES.find(t => t.value === formData.transportationType)?.label || '';
    
    return (
      <Box sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>Confirm Appointment Details</Typography>
        
        <Paper variant="outlined" sx={{ p: 2, mb: 3 }}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Typography variant="subtitle2">Customer</Typography>
              <Typography variant="body1">{selectedCustomer.name}</Typography>
            </Grid>
            
            <Grid item xs={12} sm={6}>
              <Typography variant="subtitle2">Vehicle</Typography>
              <Typography variant="body1">{selectedVehicle.name}</Typography>
            </Grid>
            
            <Grid item xs={12} sm={6}>
              <Typography variant="subtitle2">Service Type</Typography>
              <Typography variant="body1">{appointmentTypeLabel}</Typography>
            </Grid>
            
            <Grid item xs={12} sm={6}>
              <Typography variant="subtitle2">Transportation</Typography>
              <Typography variant="body1">{transportationTypeLabel}</Typography>
            </Grid>
            
            <Grid item xs={12} sm={6}>
              <Typography variant="subtitle2">Date & Time</Typography>
              <Typography variant="body1">
                {formData.date ? format(formData.date, 'MMMM d, yyyy') : ''}
                {formData.time ? ` at ${formData.time}` : ''}
              </Typography>
            </Grid>
            
            <Grid item xs={12} sm={6}>
              <Typography variant="subtitle2">Duration</Typography>
              <Typography variant="body1">
                {DURATION_OPTIONS.find(opt => opt.value === formData.duration)?.label}
              </Typography>
            </Grid>
            
            <Grid item xs={12}>
              <Typography variant="subtitle2">Service Requests / Concerns</Typography>
              <Typography variant="body1">{formData.customerConcerns || 'None provided'}</Typography>
            </Grid>
          </Grid>
        </Paper>
        
        <Typography variant="h6" gutterBottom>Contact Information</Typography>
        
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <TextField
              fullWidth
              id="contact-phone"
              label="Contact Phone Number"
              value={formData.contactPhone}
              onChange={handleInputChange('contactPhone')}
              error={!!errors.contactPhone}
              helperText={errors.contactPhone || 'Phone number for appointment reminders'}
              required
            />
          </Grid>
          
          <Grid item xs={12} md={6}>
            <TextField
              fullWidth
              id="contact-email"
              label="Contact Email"
              value={formData.contactEmail}
              onChange={handleInputChange('contactEmail')}
              error={!!errors.contactEmail}
              helperText={errors.contactEmail || 'Email for appointment confirmation'}
            />
          </Grid>
        </Grid>
      </Box>
    );
  };
  
  return (
    <Box sx={{ p: 3 }}>
      <Paper sx={{ p: 3 }}>
        <Typography variant="h4" gutterBottom>Schedule Service Appointment</Typography>
        
        {/* Success message after submission */}
        {appointmentSuccess && (
          <Alert 
            severity="success" 
            sx={{ mb: 3 }}
            onClose={() => setAppointmentSuccess(false)}
          >
            Your appointment has been successfully scheduled! A confirmation email has been sent.
          </Alert>
        )}
        
        {/* Main stepper */}
        <Stepper activeStep={activeStep} sx={{ mb: 4 }} alternativeLabel>
          {steps.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>
        
        {/* Step content */}
        {renderStepContent()}
        
        {/* Navigation buttons */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 3 }}>
          <Button
            variant="outlined"
            disabled={activeStep === 0}
            onClick={handleBack}
          >
            Back
          </Button>
          
          {activeStep === steps.length - 1 ? (
            <Button
              variant="contained"
              color="primary"
              onClick={() => setConfirmationOpen(true)}
            >
              Schedule Appointment
            </Button>
          ) : (
            <Button
              variant="contained"
              color="primary"
              onClick={handleNext}
            >
              Next
            </Button>
          )}
        </Box>
      </Paper>
      
      {/* Confirmation dialog */}
      <Dialog
        open={confirmationOpen}
        onClose={() => setConfirmationOpen(false)}
      >
        <DialogTitle>Confirm Appointment</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to schedule this service appointment?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmationOpen(false)}>Cancel</Button>
          <Button 
            onClick={handleSubmit} 
            variant="contained" 
            color="primary"
            disabled={loading}
          >
            {loading ? <CircularProgress size={24} /> : 'Confirm'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default AppointmentScheduler;
