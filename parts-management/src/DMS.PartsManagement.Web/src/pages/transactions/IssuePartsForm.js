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
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Divider,
  CircularProgress,
  Autocomplete,
  FormControl,
  InputLabel,
  Select,
  FormHelperText,
  Tooltip
} from '@mui/material';
import {
  ArrowBack,
  Search as SearchIcon,
  Save as SaveIcon,
  Add as AddIcon,
  Delete as DeleteIcon,
  ReceiptLong as ReceiptIcon
} from '@mui/icons-material';

import { TransactionsService, PartsService, InventoryService } from '../../services/api';

const IssuePartsForm = () => {
  const navigate = useNavigate();
  
  // Form state
  const [referenceType, setReferenceType] = useState('MANUAL');
  const [referenceId, setReferenceId] = useState('');
  const [locationId, setLocationId] = useState('');
  const [notes, setNotes] = useState('');
  const [locations, setLocations] = useState([]);
  
  // Item state
  const [items, setItems] = useState([]);
  const [partSearchTerm, setPartSearchTerm] = useState('');
  const [searchResults, setSearchResults] = useState([]);
  const [searching, setSearching] = useState(false);
  
  // Current item being added
  const [currentPart, setCurrentPart] = useState(null);
  const [quantity, setQuantity] = useState('1');
  const [unitPrice, setUnitPrice] = useState('');
  
  // Form submission state
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [formValidationErrors, setFormValidationErrors] = useState({});
  
  // Reference type options
  const referenceTypes = [
    { value: 'MANUAL', label: 'Manual Transaction' },
    { value: 'SERVICE_ORDER', label: 'Service Order' },
    { value: 'SALES_ORDER', label: 'Sales Order' }
  ];

  // Load locations on component mount
  useEffect(() => {
    const fetchLocations = async () => {
      try {
        // This should be adjusted to use your actual API for locations
        const locationsData = await fetch('/api/locations').then(res => res.json());
        setLocations(locationsData || []);
        
        // Set default location if available
        if (locationsData && locationsData.length > 0) {
          setLocationId(locationsData[0].id);
        }
      } catch (err) {
        console.error('Error fetching locations:', err);
        // Set dummy locations for demo
        const dummyLocations = [
          { id: 'main', name: 'Main Warehouse' },
          { id: 'service', name: 'Service Department' },
          { id: 'showroom', name: 'Showroom' }
        ];
        setLocations(dummyLocations);
        setLocationId('main');
      }
    };
    
    fetchLocations();
  }, []);
  
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
  const handleSelectPart = async (part) => {
    setCurrentPart(part);
    setPartSearchTerm(part.partNumber);
    setSearchResults([]);
    
    // Get inventory info to ensure we have stock
    try {
      const inventoryData = await InventoryService.getInventoryByPart(part.id);
      
      if (inventoryData && inventoryData.locations) {
        const locationStock = inventoryData.locations.find(loc => loc.locationId === locationId);
        if (!locationStock || locationStock.quantity <= 0) {
          alert(`Warning: No stock available for ${part.partNumber} at selected location.`);
        } else {
          // Set max quantity equal to stock on hand
          setQuantity('1'); // Reset to 1 for new part
        }
      }
      
      // Set pricing if available
      if (part.retailPrice) {
        setUnitPrice(part.retailPrice.toString());
      } else {
        setUnitPrice('');
      }
    } catch (err) {
      console.error('Error fetching inventory data:', err);
    }
  };
  
  // Add current part to items list
  const addPartToList = () => {
    if (!currentPart) return;
    
    // Validate quantity
    const qty = parseInt(quantity);
    if (isNaN(qty) || qty <= 0) {
      alert('Please enter a valid quantity');
      return;
    }
    
    // Check if part already in list
    const existingIndex = items.findIndex(item => item.partId === currentPart.id);
    if (existingIndex >= 0) {
      // Update quantity if part already in list
      const updatedItems = [...items];
      updatedItems[existingIndex].quantity += qty;
      setItems(updatedItems);
    } else {
      // Add new item
      setItems([...items, {
        partId: currentPart.id,
        partNumber: currentPart.partNumber,
        description: currentPart.description,
        quantity: qty,
        unitPrice: parseFloat(unitPrice) || 0,
        unitCost: currentPart.cost || 0,
        extendedAmount: (parseFloat(unitPrice) || 0) * qty
      }]);
    }
    
    // Reset form for next item
    setCurrentPart(null);
    setPartSearchTerm('');
    setQuantity('1');
    setUnitPrice('');
  };
  
  // Remove item from list
  const removeItem = (index) => {
    const updatedItems = [...items];
    updatedItems.splice(index, 1);
    setItems(updatedItems);
  };
  
  // Calculate total amount
  const calculateTotal = () => {
    return items.reduce((sum, item) => sum + item.extendedAmount, 0);
  };
  
  // Validate form
  const validateForm = () => {
    const errors = {};
    
    if (!locationId) {
      errors.locationId = 'Please select a location';
    }
    
    if (referenceType !== 'MANUAL' && !referenceId) {
      errors.referenceId = 'Reference ID is required for selected reference type';
    }
    
    if (items.length === 0) {
      errors.items = 'Please add at least one part to the transaction';
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
      const transactionData = {
        type: 'ISSUE',
        locationId,
        referenceType,
        referenceId: referenceId || null,
        notes,
        items: items.map(item => ({
          ...item,
          // Issue transactions decrease inventory, so quantity should be negative
          // But UI shows positive numbers for user-friendliness
          quantity: -Math.abs(item.quantity)
        })),
        totalAmount: calculateTotal()
      };
      
      const result = await TransactionsService.issueParts(transactionData);
      
      // Navigate to the created transaction's detail page
      navigate(`/transactions/${result.id}`);
    } catch (err) {
      console.error('Error creating transaction:', err);
      setError(err.message || 'Failed to issue parts. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };
  
  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ mb: 3, display: 'flex', alignItems: 'center' }}>
        <Button
          variant="outlined"
          startIcon={<ArrowBack />}
          component={Link}
          to="/transactions"
          sx={{ mr: 2 }}
        >
          Back
        </Button>
        <Typography variant="h4" component="h1" sx={{ display: 'flex', alignItems: 'center' }}>
          <ReceiptIcon sx={{ mr: 1 }} />
          Issue Parts
        </Typography>
      </Box>
      
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}
      
      <form onSubmit={handleSubmit}>
        {/* Transaction details */}
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Transaction Details
          </Typography>
          
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={4}>
              <FormControl fullWidth error={!!formValidationErrors.locationId}>
                <InputLabel>Location</InputLabel>
                <Select
                  value={locationId}
                  onChange={(e) => setLocationId(e.target.value)}
                  label="Location"
                >
                  {locations.map((location) => (
                    <MenuItem key={location.id} value={location.id}>
                      {location.name}
                    </MenuItem>
                  ))}
                </Select>
                {formValidationErrors.locationId && (
                  <FormHelperText>{formValidationErrors.locationId}</FormHelperText>
                )}
              </FormControl>
            </Grid>
            
            <Grid item xs={12} sm={6} md={4}>
              <FormControl fullWidth>
                <InputLabel>Reference Type</InputLabel>
                <Select
                  value={referenceType}
                  onChange={(e) => setReferenceType(e.target.value)}
                  label="Reference Type"
                >
                  {referenceTypes.map((type) => (
                    <MenuItem key={type.value} value={type.value}>
                      {type.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            
            <Grid item xs={12} sm={6} md={4}>
              <TextField
                fullWidth
                label="Reference ID"
                value={referenceId}
                onChange={(e) => setReferenceId(e.target.value)}
                disabled={referenceType === 'MANUAL'}
                error={!!formValidationErrors.referenceId}
                helperText={formValidationErrors.referenceId}
              />
            </Grid>
            
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Notes"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                multiline
                rows={2}
              />
            </Grid>
          </Grid>
        </Paper>
        
        {/* Add parts */}
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Add Parts
          </Typography>
          
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Search for Part by Number or Description"
                value={partSearchTerm}
                onChange={(e) => setPartSearchTerm(e.target.value)}
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
            
            <Grid item xs={12} sm={2}>
              <TextField
                fullWidth
                label="Quantity"
                type="number"
                value={quantity}
                onChange={(e) => setQuantity(e.target.value)}
                InputProps={{ inputProps: { min: 1 } }}
                disabled={!currentPart}
              />
            </Grid>
            
            <Grid item xs={12} sm={2}>
              <TextField
                fullWidth
                label="Unit Price"
                type="number"
                value={unitPrice}
                onChange={(e) => setUnitPrice(e.target.value)}
                InputProps={{
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                  inputProps: { step: "0.01", min: 0 }
                }}
                disabled={!currentPart}
              />
            </Grid>
            
            <Grid item xs={12} sm={2}>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={addPartToList}
                disabled={!currentPart}
                fullWidth
                sx={{ height: '56px' }} // Match height of text fields
              >
                Add Part
              </Button>
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
          </Grid>
          
          {formValidationErrors.items && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {formValidationErrors.items}
            </Alert>
          )}
          
          {currentPart && (
            <Box sx={{ mt: 2, p: 2, bgcolor: 'background.default', borderRadius: 1 }}>
              <Typography variant="subtitle2">Selected Part:</Typography>
              <Typography variant="body2">
                {currentPart.partNumber} - {currentPart.description}
              </Typography>
            </Box>
          )}
        </Paper>
        
        {/* Items table */}
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Parts to Issue
          </Typography>
          
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Part Number</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell align="right">Quantity</TableCell>
                  <TableCell align="right">Unit Price</TableCell>
                  <TableCell align="right">Extended</TableCell>
                  <TableCell align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {items.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      <Typography variant="body2" sx={{ py: 2 }}>
                        No parts added. Use the form above to add parts to the issue transaction.
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  items.map((item, index) => (
                    <TableRow key={index}>
                      <TableCell>{item.partNumber}</TableCell>
                      <TableCell>{item.description}</TableCell>
                      <TableCell align="right">{item.quantity}</TableCell>
                      <TableCell align="right">${item.unitPrice.toFixed(2)}</TableCell>
                      <TableCell align="right">${item.extendedAmount.toFixed(2)}</TableCell>
                      <TableCell align="center">
                        <Tooltip title="Remove Item">
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => removeItem(index)}
                          >
                            <DeleteIcon />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))
                )}
                
                {items.length > 0 && (
                  <TableRow>
                    <TableCell colSpan={3} />
                    <TableCell align="right" sx={{ fontWeight: 'bold' }}>
                      Total:
                    </TableCell>
                    <TableCell align="right" sx={{ fontWeight: 'bold' }}>
                      ${calculateTotal().toFixed(2)}
                    </TableCell>
                    <TableCell />
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
        
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 2 }}>
          <Button
            variant="outlined"
            component={Link}
            to="/transactions"
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
            {submitting ? 'Processing...' : 'Complete Transaction'}
          </Button>
        </Box>
      </form>
    </Box>
  );
};

export default IssuePartsForm;
