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
  IconButton,
  CircularProgress,
  Alert,
  Card,
  CardContent,
  Tooltip
} from '@mui/material';
import {
  ArrowBack,
  Print as PrintIcon,
  Receipt as ReceiptIcon,
  CompareArrows as TransferIcon,
  History as HistoryIcon,
  DeleteOutline as VoidIcon,
  LocalShipping as ShippingIcon
} from '@mui/icons-material';
import { format } from 'date-fns';

import { TransactionsService } from '../../services/api';

// Get color for transaction type
const getTypeColor = (type) => {
  switch(type) {
    case 'RECEIPT': return 'success';
    case 'ISSUE': return 'error';
    case 'RETURN': return 'info';
    case 'ADJUSTMENT': return 'warning';
    case 'TRANSFER': return 'primary';
    default: return 'default';
  }
};

const TransactionDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  
  const [transaction, setTransaction] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Load transaction data
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const data = await TransactionsService.getTransactionById(id);
        setTransaction(data);
        setError(null);
      } catch (err) {
        console.error('Failed to fetch transaction details:', err);
        setError('Failed to load transaction details. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [id]);

  // Handle void transaction (would need confirmation dialog in real implementation)
  const handleVoidTransaction = async () => {
    if (window.confirm('Are you sure you want to void this transaction? This cannot be undone.')) {
      try {
        // This is a placeholder - backend would need to support voiding transactions
        await TransactionsService.voidTransaction(id);
        alert('Transaction voided successfully');
        navigate('/transactions');
      } catch (err) {
        alert('Failed to void transaction: ' + (err.message || 'Unknown error'));
      }
    }
  };

  // Handle print transaction
  const handlePrint = () => {
    window.print();
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
          to="/transactions"
        >
          Back to Transactions
        </Button>
      </Box>
    );
  }

  if (!transaction) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="info" sx={{ mb: 2 }}>
          Transaction not found
        </Alert>
        <Button
          variant="contained"
          startIcon={<ArrowBack />}
          component={Link}
          to="/transactions"
        >
          Back to Transactions
        </Button>
      </Box>
    );
  }

  const isReceiptType = transaction.type === 'RECEIPT';
  const isIssueType = transaction.type === 'ISSUE';
  const isTransferType = transaction.type === 'TRANSFER';
  
  return (
    <Box sx={{ p: 3 }}>
      {/* Header with actions */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3, flexWrap: 'wrap', gap: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <Button
            variant="outlined"
            startIcon={<ArrowBack />}
            component={Link}
            to="/transactions"
            sx={{ mr: 2 }}
          >
            Back
          </Button>
          <Typography variant="h4" component="h1">
            Transaction #{id.substring(0, 8)}
          </Typography>
          <Chip 
            label={transaction.type}
            color={getTypeColor(transaction.type)}
            sx={{ ml: 2 }}
          />
        </Box>
        
        <Box>
          <Button
            variant="outlined"
            startIcon={<PrintIcon />}
            onClick={handlePrint}
            sx={{ mr: 1 }}
          >
            Print
          </Button>
          
          {(transaction.status !== 'VOIDED' && transaction.canVoid) && (
            <Button
              variant="outlined"
              color="error"
              startIcon={<VoidIcon />}
              onClick={handleVoidTransaction}
            >
              Void Transaction
            </Button>
          )}
        </Box>
      </Box>

      {/* Transaction status banner */}
      {transaction.status === 'VOIDED' && (
        <Alert severity="error" sx={{ mb: 3 }}>
          This transaction has been voided and is no longer valid.
        </Alert>
      )}

      {/* Transaction details */}
      <Grid container spacing={3}>
        {/* Main transaction information */}
        <Grid item xs={12} md={8}>
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Transaction Details
            </Typography>
            
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Transaction Date</Typography>
                <Typography variant="body1">
                  {format(new Date(transaction.timestamp), 'MMMM d, yyyy h:mm:ss a')}
                </Typography>
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Created By</Typography>
                <Typography variant="body1">{transaction.createdBy || 'System'}</Typography>
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Location</Typography>
                <Typography variant="body1">{transaction.location || 'Main Warehouse'}</Typography>
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Status</Typography>
                <Chip 
                  label={transaction.status || 'COMPLETED'}
                  color={transaction.status === 'VOIDED' ? 'error' : 'success'}
                  size="small"
                />
              </Grid>

              {transaction.notes && (
                <Grid item xs={12}>
                  <Typography variant="subtitle2" color="text.secondary">Notes</Typography>
                  <Paper variant="outlined" sx={{ p: 1, backgroundColor: 'background.default' }}>
                    <Typography variant="body2">{transaction.notes}</Typography>
                  </Paper>
                </Grid>
              )}
            </Grid>
          </Paper>

          {/* Items table */}
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Transaction Items
            </Typography>

            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Part Number</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell align="right">Quantity</TableCell>
                    {isTransferType && (
                      <>
                        <TableCell>From Location</TableCell>
                        <TableCell>To Location</TableCell>
                      </>
                    )}
                    {(isReceiptType || isIssueType) && (
                      <TableCell align="right">Unit Cost</TableCell>
                    )}
                    {isIssueType && (
                      <TableCell align="right">Unit Price</TableCell>
                    )}
                    <TableCell align="right">Extended</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {transaction.items?.map((item) => (
                    <TableRow key={item.id || `${item.partId}-${item.partNumber}`}>
                      <TableCell>
                        <Link to={`/parts/${item.partId}`}>
                          {item.partNumber}
                        </Link>
                      </TableCell>
                      <TableCell>{item.description}</TableCell>
                      <TableCell align="right">
                        <Typography
                          sx={{
                            fontWeight: 'bold',
                            color: item.quantity > 0 ? 'success.main' : 'error.main'
                          }}
                        >
                          {isIssueType ? item.quantity * -1 : item.quantity}
                        </Typography>
                      </TableCell>
                      
                      {isTransferType && (
                        <>
                          <TableCell>{item.fromLocation}</TableCell>
                          <TableCell>{item.toLocation}</TableCell>
                        </>
                      )}
                      
                      {(isReceiptType || isIssueType) && (
                        <TableCell align="right">
                          ${item.unitCost ? item.unitCost.toFixed(2) : '0.00'}
                        </TableCell>
                      )}
                      
                      {isIssueType && (
                        <TableCell align="right">
                          ${item.unitPrice ? item.unitPrice.toFixed(2) : '0.00'}
                        </TableCell>
                      )}
                      
                      <TableCell align="right">
                        ${item.extendedAmount ? item.extendedAmount.toFixed(2) : '0.00'}
                      </TableCell>
                    </TableRow>
                  ))}
                  
                  {/* Totals row */}
                  {(isReceiptType || isIssueType) && (
                    <TableRow>
                      <TableCell colSpan={isIssueType ? 5 : 4} sx={{ borderBottom: 'none' }} />
                      <TableCell align="right" sx={{ fontWeight: 'bold' }}>
                        Total:
                      </TableCell>
                      <TableCell align="right" sx={{ fontWeight: 'bold' }}>
                        ${transaction.totalAmount ? transaction.totalAmount.toFixed(2) : '0.00'}
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        </Grid>

        {/* Side panel with related information */}
        <Grid item xs={12} md={4}>
          {/* Reference information */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Reference Information
              </Typography>
              
              {transaction.referenceType === 'ORDER' && (
                <>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                    <ShippingIcon sx={{ mr: 1 }} color="primary" />
                    <Typography variant="body1">
                      Purchase Order: 
                      <Link to={`/orders/${transaction.referenceId}`} style={{ marginLeft: '8px' }}>
                        #{transaction.referenceId.substring(0, 8)}
                      </Link>
                    </Typography>
                  </Box>
                  
                  {transaction.supplier && (
                    <Typography variant="body2" sx={{ mb: 1 }}>
                      Supplier: 
                      <Link to={`/suppliers/${transaction.supplier.id}`} style={{ marginLeft: '4px' }}>
                        {transaction.supplier.name}
                      </Link>
                    </Typography>
                  )}
                </>
              )}
              
              {transaction.referenceType === 'SERVICE_ORDER' && (
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <ReceiptIcon sx={{ mr: 1 }} color="primary" />
                  <Typography variant="body1">
                    Service Order: #{transaction.referenceId.substring(0, 8)}
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                      View in Service Management
                    </Typography>
                  </Typography>
                </Box>
              )}
              
              {(!transaction.referenceType || transaction.referenceType === 'MANUAL') && (
                <Typography variant="body1">
                  Manual Transaction {transaction.referenceId && `(Ref: ${transaction.referenceId})`}
                </Typography>
              )}
              
              <Divider sx={{ my: 2 }} />
              
              <Button
                variant="text"
                startIcon={<HistoryIcon />}
                component={Link}
                to={`/parts/${transaction.items?.[0]?.partId}/history`}
                fullWidth
              >
                View Part History
              </Button>
            </CardContent>
          </Card>

          {/* Actions */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Related Actions
              </Typography>
              
              <Button
                fullWidth
                variant="outlined"
                startIcon={<TransferIcon />}
                component={Link}
                to="/transactions/transfer"
                sx={{ mb: 2 }}
              >
                Transfer Parts
              </Button>
              
              <Button
                fullWidth
                variant="outlined"
                startIcon={<ReceiptIcon />}
                component={Link}
                to="/transactions/issue"
              >
                Issue Parts
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default TransactionDetail;
