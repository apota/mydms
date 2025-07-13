import React, { useState, useEffect } from 'react';
import { 
  Box, 
  Typography, 
  Paper, 
  Grid, 
  Button, 
  CircularProgress, 
  Alert, 
  Card, 
  CardContent, 
  Stepper, 
  Step, 
  StepLabel, 
  StepContent, 
  Divider, 
  List, 
  ListItem, 
  ListItemText, 
  ListItemIcon,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  FormControl,
  InputLabel,
  Select,
} from '@mui/material';
import { 
  Timeline, 
  TimelineItem, 
  TimelineSeparator, 
  TimelineConnector, 
  TimelineContent, 
  TimelineDot, 
  TimelineOppositeContent 
} from '@mui/lab';
import {
  Person,
  Mail,
  Phone,
  DirectionsCar,
  Build,
  MonetizationOn,
  LocalOffer,
  Grade,
  Schedule,
  Done,
  AddCircle,
  Edit,
  ArrowForward,
  ArrowBack
} from '@mui/icons-material';
import { format } from 'date-fns';
import { CustomerJourneyService } from '../../services/api-services';

function CustomerJourneyVisualizer() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [customerJourneys, setCustomerJourneys] = useState([]);
  const [selectedJourney, setSelectedJourney] = useState(null);
  const [journeyStages, setJourneyStages] = useState([
    { name: 'Awareness', description: 'Customer becomes aware of dealership or vehicles' },
    { name: 'Consideration', description: 'Customer actively evaluates options' },
    { name: 'Intent', description: 'Customer shows intent to purchase' },
    { name: 'Purchase', description: 'Customer makes a purchase' },
    { name: 'Service', description: 'Customer returns for service' },
    { name: 'Repurchase', description: 'Customer considers another purchase' },
    { name: 'Advocacy', description: 'Customer becomes a brand advocate' }
  ]);
  const [touchpoints, setTouchpoints] = useState([]);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editData, setEditData] = useState({
    customerId: '',
    stage: '',
    notes: ''
  });
  const [addTouchpointDialogOpen, setAddTouchpointDialogOpen] = useState(false);
  const [touchpointData, setTouchpointData] = useState({
    type: '',
    description: '',
    outcome: '',
    date: new Date().toISOString().substr(0, 10)
  });

  useEffect(() => {
    const fetchCustomerJourneys = async () => {
      try {
        setLoading(true);
        // In a real app, you'd fetch from your API
        // const response = await CustomerJourneyService.getAllJourneys();
        // setCustomerJourneys(response);
        
        // Mock data for demonstration
        const mockJourneys = generateMockJourneys();
        setCustomerJourneys(mockJourneys);
        if (mockJourneys.length > 0) {
          setSelectedJourney(mockJourneys[0]);
          setTouchpoints(generateMockTouchpoints(mockJourneys[0].customerId));
        }
      } catch (err) {
        console.error('Error fetching customer journeys:', err);
        setError('Failed to load customer journey data. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    fetchCustomerJourneys();
  }, []);

  const handleJourneySelect = (journey) => {
    setSelectedJourney(journey);
    setTouchpoints(generateMockTouchpoints(journey.customerId));
  };

  const handleEditOpen = () => {
    setEditData({
      customerId: selectedJourney.customerId,
      stage: selectedJourney.currentStage,
      notes: selectedJourney.notes || ''
    });
    setEditDialogOpen(true);
  };

  const handleEditClose = () => {
    setEditDialogOpen(false);
  };

  const handleEditSave = async () => {
    try {
      // In a real app, you'd call your API
      // await CustomerJourneyService.updateJourneyStage(editData.customerId, {
      //   stage: editData.stage,
      //   notes: editData.notes
      // });

      // Update local state
      setCustomerJourneys(prev => 
        prev.map(journey => 
          journey.customerId === editData.customerId 
            ? { 
                ...journey, 
                currentStage: editData.stage, 
                notes: editData.notes,
                lastUpdated: new Date()
              } 
            : journey
        )
      );

      setSelectedJourney(prev => ({
        ...prev,
        currentStage: editData.stage,
        notes: editData.notes,
        lastUpdated: new Date()
      }));

      setEditDialogOpen(false);
    } catch (err) {
      console.error('Error updating journey stage:', err);
      setError('Failed to update journey stage. Please try again.');
    }
  };

  const handleAddTouchpointOpen = () => {
    setTouchpointData({
      type: '',
      description: '',
      outcome: '',
      date: new Date().toISOString().substr(0, 10)
    });
    setAddTouchpointDialogOpen(true);
  };

  const handleAddTouchpointClose = () => {
    setAddTouchpointDialogOpen(false);
  };

  const handleTouchpointChange = (e) => {
    const { name, value } = e.target;
    setTouchpointData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleTouchpointSave = () => {
    const newTouchpoint = {
      id: Date.now().toString(),
      customerId: selectedJourney.customerId,
      type: touchpointData.type,
      description: touchpointData.description,
      outcome: touchpointData.outcome,
      date: new Date(touchpointData.date),
      staffMember: 'Current User'
    };

    setTouchpoints(prev => [newTouchpoint, ...prev]);
    setAddTouchpointDialogOpen(false);
  };

  const getStageIndex = (stage) => {
    return journeyStages.findIndex(s => s.name === stage);
  };

  const getStageIcon = (stage) => {
    switch (stage) {
      case 'Awareness':
        return <Person />;
      case 'Consideration':
        return <DirectionsCar />;
      case 'Intent':
        return <LocalOffer />;
      case 'Purchase':
        return <MonetizationOn />;
      case 'Service':
        return <Build />;
      case 'Repurchase':
        return <Grade />;
      case 'Advocacy':
        return <Grade />;
      default:
        return <Person />;
    }
  };

  const getTouchpointIcon = (type) => {
    switch (type) {
      case 'Email':
        return <Mail />;
      case 'Phone':
        return <Phone />;
      case 'Visit':
        return <Person />;
      case 'Service':
        return <Build />;
      case 'Purchase':
        return <MonetizationOn />;
      case 'Web':
        return <Language />;
      default:
        return <Person />;
    }
  };

  // Mock data generators for demonstration purposes
  const generateMockJourneys = () => {
    return [
      {
        customerId: '1',
        customerName: 'John Smith',
        currentStage: 'Purchase',
        daysInCurrentStage: 5,
        journeyStartDate: new Date(2025, 4, 15),
        lastUpdated: new Date(2025, 5, 22),
        stageHistory: [
          { stage: 'Awareness', date: new Date(2025, 4, 15) },
          { stage: 'Consideration', date: new Date(2025, 5, 1) },
          { stage: 'Intent', date: new Date(2025, 5, 15) },
          { stage: 'Purchase', date: new Date(2025, 5, 22) }
        ],
        notes: 'Customer purchased a new SUV after comparing 3 different models.',
        leadSource: 'Website Inquiry'
      },
      {
        customerId: '2',
        customerName: 'Sarah Johnson',
        currentStage: 'Consideration',
        daysInCurrentStage: 12,
        journeyStartDate: new Date(2025, 5, 10),
        lastUpdated: new Date(2025, 5, 17),
        stageHistory: [
          { stage: 'Awareness', date: new Date(2025, 5, 10) },
          { stage: 'Consideration', date: new Date(2025, 5, 17) }
        ],
        notes: 'Interested in electric vehicles. Scheduled a test drive next week.',
        leadSource: 'Social Media Ad'
      },
      {
        customerId: '3',
        customerName: 'Robert Davis',
        currentStage: 'Service',
        daysInCurrentStage: 120,
        journeyStartDate: new Date(2025, 1, 5),
        lastUpdated: new Date(2025, 5, 20),
        stageHistory: [
          { stage: 'Awareness', date: new Date(2025, 1, 5) },
          { stage: 'Consideration', date: new Date(2025, 1, 12) },
          { stage: 'Intent', date: new Date(2025, 1, 20) },
          { stage: 'Purchase', date: new Date(2025, 2, 5) },
          { stage: 'Service', date: new Date(2025, 5, 20) }
        ],
        notes: 'Regular maintenance customer. Likely candidate for upgrade in next 6 months.',
        leadSource: 'Walk-in'
      }
    ];
  };

  const generateMockTouchpoints = (customerId) => {
    if (customerId === '1') {
      return [
        {
          id: '101',
          customerId: '1',
          type: 'Web',
          description: 'Visited website and viewed SUV models',
          outcome: 'Submitted contact form',
          date: new Date(2025, 4, 15),
          staffMember: 'System'
        },
        {
          id: '102',
          customerId: '1',
          type: 'Email',
          description: 'Follow-up email after website inquiry',
          outcome: 'Scheduled dealership visit',
          date: new Date(2025, 4, 17),
          staffMember: 'Jane Rep'
        },
        {
          id: '103',
          customerId: '1',
          type: 'Visit',
          description: 'Dealership visit and test drive',
          outcome: 'Interested in financing options',
          date: new Date(2025, 5, 1),
          staffMember: 'Mike Sales'
        },
        {
          id: '104',
          customerId: '1',
          type: 'Phone',
          description: 'Follow-up call about financing options',
          outcome: 'Scheduled second visit',
          date: new Date(2025, 5, 10),
          staffMember: 'Jane Rep'
        },
        {
          id: '105',
          customerId: '1',
          type: 'Visit',
          description: 'Second dealership visit',
          outcome: 'Vehicle purchase',
          date: new Date(2025, 5, 22),
          staffMember: 'Mike Sales'
        }
      ];
    } else if (customerId === '2') {
      return [
        {
          id: '201',
          customerId: '2',
          type: 'Web',
          description: 'Clicked on social media ad for electric vehicles',
          outcome: 'Visited website',
          date: new Date(2025, 5, 10),
          staffMember: 'System'
        },
        {
          id: '202',
          customerId: '2',
          type: 'Email',
          description: 'Requested electric vehicle brochure via email',
          outcome: 'Sent brochure and follow-up',
          date: new Date(2025, 5, 12),
          staffMember: 'System'
        },
        {
          id: '203',
          customerId: '2',
          type: 'Phone',
          description: 'Inbound call with questions about EV range',
          outcome: 'Scheduled test drive',
          date: new Date(2025, 5, 17),
          staffMember: 'Alex Rep'
        }
      ];
    } else {
      return [
        {
          id: '301',
          customerId: '3',
          type: 'Visit',
          description: 'Walk-in to dealership',
          outcome: 'Showed interest in sedan models',
          date: new Date(2025, 1, 5),
          staffMember: 'Chris Sales'
        },
        {
          id: '302',
          customerId: '3',
          type: 'Visit',
          description: 'Test drive of sedan model',
          outcome: 'Considering financing options',
          date: new Date(2025, 1, 15),
          staffMember: 'Chris Sales'
        },
        {
          id: '303',
          customerId: '3',
          type: 'Purchase',
          description: 'Purchased new sedan model with extended warranty',
          outcome: 'Completed sale',
          date: new Date(2025, 2, 5),
          staffMember: 'Chris Sales'
        },
        {
          id: '304',
          customerId: '3',
          type: 'Service',
          description: 'First scheduled maintenance',
          outcome: 'Completed service',
          date: new Date(2025, 4, 10),
          staffMember: 'Tom Tech'
        },
        {
          id: '305',
          customerId: '3',
          type: 'Service',
          description: 'Second scheduled maintenance',
          outcome: 'Completed service, discussed upcoming models',
          date: new Date(2025, 5, 20),
          staffMember: 'Tom Tech'
        }
      ];
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Customer Journey Visualizer
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Grid container spacing={3}>
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2, height: '100%' }}>
            <Typography variant="h6" gutterBottom>
              Select Customer Journey
            </Typography>
            <List>
              {customerJourneys.map((journey) => (
                <ListItem 
                  key={journey.customerId} 
                  button 
                  selected={selectedJourney?.customerId === journey.customerId}
                  onClick={() => handleJourneySelect(journey)}
                  divider
                >
                  <ListItemText 
                    primary={journey.customerName}
                    secondary={`Current Stage: ${journey.currentStage} (${journey.daysInCurrentStage} days)`}
                  />
                </ListItem>
              ))}
            </List>
          </Paper>
        </Grid>

        <Grid item xs={12} md={8}>
          {selectedJourney ? (
            <Paper sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h6">
                  {selectedJourney.customerName}'s Journey
                </Typography>
                <Box>
                  <Button 
                    variant="outlined" 
                    startIcon={<Edit />}
                    onClick={handleEditOpen}
                    sx={{ mr: 1 }}
                  >
                    Update Stage
                  </Button>
                  <Button 
                    variant="contained" 
                    startIcon={<AddCircle />}
                    onClick={handleAddTouchpointOpen}
                  >
                    Add Touchpoint
                  </Button>
                </Box>
              </Box>

              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <Card variant="outlined" sx={{ mb: 3 }}>
                    <CardContent>
                      <Grid container spacing={2}>
                        <Grid item xs={12} md={6}>
                          <Typography variant="subtitle1">Journey Summary</Typography>
                          <Typography variant="body2">Current Stage: <strong>{selectedJourney.currentStage}</strong></Typography>
                          <Typography variant="body2">Days in Current Stage: {selectedJourney.daysInCurrentStage}</Typography>
                          <Typography variant="body2">Journey Start: {format(new Date(selectedJourney.journeyStartDate), 'PPP')}</Typography>
                          <Typography variant="body2">Last Updated: {format(new Date(selectedJourney.lastUpdated), 'PPP')}</Typography>
                          <Typography variant="body2">Lead Source: {selectedJourney.leadSource}</Typography>
                        </Grid>
                        <Grid item xs={12} md={6}>
                          <Typography variant="subtitle1">Notes</Typography>
                          <Typography variant="body2">{selectedJourney.notes || 'No notes available.'}</Typography>
                        </Grid>
                      </Grid>
                    </CardContent>
                  </Card>
                </Grid>

                <Grid item xs={12}>
                  <Typography variant="subtitle1" gutterBottom>Journey Progress</Typography>
                  <Stepper 
                    activeStep={getStageIndex(selectedJourney.currentStage)} 
                    alternativeLabel
                    sx={{ mb: 4 }}
                  >
                    {journeyStages.map((stage) => (
                      <Step key={stage.name}>
                        <StepLabel 
                          StepIconProps={{ 
                            icon: getStageIcon(stage.name) 
                          }}
                        >
                          {stage.name}
                        </StepLabel>
                      </Step>
                    ))}
                  </Stepper>
                </Grid>

                <Grid item xs={12}>
                  <Typography variant="subtitle1" gutterBottom>Journey Timeline & Touchpoints</Typography>
                  <Timeline position="alternate">
                    {touchpoints.map((touchpoint) => (
                      <TimelineItem key={touchpoint.id}>
                        <TimelineOppositeContent color="text.secondary">
                          {format(new Date(touchpoint.date), 'PPP p')}
                        </TimelineOppositeContent>
                        <TimelineSeparator>
                          <TimelineDot color={
                            touchpoint.type === 'Purchase' ? 'success' : 
                            touchpoint.type === 'Visit' ? 'primary' : 
                            'grey'
                          }>
                            {getTouchpointIcon(touchpoint.type)}
                          </TimelineDot>
                          <TimelineConnector />
                        </TimelineSeparator>
                        <TimelineContent>
                          <Paper elevation={3} sx={{ p: 2 }}>
                            <Typography variant="subtitle2">{touchpoint.type}</Typography>
                            <Typography>{touchpoint.description}</Typography>
                            <Typography variant="body2" color="text.secondary">
                              Outcome: {touchpoint.outcome}
                            </Typography>
                            <Typography variant="caption" display="block">
                              Staff: {touchpoint.staffMember}
                            </Typography>
                          </Paper>
                        </TimelineContent>
                      </TimelineItem>
                    ))}
                  </Timeline>
                </Grid>
              </Grid>
            </Paper>
          ) : (
            <Paper sx={{ p: 3, textAlign: 'center' }}>
              <Typography>Select a customer journey from the list.</Typography>
            </Paper>
          )}
        </Grid>
      </Grid>

      {/* Edit Journey Stage Dialog */}
      <Dialog open={editDialogOpen} onClose={handleEditClose}>
        <DialogTitle>Update Customer Journey Stage</DialogTitle>
        <DialogContent>
          <FormControl fullWidth sx={{ mt: 2 }}>
            <InputLabel>Current Stage</InputLabel>
            <Select
              value={editData.stage}
              label="Current Stage"
              onChange={(e) => setEditData({...editData, stage: e.target.value})}
            >
              {journeyStages.map((stage) => (
                <MenuItem key={stage.name} value={stage.name}>
                  {stage.name} - {stage.description}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <TextField
            margin="dense"
            label="Notes"
            type="text"
            fullWidth
            multiline
            rows={4}
            variant="outlined"
            value={editData.notes}
            onChange={(e) => setEditData({...editData, notes: e.target.value})}
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleEditClose}>Cancel</Button>
          <Button onClick={handleEditSave} variant="contained">Save Changes</Button>
        </DialogActions>
      </Dialog>

      {/* Add Touchpoint Dialog */}
      <Dialog open={addTouchpointDialogOpen} onClose={handleAddTouchpointClose}>
        <DialogTitle>Add New Touchpoint</DialogTitle>
        <DialogContent>
          <FormControl fullWidth sx={{ mt: 2 }}>
            <InputLabel>Touchpoint Type</InputLabel>
            <Select
              value={touchpointData.type}
              name="type"
              label="Touchpoint Type"
              onChange={handleTouchpointChange}
            >
              <MenuItem value="Email">Email</MenuItem>
              <MenuItem value="Phone">Phone Call</MenuItem>
              <MenuItem value="Visit">Dealership Visit</MenuItem>
              <MenuItem value="Service">Service Appointment</MenuItem>
              <MenuItem value="Purchase">Purchase</MenuItem>
              <MenuItem value="Web">Website Interaction</MenuItem>
            </Select>
          </FormControl>
          
          <TextField
            margin="dense"
            name="description"
            label="Description"
            type="text"
            fullWidth
            value={touchpointData.description}
            onChange={handleTouchpointChange}
            sx={{ mt: 2 }}
          />
          
          <TextField
            margin="dense"
            name="outcome"
            label="Outcome"
            type="text"
            fullWidth
            value={touchpointData.outcome}
            onChange={handleTouchpointChange}
          />
          
          <TextField
            margin="dense"
            name="date"
            label="Date"
            type="date"
            fullWidth
            value={touchpointData.date}
            onChange={handleTouchpointChange}
            InputLabelProps={{
              shrink: true,
            }}
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleAddTouchpointClose}>Cancel</Button>
          <Button 
            onClick={handleTouchpointSave} 
            variant="contained"
            disabled={!touchpointData.type || !touchpointData.description}
          >
            Add Touchpoint
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default CustomerJourneyVisualizer;
