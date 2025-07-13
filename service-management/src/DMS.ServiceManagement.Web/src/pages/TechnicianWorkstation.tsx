import React, { useState, useEffect } from 'react';
import { Box, Typography, Grid, Card, CardHeader, CardContent, Button, 
         Avatar, Chip, LinearProgress, Divider, List, ListItem, 
         ListItemText, TextField, Dialog, DialogTitle, DialogContent, 
         DialogActions, IconButton, Paper, Tabs, Tab, 
         Alert, Snackbar, AlertColor } from '@mui/material';
import { styled } from '@mui/material/styles';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import PauseIcon from '@mui/icons-material/Pause';
import BuildIcon from '@mui/icons-material/Build';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import CameraAltIcon from '@mui/icons-material/CameraAlt';
import AddIcon from '@mui/icons-material/Add';
import DescriptionIcon from '@mui/icons-material/Description';
import PanToolIcon from '@mui/icons-material/PanTool';
import Timer from '../components/Timer';
import serviceJobService, { ServiceJob, ServiceJobPart } from '../services/serviceJobService';

// Define interfaces for local component state
interface SnackbarState {
    open: boolean;
    message: string;
    severity: AlertColor;
}

interface InspectionItem {
    name: string;
    status?: 'pass' | 'fail' | 'warning';
}

interface InspectionSection {
    title: string;
    items: InspectionItem[];
}

interface InspectionFormData {
    sections: InspectionSection[];
    notes: string;
    images: File[];
}

// Define the main component
const TechnicianWorkstation: React.FC = () => {
    // Type annotated state hooks
    const [activeJobs, setActiveJobs] = useState<ServiceJob[]>([]);
    const [pendingJobs, setPendingJobs] = useState<ServiceJob[]>([]);
    const [completedJobs, setCompletedJobs] = useState<ServiceJob[]>([]);
    const [selectedJob, setSelectedJob] = useState<ServiceJob | null>(null);
    const [isDialogOpen, setIsDialogOpen] = useState<boolean>(false);
    const [isTimerActive, setIsTimerActive] = useState<boolean>(false);
    const [tabValue, setTabValue] = useState<number>(0);
    const [notes, setNotes] = useState<string>('');
    const [snackbar, setSnackbar] = useState<SnackbarState>({
        open: false,
        message: '',
        severity: 'success'
    });
    const [isInspectionOpen, setIsInspectionOpen] = useState<boolean>(false);

    const technicianId = "550e8400-e29b-41d4-a716-446655440000"; // This would come from auth context in a real app

    useEffect(() => {
        // Fetch jobs assigned to this technician
        const fetchJobs = async () => {
            try {
                const response = await serviceJobService.getJobsByTechnicianId(technicianId);
                
                if (response) {
                    const active = response.filter(job => job.status === 'InProgress');
                    const pending = response.filter(job => job.status === 'NotStarted' || job.status === 'WaitingParts');
                    const completed = response.filter(job => job.status === 'Completed');
                    
                    setActiveJobs(active);
                    setPendingJobs(pending);
                    setCompletedJobs(completed);
                }
            } catch (error) {
                console.error('Error fetching jobs:', error);
                setSnackbar({
                    open: true,
                    message: 'Failed to fetch jobs. Please try again.',
                    severity: 'error'
                });
            }
        };

        fetchJobs();

        // Poll for updates every minute
        const interval = setInterval(fetchJobs, 60000);
        return () => clearInterval(interval);
    }, [technicianId]);
    
    const handleStartJob = async (job: ServiceJob): Promise<void> => {
        try {
            const updatedJob = await serviceJobService.startJob(job.id);
            
            // Update the lists
            setActiveJobs([...activeJobs, updatedJob]);
            setPendingJobs(pendingJobs.filter(j => j.id !== job.id));
            
            setSnackbar({
                open: true,
                message: `Job #${updatedJob.id.substring(0, 8)} started successfully`,
                severity: 'success'
            });
        } catch (error) {
            console.error('Error starting job:', error);
            setSnackbar({
                open: true,
                message: 'Failed to start job. Please try again.',
                severity: 'error'
            });
        }
    };
    
    const handleCompleteJob = async (job: ServiceJob): Promise<void> => {
        try {
            if (notes.trim() === '') {
                setSnackbar({
                    open: true,
                    message: 'Please add completion notes before finishing the job',
                    severity: 'warning'
                });
                return;
            }
            
            const jobWithNotes: Partial<ServiceJob> = {
                ...job,
                notes: notes
            };
            
            const updatedJob = await serviceJobService.completeJob(job.id, jobWithNotes);
            
            // Update the lists
            setCompletedJobs([...completedJobs, updatedJob]);
            setActiveJobs(activeJobs.filter(j => j.id !== job.id));
            
            setIsDialogOpen(false);
            setNotes('');
            
            setSnackbar({
                open: true,
                message: `Job #${updatedJob.id.substring(0, 8)} completed successfully`,
                severity: 'success'
            });
        } catch (error) {
            console.error('Error completing job:', error);
            setSnackbar({
                open: true,
                message: 'Failed to complete job. Please try again.',
                severity: 'error'
            });
        }
    };

    const handleJobClick = (job: ServiceJob): void => {
        setSelectedJob(job);
        setIsDialogOpen(true);
        setNotes(job.notes || '');
    };

    const handleCloseDialog = (): void => {
        setIsDialogOpen(false);
        setNotes('');
        setSelectedJob(null);
    };

    const handleTabChange = (_event: React.SyntheticEvent, newValue: number): void => {
        setTabValue(newValue);
    };

    const openInspectionForm = (): void => {
        setIsInspectionOpen(true);
    };

    const closeInspectionForm = (): void => {
        setIsInspectionOpen(false);
    };

    const getStatusChip = (status: ServiceJob['status']): React.ReactElement => {
        switch (status) {
            case 'NotStarted':
                return <Chip size="small" label="Not Started" color="default" />;
            case 'InProgress':
                return <Chip size="small" label="In Progress" color="primary" />;
            case 'Completed':
                return <Chip size="small" label="Completed" color="success" />;
            case 'WaitingParts':
                return <Chip size="small" label="Waiting Parts" color="warning" />;
            case 'OnHold':
                return <Chip size="small" label="On Hold" color="error" />;
            default:
                return <Chip size="small" label={status} color="default" />;
        }
    };

    const JobCard = styled(Card)(({ theme }) => ({
        cursor: 'pointer',
        transition: 'transform 0.2s',
        '&:hover': {
            transform: 'scale(1.02)',
            boxShadow: theme.shadows[4]
        },
        height: '100%',
        display: 'flex',
        flexDirection: 'column'
    }));

    const renderJobList = (jobs: ServiceJob[], showActions = false): React.ReactNode => {
        if (jobs.length === 0) {
            return <Typography variant="body2" color="textSecondary" sx={{ p: 2 }}>No jobs to display</Typography>;
        }

        return (
            <Grid container spacing={2} sx={{ mt: 1 }}>
                {jobs.map((job) => (
                    <Grid item xs={12} md={6} key={job.id} sx={{ mb: 2 }}>
                        <JobCard onClick={() => handleJobClick(job)}>
                            <CardHeader
                                avatar={<Avatar><BuildIcon /></Avatar>}
                                title={job.description}
                                subheader={`RO #${job.repairOrderId.substring(0, 8)}`}
                                action={getStatusChip(job.status)}
                            />
                            <Divider />
                            <CardContent>
                                <Box sx={{ mb: 2 }}>
                                    <Typography variant="body2" color="textSecondary">
                                        {job.jobType} - {job.laborOperationDescription || 'No operation description'}
                                    </Typography>
                                    <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
                                        <AccessTimeIcon fontSize="small" sx={{ mr: 1 }} color="action" />
                                        <Typography variant="body2">
                                            Est. Time: {job.estimatedHours} hrs
                                        </Typography>
                                    </Box>
                                </Box>
                                
                                {showActions && (
                                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
                                        <Button 
                                            size="small" 
                                            variant="outlined" 
                                            startIcon={<PlayArrowIcon />}
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                handleStartJob(job);
                                            }}
                                        >
                                            Start
                                        </Button>
                                    </Box>
                                )}
                                
                                {job.status === 'InProgress' && (
                                    <Box sx={{ mt: 1 }}>
                                        <LinearProgress 
                                            variant="determinate" 
                                            value={Math.min((job.actualHours / job.estimatedHours) * 100, 100)} 
                                            sx={{ height: 8, borderRadius: 5 }}
                                        />
                                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 0.5 }}>
                                            <Typography variant="caption" color="text.secondary">
                                                Progress
                                            </Typography>
                                            <Typography variant="caption" color="text.secondary">
                                                {Math.round((job.actualHours / job.estimatedHours) * 100)}%
                                            </Typography>
                                        </Box>
                                    </Box>
                                )}
                            </CardContent>
                        </JobCard>
                    </Grid>
                ))}
            </Grid>
        );
    };

    return (
        <Box sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h4" component="h1">Technician Workstation</Typography>
                <Chip 
                    icon={<BuildIcon />} 
                    label={`Active Jobs: ${activeJobs.length}`} 
                    color="primary" 
                    variant="outlined"
                />
            </Box>

            <Paper sx={{ mb: 3 }}>
                <Tabs
                    value={tabValue}
                    onChange={handleTabChange}
                    variant="fullWidth"
                    indicatorColor="primary"
                    textColor="primary"
                >
                    <Tab label="Active Jobs" icon={<PlayArrowIcon />} iconPosition="start" />
                    <Tab label="Pending Jobs" icon={<PauseIcon />} iconPosition="start" />
                    <Tab label="Completed Jobs" icon={<CheckCircleIcon />} iconPosition="start" />
                </Tabs>
            </Paper>

            <Box sx={{ display: tabValue === 0 ? 'block' : 'none' }}>
                <Typography variant="h6" sx={{ mb: 2 }}>Active Jobs</Typography>
                {renderJobList(activeJobs)}
            </Box>

            <Box sx={{ display: tabValue === 1 ? 'block' : 'none' }}>
                <Typography variant="h6" sx={{ mb: 2 }}>Pending Jobs</Typography>
                {renderJobList(pendingJobs, true)}
            </Box>

            <Box sx={{ display: tabValue === 2 ? 'block' : 'none' }}>
                <Typography variant="h6" sx={{ mb: 2 }}>Completed Jobs</Typography>
                {renderJobList(completedJobs)}
            </Box>

            {selectedJob && (
                <Dialog
                    open={isDialogOpen}
                    onClose={handleCloseDialog}
                    maxWidth="md"
                    fullWidth
                >
                    <DialogTitle>
                        {selectedJob.description}
                        <Typography variant="subtitle1" color="textSecondary">
                            Job ID: {selectedJob.id.substring(0, 8)}
                        </Typography>
                    </DialogTitle>
                    <DialogContent dividers>
                        <Grid container spacing={3}>
                            <Grid item xs={12} md={6}>
                                <Box sx={{ mb: 3 }}>
                                    <Typography variant="subtitle1">Job Details</Typography>
                                    <List dense>
                                        <ListItem>
                                            <ListItemText 
                                                primary="Repair Order" 
                                                secondary={selectedJob.repairOrderId.substring(0, 8)}
                                            />
                                        </ListItem>
                                        <ListItem>
                                            <ListItemText 
                                                primary="Job Type" 
                                                secondary={selectedJob.jobType}
                                            />
                                        </ListItem>
                                        <ListItem>
                                            <ListItemText 
                                                primary="Status" 
                                                secondary={getStatusChip(selectedJob.status)}
                                            />
                                        </ListItem>
                                        <ListItem>
                                            <ListItemText 
                                                primary="Labor Operation" 
                                                secondary={selectedJob.laborOperationCode}
                                            />
                                        </ListItem>
                                    </List>
                                </Box>

                                <Box sx={{ mb: 3 }}>
                                    <Typography variant="subtitle1">Time Tracking</Typography>
                                    <Box sx={{ display: 'flex', alignItems: 'center', mt: 1, mb: 2 }}>
                                        <AccessTimeIcon sx={{ mr: 1 }} />
                                        <Typography variant="body1">
                                            Estimated: {selectedJob.estimatedHours} hrs
                                        </Typography>
                                    </Box>
                                    
                                    {selectedJob.status === 'InProgress' && (
                                        <Box sx={{ border: 1, borderColor: 'divider', borderRadius: 1, p: 2, textAlign: 'center' }}>
                                            <Timer 
                                                isActive={isTimerActive} 
                                                setIsActive={setIsTimerActive} 
                                                onTime={(elapsedTime: number) => {
                                                    // Here we could update job progress
                                                    console.log('Elapsed time:', elapsedTime);
                                                }}
                                                initialSeconds={selectedJob.actualHours * 3600}
                                            />
                                        </Box>
                                    )}
                                </Box>

                                {selectedJob.status === 'InProgress' && (
                                    <Box>
                                        <Typography variant="subtitle1">Actions</Typography>
                                        <Box sx={{ display: 'flex', mt: 1, gap: 1 }}>
                                            <Button 
                                                variant="outlined" 
                                                startIcon={<CameraAltIcon />}
                                                fullWidth
                                                onClick={openInspectionForm}
                                            >
                                                Inspection
                                            </Button>
                                            <Button 
                                                variant="outlined" 
                                                startIcon={<AddIcon />}
                                                fullWidth
                                            >
                                                Request Parts
                                            </Button>
                                        </Box>
                                    </Box>
                                )}
                            </Grid>
                            
                            <Grid item xs={12} md={6}>
                                <Typography variant="subtitle1">Notes & Completion</Typography>
                                <TextField
                                    fullWidth
                                    multiline
                                    rows={6}
                                    label="Technician Notes"
                                    variant="outlined"
                                    value={notes}
                                    onChange={(e) => setNotes(e.target.value)}
                                    sx={{ mb: 2 }}
                                />
                                
                                {selectedJob.status === 'InProgress' && (
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        startIcon={<CheckCircleIcon />}
                                        fullWidth
                                        onClick={() => handleCompleteJob(selectedJob)}
                                    >
                                        Complete Job
                                    </Button>
                                )}

                                {selectedJob.status === 'Completed' && (
                                    <Alert severity="success" sx={{ mt: 2 }}>
                                        This job was completed on {new Date(selectedJob.endTime as string).toLocaleString()}
                                    </Alert>
                                )}

                                {selectedJob.status === 'NotStarted' && (
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        startIcon={<PlayArrowIcon />}
                                        fullWidth
                                        onClick={() => {
                                            handleCloseDialog();
                                            handleStartJob(selectedJob);
                                        }}
                                    >
                                        Start Job
                                    </Button>
                                )}

                                {selectedJob.status === 'WaitingParts' && (
                                    <Alert severity="warning" icon={<PanToolIcon />}>
                                        This job is waiting for parts to continue
                                    </Alert>
                                )}
                            </Grid>
                            
                            {selectedJob.parts && selectedJob.parts.length > 0 && (
                                <Grid item xs={12}>
                                    <Typography variant="subtitle1">Parts</Typography>
                                    <List dense>
                                        {selectedJob.parts.map(part => (
                                            <ListItem key={part.id}>
                                                <ListItemText
                                                    primary={part.partId}
                                                    secondary={`Qty: ${part.quantity} - Status: ${part.status}`}
                                                />
                                                <Chip 
                                                    size="small" 
                                                    label={`$${part.totalAmount.toFixed(2)}`} 
                                                    variant="outlined" 
                                                />
                                            </ListItem>
                                        ))}
                                    </List>
                                </Grid>
                            )}
                        </Grid>
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={handleCloseDialog}>Close</Button>
                    </DialogActions>
                </Dialog>
            )}

            {/* Inspection Form Dialog */}
            <Dialog
                open={isInspectionOpen}
                onClose={closeInspectionForm}
                maxWidth="md"
                fullWidth
            >
                <DialogTitle>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <DescriptionIcon sx={{ mr: 1 }} />
                        Digital Inspection
                    </Box>
                </DialogTitle>
                <DialogContent dividers>
                    <Typography variant="subtitle2" color="textSecondary" sx={{ mb: 2 }}>
                        Complete the inspection checklist for this vehicle
                    </Typography>
                    
                    {/* This would be a complete inspection form with checkboxes and photo uploads */}
                    {/* Simplified version shown here */}
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <Paper variant="outlined" sx={{ p: 2 }}>
                                <Typography variant="subtitle1">Brake System</Typography>
                                <Grid container spacing={1}>
                                    {['Front Pads', 'Rear Pads', 'Rotors', 'Brake Fluid'].map(item => (
                                        <Grid item xs={6} sm={3} key={item}>
                                            <TextField
                                                select
                                                label={item}
                                                fullWidth
                                                size="small"
                                                variant="outlined"
                                                SelectProps={{
                                                    native: true,
                                                }}
                                            >
                                                <option value="">Select</option>
                                                <option value="pass">Pass</option>
                                                <option value="fail">Fail</option>
                                                <option value="warning">Warning</option>
                                            </TextField>
                                        </Grid>
                                    ))}
                                </Grid>
                            </Paper>
                        </Grid>
                        
                        <Grid item xs={12}>
                            <Paper variant="outlined" sx={{ p: 2 }}>
                                <Typography variant="subtitle1">Fluid Levels</Typography>
                                <Grid container spacing={1}>
                                    {['Engine Oil', 'Transmission', 'Coolant', 'Power Steering', 'Washer Fluid'].map(item => (
                                        <Grid item xs={6} sm={4} key={item}>
                                            <TextField
                                                select
                                                label={item}
                                                fullWidth
                                                size="small"
                                                variant="outlined"
                                                SelectProps={{
                                                    native: true,
                                                }}
                                            >
                                                <option value="">Select</option>
                                                <option value="pass">Pass</option>
                                                <option value="fail">Fail</option>
                                                <option value="warning">Warning</option>
                                            </TextField>
                                        </Grid>
                                    ))}
                                </Grid>
                            </Paper>
                        </Grid>
                        
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                label="Inspection Notes"
                                multiline
                                rows={3}
                                variant="outlined"
                            />
                        </Grid>
                        
                        <Grid item xs={12}>
                            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                <Typography variant="subtitle1" sx={{ mr: 2 }}>Upload Photos</Typography>
                                <Button 
                                    variant="outlined" 
                                    component="label" 
                                    startIcon={<CameraAltIcon />}
                                >
                                    Upload
                                    <input
                                        type="file"
                                        hidden
                                        accept="image/*"
                                        multiple
                                    />
                                </Button>
                            </Box>
                        </Grid>
                    </Grid>
                </DialogContent>
                <DialogActions>
                    <Button onClick={closeInspectionForm}>Cancel</Button>
                    <Button variant="contained" color="primary">Save Inspection</Button>
                </DialogActions>
            </Dialog>

            <Snackbar
                open={snackbar.open}
                autoHideDuration={6000}
                onClose={() => setSnackbar({ ...snackbar, open: false })}
                anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
            >
                <Alert 
                    onClose={() => setSnackbar({ ...snackbar, open: false })} 
                    severity={snackbar.severity}
                    sx={{ width: '100%' }}
                >
                    {snackbar.message}
                </Alert>
            </Snackbar>
        </Box>
    );
};

export default TechnicianWorkstation;
