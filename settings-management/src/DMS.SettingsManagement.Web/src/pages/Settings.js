import React, { useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  Switch,
  FormControlLabel,
  Divider,
  Button,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Alert,
  CircularProgress,
  Snackbar,
  Tabs,
  Tab,
  TextField,
  Slider,
  Chip,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton
} from '@mui/material';
import {
  Settings as SettingsIcon,
  Save as SaveIcon,
  Refresh as RefreshIcon,
  Notifications as NotificationsIcon,
  Security as SecurityIcon,
  Palette as PaletteIcon,
  Language as LanguageIcon,
  ExpandMore as ExpandMoreIcon,
  Delete as DeleteIcon,
  Restore as RestoreIcon
} from '@mui/icons-material';
import { useSettings } from '../context/SettingsContext';

function TabPanel({ children, value, index, ...other }) {
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`settings-tabpanel-${index}`}
      aria-labelledby={`settings-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

export default function Settings() {
  const {
    settings,
    loading,
    saving,
    error,
    updateSetting,
    resetSettings,
    deleteSettings
  } = useSettings();

  const [showSuccess, setShowSuccess] = useState(false);
  const [showError, setShowError] = useState(false);
  const [tabValue, setTabValue] = useState(0);
  const [showResetDialog, setShowResetDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);

  const handleSwitchChange = async (key, value) => {
    try {
      await updateSetting(key, value);
      setShowSuccess(true);
    } catch (error) {
      setShowError(true);
    }
  };

  const handleSelectChange = async (key, value) => {
    try {
      await updateSetting(key, value);
      setShowSuccess(true);
    } catch (error) {
      setShowError(true);
    }
  };

  const handleSliderChange = async (key, value) => {
    try {
      await updateSetting(key, value);
      setShowSuccess(true);
    } catch (error) {
      setShowError(true);
    }
  };

  const handleReset = async () => {
    try {
      await resetSettings();
      setShowSuccess(true);
      setShowResetDialog(false);
    } catch (error) {
      setShowError(true);
    }
  };

  const handleDelete = async () => {
    try {
      await deleteSettings();
      setShowSuccess(true);
      setShowDeleteDialog(false);
    } catch (error) {
      setShowError(true);
    }
  };

  const handleTabChange = (event, newValue) => {
    setTabValue(newValue);
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" fontWeight="bold">
          Settings Management
        </Typography>
        <Box>
          <Button
            variant="outlined"
            startIcon={<RestoreIcon />}
            onClick={() => setShowResetDialog(true)}
            disabled={saving}
            sx={{ mr: 2 }}
          >
            Reset to Defaults
          </Button>
          <Button
            variant="outlined"
            color="error"
            startIcon={<DeleteIcon />}
            onClick={() => setShowDeleteDialog(true)}
            disabled={saving}
            sx={{ mr: 2 }}
          >
            Delete All Settings
          </Button>
          {saving && <CircularProgress size={24} />}
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs value={tabValue} onChange={handleTabChange} aria-label="settings tabs">
          <Tab icon={<NotificationsIcon />} label="Notifications" />
          <Tab icon={<SecurityIcon />} label="Security" />
          <Tab icon={<PaletteIcon />} label="Display" />
          <Tab icon={<LanguageIcon />} label="Regional" />
        </Tabs>
      </Box>

      <TabPanel value={tabValue} index={0}>
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Notification Preferences
                </Typography>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.emailNotifications}
                      onChange={(e) => handleSwitchChange('emailNotifications', e.target.checked)}
                    />
                  }
                  label="Email Notifications"
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.smsNotifications}
                      onChange={(e) => handleSwitchChange('smsNotifications', e.target.checked)}
                    />
                  }
                  label="SMS Notifications"
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.loginAlerts}
                      onChange={(e) => handleSwitchChange('loginAlerts', e.target.checked)}
                    />
                  }
                  label="Login Alerts"
                />
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Advanced Notifications
                </Typography>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.notifications?.email}
                      onChange={(e) => handleSwitchChange('notifications', {
                        ...settings.notifications,
                        email: e.target.checked
                      })}
                    />
                  }
                  label="Email Alerts"
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.notifications?.browser}
                      onChange={(e) => handleSwitchChange('notifications', {
                        ...settings.notifications,
                        browser: e.target.checked
                      })}
                    />
                  }
                  label="Browser Notifications"
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.notifications?.mobile}
                      onChange={(e) => handleSwitchChange('notifications', {
                        ...settings.notifications,
                        mobile: e.target.checked
                      })}
                    />
                  }
                  label="Mobile Push Notifications"
                />
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </TabPanel>

      <TabPanel value={tabValue} index={1}>
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Authentication & Security
                </Typography>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.twoFactorAuth}
                      onChange={(e) => handleSwitchChange('twoFactorAuth', e.target.checked)}
                    />
                  }
                  label="Two-Factor Authentication"
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.sessionTimeout}
                      onChange={(e) => handleSwitchChange('sessionTimeout', e.target.checked)}
                    />
                  }
                  label="Automatic Session Timeout"
                />
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Privacy Settings
                </Typography>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.privacy?.profileVisible}
                      onChange={(e) => handleSwitchChange('privacy', {
                        ...settings.privacy,
                        profileVisible: e.target.checked
                      })}
                    />
                  }
                  label="Profile Visible to Others"
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.privacy?.activityVisible}
                      onChange={(e) => handleSwitchChange('privacy', {
                        ...settings.privacy,
                        activityVisible: e.target.checked
                      })}
                    />
                  }
                  label="Activity Visible to Others"
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.privacy?.statusVisible}
                      onChange={(e) => handleSwitchChange('privacy', {
                        ...settings.privacy,
                        statusVisible: e.target.checked
                      })}
                    />
                  }
                  label="Online Status Visible"
                />
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </TabPanel>

      <TabPanel value={tabValue} index={2}>
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Theme & Appearance
                </Typography>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.darkMode}
                      onChange={(e) => handleSwitchChange('darkMode', e.target.checked)}
                    />
                  }
                  label="Dark Mode"
                />

                <FormControl fullWidth margin="normal">
                  <InputLabel>Theme</InputLabel>
                  <Select
                    value={settings.theme || 'light'}
                    label="Theme"
                    onChange={(e) => handleSelectChange('theme', e.target.value)}
                  >
                    <MenuItem value="light">Light</MenuItem>
                    <MenuItem value="dark">Dark</MenuItem>
                    <MenuItem value="auto">Auto (System)</MenuItem>
                    <MenuItem value="custom">Custom</MenuItem>
                  </Select>
                </FormControl>

                <FormControl fullWidth margin="normal">
                  <InputLabel>Dashboard Layout</InputLabel>
                  <Select
                    value={settings.dashboardLayout || 'grid'}
                    label="Dashboard Layout"
                    onChange={(e) => handleSelectChange('dashboardLayout', e.target.value)}
                  >
                    <MenuItem value="grid">Grid View</MenuItem>
                    <MenuItem value="list">List View</MenuItem>
                    <MenuItem value="compact">Compact View</MenuItem>
                  </Select>
                </FormControl>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Display Preferences
                </Typography>
                
                <Typography gutterBottom>Items per Page</Typography>
                <Slider
                  value={settings.itemsPerPage || 25}
                  onChange={(e, value) => handleSliderChange('itemsPerPage', value)}
                  min={10}
                  max={100}
                  step={5}
                  marks={[
                    { value: 10, label: '10' },
                    { value: 25, label: '25' },
                    { value: 50, label: '50' },
                    { value: 100, label: '100' }
                  ]}
                  valueLabelDisplay="auto"
                />

                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.autoSave}
                      onChange={(e) => handleSwitchChange('autoSave', e.target.checked)}
                    />
                  }
                  label="Auto-save Changes"
                />
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </TabPanel>

      <TabPanel value={tabValue} index={3}>
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Language & Localization
                </Typography>
                
                <FormControl fullWidth margin="normal">
                  <InputLabel>Language</InputLabel>
                  <Select
                    value={settings.language}
                    label="Language"
                    onChange={(e) => handleSelectChange('language', e.target.value)}
                  >
                    <MenuItem value="en">English</MenuItem>
                    <MenuItem value="es">Spanish</MenuItem>
                    <MenuItem value="fr">French</MenuItem>
                    <MenuItem value="de">German</MenuItem>
                    <MenuItem value="it">Italian</MenuItem>
                    <MenuItem value="pt">Portuguese</MenuItem>
                    <MenuItem value="zh">Chinese</MenuItem>
                    <MenuItem value="ja">Japanese</MenuItem>
                  </Select>
                </FormControl>

                <FormControl fullWidth margin="normal">
                  <InputLabel>Timezone</InputLabel>
                  <Select
                    value={settings.timezone}
                    label="Timezone"
                    onChange={(e) => handleSelectChange('timezone', e.target.value)}
                  >
                    <MenuItem value="UTC">UTC</MenuItem>
                    <MenuItem value="America/New_York">Eastern Time (ET)</MenuItem>
                    <MenuItem value="America/Chicago">Central Time (CT)</MenuItem>
                    <MenuItem value="America/Denver">Mountain Time (MT)</MenuItem>
                    <MenuItem value="America/Los_Angeles">Pacific Time (PT)</MenuItem>
                    <MenuItem value="Europe/London">London</MenuItem>
                    <MenuItem value="Europe/Paris">Paris</MenuItem>
                    <MenuItem value="Asia/Tokyo">Tokyo</MenuItem>
                    <MenuItem value="Asia/Shanghai">Shanghai</MenuItem>
                  </Select>
                </FormControl>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Format Preferences
                </Typography>
                
                <FormControl fullWidth margin="normal">
                  <InputLabel>Date Format</InputLabel>
                  <Select
                    value={settings.dateFormat}
                    label="Date Format"
                    onChange={(e) => handleSelectChange('dateFormat', e.target.value)}
                  >
                    <MenuItem value="MM/DD/YYYY">MM/DD/YYYY (US)</MenuItem>
                    <MenuItem value="DD/MM/YYYY">DD/MM/YYYY (EU)</MenuItem>
                    <MenuItem value="YYYY-MM-DD">YYYY-MM-DD (ISO)</MenuItem>
                    <MenuItem value="DD-MMM-YYYY">DD-MMM-YYYY</MenuItem>
                  </Select>
                </FormControl>

                <FormControl fullWidth margin="normal">
                  <InputLabel>Currency</InputLabel>
                  <Select
                    value={settings.currency}
                    label="Currency"
                    onChange={(e) => handleSelectChange('currency', e.target.value)}
                  >
                    <MenuItem value="USD">US Dollar (USD)</MenuItem>
                    <MenuItem value="EUR">Euro (EUR)</MenuItem>
                    <MenuItem value="GBP">British Pound (GBP)</MenuItem>
                    <MenuItem value="CAD">Canadian Dollar (CAD)</MenuItem>
                    <MenuItem value="JPY">Japanese Yen (JPY)</MenuItem>
                    <MenuItem value="AUD">Australian Dollar (AUD)</MenuItem>
                    <MenuItem value="CHF">Swiss Franc (CHF)</MenuItem>
                  </Select>
                </FormControl>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </TabPanel>

      {/* Reset Confirmation Dialog */}
      <Dialog open={showResetDialog} onClose={() => setShowResetDialog(false)}>
        <DialogTitle>Reset Settings to Defaults</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to reset all settings to their default values? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowResetDialog(false)}>Cancel</Button>
          <Button onClick={handleReset} color="primary" variant="contained">
            Reset Settings
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={showDeleteDialog} onClose={() => setShowDeleteDialog(false)}>
        <DialogTitle>Delete All Settings</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete all your settings? This will remove all customizations and cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowDeleteDialog(false)}>Cancel</Button>
          <Button onClick={handleDelete} color="error" variant="contained">
            Delete Settings
          </Button>
        </DialogActions>
      </Dialog>

      {/* Success Snackbar */}
      <Snackbar
        open={showSuccess}
        autoHideDuration={3000}
        onClose={() => setShowSuccess(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert onClose={() => setShowSuccess(false)} severity="success">
          Settings updated successfully!
        </Alert>
      </Snackbar>

      {/* Error Snackbar */}
      <Snackbar
        open={showError}
        autoHideDuration={5000}
        onClose={() => setShowError(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert onClose={() => setShowError(false)} severity="error">
          Failed to update settings. Please try again.
        </Alert>
      </Snackbar>
    </Box>
  );
}
