// @ts-nocheck
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Container,
  Divider,
  Grid,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Paper,
  Stepper,
  Step,
  StepLabel,
  Typography,
  Alert,
} from '@mui/material';
import {
  Assignment as AssignmentIcon,
  GppGood as GppGoodIcon,
  QueryBuilder as QueryBuilderIcon,
  Error as ErrorIcon,
  CheckCircleOutline as CheckCircleOutlineIcon,
  Description as DescriptionIcon,
} from '@mui/icons-material';
import { registerVehicleWithDmv } from '../../services/integrationService';
import { DmvRegistrationResultDto } from '../../types/integration';
import { Deal } from '../../types/deal';

const DmvRegistration: React.FC = () => {
  const { id: dealId } = useParams();
  const navigate = useNavigate();
  const [deal, setDeal] = useState<Deal | null>(null);
  const [registrationResult, setRegistrationResult] = useState<DmvRegistrationResultDto | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [activeStep, setActiveStep] = useState<number>(0);
  
  // Mock data - in real app, this would be fetched
  useEffect(() => {
    const mockDeal = {
      id: dealId,
      dealNumber: 'DEAL-12345',
      status: 'Approved',
      type: 'New',
      vehicle: {
        id: 'VEH-789',
        make: 'Toyota',
        model: 'Camry',
        year: 2023,
        vin: '1HGCM82633A123456',
        stockNumber: 'ST-456',
        listPrice: 28500,
        imageUrl: 'https://example.com/car.jpg',
      },
      customer: {
        id: 'CUST-123',
        firstName: 'John',
        lastName: 'Smith',
        email: 'john.smith@example.com',
        phone: '555-123-4567',
      },
      salesRepId: 'REP-001',
      addOns: [],
      payments: [
        {
          id: 'PMT-001',
          type: 'Deposit',
          amount: 2000,
          date: '2023-06-15',
          reference: 'REF123456',
        }
      ],
      financing: {
        lender: 'ABC Bank',
        amount: 26500,
        term: 60,
        rate: 3.9,
        monthlyPayment: 489.75,
        approved: true,
        applicationDate: '2023-06-10',
        approvalDate: '2023-06-12',
      },
      subtotal: 28500,
      taxes: 1710,
      total: 30210,
      deposit: 2000,
      balance: 28210,
      createdAt: '2023-06-09T14:30:00Z',
      updatedAt: '2023-06-12T10:15:00Z',
    };
    
    setDeal(mockDeal);
  }, [dealId]);
  
  const handleRegisterVehicle = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // In a real app, this would call the actual API
      const result = await registerVehicleWithDmv(dealId);
      
      // For demo, let's create a mock result
      const mockResult = {
        registrationId: `REG-${Math.floor(Math.random() * 10000)}`,
        status: 'Complete', // 'Complete', 'Pending', 'Rejected'
        registrationDate: new Date().toISOString(),
        expirationDate: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString(),
        plateNumber: 'ABC1234',
        registrationFee: 150,
        taxesPaid: 75,
        requiredDocuments: [],
        comments: 'Registration successfully completed.',
      };
      
      setRegistrationResult(mockResult);
      setActiveStep(3); // Skip to completed step
    } catch (err) {
      setError('Failed to register vehicle with DMV. Please try again.');
      console.error('Error registering vehicle:', err);
    } finally {
      setLoading(false);
    }
  };
  
  const getRegistrationStatusChip = (status: string) => {
    switch (status.toLowerCase()) {
      case 'complete':
        return <Chip label="Complete" color="success" icon={<CheckCircleOutlineIcon />} />;
      case 'pending':
        return <Chip label="Pending" color="warning" icon={<QueryBuilderIcon />} />;
      case 'rejected':
        return <Chip label="Rejected" color="error" icon={<ErrorIcon />} />;
      default:
        return <Chip label={status} />;
    }
  };
  
  const formatDate = (dateString: string) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };
  
  if (!deal) {
    return (
      <Container maxWidth="md">
        <Box display="flex" justifyContent="center" alignItems="center" height="400px">
          <CircularProgress />
        </Box>
      </Container>
    );
  }
  
  return (
    <Container maxWidth="md">
      <Box mb={4}>
        <Typography variant="h4" gutterBottom>
          DMV Registration
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Register vehicle with the Department of Motor Vehicles
        </Typography>
      </Box>
      
      <Stepper activeStep={activeStep} alternativeLabel sx={{ mb: 4 }}>
        <Step completed={activeStep > 0}>
          <StepLabel>Verify Information</StepLabel>
        </Step>
        <Step completed={activeStep > 1}>
          <StepLabel>Submit Documents</StepLabel>
        </Step>
        <Step completed={activeStep > 2}>
          <StepLabel>Process Registration</StepLabel>
        </Step>
        <Step completed={activeStep > 3}>
          <StepLabel>Registration Complete</StepLabel>
        </Step>
      </Stepper>
      
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}
      
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Vehicle Information
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid item xs={4}>
                  <Typography variant="body2" color="text.secondary">
                    Make
                  </Typography>
                </Grid>
                <Grid item xs={8}>
                  <Typography variant="body1">{deal.vehicle.make}</Typography>
                </Grid>
                <Grid item xs={4}>
                  <Typography variant="body2" color="text.secondary">
                    Model
                  </Typography>
                </Grid>
                <Grid item xs={8}>
                  <Typography variant="body1">{deal.vehicle.model}</Typography>
                </Grid>
                <Grid item xs={4}>
                  <Typography variant="body2" color="text.secondary">
                    Year
                  </Typography>
                </Grid>
                <Grid item xs={8}>
                  <Typography variant="body1">{deal.vehicle.year}</Typography>
                </Grid>
                <Grid item xs={4}>
                  <Typography variant="body2" color="text.secondary">
                    VIN
                  </Typography>
                </Grid>
                <Grid item xs={8}>
                  <Typography variant="body1">{deal.vehicle.vin}</Typography>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Owner Information
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid item xs={4}>
                  <Typography variant="body2" color="text.secondary">
                    Name
                  </Typography>
                </Grid>
                <Grid item xs={8}>
                  <Typography variant="body1">
                    {deal.customer.firstName} {deal.customer.lastName}
                  </Typography>
                </Grid>
                <Grid item xs={4}>
                  <Typography variant="body2" color="text.secondary">
                    Email
                  </Typography>
                </Grid>
                <Grid item xs={8}>
                  <Typography variant="body1">{deal.customer.email}</Typography>
                </Grid>
                <Grid item xs={4}>
                  <Typography variant="body2" color="text.secondary">
                    Phone
                  </Typography>
                </Grid>
                <Grid item xs={8}>
                  <Typography variant="body1">{deal.customer.phone}</Typography>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Required Documents
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <List>
                <ListItem>
                  <ListItemIcon>
                    <AssignmentIcon color="primary" />
                  </ListItemIcon>
                  <ListItemText 
                    primary="Bill of Sale" 
                    secondary="Proof of purchase document"
                  />
                  <Chip label="Uploaded" color="success" size="small" />
                </ListItem>
                <ListItem>
                  <ListItemIcon>
                    <AssignmentIcon color="primary" />
                  </ListItemIcon>
                  <ListItemText 
                    primary="Title Application" 
                    secondary="Completed application for title"
                  />
                  <Chip label="Uploaded" color="success" size="small" />
                </ListItem>
                <ListItem>
                  <ListItemIcon>
                    <AssignmentIcon color="primary" />
                  </ListItemIcon>
                  <ListItemText 
                    primary="Odometer Disclosure" 
                    secondary="Verification of vehicle mileage"
                  />
                  <Chip label="Uploaded" color="success" size="small" />
                </ListItem>
                <ListItem>
                  <ListItemIcon>
                    <GppGoodIcon color="primary" />
                  </ListItemIcon>
                  <ListItemText 
                    primary="Proof of Insurance" 
                    secondary="Current insurance policy information"
                  />
                  <Chip label="Uploaded" color="success" size="small" />
                </ListItem>
              </List>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
      
      {registrationResult ? (
        <Paper sx={{ mt: 3, p: 3 }}>
          <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
            <Typography variant="h5">Registration Result</Typography>
            {getRegistrationStatusChip(registrationResult.status)}
          </Box>
          
          <Divider sx={{ mb: 3 }} />
          
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <Typography variant="body2" color="text.secondary">
                Registration ID
              </Typography>
              <Typography variant="body1" gutterBottom>
                {registrationResult.registrationId}
              </Typography>
            </Grid>
            <Grid item xs={6}>
              <Typography variant="body2" color="text.secondary">
                Plate Number
              </Typography>
              <Typography variant="body1" gutterBottom>
                {registrationResult.plateNumber}
              </Typography>
            </Grid>
            <Grid item xs={6}>
              <Typography variant="body2" color="text.secondary">
                Registration Date
              </Typography>
              <Typography variant="body1" gutterBottom>
                {formatDate(registrationResult.registrationDate)}
              </Typography>
            </Grid>
            <Grid item xs={6}>
              <Typography variant="body2" color="text.secondary">
                Expiration Date
              </Typography>
              <Typography variant="body1" gutterBottom>
                {formatDate(registrationResult.expirationDate)}
              </Typography>
            </Grid>
            <Grid item xs={6}>
              <Typography variant="body2" color="text.secondary">
                Registration Fee
              </Typography>
              <Typography variant="body1" gutterBottom>
                ${registrationResult.registrationFee.toFixed(2)}
              </Typography>
            </Grid>
            <Grid item xs={6}>
              <Typography variant="body2" color="text.secondary">
                Taxes Paid
              </Typography>
              <Typography variant="body1" gutterBottom>
                ${registrationResult.taxesPaid.toFixed(2)}
              </Typography>
            </Grid>
            
            {registrationResult.comments && (
              <Grid item xs={12}>
                <Typography variant="body2" color="text.secondary">
                  Comments
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {registrationResult.comments}
                </Typography>
              </Grid>
            )}
          </Grid>
          
          {registrationResult.status === 'Complete' && (
            <Box display="flex" justifyContent="center" mt={3}>
              <Button
                variant="contained"
                color="primary"
                startIcon={<DescriptionIcon />}
                onClick={() => alert('Registration documents downloaded')}
              >
                Download Registration Documents
              </Button>
            </Box>
          )}
          
          <Box display="flex" justifyContent="flex-end" mt={3}>
            <Button
              variant="contained"
              onClick={() => navigate(`/deals/${dealId}`)}
            >
              Return to Deal
            </Button>
          </Box>
        </Paper>
      ) : (
        <Box display="flex" justifyContent="center" mt={4}>
          <Button
            variant="contained"
            color="primary"
            size="large"
            onClick={handleRegisterVehicle}
            disabled={loading}
          >
            {loading ? <CircularProgress size={24} /> : 'Register Vehicle with DMV'}
          </Button>
        </Box>
      )}
    </Container>
  );
};

export default DmvRegistration;
