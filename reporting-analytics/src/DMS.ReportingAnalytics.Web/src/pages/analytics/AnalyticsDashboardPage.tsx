import React, { useState } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Container from '@mui/material/Container';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import Drawer from '@mui/material/Drawer';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Button from '@mui/material/Button';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import IconButton from '@mui/material/IconButton';
import Breadcrumbs from '@mui/material/Breadcrumbs';
import Link from '@mui/material/Link';
import DashboardIcon from '@mui/icons-material/Dashboard';
import ShowChartIcon from '@mui/icons-material/ShowChart';
import BarChartIcon from '@mui/icons-material/BarChart';
import TableChartIcon from '@mui/icons-material/TableChart';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import SettingsIcon from '@mui/icons-material/Settings';
import FileDownloadIcon from '@mui/icons-material/FileDownload';
import RefreshIcon from '@mui/icons-material/Refresh';
import KpiDashboard from '../../components/analytics/KpiDashboard';
import TrendChart from '../../components/analytics/TrendChart';
import InventoryRecommendations from '../../components/analytics/InventoryRecommendations';
import CustomerChurnPredictions from '../../components/analytics/CustomerChurnPredictions';
import SalesForecast from '../../components/analytics/SalesForecast';

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
      id={`analytics-tabpanel-${index}`}
      aria-labelledby={`analytics-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ pt: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

function a11yProps(index: number) {
  return {
    id: `analytics-tab-${index}`,
    'aria-controls': `analytics-tabpanel-${index}`,
  };
}

const AnalyticsDashboardPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [exportMenu, setExportMenu] = useState<null | HTMLElement>(null);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  const handleExportMenuOpen = (event: React.MouseEvent<HTMLButtonElement>) => {
    setExportMenu(event.currentTarget);
  };

  const handleExportMenuClose = () => {
    setExportMenu(null);
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100vh' }}>
      <AppBar position="static" color="default" elevation={1}>
        <Toolbar>
          <DashboardIcon sx={{ mr: 1 }} />
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            Analytics Dashboard
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <IconButton color="inherit">
              <RefreshIcon />
            </IconButton>
            
            <Button
              endIcon={<ExpandMoreIcon />}
              onClick={handleExportMenuOpen}
              startIcon={<FileDownloadIcon />}
              variant="outlined"
            >
              Export
            </Button>
            
            <Menu
              anchorEl={exportMenu}
              open={Boolean(exportMenu)}
              onClose={handleExportMenuClose}
            >
              <MenuItem onClick={handleExportMenuClose}>Export to Excel</MenuItem>
              <MenuItem onClick={handleExportMenuClose}>Export to PDF</MenuItem>
              <MenuItem onClick={handleExportMenuClose}>Export as Image</MenuItem>
            </Menu>
            
            <IconButton color="inherit">
              <SettingsIcon />
            </IconButton>
          </Box>
        </Toolbar>
      </AppBar>
      
      <Box sx={{ display: 'flex', flexGrow: 1, overflow: 'hidden' }}>
        <Drawer
          variant="permanent"
          sx={{
            width: 240,
            flexShrink: 0,
            '& .MuiDrawer-paper': {
              width: 240,
              boxSizing: 'border-box',
              position: 'relative',
              height: '100%',
              zIndex: 1,
            },
          }}
        >
          <List>
            <ListItem button selected={true}>
              <ListItemIcon>
                <DashboardIcon />
              </ListItemIcon>
              <ListItemText primary="Executive Overview" />
            </ListItem>
            <ListItem button>
              <ListItemIcon>
                <ShowChartIcon />
              </ListItemIcon>
              <ListItemText primary="Sales Analytics" />
            </ListItem>
            <ListItem button>
              <ListItemIcon>
                <TableChartIcon />
              </ListItemIcon>
              <ListItemText primary="Inventory Analytics" />
            </ListItem>
            <ListItem button>
              <ListItemIcon>
                <BarChartIcon />
              </ListItemIcon>
              <ListItemText primary="Service Analytics" />
            </ListItem>
            <ListItem button>
              <ListItemIcon>
                <TableChartIcon />
              </ListItemIcon>
              <ListItemText primary="Financial Analytics" />
            </ListItem>
          </List>
        </Drawer>
        
        <Box sx={{ flexGrow: 1, p: 3, overflow: 'auto' }}>
          <Breadcrumbs sx={{ mb: 2 }}>
            <Link color="inherit" href="/">Home</Link>
            <Link color="inherit" href="/analytics">Analytics</Link>
            <Typography color="text.primary">Dashboard</Typography>
          </Breadcrumbs>
          
          <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tabs value={activeTab} onChange={handleTabChange} aria-label="analytics tabs">
              <Tab label="Key Metrics" {...a11yProps(0)} />
              <Tab label="Forecasts" {...a11yProps(1)} />
              <Tab label="Inventory" {...a11yProps(2)} />
              <Tab label="Customer Insights" {...a11yProps(3)} />
            </Tabs>
          </Box>
          
          <TabPanel value={activeTab} index={0}>
            <KpiDashboard />
                      
            <Box sx={{ mt: 3 }}>
              <TrendChart 
                metricId="sales_total" 
                title="Total Sales Trend"
              />
            </Box>
          </TabPanel>
          
          <TabPanel value={activeTab} index={1}>
            <SalesForecast />
          </TabPanel>
          
          <TabPanel value={activeTab} index={2}>
            <InventoryRecommendations />
          </TabPanel>
          
          <TabPanel value={activeTab} index={3}>
            <CustomerChurnPredictions />
          </TabPanel>
        </Box>
      </Box>
    </Box>
  );
};

export default AnalyticsDashboardPage;
