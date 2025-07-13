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
  CircularProgress,
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
  CompareArrows as TransferIcon
} from '@mui/icons-material';

import { TransactionsService, PartsService, InventoryService } from '../../services/api';

const TransferPartsForm = () => {
  const navigate = useNavigate();
  
  // Form state
  const [fromLocationId, setFromLocationId] = useState('');
  const [toLocationId, setToLocationId] = useState('');
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
  const [availableQuantity, setAvailableQuantity] = useState(0);
  
  // Form submission state
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [formValidationErrors, setFormValidationErrors] = useState({});
  
  // Load locations on component mount
  useEffect(() => {
    const fetchLocations = async () => {
      try {
        // This should be adjusted to use your actual API for locations
        const locationsData = await fetch('/api/locations').then(res => res.json());
        setLocations(locationsData || []);
        
        // Set default locations if available
        if (locationsData && locationsData.length > 1) {
          setFromLocationId(locationsData[0].id);
          setToLocationId(locationsData[1].id);
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
        setFromLocationId('main');
        setToLocationId('service');
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
    
    // Get inventory info to ensure we have stock at the source location
    try {
      const inventoryData = await InventoryService.getInventoryByPart(part.id);
      
      if (inventoryData && inventoryData.locations) {
        const locationStock = inventoryData.locations.find(loc => loc.locationId === fromLocationId);
        if (!locationStock || locationStock.quantity <= 0) {
          alert(`Warning: No stock available for ${part.partNumber} at source location.`);
          setAvailableQuantity(0);
        } else {
          setAvailableQuantity(locationStock.quantity);
          setQuantity('1'); // Reset to 1 for new part
        }
      } else {
        setAvailableQuantity(0);
      }
    } catch (err) {
      console.error('Error fetching inventory data:', err);
      setAvailableQuantity(0);
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
    
    if (qty > availableQuantity) {
      alert(`Cannot transfer more than available quantity (${availableQuantity})`);
      return;
    }
    
    // Check if part already in list
    const existingIndex = items.findIndex(item => item.partId === currentPart.id);
    if (existingIndex >= 0) {
      // Update quantity if part already in list
      const updatedItems = [...items];
      updatedItems[existingIndex].quantity += qty;
      
      // Check that total doesn't exceed available
      if (updatedItems[existingIndex].quantity > availableQuantity) {
        alert(`Total transfer quantity (${updatedItems[existingIndex].quantity}) exceeds available quantity (${availableQuantity})`);
        return;
      }
      
      setItems(updatedItems);
    } else {
      // Add new item
      setItems([...items, {
        partId: currentPart.id,
        partNumber: currentPart.partNumber,
        description: currentPart.description,
        quantity: qty,
        fromLocation: locations.find(loc => loc.id === fromLocationId)?.name || fromLocationId,
        toLocation: locations.find(loc => loc.id === toLocationId)?.name || toLocationId
      }]);
    }
    
    // Reset form for next item
    setCurrentPart(null);
    setPartSearchTerm('');
    setQuantity('1');
    setAvailableQuantity(0);
  };
  
  // Remove item from list
  const removeItem = (index) => {
    const updatedItems = [...items];
    updatedItems.splice(index, 1);
    setItems(updatedItems);
  };
  
  // Validate form
  const validateForm = () => {
    const errors = {};
    
    if (!fromLocationId) {
      errors.fromLocationId = 'Please select a source location';
    }
    
    if (!toLocationId) {
      errors.toLocationId = 'Please select a destination location';
    }
    
    if (fromLocationId === toLocationId) {
      errors.toLocationId = 'Source and destination locations cannot be the same';
    }
    
    if (items.length === 0) {
      errors.items = 'Please add at least one part to transfer';
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
      const transferData = {
        fromLocationId,
        toLocationId,
        notes,
        items: items.map(item => ({
          partId: item.partId,
          partNumber: item.partNumber,
          description: item.description,
          quantity: item.quantity
        }))
      };
      
      const result = await TransactionsService.transferParts(transferData);
      
      // Navigate to the created transaction's detail page
      navigate(`/transactions/${result.id}`);
    } catch (err) {
      console.error('Error creating transfer transaction:', err);
      setError(err.message || 'Failed to transfer parts. Please try again.');
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
          <TransferIcon sx={{ mr: 1 }} />
          Transfer Parts
        </Typography>
      </Box>
      
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}
      
      <form onSubmit={handleSubmit}>
        {/* Transfer details */}
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Transfer Details
          </Typography>
          
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth error={!!formValidationErrors.fromLocationId}>
                <InputLabel>From Location</InputLabel>
                <Select
                  value={fromLocationId}
                  onChange={(e) => {
                    setFromLocationId(e.target.value);
                    // Reset current part selection as available quantity will change
                    setCurrentPart(null);
                    setPartSearchTerm('');
                    setAvailableQuantity(0);
                  }}
                  label="From Location"
                >
                  {locations.map((location) => (
                    <MenuItem 
                      key={location.id} 
                      value={location.id}
                      disabled={location.id === toLocationId}
                    >
                      {location.name}
                    </MenuItem>
                  ))}
                </Select>
                {formValidationErrors.fromLocationId && (
                  <FormHelperText>{formValidationErrors.fromLocationId}</FormHelperText>
                )}
              </FormControl>
            </Grid>
            
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth error={!!formValidationErrors.toLocationId}>
                <InputLabel>To Location</InputLabel>
                <Select
                  value={toLocationId}
                  onChange={(e) => setToLocationId(e.target.value)}
                  label="To Location"
                >
                  {locations.map((location) => (
                    <MenuItem 
                      key={location.id} 
                      value={location.id}
                      disabled={location.id === fromLocationId}
                    >
                      {location.name}
                    </MenuItem>
                  ))}
                </Select>
                {formValidationErrors.toLocationId && (
                  <FormHelperText>{formValidationErrors.toLocationId}</FormHelperText>
                )}
              </FormControl>
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
            Add Parts to Transfer
          </Typography>
          
          {!fromLocationId && (
            <Alert severity="info" sx={{ mb: 2 }}>
              Select a source location first to search for parts to transfer.
            </Alert>
          )}
          
          <Grid container spacing={2}>
            <Grid item xs={12} sm={8}>
              <TextField
                fullWidth
                label="Search for Part by Number or Description"
                value={partSearchTerm}
                onChange={(e) => setPartSearchTerm(e.target.value)}
                disabled={!fromLocationId}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton 
                        onClick={searchParts}
                        edge="end"
                        disabled={searching || !partSearchTerm || !fromLocationId}
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
                InputProps={{ 
                  inputProps: { min: 1, max: availableQuantity }
                }}
                disabled={!currentPart}
                helperText={availableQuantity > 0 ? `Available: ${availableQuantity}` : ''}
              />
            </Grid>
            
            <Grid item xs={12} sm={2}>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={addPartToList}
                disabled={!currentPart || availableQuantity === 0}
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
              {availableQuantity === 0 && (
                <Typography variant="body2" color="error" sx={{ mt: 1 }}>
                  No stock available at source location
                </Typography>
              )}
            </Box>
          )}
        </Paper>
        
        {/* Items table */}
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Parts to Transfer
          </Typography>
          
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Part Number</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell align="right">Quantity</TableCell>
                  <TableCell>From Location</TableCell>
                  <TableCell>To Location</TableCell>
                  <TableCell align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {items.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      <Typography variant="body2" sx={{ py: 2 }}>
                        No parts added. Use the form above to add parts to transfer.
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  items.map((item, index) => (
                    <TableRow key={index}>
                      <TableCell>{item.partNumber}</TableCell>
                      <TableCell>{item.description}</TableCell>
                      <TableCell align="right">{item.quantity}</TableCell>
                      <TableCell>{item.fromLocation}</TableCell>
                      <TableCell>{item.toLocation}</TableCell>
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
                    <TableCell colSpan={2} />
                    <TableCell align="right" sx={{ fontWeight: 'bold' }}>
                      Total Items: {items.reduce((sum, item) => sum + item.quantity, 0)}
                    </TableCell>
                    <TableCell colSpan={3} />
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
            {submitting ? 'Processing...' : 'Complete Transfer'}
          </Button>
        </Box>
      </form>
    </Box>
  );
};

export default TransferPartsForm;
