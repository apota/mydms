// @ts-nocheck
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
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
  CircularProgress,
  Divider,
  Card,
  CardContent,
  IconButton
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { Formik, Form, FormikHelpers } from 'formik';
import * as Yup from 'yup';
import { createLead } from '../../services/leadService';
import { InterestType } from '../../types/lead';
import { useNotification } from '../../contexts/NotificationContext';

// Import services for integration with other modules
import { getVehiclesByType } from '../../services/integrationServices';

interface Address {
  street: string;
  city: string;
  state: string;
  zip: string;
}

interface LeadFormValues {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  source: string;
  interestType: InterestType;
  interestVehicleId?: string;
  address?: Address;
  comments?: string;
  followupDate?: string;
}

const phoneRegExp = /^((\\+[1-9]{1,4}[ \\-]*)|(\\([0-9]{2,3}\\)[ \\-]*)|([0-9]{2,4})[ \\-]*)*?[0-9]{3,4}?[ \\-]*[0-9]{3,4}?$/;

const validationSchema = Yup.object({
  firstName: Yup.string().required('First name is required'),
  lastName: Yup.string().required('Last name is required'),
  email: Yup.string().email('Invalid email address').required('Email is required'),
  phone: Yup.string()
    .matches(phoneRegExp, 'Phone number is not valid')
    .required('Phone number is required'),
  source: Yup.string().required('Source is required'),
  interestType: Yup.string().required('Interest type is required'),
  address: Yup.object().shape({
    street: Yup.string(),
    city: Yup.string(),
    state: Yup.string().max(2, 'Please use 2-letter state code'),
    zip: Yup.string().matches(/^\d{5}(-\d{4})?$/, 'Invalid ZIP code')
  }),
  followupDate: Yup.date().nullable().min(new Date(), 'Follow-up date cannot be in the past')
});

const CreateLead: React.FC = () => {
  const navigate = useNavigate();
  const { showSuccess, showError } = useNotification();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [vehicles, setVehicles] = useState([]);
  const [loadingVehicles, setLoadingVehicles] = useState(false);
  
  // Effect to fetch vehicles based on selected interest type
  useEffect(() => {
    const fetchVehicles = async (interestType) => {
      if (!interestType) return;
      
      setLoadingVehicles(true);
      try {
        // Integration with inventory management
        const data = await getVehiclesByType(interestType.toLowerCase());
        setVehicles(data || []);
      } catch (error) {
        console.error('Error fetching vehicles:', error);
      } finally {
        setLoadingVehicles(false);
      }
    };
    
    // Initial fetch of new vehicles
    fetchVehicles(InterestType.New);
  }, []);

  const initialValues: LeadFormValues = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    source: 'Website',
    interestType: InterestType.New,
    comments: '',
    address: {
      street: '',
      city: '',
      state: '',
      zip: ''
    },
    followupDate: ''
  };

  const handleSubmit = async (
    values: LeadFormValues,
    { setSubmitting, resetForm, setFieldError }: FormikHelpers<LeadFormValues>
  ) => {
    setIsSubmitting(true);
    try {
      const response = await createLead(values);
      showSuccess('Lead created successfully!');
      resetForm();
      navigate(`/leads/${response.id}`);
    } catch (error) {
      console.error('Error creating lead:', error);
      
      if (error.response && error.response.data) {
        // Handle validation errors from the server
        if (error.response.data.errors) {
          const serverErrors = error.response.data.errors;
          Object.keys(serverErrors).forEach((key) => {
            setFieldError(key, serverErrors[key][0]);
          });
        } else {
          showError(error.response.data.message || 'Failed to create lead');
        }
      } else {
        showError('An unexpected error occurred. Please try again.');
      }
    } finally {      setIsSubmitting(false);
      setSubmitting(false);    }
  };

  // Handle interest type change to fetch relevant vehicles
  const handleInterestTypeChange = async (interestType, setFieldValue) => {
    setFieldValue('interestType', interestType);
    setFieldValue('interestVehicleId', ''); // Reset vehicle selection
    
    if (interestType === InterestType.Service) return; // No vehicles needed for service
    
    setLoadingVehicles(true);
    try {
      const data = await getVehiclesByType(interestType.toLowerCase());
      setVehicles(data || []);
    } catch (error) {
      console.error('Error fetching vehicles for interest type:', error);
      showError('Failed to load vehicles. Please try again.');
    } finally {
      setLoadingVehicles(false);
    }
  };

  return (    <Box p={3}>
      <Box mb={3} display="flex" alignItems="center">
        <IconButton 
          onClick={() => navigate('/leads')} 
          sx={{ mr: 2 }}
          aria-label="back to leads"
        >
          <ArrowBackIcon />
        </IconButton>
        <Typography variant="h4">Create New Lead</Typography>
      </Box>
      
      <Paper elevation={3} sx={{ p: 3 }}>
        <Formik
          initialValues={initialValues}
          validationSchema={validationSchema}
          onSubmit={handleSubmit}
        >
          {({ values, errors, touched, handleChange, handleBlur, isSubmitting, handleSubmit }) => (
            <Form onSubmit={handleSubmit}>
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <TextField
                    fullWidth
                    id="firstName"
                    name="firstName"
                    label="First Name"
                    value={values.firstName}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.firstName && Boolean(errors.firstName)}
                    helperText={touched.firstName && errors.firstName}
                  />
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <TextField
                    fullWidth
                    id="lastName"
                    name="lastName"
                    label="Last Name"
                    value={values.lastName}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.lastName && Boolean(errors.lastName)}
                    helperText={touched.lastName && errors.lastName}
                  />
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <TextField
                    fullWidth
                    id="email"
                    name="email"
                    label="Email"
                    value={values.email}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.email && Boolean(errors.email)}
                    helperText={touched.email && errors.email}
                  />
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <TextField
                    fullWidth
                    id="phone"
                    name="phone"
                    label="Phone Number"
                    value={values.phone}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.phone && Boolean(errors.phone)}
                    helperText={touched.phone && errors.phone}
                  />
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <FormControl fullWidth error={touched.source && Boolean(errors.source)}>
                    <InputLabel id="source-label">Source</InputLabel>
                    <Select
                      labelId="source-label"
                      id="source"
                      name="source"
                      value={values.source}
                      label="Source"
                      onChange={handleChange}
                      onBlur={handleBlur}
                    >
                      {Object.values(LeadSource).map((source) => (
                        <MenuItem key={source} value={source}>
                          {source}
                        </MenuItem>
                      ))}
                    </Select>
                    {touched.source && errors.source && (
                      <FormHelperText>{errors.source}</FormHelperText>
                    )}
                  </FormControl>
                </Grid>
                  <Grid item xs={12} md={6}>
                  <FormControl fullWidth error={touched.interestType && Boolean(errors.interestType)}>
                    <InputLabel id="interestType-label">Interest Type</InputLabel>
                    <Select
                      labelId="interestType-label"
                      id="interestType"
                      name="interestType"
                      value={values.interestType}
                      label="Interest Type"
                      onChange={(e) => handleInterestTypeChange(e.target.value, setFieldValue)}
                      onBlur={handleBlur}
                    >
                      {Object.values(InterestType).map((type) => (
                        <MenuItem key={type} value={type}>
                          {type}
                        </MenuItem>
                      ))}
                    </Select>
                    {touched.interestType && errors.interestType && (
                      <FormHelperText>{errors.interestType}</FormHelperText>
                    )}
                  </FormControl>
                </Grid>
                
                {values.interestType !== InterestType.Service && (
                  <Grid item xs={12} md={12}>
                    <FormControl fullWidth>
                      <InputLabel id="interestVehicleId-label">Interested Vehicle</InputLabel>
                      <Select
                        labelId="interestVehicleId-label"
                        id="interestVehicleId"
                        name="interestVehicleId"
                        value={values.interestVehicleId}
                        label="Interested Vehicle"
                        onChange={handleChange}
                        onBlur={handleBlur}
                        disabled={loadingVehicles || vehicles.length === 0}
                      >
                        {loadingVehicles ? (
                          <MenuItem value="">
                            <CircularProgress size={20} /> Loading vehicles...
                          </MenuItem>
                        ) : vehicles.length === 0 ? (
                          <MenuItem value="">No vehicles available</MenuItem>
                        ) : (
                          vehicles.map((vehicle) => (
                            <MenuItem key={vehicle.id} value={vehicle.id}>
                              {vehicle.year} {vehicle.make} {vehicle.model} (${vehicle.price})
                            </MenuItem>
                          ))
                        )}
                      </Select>
                    </FormControl>
                  </Grid>
                )}
                  <Grid item xs={12} md={6}>
                  <TextField
                    fullWidth
                    id="followupDate"
                    name="followupDate"
                    label="Follow-up Date"
                    type="date"
                    value={values.followupDate}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.followupDate && Boolean(errors.followupDate)}
                    helperText={touched.followupDate && errors.followupDate}
                    InputLabelProps={{
                      shrink: true,
                    }}
                  />
                </Grid>
                
                <Grid item xs={12}>
                  <TextField
                    fullWidth                    id="comments"
                    name="comments"
                    label="Comments"
                    multiline
                    rows={4}
                    value={values.comments}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
                
                <Grid item xs={12}>
                  <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 2 }}>
                    <Button 
                      variant="outlined" 
                      onClick={() => navigate('/leads')}
                      disabled={isSubmitting}
                    >
                      Cancel
                    </Button>
                    <Button 
                      type="submit"                      variant="contained" 
                      color="primary"
                      disabled={isSubmitting}
                    >
                      {isSubmitting ? <CircularProgress size={24} /> : 'Create Lead'}
                    </Button>
                  </Box>
                </Grid>
              </Grid>
            </Form>
          )}
        </Formik>
      </Paper>
    </Box>
  );
};

export default CreateLead;
