// @ts-nocheck
import React, { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Divider,
  FormControl,
  FormControlLabel,
  FormLabel,
  Grid,
  Radio,
  RadioGroup,
  Slider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
  Typography,
  Paper,
} from '@mui/material';
import { FinancialQuoteDto } from '../../types/integration';
import { getFinancialQuotesForDeal, submitDealForFinancing } from '../../services/integrationService';

interface FinancingOptionsProps {
  dealId: string;
  vehiclePrice: number;
  onFinancingSelected: (financingData: any) => void;
}

const FinancingOptions: React.FC<FinancingOptionsProps> = ({
  dealId,
  vehiclePrice,
  onFinancingSelected,
}) => {
  const [quotes, setQuotes] = useState<FinancialQuoteDto[]>([]);
  const [selectedQuoteId, setSelectedQuoteId] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [downPayment, setDownPayment] = useState<number>(5000);
  const [termMonths, setTermMonths] = useState<number>(60);
  
  useEffect(() => {
    const fetchFinancialQuotes = async () => {
      try {
        setLoading(true);
        const quotesData = await getFinancialQuotesForDeal(dealId);
        setQuotes(quotesData);
        if (quotesData.length > 0) {
          setSelectedQuoteId(quotesData[0].id);
        }
        setError(null);
      } catch (err) {
        setError('Failed to load financing options. Please try again.');
        console.error('Error loading financial quotes:', err);
      } finally {
        setLoading(false);
      }
    };
    
    if (dealId) {
      fetchFinancialQuotes();
    }
  }, [dealId]);
  
  const handleQuoteSelect = (quoteId: string) => {
    setSelectedQuoteId(quoteId);
    const quote = quotes.find(q => q.id === quoteId);
    if (quote) {
      setDownPayment(quote.downPayment);
      setTermMonths(quote.termMonths);
      
      // Notify parent component
      onFinancingSelected({
        quoteId: quote.id,
        lenderId: quote.id.split('-')[0], // Simplified example
        amount: quote.amount,
        interestRate: quote.interestRate,
        termMonths: quote.termMonths,
        monthlyPayment: quote.monthlyPayment,
        downPayment: quote.downPayment,
        lenderName: quote.lenderName
      });
    }
  };
  
  const handleDownPaymentChange = (event: Event, newValue: number | number[]) => {
    setDownPayment(newValue as number);
    recalculatePayment();
  };
  
  const handleTermChange = (event: Event, newValue: number | number[]) => {
    setTermMonths(newValue as number);
    recalculatePayment();
  };
  
  const recalculatePayment = () => {
    // Simple payment calculation
    const selectedQuote = quotes.find(q => q.id === selectedQuoteId);
    if (selectedQuote) {
      const financeAmount = vehiclePrice - downPayment;
      const monthlyRate = selectedQuote.interestRate / 100 / 12;
      const monthlyPayment = 
        (financeAmount * monthlyRate * Math.pow(1 + monthlyRate, termMonths)) / 
        (Math.pow(1 + monthlyRate, termMonths) - 1);
      
      // Update the selected quote
      onFinancingSelected({
        quoteId: selectedQuote.id,
        lenderId: selectedQuote.id.split('-')[0], // Simplified example
        amount: financeAmount,
        interestRate: selectedQuote.interestRate,
        termMonths: termMonths,
        monthlyPayment: monthlyPayment,
        downPayment: downPayment,
        lenderName: selectedQuote.lenderName
      });
    }
  };
  
  const handleSubmitApplication = async () => {
    try {
      setLoading(true);
      
      const selectedQuote = quotes.find(q => q.id === selectedQuoteId);
      if (!selectedQuote) return;
      
      const financingRequest = {
        quoteId: selectedQuote.id,
        lenderId: selectedQuote.id.split('-')[0],
        requestedAmount: vehiclePrice - downPayment,
        termMonths: termMonths,
        downPayment: downPayment,
        additionalDocumentIds: []
      };
      
      const result = await submitDealForFinancing(dealId, financingRequest);
      
      // Handle result
      if (result.status === "Approved") {
        // Update parent component with approved financing
        onFinancingSelected({
          ...financingRequest,
          applicationId: result.applicationId,
          status: result.status,
          approvedAmount: result.approvedAmount,
          interestRate: result.interestRate,
          monthlyPayment: result.monthlyPayment,
          lenderName: result.lenderName
        });
      } else {
        // Handle other statuses
        setError(`Financing application status: ${result.status}. ${result.comments}`);
      }
    } catch (err) {
      setError('Failed to submit financing application. Please try again.');
      console.error('Error submitting financing:', err);
    } finally {
      setLoading(false);
    }
  };
  
  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={3}>
        <CircularProgress />
      </Box>
    );
  }
  
  if (error) {
    return (
      <Paper elevation={0} sx={{ p: 2, bgcolor: 'error.light', color: 'error.contrastText' }}>
        <Typography>{error}</Typography>
      </Paper>
    );
  }
  
  if (quotes.length === 0) {
    return (
      <Paper elevation={0} sx={{ p: 2 }}>
        <Typography>No financing options available for this deal.</Typography>
      </Paper>
    );
  }
  
  const selectedQuote = quotes.find(q => q.id === selectedQuoteId) || quotes[0];
  
  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Financing Options
      </Typography>
      
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <FormControl component="fieldset">
            <FormLabel component="legend">Select Lender</FormLabel>
            <RadioGroup
              aria-label="financing-options"
              name="financing-options"
              value={selectedQuoteId || ''}
              onChange={(e) => handleQuoteSelect(e.target.value)}
            >
              {quotes.map((quote) => (
                <FormControlLabel
                  key={quote.id}
                  value={quote.id}
                  control={<Radio />}
                  label={
                    <Box>
                      <Typography variant="subtitle1">{quote.lenderName}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        {quote.productType} - {quote.interestRate}% APR
                        {quote.isPromotional && (
                          <Typography component="span" color="success.main" sx={{ ml: 1 }}>
                            {quote.promotionDescription}
                          </Typography>
                        )}
                      </Typography>
                    </Box>
                  }
                />
              ))}
            </RadioGroup>
          </FormControl>
        </Grid>
        
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Payment Calculator
              </Typography>
              
              <Box sx={{ mb: 3 }}>
                <Typography id="down-payment-slider" gutterBottom>
                  Down Payment: ${downPayment.toLocaleString()}
                </Typography>
                <Slider
                  value={downPayment}
                  onChange={handleDownPaymentChange}
                  aria-labelledby="down-payment-slider"
                  min={1000}
                  max={Math.round(vehiclePrice * 0.5)}
                  step={500}
                  valueLabelDisplay="auto"
                  valueLabelFormat={(value) => `$${value.toLocaleString()}`}
                />
              </Box>
              
              <Box sx={{ mb: 3 }}>
                <Typography id="term-slider" gutterBottom>
                  Term: {termMonths} months
                </Typography>
                <Slider
                  value={termMonths}
                  onChange={handleTermChange}
                  aria-labelledby="term-slider"
                  min={24}
                  max={84}
                  step={12}
                  marks={[
                    { value: 24, label: '24' },
                    { value: 36, label: '36' },
                    { value: 48, label: '48' },
                    { value: 60, label: '60' },
                    { value: 72, label: '72' },
                    { value: 84, label: '84' },
                  ]}
                  valueLabelDisplay="auto"
                />
              </Box>
              
              <Divider sx={{ my: 2 }} />
              
              <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
                <Table size="small">
                  <TableBody>
                    <TableRow>
                      <TableCell>Vehicle Price</TableCell>
                      <TableCell align="right">${vehiclePrice.toLocaleString()}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Down Payment</TableCell>
                      <TableCell align="right">${downPayment.toLocaleString()}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Amount Financed</TableCell>
                      <TableCell align="right">${(vehiclePrice - downPayment).toLocaleString()}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Term</TableCell>
                      <TableCell align="right">{termMonths} months</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Interest Rate</TableCell>
                      <TableCell align="right">{selectedQuote?.interestRate}%</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell sx={{ fontWeight: 'bold' }}>Monthly Payment</TableCell>
                      <TableCell align="right" sx={{ fontWeight: 'bold' }}>
                        ${selectedQuote?.monthlyPayment.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                      </TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </TableContainer>
              
              <Button
                variant="contained"
                color="primary"
                fullWidth
                onClick={handleSubmitApplication}
                disabled={!selectedQuoteId || loading}
              >
                {loading ? <CircularProgress size={24} /> : 'Apply for Financing'}
              </Button>
              
              {selectedQuote?.requirements?.length > 0 && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Requirements:
                  </Typography>
                  <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
                    {selectedQuote.requirements.map((req, index) => (
                      <li key={index}>
                        <Typography variant="body2" color="text.secondary">
                          {req}
                        </Typography>
                      </li>
                    ))}
                  </ul>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default FinancingOptions;
