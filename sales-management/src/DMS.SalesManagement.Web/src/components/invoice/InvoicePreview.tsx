// @ts-nocheck
import React from 'react';
import {
  Box,
  Button,
  Chip,
  Divider,
  Grid,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableFooter,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import { InvoiceDto } from '../../types/integration';
import PrintIcon from '@mui/icons-material/Print';
import MailIcon from '@mui/icons-material/Mail';
import DownloadIcon from '@mui/icons-material/Download';

interface InvoicePreviewProps {
  invoice: InvoiceDto;
  onPrint?: () => void;
  onSendByEmail?: () => void;
  onDownload?: () => void;
}

const InvoicePreview: React.FC<InvoicePreviewProps> = ({
  invoice,
  onPrint,
  onSendByEmail,
  onDownload,
}) => {
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const formatCurrency = (amount: number) => {
    return amount.toLocaleString('en-US', {
      style: 'currency',
      currency: 'USD',
    });
  };
  
  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'paid':
        return 'success';
      case 'partiallypaid':
        return 'warning';
      case 'unpaid':
        return 'error';
      default:
        return 'default';
    }
  };

  return (
    <Paper sx={{ p: 3, mb: 3 }}>
      <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={3}>
        <Box>
          <Typography variant="h5" gutterBottom>
            Invoice #{invoice.invoiceNumber}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Issued: {formatDate(invoice.invoiceDate)}
          </Typography>
        </Box>
        <Chip 
          label={invoice.status} 
          color={getStatusColor(invoice.status)} 
          variant="outlined"
        />
      </Box>
      
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} md={6}>
          <Typography variant="subtitle2" gutterBottom>
            Dealership Information
          </Typography>
          <Typography variant="body2">Great Auto Dealership</Typography>
          <Typography variant="body2">123 Auto Plaza Drive</Typography>
          <Typography variant="body2">Anytown, ST 12345</Typography>
          <Typography variant="body2">Phone: (123) 456-7890</Typography>
          <Typography variant="body2">sales@greatautodealership.com</Typography>
        </Grid>
        <Grid item xs={12} md={6}>
          <Typography variant="subtitle2" gutterBottom>
            Customer
          </Typography>
          <Typography variant="body2">{invoice.customerName}</Typography>
          <Typography variant="subtitle2" gutterBottom sx={{ mt: 2 }}>
            Vehicle
          </Typography>
          <Typography variant="body2">{invoice.vehicleDescription}</Typography>
        </Grid>
      </Grid>
      
      <TableContainer component={Paper} variant="outlined" sx={{ mb: 3 }}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Description</TableCell>
              <TableCell align="right">Unit Price</TableCell>
              <TableCell align="right">Quantity</TableCell>
              <TableCell align="right">Amount</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {invoice.lineItems.map((item, index) => (
              <TableRow key={index}>
                <TableCell>
                  {item.description}
                  <Typography variant="caption" color="text.secondary" display="block">
                    {item.itemType}
                  </Typography>
                </TableCell>
                <TableCell align="right">{formatCurrency(item.unitPrice)}</TableCell>
                <TableCell align="right">{item.quantity}</TableCell>
                <TableCell align="right">{formatCurrency(item.extendedPrice)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
          <TableFooter>
            <TableRow>
              <TableCell colSpan={2} />
              <TableCell align="right">
                <Typography variant="subtitle2">Subtotal:</Typography>
              </TableCell>
              <TableCell align="right">{formatCurrency(invoice.subtotal)}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell colSpan={2} />
              <TableCell align="right">
                <Typography variant="subtitle2">
                  Tax ({(invoice.taxRate * 100).toFixed(2)}%):
                </Typography>
              </TableCell>
              <TableCell align="right">{formatCurrency(invoice.taxAmount)}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell colSpan={2} />
              <TableCell align="right">
                <Typography variant="subtitle1">Total:</Typography>
              </TableCell>
              <TableCell align="right">
                <Typography variant="subtitle1">{formatCurrency(invoice.total)}</Typography>
              </TableCell>
            </TableRow>
          </TableFooter>
        </Table>
      </TableContainer>
      
      {invoice.payments && invoice.payments.length > 0 && (
        <>
          <Typography variant="h6" gutterBottom>
            Payments
          </Typography>
          <TableContainer component={Paper} variant="outlined" sx={{ mb: 3 }}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Date</TableCell>
                  <TableCell>Method</TableCell>
                  <TableCell>Reference</TableCell>
                  <TableCell align="right">Amount</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {invoice.payments.map((payment, index) => (
                  <TableRow key={index}>
                    <TableCell>{formatDate(payment.paymentDate)}</TableCell>
                    <TableCell>{payment.paymentMethod}</TableCell>
                    <TableCell>{payment.referenceNumber || '-'}</TableCell>
                    <TableCell align="right">{formatCurrency(payment.amount)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
              <TableFooter>
                <TableRow>
                  <TableCell colSpan={2} />
                  <TableCell align="right">
                    <Typography variant="subtitle2">Balance:</Typography>
                  </TableCell>
                  <TableCell align="right">{formatCurrency(invoice.balance)}</TableCell>
                </TableRow>
              </TableFooter>
            </Table>
          </TableContainer>
        </>
      )}
      
      <Divider sx={{ my: 2 }} />
      
      <Box display="flex" justifyContent="flex-end" gap={1}>
        <Button
          variant="outlined"
          startIcon={<PrintIcon />}
          onClick={onPrint}
        >
          Print
        </Button>
        <Button
          variant="outlined"
          startIcon={<MailIcon />}
          onClick={onSendByEmail}
        >
          Email
        </Button>
        <Button
          variant="outlined"
          startIcon={<DownloadIcon />}
          onClick={onDownload}
        >
          Download PDF
        </Button>
      </Box>
      
      <Box mt={3}>
        <Typography variant="body2" align="center" color="text.secondary">
          Thank you for your business!
        </Typography>
      </Box>
    </Paper>
  );
};

export default InvoicePreview;
