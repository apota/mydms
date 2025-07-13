// @ts-nocheck
import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { 
  Box, 
  Typography, 
  TextField, 
  Button, 
  Grid, 
  Paper, 
  FormControl, 
  InputLabel, 
  Select, 
  MenuItem, 
  FormHelperText,
  Divider,
  CircularProgress,
  IconButton,
  Card,
  CardContent,
  Stepper,
  Step,
  StepLabel
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { Formik, Form, FormikHelpers } from 'formik';
import * as Yup from 'yup';
import { createDeal } from '../../services/dealService';
import { useNotification } from '../../contexts/NotificationContext';
import { DealType } from '../../types/deal';
import { getVehiclesByType, getCrmCustomers } from '../../services/integrationServices';

interface DealFormValues {
  customerId: string;
  vehicleId: string;
  salesRepId: string;
  type: DealType;
  tradeIn?: {
    make: string;
    model: string;
    year: number;
    value: number;
  };
  notes?: string;
}

// Step interfaces for multi-step form
interface CustomerStep {
  customerId: string;
}

interface VehicleStep {
  vehicleId: string;
  type: DealType;
}

interface TradeInStep {
  hasTradeIn: boolean;
  tradeIn?: {
    make: string;
    model: string;
    year: number;
    value: number;
  };
}

interface FinalStep {
  salesRepId: string;
  notes?: string;
}

type FormStep = CustomerStep | VehicleStep | TradeInStep | FinalStep;

const DealCreate: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { showSuccess, showError } = useNotification();
  const [activeStep, setActiveStep] = useState(0);
  const [customers, setCustomers] = useState([]);
  const [vehicles, setVehicles] = useState([]);
  const [salesReps, setSalesReps] = useState([]);
  const [loading, setLoading] = useState(false);
  const [loadingVehicles, setLoadingVehicles] = useState(false);  const [formData, setFormData] = useState<DealFormValues>({
    customerId: '',
    vehicleId: '',
    salesRepId: '',
    type: DealType.New,
    notes: ''
  });

  const validationSchema = Yup.object({
    title: Yup.string().required('Title is required'),
    customerId: Yup.string().required('Customer is required'),
    vehicleId: Yup.string().required('Vehicle is required'),
    totalAmount: Yup.number().min(0, 'Must be a positive value').required('Total amount is required')
  });
  // Get current step validation schema
  const getStepValidationSchema = (step: number) => {
    switch (step) {
      case 0:
        return customerStepValidation;
      case 1:
        return vehicleStepValidation;
      case 2:
        return tradeInStepValidation;
      case 3:
        return finalStepValidation;
      default:
        return Yup.object({});
    }
  };

  const handleNext = async (values: FormStep) => {
    // Merge the values from this step into the formData
    setFormData(prevData => ({ ...prevData, ...values }));
    
    // If selecting deal type, fetch vehicles
    if (activeStep === 1 && 'type' in values) {
      fetchVehiclesByType(values.type);
    }
    
    setActiveStep(prevStep => prevStep + 1);
  };

  const handleBack = () => {
    setActiveStep(prevStep => prevStep - 1);
  };

  const handleSubmit = async (values: DealFormValues, { setSubmitting }: FormikHelpers<DealFormValues>) => {
    setLoading(true);
    try {
      // Submit the final form with all collected data
      const finalFormData = { ...formData, ...values };
      const result = await createDeal(finalFormData);
      showSuccess('Deal created successfully!');
      setTimeout(() => navigate(`/deals/${result.id}`), 1500);
    } catch (error) {      console.error('Error creating deal:', error);
      showError('Failed to create deal: ' + (error.response?.data?.message || 'Unknown error'));
    } finally {
      setSubmitting(false);
      setLoading(false);
    }
  };
  
  // Render the appropriate step form
  const renderStepContent = (step: number) => {
    switch (step) {
      case 0:
        return (
          <CustomerStep 
            initialValues={{ customerId: formData.customerId }}
            customers={customers}
            loading={loading}
            onSubmit={handleNext}
          />
        );
      case 1:
        return (
          <VehicleStep
            initialValues={{ vehicleId: formData.vehicleId, type: formData.type }}
            vehicles={vehicles}
            loading={loadingVehicles}
            onSubmit={handleNext}
          />
        );
      case 2:
        return (
          <TradeInStep
            initialValues={{ 
              hasTradeIn: Boolean(formData.tradeIn),
              tradeIn: formData.tradeIn 
            }}
            onSubmit={handleNext}
          />
        );
      case 3:
        return (
          <FinalStep
            initialValues={{ 
              salesRepId: formData.salesRepId,
              notes: formData.notes 
            }}
            salesReps={salesReps}
            loading={loading}
            onSubmit={handleSubmit}
          />
        );
      default:
        return null;
    }
  };

  return (
    <Box p={3}>
      <Typography variant="h4" gutterBottom>
        Create New Deal
      </Typography>
      
      <Paper elevation={3} sx={{ p: 3 }}>
        <Formik
          initialValues={initialValues}
          validationSchema={validationSchema}
          onSubmit={handleSubmit}
        >
          {({ values, errors, touched, handleChange, handleBlur, isSubmitting }: {
            values: DealFormValues;
            errors: Record<string, string>;
            touched: Record<string, boolean>;
            handleChange: (e: React.ChangeEvent<any>) => void;
            handleBlur: (e: React.FocusEvent<any>) => void;
            isSubmitting: boolean;
          }) => (
            <Form>
              <Grid container spacing={3}>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    id="title"
                    name="title"
                    label="Deal Title"
                    value={values.title}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.title && Boolean(errors.title)}
                    helperText={touched.title && errors.title}
                  />
                </Grid>
                
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    id="description"
                    name="description"
                    label="Description"
                    multiline
                    rows={4}
                    value={values.description}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.description && Boolean(errors.description)}
                    helperText={touched.description && errors.description}
                  />
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <FormControl fullWidth error={touched.customerId && Boolean(errors.customerId)}>
                    <InputLabel id="customer-label">Customer</InputLabel>
                    <Select
                      labelId="customer-label"
                      id="customerId"
                      name="customerId"
                      value={values.customerId}
                      label="Customer"
                      onChange={handleChange}
                      onBlur={handleBlur}
                    >
                      <MenuItem value="customer1">John Doe</MenuItem>
                      <MenuItem value="customer2">Jane Smith</MenuItem>
                      <MenuItem value="customer3">Robert Johnson</MenuItem>
                    </Select>
                    {touched.customerId && errors.customerId && (
                      <FormHelperText>{errors.customerId}</FormHelperText>
                    )}
                  </FormControl>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <FormControl fullWidth error={touched.vehicleId && Boolean(errors.vehicleId)}>
                    <InputLabel id="vehicle-label">Vehicle</InputLabel>
                    <Select
                      labelId="vehicle-label"
                      id="vehicleId"
                      name="vehicleId"
                      value={values.vehicleId}
                      label="Vehicle"
                      onChange={handleChange}
                      onBlur={handleBlur}
                    >
                      <MenuItem value="vehicle1">2023 Toyota Camry</MenuItem>
                      <MenuItem value="vehicle2">2023 Honda Civic</MenuItem>
                      <MenuItem value="vehicle3">2022 Ford F-150</MenuItem>
                    </Select>
                    {touched.vehicleId && errors.vehicleId && (
                      <FormHelperText>{errors.vehicleId}</FormHelperText>
                    )}
                  </FormControl>
                </Grid>
                  <Grid item xs={12} md={6}>
                  <FormControl fullWidth>
                    <InputLabel id="salesperson-label">Sales Person</InputLabel>
                    <Select
                      labelId="salesperson-label"
                      id="salesRepId"
                      name="salesRepId"
                      value={values.salesRepId}
                      label="Sales Person"
                      onChange={handleChange}
                      onBlur={handleBlur}
                    >
                      <MenuItem value="salesperson1">Alex Smith</MenuItem>
                      <MenuItem value="salesperson2">Maria Rodriguez</MenuItem>
                      <MenuItem value="salesperson3">David Johnson</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <TextField
                    fullWidth
                    id="totalAmount"
                    name="totalAmount"
                    label="Total Amount"
                    type="number"
                    inputProps={{ min: 0, step: 0.01 }}
                    value={values.totalAmount}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.totalAmount && Boolean(errors.totalAmount)}
                    helperText={touched.totalAmount && errors.totalAmount}
                  />
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <FormControl fullWidth>
                    <InputLabel id="status-label">Status</InputLabel>
                    <Select
                      labelId="status-label"
                      id="status"
                      name="status"
                      value={values.status}
                      label="Status"
                      onChange={handleChange}
                      onBlur={handleBlur}
                    >
                      <MenuItem value="NEW">New</MenuItem>
                      <MenuItem value="NEGOTIATION">In Negotiation</MenuItem>
                      <MenuItem value="PENDING_APPROVAL">Pending Approval</MenuItem>
                      <MenuItem value="APPROVED">Approved</MenuItem>
                      <MenuItem value="CLOSED">Closed</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                
                <Grid item xs={12}>
                  <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 2 }}>
                    <Button 
                      variant="outlined" 
                      onClick={() => navigate('/deals')}
                    >
                      Cancel
                    </Button>
                    <Button 
                      type="submit"
                      variant="contained" 
                      color="primary"
                      disabled={isSubmitting}
                    >
                      Create Deal
                    </Button>
                  </Box>
                </Grid>
              </Grid>
            </Form>
          )}
        </Formik>
      </Paper>
      
      <Snackbar 
        open={snackbarOpen} 
        autoHideDuration={6000} 
        onClose={handleSnackbarClose}
      >
        <Alert 
          onClose={handleSnackbarClose} 
          severity={snackbarSeverity} 
          sx={{ width: '100%' }}
        >
          {snackbarMessage}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default DealCreate;
