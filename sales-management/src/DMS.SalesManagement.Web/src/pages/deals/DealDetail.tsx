// @ts-nocheck
import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Button,
  Tabs,
  Tab,
  Chip,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Edit as EditIcon,
  Save as SaveIcon,
  Cancel as CancelIcon,
  Print as PrintIcon,
  Assignment as AssignmentIcon,
  Directions as DirectionsIcon,
  Person as PersonIcon,
  Receipt as ReceiptIcon,
  AttachMoney as MoneyIcon,
  Add as AddIcon,
  DeleteOutline as DeleteOutlineIcon,
} from '@mui/icons-material';
import { fetchDeal, updateDeal, addDealAddOns } from '../../services/dealService';
import { DealStatus, Deal, UpdateDealRequest } from '../../types/deal';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`deal-tabpanel-${index}`}
      aria-labelledby={`deal-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const getStatusColor = (status: DealStatus) => {
  switch (status) {
    case DealStatus.Pending:
      return 'warning';
    case DealStatus.Approved:
    case DealStatus.FinancingApproved:
    case DealStatus.Completed:
      return 'success';
    case DealStatus.FinancingRequired:
    case DealStatus.DepositPaid:
      return 'info';
    case DealStatus.FinancingRejected:
    case DealStatus.Cancelled:
      return 'error';
    default:
      return 'default';
  }
};

const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
};

const DealDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [tabValue, setTabValue] = useState(0);
  const [editMode, setEditMode] = useState(false);
  const [addOnDialogOpen, setAddOnDialogOpen] = useState(false);
  const queryClient = useQueryClient();
  
  // Form state
  const [editForm, setEditForm] = useState<UpdateDealRequest>({});
  
  // Mock data for available add-ons
  const availableAddOns = [
    { id: '1', name: 'Extended Warranty', type: 'Warranty', price: 1200, description: '3-year extended warranty' },
    { id: '2', name: 'Paint Protection', type: 'Protection', price: 800, description: 'Premium paint protection' },
    { id: '3', name: 'Rust Proofing', type: 'Protection', price: 750, description: 'Complete rust proofing' },
    { id: '4', name: 'Tinting', type: 'Accessory', price: 450, description: 'Window tinting' },
    { id: '5', name: 'All-Weather Floor Mats', type: 'Accessory', price: 250, description: 'Premium floor mats' }
  ];
  
  // Fetch deal details
  const { data: deal, isLoading } = useQuery(
    ['deal', id],
    () => fetchDeal(id as string),
    {
      enabled: !!id,
      onSuccess: (data) => {
        setEditForm({
          status: data.status,
          notes: data.notes,
        });
      }
    }
  );
  
  // Update deal mutation
  const updateDealMutation = useMutation(
    (data: { id: string, deal: UpdateDealRequest }) => updateDeal(data.id, data.deal),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(['deal', id]);
        setEditMode(false);
      }
    }
  );
  
  // Add add-ons mutation
  const addAddOnsMutation = useMutation(
    (data: { id: string, addOnIds: string[] }) => addDealAddOns(data.id, data.addOnIds),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(['deal', id]);
        setAddOnDialogOpen(false);
      }
    }
  );

  // Tab change handler
  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  // Edit mode handlers
  const handleEditClick = () => {
    setEditMode(true);
  };

  const handleCancelEdit = () => {
    setEditMode(false);
    if (deal) {
      setEditForm({
        status: deal.status,
        notes: deal.notes,
      });
    }
  };

  const handleSaveEdit = () => {
    if (id) {
      updateDealMutation.mutate({
        id,
        deal: editForm
      });
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | { name?: string; value: unknown }>) => {
    const { name, value } = e.target;
    if (name) {
      setEditForm(prev => ({
        ...prev,
        [name]: value
      }));
    }
  };

  // Add-on dialog handlers
  const handleOpenAddOnDialog = () => {
    setAddOnDialogOpen(true);
  };

  const handleCloseAddOnDialog = () => {
    setAddOnDialogOpen(false);
  };
  
  const handleAddAddOns = (addOnIds: string[]) => {
    if (id) {
      addAddOnsMutation.mutate({
        id,
        addOnIds
      });
    }
  };

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" height="100%">
        <CircularProgress />
      </Box>
    );
  }

  if (!deal) {
    return (
      <Box p={3}>
        <Typography color="error">Error loading deal details</Typography>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/deals')}
          sx={{ mt: 2 }}
        >
          Back to Deals
        </Button>
      </Box>
    );
  }

  return (
    <Box p={3}>
      {/* Header */}
      <Box display="flex" alignItems="center" mb={3}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/deals')}
          sx={{ mr: 2 }}
        >
          Back
        </Button>
        <Typography variant="h4" component="h1" sx={{ flexGrow: 1 }}>
          Deal #{deal.dealNumber}
        </Typography>
        <Box>
          {!editMode ? (
            <>
              <Button
                variant="outlined"
                startIcon={<PrintIcon />}
                sx={{ mr: 1 }}
              >
                Print
              </Button>
              <Button
                variant="outlined"
                startIcon={<EditIcon />}
                onClick={handleEditClick}
              >
                Edit
              </Button>
            </>
          ) : (
            <>
              <Button
                variant="outlined"
                startIcon={<CancelIcon />}
                onClick={handleCancelEdit}
                sx={{ mr: 1 }}
              >
                Cancel
              </Button>
              <Button
                variant="contained"
                startIcon={<SaveIcon />}
                onClick={handleSaveEdit}
                disabled={updateDealMutation.isLoading}
              >
                Save
              </Button>
            </>
          )}
        </Box>
      </Box>

      {/* Deal status */}
      <Box mb={3}>
        {!editMode ? (
          <Chip
            label={deal.status}
            color={getStatusColor(deal.status)}
            sx={{ fontWeight: 'bold' }}
          />
        ) : (
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel>Status</InputLabel>
            <Select
              name="status"
              value={editForm.status}
              label="Status"
              onChange={handleInputChange}
            >
              {Object.values(DealStatus).map((status) => (
                <MenuItem key={status} value={status}>
                  {status}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        )}
        <Typography
          variant="caption"
          sx={{ ml: 2, color: 'text.secondary' }}
        >
          Created on {new Date(deal.createdAt).toLocaleDateString()}
        </Typography>
      </Box>

      {/* Tabs */}
      <Paper sx={{ width: '100%', mb: 3 }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={tabValue} onChange={handleTabChange}>
            <Tab label="Summary" id="deal-tab-0" />
            <Tab label="Financial" id="deal-tab-1" />
            <Tab label="Add-ons" id="deal-tab-2" />
            <Tab label="Documents" id="deal-tab-3" />
            <Tab label="Notes" id="deal-tab-4" />
          </Tabs>
        </Box>

        {/* Summary Tab */}
        <TabPanel value={tabValue} index={0}>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Typography variant="h6" gutterBottom>
                Customer Information
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <Box display="flex" alignItems="center">
                    <PersonIcon sx={{ mr: 1, color: 'primary.main' }} />
                    <Typography variant="subtitle1">
                      {deal.customer.firstName} {deal.customer.lastName}
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="subtitle2">Email</Typography>
                  <Typography>{deal.customer.email}</Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="subtitle2">Phone</Typography>
                  <Typography>{deal.customer.phone}</Typography>
                </Grid>
              </Grid>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="h6" gutterBottom>
                Vehicle Information
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <Box display="flex" alignItems="center">
                    <DirectionsIcon sx={{ mr: 1, color: 'primary.main' }} />
                    <Typography variant="subtitle1">
                      {deal.vehicle.year} {deal.vehicle.make} {deal.vehicle.model}
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="subtitle2">VIN</Typography>
                  <Typography>{deal.vehicle.vin}</Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="subtitle2">Stock #</Typography>
                  <Typography>{deal.vehicle.stockNumber}</Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="subtitle2">List Price</Typography>
                  <Typography>{formatCurrency(deal.vehicle.listPrice)}</Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="subtitle2">Type</Typography>
                  <Typography>{deal.type}</Typography>
                </Grid>
              </Grid>
            </Grid>
          </Grid>
          
          <Box mt={4}>
            <Typography variant="h6" gutterBottom>
              Deal Summary
            </Typography>
            <Divider sx={{ mb: 2 }} />
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Card>
                  <CardContent>
                    <Grid container spacing={2}>
                      <Grid item xs={6} sm={3}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Subtotal
                        </Typography>
                        <Typography variant="body1">
                          {formatCurrency(deal.subtotal)}
                        </Typography>
                      </Grid>
                      <Grid item xs={6} sm={3}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Taxes
                        </Typography>
                        <Typography variant="body1">
                          {formatCurrency(deal.taxes)}
                        </Typography>
                      </Grid>
                      <Grid item xs={6} sm={3}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Add-ons
                        </Typography>
                        <Typography variant="body1">
                          {formatCurrency(deal.addOns.reduce((sum, addon) => sum + addon.price, 0))}
                        </Typography>
                      </Grid>
                      <Grid item xs={6} sm={3}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Total
                        </Typography>
                        <Typography variant="h6" color="primary.main">
                          {formatCurrency(deal.total)}
                        </Typography>
                      </Grid>
                      
                      <Grid item xs={12}>
                        <Divider sx={{ my: 1 }} />
                      </Grid>
                      
                      <Grid item xs={6} sm={3}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Deposit Paid
                        </Typography>
                        <Typography variant="body1">
                          {formatCurrency(deal.deposit)}
                        </Typography>
                      </Grid>
                      {deal.tradeIn && (
                        <Grid item xs={6} sm={3}>
                          <Typography variant="subtitle2" color="textSecondary">
                            Trade-in Value
                          </Typography>
                          <Typography variant="body1">
                            {formatCurrency(deal.tradeIn.value)}
                          </Typography>
                        </Grid>
                      )}
                      <Grid item xs={6} sm={3}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Balance Due
                        </Typography>
                        <Typography variant="body1" color="error.main">
                          {formatCurrency(deal.balance)}
                        </Typography>
                      </Grid>
                    </Grid>
                  </CardContent>
                </Card>
              </Grid>
            </Grid>
          </Box>
        </TabPanel>

        {/* Financial Tab */}
        <TabPanel value={tabValue} index={1}>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Typography variant="h6" gutterBottom>
                Payment Information
              </Typography>
              <Divider sx={{ mb: 2 }} />
              
              <TableContainer component={Paper} variant="outlined">
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Type</TableCell>
                      <TableCell>Date</TableCell>
                      <TableCell>Amount</TableCell>
                      <TableCell>Reference</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {deal.payments && deal.payments.length > 0 ? (
                      deal.payments.map((payment) => (
                        <TableRow key={payment.id}>
                          <TableCell>{payment.type}</TableCell>
                          <TableCell>{new Date(payment.date).toLocaleDateString()}</TableCell>
                          <TableCell>{formatCurrency(payment.amount)}</TableCell>
                          <TableCell>{payment.reference || '-'}</TableCell>
                        </TableRow>
                      ))
                    ) : (
                      <TableRow>
                        <TableCell colSpan={4} align="center">No payments recorded</TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
              
              <Button 
                variant="outlined" 
                startIcon={<AddIcon />}
                sx={{ mt: 2 }}
              >
                Add Payment
              </Button>
            </Grid>
            
            <Grid item xs={12} md={6}>
              <Typography variant="h6" gutterBottom>
                Financing Details
              </Typography>
              <Divider sx={{ mb: 2 }} />
              
              {deal.financing ? (
                <Card variant="outlined">
                  <CardContent>
                    <Grid container spacing={2}>
                      <Grid item xs={6}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Lender
                        </Typography>
                        <Typography>{deal.financing.lender || '-'}</Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Status
                        </Typography>
                        <Typography>
                          {deal.financing.approved ? 
                            <Chip size="small" label="Approved" color="success" /> : 
                            <Chip size="small" label="Pending" color="warning" />
                          }
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Amount
                        </Typography>
                        <Typography>{deal.financing.amount ? formatCurrency(deal.financing.amount) : '-'}</Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Term
                        </Typography>
                        <Typography>{deal.financing.term ? `${deal.financing.term} months` : '-'}</Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Interest Rate
                        </Typography>
                        <Typography>{deal.financing.rate ? `${deal.financing.rate}%` : '-'}</Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Monthly Payment
                        </Typography>
                        <Typography>
                          {deal.financing.monthlyPayment ? formatCurrency(deal.financing.monthlyPayment) : '-'}
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Application Date
                        </Typography>
                        <Typography>
                          {deal.financing.applicationDate ? 
                            new Date(deal.financing.applicationDate).toLocaleDateString() : '-'}
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Approval Date
                        </Typography>
                        <Typography>
                          {deal.financing.approvalDate ? 
                            new Date(deal.financing.approvalDate).toLocaleDateString() : '-'}
                        </Typography>
                      </Grid>
                    </Grid>
                  </CardContent>
                </Card>
              ) : (
                <Box display="flex" flexDirection="column" alignItems="center" p={3}>
                  <Typography color="textSecondary" mb={2}>No financing information available</Typography>
                  <Button variant="contained" startIcon={<MoneyIcon />}>
                    Apply for Financing
                  </Button>
                </Box>
              )}
            </Grid>
          </Grid>
        </TabPanel>

        {/* Add-ons Tab */}
        <TabPanel value={tabValue} index={2}>
          <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
            <Typography variant="h6">
              Add-ons and Accessories
            </Typography>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleOpenAddOnDialog}
            >
              Add Items
            </Button>
          </Box>
          
          <TableContainer component={Paper} variant="outlined">
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell align="right">Price</TableCell>
                  <TableCell align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {deal.addOns && deal.addOns.length > 0 ? (
                  deal.addOns.map((addon) => (
                    <TableRow key={addon.id}>
                      <TableCell>{addon.name}</TableCell>
                      <TableCell>{addon.type}</TableCell>
                      <TableCell>{addon.description || '-'}</TableCell>
                      <TableCell align="right">{formatCurrency(addon.price)}</TableCell>
                      <TableCell align="center">
                        <Tooltip title="Remove">
                          <IconButton size="small" color="error">
                            <DeleteOutlineIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={5} align="center">No add-ons or accessories</TableCell>
                  </TableRow>
                )}
                {deal.addOns && deal.addOns.length > 0 && (
                  <TableRow>
                    <TableCell colSpan={3} align="right">
                      <Typography variant="subtitle2">Total</Typography>
                    </TableCell>
                    <TableCell align="right">
                      <Typography variant="subtitle1">
                        {formatCurrency(deal.addOns.reduce((sum, addon) => sum + addon.price, 0))}
                      </Typography>
                    </TableCell>
                    <TableCell />
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </TabPanel>

        {/* Documents Tab */}
        <TabPanel value={tabValue} index={3}>
          <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
            <Typography variant="h6">
              Deal Documents
            </Typography>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
            >
              Upload Document
            </Button>
          </Box>
          
          <List>
            <ListItem>
              <ListItemIcon>
                <AssignmentIcon />
              </ListItemIcon>
              <ListItemText
                primary="Purchase Agreement"
                secondary="Signed on June 15, 2025"
              />
              <Button size="small">View</Button>
            </ListItem>
            <ListItem>
              <ListItemIcon>
                <ReceiptIcon />
              </ListItemIcon>
              <ListItemText
                primary="Deposit Receipt"
                secondary="Uploaded on June 16, 2025"
              />
              <Button size="small">View</Button>
            </ListItem>
            <ListItem>
              <ListItemIcon>
                <AssignmentIcon />
              </ListItemIcon>
              <ListItemText
                primary="Vehicle Inspection Report"
                secondary="Uploaded on June 14, 2025"
              />
              <Button size="small">View</Button>
            </ListItem>
          </List>
        </TabPanel>

        {/* Notes Tab */}
        <TabPanel value={tabValue} index={4}>
          <Typography variant="h6" gutterBottom>
            Notes
          </Typography>
          
          {!editMode ? (
            <Paper variant="outlined" sx={{ p: 2, minHeight: '150px' }}>
              {deal.notes ? (
                <Typography>{deal.notes}</Typography>
              ) : (
                <Typography color="text.secondary" align="center">
                  No notes available.
                </Typography>
              )}
            </Paper>
          ) : (
            <TextField
              fullWidth
              name="notes"
              label="Notes"
              multiline
              rows={6}
              value={editForm.notes || ''}
              onChange={handleInputChange}
            />
          )}
        </TabPanel>
      </Paper>
      
      {/* Add-on Dialog */}
      <Dialog
        open={addOnDialogOpen}
        onClose={handleCloseAddOnDialog}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Add Items to Deal</DialogTitle>
        <DialogContent>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell padding="checkbox">Select</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell align="right">Price</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {availableAddOns.map((addon) => (
                  <TableRow key={addon.id}>
                    <TableCell padding="checkbox">
                      <input 
                        type="checkbox" 
                        disabled={deal.addOns.some(a => a.id === addon.id)}
                      />
                    </TableCell>
                    <TableCell>{addon.name}</TableCell>
                    <TableCell>{addon.type}</TableCell>
                    <TableCell>{addon.description}</TableCell>
                    <TableCell align="right">{formatCurrency(addon.price)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseAddOnDialog}>Cancel</Button>
          <Button 
            variant="contained" 
            onClick={() => handleAddAddOns(['1', '5'])}
            disabled={addAddOnsMutation.isLoading}
          >
            Add Selected Items
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default DealDetail;
