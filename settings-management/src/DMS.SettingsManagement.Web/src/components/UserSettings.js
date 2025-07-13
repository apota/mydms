import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Switch,
  FormControlLabel,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Button,
  Divider,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Chip,
  Stack,
  Card,
  CardContent,
  CardActions,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  Save as SaveIcon,
  RestoreRounded as ResetIcon,
  Person as PersonIcon,
  Notifications as NotificationsIcon,
  Security as SecurityIcon,
  Monitor as DisplayIcon,
  Settings as SettingsIcon,
  Lock as PrivacyIcon,
  Code as AdvancedIcon
} from '@mui/icons-material';
import { useSettings } from '../context/SettingsContext';

const UserSettings = ({ showNotification }) => {
  const { userSettings, loading, currentUserId, api } = useSettings();
  const [localSettings, setLocalSettings] = useState(null);
  const [hasChanges, setHasChanges] = useState(false);
  const [expandedPanel, setExpandedPanel] = useState('account');
  const [saving, setSaving] = useState(false);
  const [confirmDialog, setConfirmDialog] = useState({ open: false, action: null, title: '', message: '' });

  useEffect(() => {
    if (userSettings?.settings) {
      setLocalSettings(userSettings.settings);
      setHasChanges(false);
    }
  }, [userSettings]);

  const handleSettingChange = (category, key, value) => {
    if (!localSettings) return;

    const newSettings = { ...localSettings };
    if (category) {
      newSettings[category] = { ...newSettings[category], [key]: value };
    } else {
      newSettings[key] = value;
    }

    setLocalSettings(newSettings);
    setHasChanges(true);
  };

  const validateSettings = (settings) => {
    const errors = [];
    
    if (settings.itemsPerPage && (settings.itemsPerPage < 10 || settings.itemsPerPage > 100)) {
      errors.push('Items per page must be between 10 and 100');
    }
    
    if (settings.advanced?.apiTimeout && settings.advanced.apiTimeout < 5000) {
      errors.push('API timeout must be at least 5 seconds');
    }
    
    return errors;
  };

  const handleSaveSettings = async () => {
    if (!localSettings) return;
    
    const validationErrors = validateSettings(localSettings);
    if (validationErrors.length > 0) {
      showNotification(validationErrors.join(', '), 'error');
      return;
    }

    setSaving(true);
    try {
      await api.updateUserSettings(localSettings);
      setHasChanges(false);
      showNotification('Your settings have been saved successfully!', 'success');
    } catch (error) {
      showNotification('We encountered an issue saving your settings. Please try again in a moment.', 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleResetSettings = async () => {
    try {
      setSaving(true);
      await api.getUserSettings();
      setHasChanges(false);
      showNotification('Your settings have been reset to the last saved values', 'info');
    } catch (error) {
      showNotification('Unable to reset settings. Please refresh the page and try again.', 'error');
    } finally {
      setSaving(false);
    }
  };

  const showConfirmation = (action, title, message) => {
    setConfirmDialog({ open: true, action, title, message });
  };

  const handleConfirmedAction = async () => {
    const { action } = confirmDialog;
    setConfirmDialog({ open: false, action: null, title: '', message: '' });
    
    if (action === 'reset') {
      try {
        setSaving(true);
        await api.getUserSettings();
        setHasChanges(false);
        showNotification('Your settings have been reset to the last saved values', 'info');
      } catch (error) {
        showNotification('Unable to reset settings. Please refresh the page and try again.', 'error');
      } finally {
        setSaving(false);
      }
    }
  };

  const handlePanelChange = (panel) => (event, isExpanded) => {
    setExpandedPanel(isExpanded ? panel : false);
  };

  if (loading || !localSettings) {
    return (
      <Box sx={{ p: 3 }}>
        <Typography>Loading settings...</Typography>
      </Box>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h4" gutterBottom>
            User Settings
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Customize your experience and preferences (User ID: {currentUserId})
          </Typography>
        </Box>
        <Stack direction="row" spacing={2}>
          <Button
            variant="outlined"
            startIcon={<ResetIcon />}
            onClick={() => showConfirmation('reset', 'Reset Settings', 'Are you sure you want to reset your settings to the last saved values?')}
            disabled={!hasChanges}
          >
            Reset
          </Button>
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            onClick={handleSaveSettings}
            disabled={!hasChanges || saving}
          >
            {saving ? 'Saving...' : 'Save Changes'}
          </Button>
        </Stack>
      </Box>

      {hasChanges && (
        <Alert severity="warning" sx={{ mb: 3 }}>
          You have unsaved changes. Don't forget to save your settings!
        </Alert>
      )}

      {/* Account Settings */}
      <Accordion 
        expanded={expandedPanel === 'account'} 
        onChange={handlePanelChange('account')}
        sx={{ mb: 2 }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <PersonIcon color="primary" />
            <Box>
              <Typography variant="h6">Account Settings</Typography>
              <Typography variant="body2" color="text.secondary">
                Security and authentication preferences
              </Typography>
            </Box>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.twoFactorAuth || false}
                    onChange={(e) => handleSettingChange(null, 'twoFactorAuth', e.target.checked)}
                  />
                }
                label="Two-Factor Authentication"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.loginAlerts || false}
                    onChange={(e) => handleSettingChange(null, 'loginAlerts', e.target.checked)}
                  />
                }
                label="Login Alerts"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.sessionTimeout || false}
                    onChange={(e) => handleSettingChange(null, 'sessionTimeout', e.target.checked)}
                  />
                }
                label="Auto Session Timeout"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Language</InputLabel>
                <Select
                  value={localSettings.language || 'en'}
                  label="Language"
                  onChange={(e) => handleSettingChange(null, 'language', e.target.value)}
                >
                  <MenuItem value="en">English</MenuItem>
                  <MenuItem value="es">Spanish</MenuItem>
                  <MenuItem value="fr">French</MenuItem>
                  <MenuItem value="de">German</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Timezone</InputLabel>
                <Select
                  value={localSettings.timezone || 'UTC'}
                  label="Timezone"
                  onChange={(e) => handleSettingChange(null, 'timezone', e.target.value)}
                >
                  <MenuItem value="UTC">UTC</MenuItem>
                  <MenuItem value="America/New_York">Eastern Time</MenuItem>
                  <MenuItem value="America/Chicago">Central Time</MenuItem>
                  <MenuItem value="America/Denver">Mountain Time</MenuItem>
                  <MenuItem value="America/Los_Angeles">Pacific Time</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Display Settings */}
      <Accordion 
        expanded={expandedPanel === 'display'} 
        onChange={handlePanelChange('display')}
        sx={{ mb: 2 }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <DisplayIcon color="primary" />
            <Box>
              <Typography variant="h6">Display Settings</Typography>
              <Typography variant="body2" color="text.secondary">
                Appearance and layout preferences
              </Typography>
            </Box>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.darkMode || false}
                    onChange={(e) => handleSettingChange(null, 'darkMode', e.target.checked)}
                  />
                }
                label="Dark Mode"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.compactMode || false}
                    onChange={(e) => handleSettingChange(null, 'compactMode', e.target.checked)}
                  />
                }
                label="Compact Mode"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Theme</InputLabel>
                <Select
                  value={localSettings.theme || 'light'}
                  label="Theme"
                  onChange={(e) => handleSettingChange(null, 'theme', e.target.value)}
                >
                  <MenuItem value="light">Light</MenuItem>
                  <MenuItem value="dark">Dark</MenuItem>
                  <MenuItem value="auto">Auto</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Date Format</InputLabel>
                <Select
                  value={localSettings.dateFormat || 'MM/DD/YYYY'}
                  label="Date Format"
                  onChange={(e) => handleSettingChange(null, 'dateFormat', e.target.value)}
                >
                  <MenuItem value="MM/DD/YYYY">MM/DD/YYYY</MenuItem>
                  <MenuItem value="DD/MM/YYYY">DD/MM/YYYY</MenuItem>
                  <MenuItem value="YYYY-MM-DD">YYYY-MM-DD</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Time Format</InputLabel>
                <Select
                  value={localSettings.timeFormat || '12h'}
                  label="Time Format"
                  onChange={(e) => handleSettingChange(null, 'timeFormat', e.target.value)}
                >
                  <MenuItem value="12h">12 Hour</MenuItem>
                  <MenuItem value="24h">24 Hour</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Items Per Page"
                type="number"
                value={localSettings.itemsPerPage || 25}
                onChange={(e) => handleSettingChange(null, 'itemsPerPage', parseInt(e.target.value))}
                inputProps={{ min: 10, max: 100 }}
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Notification Settings */}
      <Accordion 
        expanded={expandedPanel === 'notifications'} 
        onChange={handlePanelChange('notifications')}
        sx={{ mb: 2 }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <NotificationsIcon color="primary" />
            <Box>
              <Typography variant="h6">Notification Settings</Typography>
              <Typography variant="body2" color="text.secondary">
                Choose how you want to be notified
              </Typography>
            </Box>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.notifications?.emailNotifications || false}
                    onChange={(e) => handleSettingChange('notifications', 'emailNotifications', e.target.checked)}
                  />
                }
                label="Email Notifications"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.notifications?.smsNotifications || false}
                    onChange={(e) => handleSettingChange('notifications', 'smsNotifications', e.target.checked)}
                  />
                }
                label="SMS Notifications"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.notifications?.browserNotifications || false}
                    onChange={(e) => handleSettingChange('notifications', 'browserNotifications', e.target.checked)}
                  />
                }
                label="Browser Notifications"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.notifications?.soundEnabled || false}
                    onChange={(e) => handleSettingChange('notifications', 'soundEnabled', e.target.checked)}
                  />
                }
                label="Sound Notifications"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.notifications?.documentUpdates || false}
                    onChange={(e) => handleSettingChange('notifications', 'documentUpdates', e.target.checked)}
                  />
                }
                label="Document Updates"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.notifications?.systemAlerts || false}
                    onChange={(e) => handleSettingChange('notifications', 'systemAlerts', e.target.checked)}
                  />
                }
                label="System Alerts"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Privacy Settings */}
      <Accordion 
        expanded={expandedPanel === 'privacy'} 
        onChange={handlePanelChange('privacy')}
        sx={{ mb: 2 }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <PrivacyIcon color="primary" />
            <Box>
              <Typography variant="h6">Privacy Settings</Typography>
              <Typography variant="body2" color="text.secondary">
                Control your privacy and data sharing
              </Typography>
            </Box>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.privacy?.profileVisible || false}
                    onChange={(e) => handleSettingChange('privacy', 'profileVisible', e.target.checked)}
                  />
                }
                label="Profile Visible to Others"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.privacy?.activityVisible || false}
                    onChange={(e) => handleSettingChange('privacy', 'activityVisible', e.target.checked)}
                  />
                }
                label="Activity Status Visible"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.privacy?.shareAnalytics || false}
                    onChange={(e) => handleSettingChange('privacy', 'shareAnalytics', e.target.checked)}
                  />
                }
                label="Share Usage Analytics"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.privacy?.allowTracking || false}
                    onChange={(e) => handleSettingChange('privacy', 'allowTracking', e.target.checked)}
                  />
                }
                label="Allow Tracking"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Application Settings */}
      <Accordion 
        expanded={expandedPanel === 'application'} 
        onChange={handlePanelChange('application')}
        sx={{ mb: 2 }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <SettingsIcon color="primary" />
            <Box>
              <Typography variant="h6">Application Settings</Typography>
              <Typography variant="body2" color="text.secondary">
                General application behavior
              </Typography>
            </Box>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.autoSave || false}
                    onChange={(e) => handleSettingChange(null, 'autoSave', e.target.checked)}
                  />
                }
                label="Auto Save"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.autoRefresh || false}
                    onChange={(e) => handleSettingChange(null, 'autoRefresh', e.target.checked)}
                  />
                }
                label="Auto Refresh"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.showTooltips || false}
                    onChange={(e) => handleSettingChange(null, 'showTooltips', e.target.checked)}
                  />
                }
                label="Show Tooltips"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Dashboard Layout</InputLabel>
                <Select
                  value={localSettings.dashboardLayout || 'grid'}
                  label="Dashboard Layout"
                  onChange={(e) => handleSettingChange(null, 'dashboardLayout', e.target.value)}
                >
                  <MenuItem value="grid">Grid</MenuItem>
                  <MenuItem value="list">List</MenuItem>
                  <MenuItem value="cards">Cards</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Advanced Settings */}
      <Accordion 
        expanded={expandedPanel === 'advanced'} 
        onChange={handlePanelChange('advanced')}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <AdvancedIcon color="primary" />
            <Box>
              <Typography variant="h6">Advanced Settings</Typography>
              <Typography variant="body2" color="text.secondary">
                Technical preferences and debugging
              </Typography>
            </Box>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.advanced?.debugMode || false}
                    onChange={(e) => handleSettingChange('advanced', 'debugMode', e.target.checked)}
                  />
                }
                label="Debug Mode"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="API Timeout (ms)"
                type="number"
                value={localSettings.advanced?.apiTimeout || 30000}
                onChange={(e) => handleSettingChange('advanced', 'apiTimeout', parseInt(e.target.value))}
                inputProps={{ min: 5000, max: 60000 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Max Retries"
                type="number"
                value={localSettings.advanced?.maxRetries || 3}
                onChange={(e) => handleSettingChange('advanced', 'maxRetries', parseInt(e.target.value))}
                inputProps={{ min: 0, max: 10 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Log Level</InputLabel>
                <Select
                  value={localSettings.advanced?.logLevel || 'info'}
                  label="Log Level"
                  onChange={(e) => handleSettingChange('advanced', 'logLevel', e.target.value)}
                >
                  <MenuItem value="error">Error</MenuItem>
                  <MenuItem value="warn">Warning</MenuItem>
                  <MenuItem value="info">Info</MenuItem>
                  <MenuItem value="debug">Debug</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Confirmation Dialog */}
      <Dialog open={confirmDialog.open} onClose={() => setConfirmDialog({ open: false, action: null, title: '', message: '' })}>
        <DialogTitle>{confirmDialog.title}</DialogTitle>
        <DialogContent>
          <Typography>{confirmDialog.message}</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmDialog({ open: false, action: null, title: '', message: '' })}>
            Cancel
          </Button>
          <Button onClick={handleConfirmedAction} variant="contained" color="primary">
            Continue
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default UserSettings;
