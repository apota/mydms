import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  Grid,
  TextField,
  Button,
  MenuItem,
  InputAdornment,
  IconButton,
  Alert,
  Divider,
  CircularProgress,
  Autocomplete
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import {
  ArrowBack,
  Search as SearchIcon,
  Save as SaveIcon
} from '@mui/icons-material';
import { format, addDays } from 'date-fns';

import { CoreTrackingService, PartsService, IntegrationService } from '../../services/api';

const TrackNewCore = () => {
  const navigate = useNavigate();
  
  // Form state
  const [partSearchTerm, setPartSearchTerm] = useState('');
  const [selectedPart, setSelectedPart] = useState(null);
  const [searchResults, setSearchResults] = useState([]);
  const [searching, setSearching] = useState(false);
  
  const [customerId, setCustomerId] = useState('');
  const [customerName, setCustomerName] = useState('');
  const [customerSearchTerm, setCustomerSearchTerm] = useState('');
  const [customerResults, setCustomerResults] = useState([]);
  const [searchingCustomers, setSearchingCustomers] = useState(false);
  
  const [issueDate, setIssueDate] = useState(new Date());
  const [dueDate, setDueDate] = useState(addDays(new Date(), 30)); // Default 30 days
  const [coreValue, setCoreValue] = useState('');
  const [notes, setNotes] = useState('');
  
  // Form submission state
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [formValidationErrors, setFormValidationErrors] = useState({});
  
  // Search for parts
  const searchParts = async () => {
    if (!partSearchTerm) return;
    
    setSearching(true);
    try {
      const data = await PartsService.searchParts(partSearchTerm);
      setSearchResults(data.items || []);
    } catch (err) {
      console.error('Error searching parts:', err);
      setSearchResults([]);
    } finally {
      setSearching(false);
    }
  };
  
  // Handle part selection
  const handleSelectPart = (part) => {
    setSelectedPart(part);
    setPartSearchTerm(part.partNumber);
    if (part.coreValue) {
      setCoreValue(part.coreValue.toString());
    }
    setSearchResults([]);
  };
  
  // Search for customers
  const searchCustomers = async () => {
    if (!customerSearchTerm) return;
    
    setSearchingCustomers(true);
    try {
      // This assumes you have an API to search for customers
      // You might need to integrate with the CRM module
      const data = await IntegrationService.searchCustomers(customerSearchTerm);
      setCustomerResults(data || []);
    } catch (err) {
      console.error('Error searching customers:', err);
      setCustomerResults([]);
    } finally {
      setSearchingCustomers(false);
    }
  };
  
  // Handle customer selection
  const handleSelectCustomer = (customer) => {
    setCustomerId(customer.id);
    setCustomerName(customer.name);
    setCustomerSearchTerm(customer.name);
    setCustomerResults([]);
  };
  
  // Validate form
  const validateForm = () => {
    const errors = {};
    
    if (!selectedPart) {
      errors.part = 'You must select a valid part';
    }
    
    if (!customerName) {
      errors.customer = 'Customer information is required';
    }
    
    if (!coreValue || isNaN(parseFloat(coreValue)) || parseFloat(coreValue) <= 0) {
      errors.coreValue = 'A valid core value greater than 0 is required';
    }
    
    if (!issueDate) {
      errors.issueDate = 'Issue date is required';
    }
    
    if (!dueDate) {
      errors.dueDate = 'Due date is required';
    } else if (dueDate <= issueDate) {
      errors.dueDate = 'Due date must be after issue date';
    }
    
    setFormValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };
  
  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) return;
    
    setSubmitting(true);
    setError(null);
    
    try {
      const coreData = {
        partId: selectedPart.id,
        partNumber: selectedPart.partNumber,
        partDescription: selectedPart.description,
        customerId: customerId || null,
        customerName: customerName,
        issueDate: issueDate.toISOString(),
        dueDate: dueDate.toISOString(),
        coreValue: parseFloat(coreValue),
        notes: notes,
        status: 'PENDING_RETURN'
      };
      
      const result = await CoreTrackingService.createCoreTracking(coreData);
      
      // Navigate to the created core's detail page
      navigate(`/core-tracking/${result.id}`);
    } catch (err) {
      console.error('Error creating core tracking record:', err);
      setError(err.message || 'Failed to create core tracking record. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };
  
  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ mb: 3 }}>
        <Button
          variant="outlined"
          startIcon={<ArrowBack />}
          component={Link}
          to="/core-tracking"
          sx={{ mr: 2 }}
        >
          Back
        </Button>
        <Typography variant="h4" component="h1">
          Track New Core
        </Typography>
      </Box>
      
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}
      
      <form onSubmit={handleSubmit}>
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Part Information
          </Typography>
          
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Search for Part by Part Number or Description"
                value={partSearchTerm}
                onChange={(e) => setPartSearchTerm(e.target.value)}
                error={!!formValidationErrors.part}
                helperText={formValidationErrors.part}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton 
                        onClick={searchParts}
                        edge="end"
                        disabled={searching || !partSearchTerm}
                      >
                        {searching ? <CircularProgress size={20} /> : <SearchIcon />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                onKeyPress={(e) => {
                  if (e.key === 'Enter') {
                    e.preventDefault();
                    searchParts();
                  }
                }}
              />
            </Grid>
            
            {searchResults.length > 0 && (
              <Grid item xs={12}>
                <Paper variant="outlined" sx={{ mt: 1, maxHeight: 200, overflow: 'auto' }}>
                  {searchResults.map((part) => (
                    <Box 
                      key={part.id} 
                      sx={{ 
                        p: 1, 
                        '&:hover': { backgroundColor: 'action.hover' },
                        cursor: 'pointer',
                        borderBottom: '1px solid',
                        borderColor: 'divider'
                      }}
                      onClick={() => handleSelectPart(part)}
                    >
                      <Typography variant="subtitle2">{part.partNumber}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        {part.description}
                      </Typography>
                    </Box>
                  ))}
                </Paper>
              </Grid>
            )}
            
            {selectedPart && (
              <Grid item xs={12}>
                <Paper variant="outlined" sx={{ p: 2, mt: 1, backgroundColor: 'background.default' }}>
                  <Typography variant="subtitle1">Selected Part</Typography>
                  <Typography variant="body1">{selectedPart.partNumber} - {selectedPart.description}</Typography>
                  {selectedPart.manufacturer && (
                    <Typography variant="body2" color="text.secondary">
                      Manufacturer: {selectedPart.manufacturer}
                    </Typography>
                  )}
                </Paper>
              </Grid>
            )}
          </Grid>
        </Paper>
        
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Customer Information
          </Typography>
          
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Search for Customer"
                value={customerSearchTerm}
                onChange={(e) => setCustomerSearchTerm(e.target.value)}
                error={!!formValidationErrors.customer}
                helperText={formValidationErrors.customer}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton 
                        onClick={searchCustomers}
                        edge="end"
                        disabled={searchingCustomers || !customerSearchTerm}
                      >
                        {searchingCustomers ? <CircularProgress size={20} /> : <SearchIcon />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                onKeyPress={(e) => {
                  if (e.key === 'Enter') {
                    e.preventDefault();
                    searchCustomers();
                  }
                }}
              />
            </Grid>
            
            {customerResults.length > 0 && (
              <Grid item xs={12}>
                <Paper variant="outlined" sx={{ mt: 1, maxHeight: 200, overflow: 'auto' }}>
                  {customerResults.map((customer) => (
                    <Box 
                      key={customer.id} 
                      sx={{ 
                        p: 1, 
                        '&:hover': { backgroundColor: 'action.hover' },
                        cursor: 'pointer',
                        borderBottom: '1px solid',
                        borderColor: 'divider'
                      }}
                      onClick={() => handleSelectCustomer(customer)}
                    >
                      <Typography variant="subtitle2">{customer.name}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        {customer.phone || customer.email}
                      </Typography>
                    </Box>
                  ))}
                </Paper>
              </Grid>
            )}
            
            <Grid item xs={12}>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                If customer is not in system, enter name manually:
              </Typography>
              <TextField
                fullWidth
                label="Customer Name"
                value={customerName}
                onChange={(e) => setCustomerName(e.target.value)}
                error={!!formValidationErrors.customer}
              />
            </Grid>
          </Grid>
        </Paper>
        
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Core Details
          </Typography>
          
          <Grid container spacing={3}>
            <Grid item xs={12} sm={6} md={4}>
              <LocalizationProvider dateAdapter={AdapterDateFns}>
                <DatePicker
                  label="Issue Date"
                  value={issueDate}
                  onChange={(newValue) => setIssueDate(newValue)}
                  renderInput={(params) => (
                    <TextField 
                      {...params} 
                      fullWidth 
                      error={!!formValidationErrors.issueDate}
                      helperText={formValidationErrors.issueDate}
                    />
                  )}
                />
              </LocalizationProvider>
            </Grid>
            
            <Grid item xs={12} sm={6} md={4}>
              <LocalizationProvider dateAdapter={AdapterDateFns}>
                <DatePicker
                  label="Due Date"
                  value={dueDate}
                  onChange={(newValue) => setDueDate(newValue)}
                  renderInput={(params) => (
                    <TextField 
                      {...params} 
                      fullWidth 
                      error={!!formValidationErrors.dueDate}
                      helperText={formValidationErrors.dueDate}
                    />
                  )}
                />
              </LocalizationProvider>
            </Grid>
            
            <Grid item xs={12} sm={6} md={4}>
              <TextField
                fullWidth
                label="Core Value"
                value={coreValue}
                onChange={(e) => setCoreValue(e.target.value)}
                type="number"
                InputProps={{
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                }}
                error={!!formValidationErrors.coreValue}
                helperText={formValidationErrors.coreValue}
              />
            </Grid>
            
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Notes"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                multiline
                rows={3}
              />
            </Grid>
          </Grid>
        </Paper>
        
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 2 }}>
          <Button
            variant="outlined"
            component={Link}
            to="/core-tracking"
            sx={{ mr: 2 }}
            disabled={submitting}
          >
            Cancel
          </Button>
          <Button
            type="submit"
            variant="contained"
            startIcon={submitting ? <CircularProgress size={20} /> : <SaveIcon />}
            disabled={submitting}
          >
            {submitting ? 'Saving...' : 'Save Core Record'}
          </Button>
        </Box>
      </form>
    </Box>
  );
};

export default TrackNewCore;
