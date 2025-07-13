// Service Advisor Dashboard - Main dashboard for service advisors
import React, { useState, useEffect } from 'react';
import { 
  Box, 
  Typography, 
  Grid, 
  Card, 
  CardContent, 
  Button, 
  Chip,
  LinearProgress,
  Divider 
} from '@mui/material';
import { 
  EventAvailable as CalendarIcon,
  Person as PersonIcon,
  DirectionsCar as CarIcon,
  AccessTime as TimeIcon,
  Build as RepairIcon
} from '@mui/icons-material';
import { format } from 'date-fns';
import { Appointment, getAppointments } from '../services/appointmentService';

const STATUS_COLORS = {
  Scheduled: 'primary',
  Confirmed: 'info',
  InProgress: 'warning',
  Completed: 'success',
  Canceled: 'error'
};

const AppointmentStatusChip = ({ status }) => (
  <Chip 
    label={status} 
    color={STATUS_COLORS[status] || 'default'} 
    size="small" 
    sx={{ fontWeight: 'bold' }} 
  />
);

const ServiceAdvisorDashboard = () => {
  const [todayAppointments, setTodayAppointments] = useState<Appointment[]>([]);
  const [upcomingAppointments, setUpcomingAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAppointments = async () => {
      try {
        setLoading(true);
        const appointments = await getAppointments();
        
        const now = new Date();
        const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
        const tomorrow = new Date(today);
        tomorrow.setDate(tomorrow.getDate() + 1);
        
        // Filter today's appointments
        const todayAppts = appointments.filter(appointment => {
          const appointmentDate = new Date(appointment.scheduledStartTime);
          return appointmentDate >= today && appointmentDate < tomorrow;
        });
        
        // Filter upcoming appointments (next 7 days, excluding today)
        const nextWeek = new Date(today);
        nextWeek.setDate(nextWeek.getDate() + 7);
        
        const upcomingAppts = appointments.filter(appointment => {
          const appointmentDate = new Date(appointment.scheduledStartTime);
          return appointmentDate >= tomorrow && appointmentDate < nextWeek;
        });
        
        setTodayAppointments(todayAppts);
        setUpcomingAppointments(upcomingAppts);
      } catch (err) {
        setError('Failed to load appointments. Please try again.');
        console.error('Error fetching appointments:', err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchAppointments();
    
    // Refresh data every 5 minutes
    const intervalId = setInterval(fetchAppointments, 5 * 60 * 1000);
    
    return () => clearInterval(intervalId);
  }, []);

  if (loading) {
    return (
      <Box sx={{ width: '100%', mt: 4 }}>
        <LinearProgress />
        <Typography sx={{ mt: 2, textAlign: 'center' }}>Loading dashboard...</Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Box sx={{ mt: 4, textAlign: 'center' }}>
        <Typography color="error">{error}</Typography>
        <Button 
          variant="contained" 
          onClick={() => window.location.reload()} 
          sx={{ mt: 2 }}
        >
          Retry
        </Button>
      </Box>
    );
  }

  return (
    <Box sx={{ flexGrow: 1, p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Service Advisor Dashboard
      </Typography>
      
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Typography variant="h5" sx={{ mb: 2 }}>
            Today's Appointments ({todayAppointments.length})
          </Typography>
          
          {todayAppointments.length === 0 ? (
            <Card>
              <CardContent>
                <Typography variant="body1">No appointments scheduled for today.</Typography>
              </CardContent>
            </Card>
          ) : (
            <Grid container spacing={2}>
              {todayAppointments.map(appointment => (
                <Grid item xs={12} md={6} lg={4} key={appointment.id}>
                  <AppointmentCard appointment={appointment} />
                </Grid>
              ))}
            </Grid>
          )}
        </Grid>

        <Grid item xs={12}>
          <Typography variant="h5" sx={{ mb: 2, mt: 4 }}>
            Upcoming Appointments ({upcomingAppointments.length})
          </Typography>
          
          {upcomingAppointments.length === 0 ? (
            <Card>
              <CardContent>
                <Typography variant="body1">No upcoming appointments in the next 7 days.</Typography>
              </CardContent>
            </Card>
          ) : (
            <Grid container spacing={2}>
              {upcomingAppointments.map(appointment => (
                <Grid item xs={12} md={6} lg={4} key={appointment.id}>
                  <AppointmentCard appointment={appointment} />
                </Grid>
              ))}
            </Grid>
          )}
        </Grid>
      </Grid>
    </Box>
  );
};

const AppointmentCard = ({ appointment }) => {
  const startTime = new Date(appointment.scheduledStartTime);
  const endTime = new Date(appointment.scheduledEndTime);
  
  return (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
          <AppointmentStatusChip status={appointment.status} />
          <Typography variant="body2" color="text.secondary">
            {appointment.appointmentType}
          </Typography>
        </Box>
        
        <Typography variant="h6" gutterBottom sx={{ mt: 1 }}>
          Customer #{appointment.customerId.substring(0, 8)}
        </Typography>
        
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
          <CalendarIcon fontSize="small" sx={{ mr: 1, color: 'primary.main' }} />
          <Typography variant="body2">
            {format(startTime, 'MMM d, yyyy')}
          </Typography>
        </Box>
        
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
          <TimeIcon fontSize="small" sx={{ mr: 1, color: 'primary.main' }} />
          <Typography variant="body2">
            {format(startTime, 'h:mm a')} - {format(endTime, 'h:mm a')}
          </Typography>
        </Box>
        
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
          <CarIcon fontSize="small" sx={{ mr: 1, color: 'primary.main' }} />
          <Typography variant="body2">
            Vehicle #{appointment.vehicleId.substring(0, 8)}
          </Typography>
        </Box>
        
        {appointment.customerConcerns && (
          <>
            <Divider sx={{ my: 1 }} />
            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
              Customer Concerns:
            </Typography>
            <Typography variant="body2" sx={{ mb: 1 }}>
              {appointment.customerConcerns}
            </Typography>
          </>
        )}
        
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
          <Button 
            size="small" 
            variant="outlined"
            startIcon={<RepairIcon />}
            disabled={appointment.status === 'Canceled'}
          >
            Create RO
          </Button>
          
          <Button 
            size="small" 
            variant="contained" 
            color="primary"
            disabled={appointment.status === 'Canceled' || appointment.status === 'Completed'}
          >
            Check In
          </Button>
        </Box>
      </CardContent>
    </Card>
  );
};

export default ServiceAdvisorDashboard;
