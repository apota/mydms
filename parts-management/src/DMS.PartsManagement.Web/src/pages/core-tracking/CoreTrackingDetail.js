import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Chip,
  Button,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
  Alert,
  Card,
  CardContent,
  Timeline,
  TimelineItem,
  TimelineSeparator,
  TimelineConnector,
  TimelineContent,
  TimelineDot,
  TimelineOppositeContent,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  TextField,
  MenuItem
} from '@mui/material';
import {
  ArrowBack,
  CheckCircle as CheckIcon,
  MoneyOff as RejectedIcon,
  AttachMoney as MoneyIcon,
  History as HistoryIcon,
  WarningAmber as WarningIcon,
  AccessTime as ClockIcon
} from '@mui/icons-material';
import { AlertTitle } from '@mui/material';
import { format, differenceInDays, isPast } from 'date-fns';

import { CoreTrackingService } from '../../services/api';

// Get color for core status
const getStatusColor = (status) => {
  switch (status) {
    case 'PENDING_RETURN': return 'warning';
    case 'RETURNED': return 'info';
    case 'CREDITED': return 'success';
    case 'REJECTED': return 'error';
    case 'EXPIRED': return 'error';
    default: return 'default';
  }
};

// Get status timeline icon
const getStatusIcon = (status) => {
  switch (status) {
    case 'PENDING_RETURN': return <ClockIcon />;
    case 'RETURNED': return <CheckIcon />;
    case 'CREDITED': return <MoneyIcon />;
    case 'REJECTED': return <RejectedIcon />;
    case 'EXPIRED': return <WarningIcon />;
    default: return <ClockIcon />;
  }
};

const CoreTrackingDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  
  const [core, setCore] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Dialog state
  const [returnDialogOpen, setReturnDialogOpen] = useState(false);
  const [creditDialogOpen, setCreditDialogOpen] = useState(false);
  const [processingAction, setProcessingAction] = useState(false);
  const [returnCondition, setReturnCondition] = useState('GOOD');
  const [returnNotes, setReturnNotes] = useState('');
  const [creditAmount, setCreditAmount] = useState(0);
  
  // Condition options
  const conditionOptions = [
    { value: 'GOOD', label: 'Good - Full Credit' },
    { value: 'DAMAGED', label: 'Damaged - Partial Credit' },
    { value: 'NOT_USABLE', label: 'Not Usable - No Credit' }
  ];

  // Load core tracking data
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const data = await CoreTrackingService.getCoreById(id);
        setCore(data);
        if (data) {
          setCreditAmount(data.coreValue);
        }
        setError(null);
      } catch (err) {
        console.error('Failed to fetch core tracking details:', err);
        setError('Failed to load core details. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [id]);

  // Calculate days remaining or overdue
  const getDaysRemainingText = (core) => {
    if (!core.dueDate) return null;
    
    const dueDate = new Date(core.dueDate);
    const today = new Date();
    const diff = differenceInDays(dueDate, today);
    
    if (diff < 0) {
      return `${Math.abs(diff)} days overdue`;
    } else if (diff === 0) {
      return "Due today";
    } else {
      return `${diff} days remaining`;
    }
  };
  
  // Handle process return dialog
  const handleOpenReturnDialog = () => {
    setReturnDialogOpen(true);
  };
  
  const handleCloseReturnDialog = () => {
    setReturnDialogOpen(false);
    setReturnCondition('GOOD');
    setReturnNotes('');
  };
  
  const handleOpenCreditDialog = () => {
    setCreditDialogOpen(true);
  };
  
  const handleCloseCreditDialog = () => {
    setCreditDialogOpen(false);
    setCreditAmount(core?.coreValue || 0);
  };
  
  // Process core return
  const processReturn = async () => {
    setProcessingAction(true);
    try {
      const returnData = {
        condition: returnCondition,
        notes: returnNotes,
        returnDate: new Date().toISOString()
      };
      
      await CoreTrackingService.processCoreReturn(id, returnData);
      // Reload the page with updated data
      const updatedCore = await CoreTrackingService.getCoreById(id);
      setCore(updatedCore);
      
      handleCloseReturnDialog();
    } catch (err) {
      console.error('Failed to process core return:', err);
      alert('Failed to process return: ' + (err.message || 'Unknown error'));
    } finally {
      setProcessingAction(false);
    }
  };
  
  // Apply core credit
  const applyCredit = async () => {
    setProcessingAction(true);
    try {
      const creditData = {
        creditAmount: parseFloat(creditAmount),
        notes: returnNotes,
        processingDate: new Date().toISOString()
      };
      
      await CoreTrackingService.applyCoreCredit(id, creditData);
      // Reload the page with updated data
      const updatedCore = await CoreTrackingService.getCoreById(id);
      setCore(updatedCore);
      
      handleCloseCreditDialog();
    } catch (err) {
      console.error('Failed to apply core credit:', err);
      alert('Failed to apply credit: ' + (err.message || 'Unknown error'));
    } finally {
      setProcessingAction(false);
    }
  };
  
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
        <Button
          variant="contained"
          startIcon={<ArrowBack />}
          component={Link}
          to="/core-tracking"
        >
          Back to Core Tracking
        </Button>
      </Box>
    );
  }

  if (!core) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="info" sx={{ mb: 2 }}>
          Core tracking record not found
        </Alert>
        <Button
          variant="contained"
          startIcon={<ArrowBack />}
          component={Link}
          to="/core-tracking"
        >
          Back to Core Tracking
        </Button>
      </Box>
    );
  }

  // Is the core overdue
  const isOverdue = isPast(new Date(core.dueDate)) && core.status === 'PENDING_RETURN';
  const isPendingReturn = core.status === 'PENDING_RETURN';
  const isReturned = core.status === 'RETURNED';
  
  return (
    <Box sx={{ p: 3 }}>
      {/* Header with actions */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3, alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
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
            Core #{id.substring(0, 8)}
          </Typography>
          <Chip 
            label={core.status}
            color={getStatusColor(core.status)}
            sx={{ ml: 2 }}
          />
        </Box>
        
        <Box>
          {isPendingReturn && (
            <Button
              variant="contained"
              color="primary"
              onClick={handleOpenReturnDialog}
              startIcon={<CheckIcon />}
            >
              Process Return
            </Button>
          )}
          
          {isReturned && (
            <Button
              variant="contained"
              color="primary"
              onClick={handleOpenCreditDialog}
              startIcon={<MoneyIcon />}
            >
              Apply Credit
            </Button>
          )}
        </Box>
      </Box>

      {/* Warning banner for overdue cores */}
      {isOverdue && (
        <Alert severity="warning" sx={{ mb: 3 }}>
          <AlertTitle>Overdue Core Return</AlertTitle>
          This core is {getDaysRemainingText(core)} and may be subject to penalties or loss of credit value.
        </Alert>
      )}

      {/* Core details */}
      <Grid container spacing={3}>
        {/* Main core information */}
        <Grid item xs={12} md={8}>
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Core Details
            </Typography>
            
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Part Number</Typography>
                <Typography variant="body1">
                  <Link to={`/parts/${core.partId}`}>
                    {core.partNumber}
                  </Link>
                </Typography>
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Description</Typography>
                <Typography variant="body1">{core.partDescription}</Typography>
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Customer</Typography>
                <Typography variant="body1">
                  {core.customerId ? (
                    <Link to={`/customers/${core.customerId}`}>
                      {core.customerName}
                    </Link>
                  ) : (
                    core.customerName || 'N/A'
                  )}
                </Typography>
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Core Value</Typography>
                <Typography variant="body1" sx={{ fontWeight: 'bold', color: 'success.main' }}>
                  ${core.coreValue.toFixed(2)}
                </Typography>
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Issue Date</Typography>
                <Typography variant="body1">
                  {format(new Date(core.issueDate), 'MMMM d, yyyy')}
                </Typography>
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Due Date</Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <Typography variant="body1" sx={{ mr: 1 }}>
                    {format(new Date(core.dueDate), 'MMMM d, yyyy')}
                  </Typography>
                  {isPendingReturn && (
                    <Chip 
                      size="small" 
                      label={getDaysRemainingText(core)} 
                      color={isOverdue ? 'error' : 'primary'} 
                    />
                  )}
                </Box>
              </Grid>

              {core.notes && (
                <Grid item xs={12}>
                  <Typography variant="subtitle2" color="text.secondary">Notes</Typography>
                  <Paper variant="outlined" sx={{ p: 1, backgroundColor: 'background.default' }}>
                    <Typography variant="body2">{core.notes}</Typography>
                  </Paper>
                </Grid>
              )}
            </Grid>
          </Paper>

          {/* Return information (if applicable) */}
          {(core.returnDate || core.condition) && (
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" gutterBottom>
                Return Information
              </Typography>
              
              <Grid container spacing={2}>
                {core.returnDate && (
                  <Grid item xs={12} sm={6}>
                    <Typography variant="subtitle2" color="text.secondary">Return Date</Typography>
                    <Typography variant="body1">
                      {format(new Date(core.returnDate), 'MMMM d, yyyy')}
                    </Typography>
                  </Grid>
                )}
                
                {core.condition && (
                  <Grid item xs={12} sm={6}>
                    <Typography variant="subtitle2" color="text.secondary">Condition Upon Return</Typography>
                    <Chip 
                      label={core.condition} 
                      color={
                        core.condition === 'GOOD' 
                          ? 'success' 
                          : core.condition === 'DAMAGED' 
                            ? 'warning' 
                            : 'error'
                      } 
                    />
                  </Grid>
                )}
                
                {core.returnedBy && (
                  <Grid item xs={12} sm={6}>
                    <Typography variant="subtitle2" color="text.secondary">Processed By</Typography>
                    <Typography variant="body1">{core.returnedBy}</Typography>
                  </Grid>
                )}
              </Grid>
              
              {core.returnNotes && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="subtitle2" color="text.secondary">Return Notes</Typography>
                  <Paper variant="outlined" sx={{ p: 1, backgroundColor: 'background.default' }}>
                    <Typography variant="body2">{core.returnNotes}</Typography>
                  </Paper>
                </Box>
              )}
            </Paper>
          )}
          
          {/* Credit information (if applicable) */}
          {core.status === 'CREDITED' && (
            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Credit Information
              </Typography>
              
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <Typography variant="subtitle2" color="text.secondary">Credit Amount</Typography>
                  <Typography variant="body1" sx={{ fontWeight: 'bold', color: 'success.main' }}>
                    ${core.creditAmount.toFixed(2)}
                  </Typography>
                </Grid>
                
                <Grid item xs={12} sm={6}>
                  <Typography variant="subtitle2" color="text.secondary">Credit Date</Typography>
                  <Typography variant="body1">
                    {format(new Date(core.creditDate), 'MMMM d, yyyy')}
                  </Typography>
                </Grid>
                
                <Grid item xs={12} sm={6}>
                  <Typography variant="subtitle2" color="text.secondary">Processed By</Typography>
                  <Typography variant="body1">{core.creditedBy}</Typography>
                </Grid>
                
                <Grid item xs={12} sm={6}>
                  <Typography variant="subtitle2" color="text.secondary">Reference</Typography>
                  <Typography variant="body1">
                    {core.creditReference ? (
                      <Link to={`/financial/credits/${core.creditReference}`}>
                        Credit #{core.creditReference}
                      </Link>
                    ) : (
                      'N/A'
                    )}
                  </Typography>
                </Grid>
              </Grid>
              
              {core.creditNotes && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="subtitle2" color="text.secondary">Credit Notes</Typography>
                  <Paper variant="outlined" sx={{ p: 1, backgroundColor: 'background.default' }}>
                    <Typography variant="body2">{core.creditNotes}</Typography>
                  </Paper>
                </Box>
              )}
            </Paper>
          )}
        </Grid>

        {/* Side panel with status timeline and related information */}
        <Grid item xs={12} md={4}>
          {/* Status timeline */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Status Timeline
              </Typography>
              
              <Timeline position="right">
                <TimelineItem>
                  <TimelineOppositeContent color="text.secondary">
                    {format(new Date(core.issueDate), 'MMM d, yyyy')}
                  </TimelineOppositeContent>
                  <TimelineSeparator>
                    <TimelineDot color="primary" />
                    <TimelineConnector />
                  </TimelineSeparator>
                  <TimelineContent>Core Issued</TimelineContent>
                </TimelineItem>
                
                {core.status === 'PENDING_RETURN' ? (
                  <TimelineItem>
                    <TimelineOppositeContent color="text.secondary">
                      {format(new Date(core.dueDate), 'MMM d, yyyy')}
                    </TimelineOppositeContent>
                    <TimelineSeparator>
                      <TimelineDot color="warning" />
                      {false && <TimelineConnector />}
                    </TimelineSeparator>
                    <TimelineContent>Return Due Date</TimelineContent>
                  </TimelineItem>
                ) : (
                  <>
                    {core.returnDate && (
                      <TimelineItem>
                        <TimelineOppositeContent color="text.secondary">
                          {format(new Date(core.returnDate), 'MMM d, yyyy')}
                        </TimelineOppositeContent>
                        <TimelineSeparator>
                          <TimelineDot color="info" />
                          {core.status === 'CREDITED' && <TimelineConnector />}
                        </TimelineSeparator>
                        <TimelineContent>Core Returned</TimelineContent>
                      </TimelineItem>
                    )}
                    
                    {core.status === 'CREDITED' && core.creditDate && (
                      <TimelineItem>
                        <TimelineOppositeContent color="text.secondary">
                          {format(new Date(core.creditDate), 'MMM d, yyyy')}
                        </TimelineOppositeContent>
                        <TimelineSeparator>
                          <TimelineDot color="success" />
                        </TimelineSeparator>
                        <TimelineContent>Credit Applied</TimelineContent>
                      </TimelineItem>
                    )}
                    
                    {core.status === 'REJECTED' && (
                      <TimelineItem>
                        <TimelineOppositeContent color="text.secondary">
                          {core.rejectionDate ? format(new Date(core.rejectionDate), 'MMM d, yyyy') : 'N/A'}
                        </TimelineOppositeContent>
                        <TimelineSeparator>
                          <TimelineDot color="error" />
                        </TimelineSeparator>
                        <TimelineContent>Core Rejected</TimelineContent>
                      </TimelineItem>
                    )}
                    
                    {core.status === 'EXPIRED' && (
                      <TimelineItem>
                        <TimelineOppositeContent color="text.secondary">
                          {format(new Date(core.dueDate), 'MMM d, yyyy')}
                        </TimelineOppositeContent>
                        <TimelineSeparator>
                          <TimelineDot color="error" />
                        </TimelineSeparator>
                        <TimelineContent>Return Period Expired</TimelineContent>
                      </TimelineItem>
                    )}
                  </>
                )}
              </Timeline>
            </CardContent>
          </Card>

          {/* Related information */}
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Related Information
              </Typography>
              
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" color="text.secondary">Original Transaction</Typography>
                {core.transactionId ? (
                  <Button
                    variant="text"
                    component={Link}
                    to={`/transactions/${core.transactionId}`}
                    sx={{ textTransform: 'none' }}
                  >
                    View Transaction #{core.transactionId.substring(0, 8)}
                  </Button>
                ) : (
                  <Typography variant="body2">No linked transaction</Typography>
                )}
              </Box>
              
              <Divider sx={{ my: 2 }} />
              
              <Button
                variant="text"
                startIcon={<HistoryIcon />}
                component={Link}
                to={`/parts/${core.partId}`}
                fullWidth
              >
                View Part Details
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Return processing dialog */}
      <Dialog
        open={returnDialogOpen}
        onClose={() => !processingAction && handleCloseReturnDialog()}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Process Core Return</DialogTitle>
        <DialogContent>
          <DialogContentText sx={{ mb: 2 }}>
            Record the return of core #{id.substring(0, 8)} for part {core.partNumber}
          </DialogContentText>
          
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                select
                fullWidth
                label="Core Condition"
                value={returnCondition}
                onChange={(e) => setReturnCondition(e.target.value)}
                margin="normal"
                disabled={processingAction}
              >
                {conditionOptions.map((option) => (
                  <MenuItem key={option.value} value={option.value}>
                    {option.label}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
            
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Notes"
                value={returnNotes}
                onChange={(e) => setReturnNotes(e.target.value)}
                margin="normal"
                multiline
                rows={3}
                disabled={processingAction}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button 
            onClick={handleCloseReturnDialog} 
            disabled={processingAction}
          >
            Cancel
          </Button>
          <Button 
            onClick={processReturn} 
            variant="contained" 
            color="primary"
            disabled={processingAction}
          >
            {processingAction ? 'Processing...' : 'Process Return'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Credit application dialog */}
      <Dialog
        open={creditDialogOpen}
        onClose={() => !processingAction && handleCloseCreditDialog()}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Apply Core Credit</DialogTitle>
        <DialogContent>
          <DialogContentText sx={{ mb: 2 }}>
            Apply credit for returned core #{id.substring(0, 8)}
          </DialogContentText>
          
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Credit Amount"
                type="number"
                value={creditAmount}
                onChange={(e) => setCreditAmount(e.target.value)}
                InputProps={{
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                }}
                margin="normal"
                disabled={processingAction}
              />
              <Typography variant="caption" color="text.secondary">
                Maximum credit value: ${core.coreValue.toFixed(2)}
              </Typography>
            </Grid>
            
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Notes"
                value={returnNotes}
                onChange={(e) => setReturnNotes(e.target.value)}
                margin="normal"
                multiline
                rows={3}
                disabled={processingAction}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button 
            onClick={handleCloseCreditDialog} 
            disabled={processingAction}
          >
            Cancel
          </Button>
          <Button 
            onClick={applyCredit} 
            variant="contained" 
            color="primary"
            disabled={processingAction || creditAmount <= 0 || creditAmount > core.coreValue}
          >
            {processingAction ? 'Processing...' : 'Apply Credit'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CoreTrackingDetail;
