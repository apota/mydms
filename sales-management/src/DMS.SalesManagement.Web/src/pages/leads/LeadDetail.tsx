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
  Tab,
  Tabs,
  Chip,
  Divider,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemText,
  ListItemIcon
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Phone as PhoneIcon,
  Email as EmailIcon,
  Event as EventIcon,
  Directions as DirectionsIcon,
  Comment as CommentIcon,
  Add as AddIcon
} from '@mui/icons-material';
import { format } from 'date-fns';
import { 
  Lead, 
  LeadStatus, 
  LeadActivity, 
  LeadActivityType, 
  UpdateLeadRequest,
  AddLeadActivityRequest
} from '../../types/lead';

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
      id={`lead-tabpanel-${index}`}
      aria-labelledby={`lead-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const LeadDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [tabValue, setTabValue] = useState(0);
  const [editMode, setEditMode] = useState(false);
  const [activityDialogOpen, setActivityDialogOpen] = useState(false);
  
  // Form state
  const [editForm, setEditForm] = useState<UpdateLeadRequest>({});
  const [activityForm, setActivityForm] = useState<AddLeadActivityRequest>({
    type: LeadActivityType.Call,
    notes: ''
  });

  // Fetch lead details
  const { data: lead, isLoading, error } = useQuery(
    ['lead', id],
    () => fetchLead(id as string),
    {
      enabled: !!id,
      onSuccess: (data) => {
        setEditForm({
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          phone: data.phone,
          status: data.status,
          assignedSalesRepId: data.assignedSalesRepId || ''
        });
      }
    }
  );

  // Fetch sales reps
  const { data: salesReps } = useQuery(
    ['salesReps'],
    () => fetchSalesReps(),
    {
      staleTime: 5 * 60 * 1000, // 5 minutes
    }
  );

  // Update lead mutation
  const updateLeadMutation = useMutation(
    (data: { id: string, lead: UpdateLeadRequest }) => updateLead(data.id, data.lead),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(['lead', id]);
        setEditMode(false);
      }
    }
  );

  // Add lead activity mutation
  const addActivityMutation = useMutation(
    (data: { leadId: string, activity: AddLeadActivityRequest }) => 
      addLeadActivity(data.leadId, data.activity),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(['lead', id]);
        setActivityDialogOpen(false);
        setActivityForm({
          type: LeadActivityType.Call,
          notes: ''
        });
      }
    }
  );

  // Tab change handler
  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  // Edit form handlers
  const handleEditClick = () => {
    setEditMode(true);
  };

  const handleCancelEdit = () => {
    setEditMode(false);
    if (lead) {
      setEditForm({
        firstName: lead.firstName,
        lastName: lead.lastName,
        email: lead.email,
        phone: lead.phone,
        status: lead.status,
        assignedSalesRepId: lead.assignedSalesRepId || ''
      });
    }
  };

  const handleSaveEdit = () => {
    if (id) {
      updateLeadMutation.mutate({
        id,
        lead: editForm
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

  // Activity dialog handlers
  const handleOpenActivityDialog = () => {
    setActivityDialogOpen(true);
  };

  const handleCloseActivityDialog = () => {
    setActivityDialogOpen(false);
    setActivityForm({
      type: LeadActivityType.Call,
      notes: ''
    });
  };

  const handleActivityInputChange = (e: React.ChangeEvent<HTMLInputElement | { name?: string; value: unknown }>) => {
    const { name, value } = e.target;
    if (name) {
      setActivityForm(prev => ({
        ...prev,
        [name]: value
      }));
    }
  };

  const handleAddActivity = () => {
    if (id) {
      addActivityMutation.mutate({
        leadId: id,
        activity: {
          ...activityForm,
          date: activityForm.date || new Date().toISOString()
        }
      });
    }
  };

  // For demonstration - in real implementation, these would be API calls
  const fetchLead = async (leadId: string): Promise<Lead> => {
    // This would be an actual API call
    console.log(`Fetching lead ${leadId}`);
    // Mock data
    return {
      id: leadId,
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@example.com',
      phone: '(555) 123-4567',
      status: LeadStatus.Contacted,
      source: 'Website',
      interestType: 'New',
      comments: 'Interested in SUV models',
      createdAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
      updatedAt: new Date().toISOString(),
      lastActivityDate: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
      followupDate: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
      activities: [
        {
          id: '1',
          type: LeadActivityType.Call,
          date: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
          userId: 'user1',
          notes: 'Initial contact, customer interested in new SUVs.'
        },
        {
          id: '2',
          type: LeadActivityType.Email,
          date: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
          userId: 'user1',
          notes: 'Sent brochure for 2023 models.'
        },
        {
          id: '3',
          type: LeadActivityType.TestDrive,
          date: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
          userId: 'user1',
          notes: 'Test drove the XC90. Seemed very interested.'
        }
      ]
    };
  };

  const fetchSalesReps = async () => {
    // This would be an actual API call
    return [
      { id: 'user1', name: 'Alice Johnson' },
      { id: 'user2', name: 'Bob Smith' },
      { id: 'user3', name: 'Carol White' }
    ];
  };

  const updateLead = async (leadId: string, data: UpdateLeadRequest): Promise<Lead> => {
    // This would be an actual API call
    console.log(`Updating lead ${leadId}`, data);
    // Return mock data
    return {
      id: leadId,
      firstName: data.firstName || 'John',
      lastName: data.lastName || 'Doe',
      email: data.email || 'john.doe@example.com',
      phone: data.phone || '(555) 123-4567',
      status: data.status || LeadStatus.Contacted,
      source: 'Website',
      interestType: 'New',
      comments: 'Interested in SUV models',
      createdAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
      updatedAt: new Date().toISOString(),
      lastActivityDate: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
      followupDate: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
      activities: []
    };
  };

  const addLeadActivity = async (leadId: string, activity: AddLeadActivityRequest): Promise<LeadActivity> => {
    // This would be an actual API call
    console.log(`Adding activity to lead ${leadId}`, activity);
    // Return mock data
    return {
      id: Date.now().toString(),
      type: activity.type,
      date: activity.date || new Date().toISOString(),
      userId: 'user1',
      notes: activity.notes
    };
  };

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" height="100%">
        <CircularProgress />
      </Box>
    );
  }

  if (error || !lead) {
    return (
      <Box p={3}>
        <Typography color="error">Error loading lead details</Typography>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/leads')}
          sx={{ mt: 2 }}
        >
          Back to Leads
        </Button>
      </Box>
    );
  }

  // Helper function to get color for status chip
  const getStatusColor = (status: LeadStatus) => {
    switch (status) {
      case LeadStatus.New:
        return 'primary';
      case LeadStatus.Contacted:
        return 'info';
      case LeadStatus.Qualified:
        return 'success';
      case LeadStatus.Appointment:
        return 'secondary';
      case LeadStatus.Sold:
        return 'success';
      case LeadStatus.Lost:
        return 'error';
      default:
        return 'default';
    }
  };

  // Helper function for activity icon
  const getActivityIcon = (type: LeadActivityType) => {
    switch (type) {
      case LeadActivityType.Call:
        return <PhoneIcon />;
      case LeadActivityType.Email:
        return <EmailIcon />;
      case LeadActivityType.Visit:
      case LeadActivityType.TestDrive:
        return <DirectionsIcon />;
      case LeadActivityType.Appointment:
        return <EventIcon />;
      default:
        return <CommentIcon />;
    }
  };

  return (
    <Box p={3}>
      {/* Header */}
      <Box display="flex" alignItems="center" mb={3}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/leads')}
          sx={{ mr: 2 }}
        >
          Back
        </Button>
        <Typography variant="h4" component="h1" sx={{ flexGrow: 1 }}>
          {!editMode ? (
            `${lead.firstName} ${lead.lastName}`
          ) : (
            "Edit Lead"
          )}
        </Typography>
        {!editMode ? (
          <Button
            variant="outlined"
            startIcon={<EditIcon />}
            onClick={handleEditClick}
          >
            Edit
          </Button>
        ) : (
          <>
            <Button
              variant="outlined"
              onClick={handleCancelEdit}
              sx={{ mr: 1 }}
            >
              Cancel
            </Button>
            <Button
              variant="contained"
              onClick={handleSaveEdit}
              disabled={updateLeadMutation.isLoading}
            >
              Save
            </Button>
          </>
        )}
      </Box>

      {/* Lead status */}
      <Box mb={3}>
        <Chip
          label={lead.status}
          color={getStatusColor(lead.status)}
          sx={{ fontWeight: 'bold' }}
        />
        <Typography 
          variant="caption" 
          sx={{ ml: 2, color: 'text.secondary' }}
        >
          Created on {new Date(lead.createdAt).toLocaleDateString()}
        </Typography>
      </Box>

      {/* Tabs */}
      <Paper sx={{ width: '100%', mb: 3 }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={tabValue} onChange={handleTabChange}>
            <Tab label="Details" id="lead-tab-0" />
            <Tab label="Activities" id="lead-tab-1" />
            <Tab label="Notes" id="lead-tab-2" />
          </Tabs>
        </Box>

        {/* Details Tab */}
        <TabPanel value={tabValue} index={0}>
          <Grid container spacing={3}>
            {/* Left column - Contact Information */}
            <Grid item xs={12} md={6}>
              <Typography variant="h6" gutterBottom>
                Contact Information
              </Typography>
              <Divider sx={{ mb: 2 }} />

              {!editMode ? (
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2">Name</Typography>
                    <Typography>
                      {lead.firstName} {lead.lastName}
                    </Typography>
                  </Grid>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2">Email</Typography>
                    <Typography>{lead.email}</Typography>
                  </Grid>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2">Phone</Typography>
                    <Typography>{lead.phone}</Typography>
                  </Grid>
                </Grid>
              ) : (
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <TextField
                      fullWidth
                      label="First Name"
                      name="firstName"
                      value={editForm.firstName}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextField
                      fullWidth
                      label="Last Name"
                      name="lastName"
                      value={editForm.lastName}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Email"
                      name="email"
                      value={editForm.email}
                      onChange={handleInputChange}
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Phone"
                      name="phone"
                      value={editForm.phone}
                      onChange={handleInputChange}
                    />
                  </Grid>
                </Grid>
              )}
            </Grid>

            {/* Right column - Lead Information */}
            <Grid item xs={12} md={6}>
              <Typography variant="h6" gutterBottom>
                Lead Information
              </Typography>
              <Divider sx={{ mb: 2 }} />

              {!editMode ? (
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2">Status</Typography>
                    <Typography>
                      <Chip
                        label={lead.status}
                        color={getStatusColor(lead.status)}
                        size="small"
                      />
                    </Typography>
                  </Grid>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2">Source</Typography>
                    <Typography>{lead.source}</Typography>
                  </Grid>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2">Interest</Typography>
                    <Typography>{lead.interestType}</Typography>
                  </Grid>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2">Assigned To</Typography>
                    <Typography>
                      {lead.assignedSalesRepId ? 
                        salesReps?.find(rep => rep.id === lead.assignedSalesRepId)?.name || 
                        lead.assignedSalesRepId : 
                        'Unassigned'}
                    </Typography>
                  </Grid>
                </Grid>
              ) : (
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <FormControl fullWidth>
                      <InputLabel>Status</InputLabel>
                      <Select
                        name="status"
                        value={editForm.status}
                        label="Status"
                        onChange={handleInputChange}
                      >
                        {Object.values(LeadStatus).map(status => (
                          <MenuItem key={status} value={status}>
                            {status}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12}>
                    <FormControl fullWidth>
                      <InputLabel>Assigned To</InputLabel>
                      <Select
                        name="assignedSalesRepId"
                        value={editForm.assignedSalesRepId}
                        label="Assigned To"
                        onChange={handleInputChange}
                      >
                        <MenuItem value="">
                          <em>Unassigned</em>
                        </MenuItem>
                        {salesReps?.map(rep => (
                          <MenuItem key={rep.id} value={rep.id}>
                            {rep.name}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                </Grid>
              )}
            </Grid>
          </Grid>
        </TabPanel>

        {/* Activities Tab */}
        <TabPanel value={tabValue} index={1}>
          <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
            <Typography variant="h6">
              Activities
            </Typography>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleOpenActivityDialog}
            >
              Add Activity
            </Button>
          </Box>

          {lead.activities && lead.activities.length > 0 ? (
            <List>
              {lead.activities.map((activity, index) => (
                <React.Fragment key={activity.id || index}>
                  <ListItem alignItems="flex-start">
                    <ListItemIcon>
                      {getActivityIcon(activity.type)}
                    </ListItemIcon>
                    <ListItemText
                      primary={
                        <Box display="flex" justifyContent="space-between">
                          <Typography variant="subtitle1">
                            {activity.type}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {new Date(activity.date).toLocaleString()}
                          </Typography>
                        </Box>
                      }
                      secondary={
                        <React.Fragment>
                          <Typography
                            component="span"
                            variant="body2"
                            color="text.primary"
                          >
                            {salesReps?.find(rep => rep.id === activity.userId)?.name || activity.userId}
                          </Typography>
                          {" — "}{activity.notes}
                        </React.Fragment>
                      }
                    />
                  </ListItem>
                  {index < lead.activities.length - 1 && <Divider variant="inset" component="li" />}
                </React.Fragment>
              ))}
            </List>
          ) : (
            <Typography color="text.secondary" align="center" py={4}>
              No activities recorded yet.
            </Typography>
          )}
        </TabPanel>

        {/* Notes Tab */}
        <TabPanel value={tabValue} index={2}>
          <Typography variant="h6" gutterBottom>
            Notes
          </Typography>
          <Paper variant="outlined" sx={{ p: 2, minHeight: '150px' }}>
            {lead.comments ? (
              <Typography>{lead.comments}</Typography>
            ) : (
              <Typography color="text.secondary" align="center">
                No notes available.
              </Typography>
            )}
          </Paper>
        </TabPanel>
      </Paper>

      {/* Activity Dialog */}
      <Dialog
        open={activityDialogOpen}
        onClose={handleCloseActivityDialog}
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle>Add New Activity</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Activity Type</InputLabel>
                <Select
                  name="type"
                  value={activityForm.type}
                  label="Activity Type"
                  onChange={handleActivityInputChange}
                >
                  {Object.values(LeadActivityType).map(type => (
                    <MenuItem key={type} value={type}>
                      {type}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                name="notes"
                label="Notes"
                multiline
                rows={4}
                value={activityForm.notes}
                onChange={handleActivityInputChange}
                fullWidth
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseActivityDialog}>Cancel</Button>
          <Button 
            onClick={handleAddActivity} 
            variant="contained"
            disabled={activityMutation.isLoading || !activityForm.notes}
          >
            {activityMutation.isLoading ? <CircularProgress size={24} /> : 'Add'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default LeadDetail;
