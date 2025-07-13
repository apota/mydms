// App.tsx - Service Management Module
import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import Box from '@mui/material/Box';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import Drawer from '@mui/material/Drawer';
import Divider from '@mui/material/Divider';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import IconButton from '@mui/material/IconButton';
import MenuIcon from '@mui/icons-material/Menu';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import DashboardIcon from '@mui/icons-material/Dashboard';
import EventIcon from '@mui/icons-material/Event';
import BuildIcon from '@mui/icons-material/Build';
import AssignmentIcon from '@mui/icons-material/Assignment';
import DirectionsCarIcon from '@mui/icons-material/DirectionsCar';
import BarChartIcon from '@mui/icons-material/BarChart';
import PeopleIcon from '@mui/icons-material/People';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';

// Import pages
import ServiceAdvisorDashboard from './pages/ServiceAdvisorDashboard';
import AppointmentScheduler from './pages/AppointmentScheduler';
import RepairOrderManagement from './pages/RepairOrderManagement';

// Create a theme instance
const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
    background: {
      default: '#f5f5f5',
    },
  },
});

const drawerWidth = 240;

const App = () => {
  const [open, setOpen] = React.useState(true);
  
  const handleDrawerOpen = () => {
    setOpen(true);
  };
  
  const handleDrawerClose = () => {
    setOpen(false);
  };
  
  const menuItems = [
    { text: 'Dashboard', icon: <DashboardIcon />, path: '/' },
    { text: 'Schedule Appointment', icon: <EventIcon />, path: '/schedule' },
    { text: 'Repair Orders', icon: <AssignmentIcon />, path: '/repair-orders' },
    { text: 'Service Jobs', icon: <BuildIcon />, path: '/service-jobs' },
    { text: 'Loaner Vehicles', icon: <DirectionsCarIcon />, path: '/loaners' },
    { text: 'Technicians', icon: <PeopleIcon />, path: '/technicians' },
    { text: 'Reports', icon: <BarChartIcon />, path: '/reports' },
  ];
  
  return (
    <ThemeProvider theme={theme}>
      <LocalizationProvider dateAdapter={AdapterDateFns}>
        <Router>
          <Box sx={{ display: 'flex' }}>
            <CssBaseline />
            
            {/* App Bar */}
            <AppBar position="fixed" sx={{ zIndex: theme.zIndex.drawer + 1 }}>
              <Toolbar>
                <IconButton
                  color="inherit"
                  aria-label="open drawer"
                  onClick={handleDrawerOpen}
                  edge="start"
                  sx={{ mr: 2, ...(open && { display: 'none' }) }}
                >
                  <MenuIcon />
                </IconButton>
                <Typography variant="h6" noWrap component="div">
                  DMS Service Management
                </Typography>
              </Toolbar>
            </AppBar>
            
            {/* Side Drawer */}
            <Drawer
              sx={{
                width: drawerWidth,
                flexShrink: 0,
                '& .MuiDrawer-paper': {
                  width: drawerWidth,
                  boxSizing: 'border-box',
                },
              }}
              variant="persistent"
              anchor="left"
              open={open}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', padding: theme.spacing(0, 1) }}>
                <IconButton onClick={handleDrawerClose}>
                  <ChevronLeftIcon />
                </IconButton>
                <Typography variant="h6" sx={{ ml: 1 }}>
                  Service Menu
                </Typography>
              </Box>
              <Divider />
              <List>
                {menuItems.map((item) => (
                  <ListItem key={item.text} disablePadding>
                    <ListItemButton component="a" href={item.path}>
                      <ListItemIcon>{item.icon}</ListItemIcon>
                      <ListItemText primary={item.text} />
                    </ListItemButton>
                  </ListItem>
                ))}
              </List>
            </Drawer>
            
            {/* Main Content */}
            <Box
              component="main"
              sx={{ 
                flexGrow: 1, 
                p: 3,
                mt: 8,
                ml: open ? `${drawerWidth}px` : 0,
                transition: theme.transitions.create('margin', {
                  easing: theme.transitions.easing.sharp,
                  duration: theme.transitions.duration.leavingScreen,
                }),
              }}
            >
              <Routes>
                <Route path="/" element={<ServiceAdvisorDashboard />} />
                <Route path="/schedule" element={<AppointmentScheduler />} />
                <Route path="/repair-orders" element={<RepairOrderManagement />} />
                {/* Add other routes as they're developed */}
                <Route path="*" element={<ServiceAdvisorDashboard />} />
              </Routes>
            </Box>
          </Box>
        </Router>
      </LocalizationProvider>
    </ThemeProvider>
  );
};

export default App;
