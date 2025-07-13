import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Switch,
  FormControlLabel,
  TextField,
  Button,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Stack,
  Chip,
  Card,
  CardContent,
  CardActions,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  Save as SaveIcon,
  Security as SecurityIcon,
  Build as MaintenanceIcon,
  Tune as FeaturesIcon,
  Speed as LimitsIcon,
  Notifications as NotificationsIcon,
  Warning as WarningIcon,
  Edit as EditIcon
} from '@mui/icons-material';
import { useSettings } from '../context/SettingsContext';

const SystemSettings = ({ showNotification }) => {
  const { systemSettings, loading, api } = useSettings();
  const [localSettings, setLocalSettings] = useState(null);
  const [hasChanges, setHasChanges] = useState(false);
  const [expandedPanel, setExpandedPanel] = useState('maintenance');
  const [editDialog, setEditDialog] = useState({ open: false, category: '', key: '', value: '', description: '' });

  useEffect(() => {
    if (systemSettings?.settings) {
      setLocalSettings(systemSettings.settings);
      setHasChanges(false);
    }
  }, [systemSettings]);

  const handleSettingChange = (category, key, value) => {
    if (!localSettings) return;

    const newSettings = { ...localSettings };
    newSettings[category] = { ...newSettings[category], [key]: value };

    setLocalSettings(newSettings);
    setHasChanges(true);
  };

  const handleSaveCategory = async (category) => {
    try {
      const categorySettings = localSettings[category];
      
      // Save each setting in the category individually
      for (const [key, value] of Object.entries(categorySettings)) {
        const settingKey = `${category}.${key}`;
        await api.updateSystemSetting(settingKey, value, `${category} - ${key}`, false);
      }
      
      setHasChanges(false);
      showNotification(`${category} settings saved successfully!`, 'success');
    } catch (error) {
      showNotification(`Unable to save ${category} settings. Please try again.`, 'error');
    }
  };

  const handleEditSetting = (category, key, value) => {
    setEditDialog({
      open: true,
      category,
      key,
      value,
      description: `${category} - ${key}`
    });
  };

  const handleSaveEdit = async () => {
    try {
      const settingKey = `${editDialog.category}.${editDialog.key}`;
      await api.updateSystemSetting(settingKey, editDialog.value, editDialog.description, false);
      
      // Update local settings
      const newSettings = { ...localSettings };
      newSettings[editDialog.category][editDialog.key] = editDialog.value;
      setLocalSettings(newSettings);
      
      setEditDialog({ open: false, category: '', key: '', value: '', description: '' });
      showNotification('Setting updated successfully!', 'success');
    } catch (error) {
      showNotification('Failed to update setting: ' + error.message, 'error');
    }
  };

  const handlePanelChange = (panel) => (event, isExpanded) => {
    setExpandedPanel(isExpanded ? panel : false);
  };

  if (loading || !localSettings) {
    return (
      <Box sx={{ p: 3 }}>
        <Typography>Loading system settings...</Typography>
      </Box>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box sx={{ mb: 3 }}>
        <Typography variant="h4" gutterBottom>
          System Settings
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Configure global system behavior and security settings
        </Typography>
        <Alert severity="warning" sx={{ mt: 2 }}>
          <strong>Administrator Access Required:</strong> Changes to system settings affect all users.
        </Alert>
      </Box>

      {hasChanges && (
        <Alert severity="info" sx={{ mb: 3 }}>
          You have unsaved changes. Save individual categories to apply changes.
        </Alert>
      )}

      {/* Maintenance Settings */}
      <Accordion 
        expanded={expandedPanel === 'maintenance'} 
        onChange={handlePanelChange('maintenance')}
        sx={{ mb: 2 }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <MaintenanceIcon color="primary" />
            <Box>
              <Typography variant="h6">Maintenance Mode</Typography>
              <Typography variant="body2" color="text.secondary">
                Control system maintenance and downtime
              </Typography>
            </Box>
            {localSettings.maintenance?.enabled && (
              <Chip label="Active" color="warning" size="small" />
            )}
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.maintenance?.enabled || false}
                    onChange={(e) => handleSettingChange('maintenance', 'enabled', e.target.checked)}
                  />
                }
                label="Enable Maintenance Mode"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Maintenance Message"
                multiline
                rows={3}
                value={localSettings.maintenance?.message || ''}
                onChange={(e) => handleSettingChange('maintenance', 'message', e.target.value)}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Start Time"
                type="datetime-local"
                value={localSettings.maintenance?.startTime || ''}
                onChange={(e) => handleSettingChange('maintenance', 'startTime', e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="End Time"
                type="datetime-local"
                value={localSettings.maintenance?.endTime || ''}
                onChange={(e) => handleSettingChange('maintenance', 'endTime', e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12}>
              <Button
                variant="contained"
                startIcon={<SaveIcon />}
                onClick={() => handleSaveCategory('maintenance')}
                color={localSettings.maintenance?.enabled ? 'warning' : 'primary'}
              >
                Save Maintenance Settings
              </Button>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Security Settings */}
      <Accordion 
        expanded={expandedPanel === 'security'} 
        onChange={handlePanelChange('security')}
        sx={{ mb: 2 }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <SecurityIcon color="primary" />
            <Box>
              <Typography variant="h6">Security Settings</Typography>
              <Typography variant="body2" color="text.secondary">
                Password policies and session management
              </Typography>
            </Box>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Minimum Password Length"
                type="number"
                value={localSettings.security?.passwordMinLength || 8}
                onChange={(e) => handleSettingChange('security', 'passwordMinLength', parseInt(e.target.value))}
                inputProps={{ min: 6, max: 32 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.security?.passwordRequireSpecialChars || false}
                    onChange={(e) => handleSettingChange('security', 'passwordRequireSpecialChars', e.target.checked)}
                  />
                }
                label="Require Special Characters"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Session Timeout (minutes)"
                type="number"
                value={localSettings.security?.sessionTimeoutMinutes || 60}
                onChange={(e) => handleSettingChange('security', 'sessionTimeoutMinutes', parseInt(e.target.value))}
                inputProps={{ min: 5, max: 480 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Max Login Attempts"
                type="number"
                value={localSettings.security?.maxLoginAttempts || 5}
                onChange={(e) => handleSettingChange('security', 'maxLoginAttempts', parseInt(e.target.value))}
                inputProps={{ min: 3, max: 10 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Lockout Duration (minutes)"
                type="number"
                value={localSettings.security?.lockoutDurationMinutes || 15}
                onChange={(e) => handleSettingChange('security', 'lockoutDurationMinutes', parseInt(e.target.value))}
                inputProps={{ min: 5, max: 60 }}
              />
            </Grid>
            <Grid item xs={12}>
              <Button
                variant="contained"
                startIcon={<SaveIcon />}
                onClick={() => handleSaveCategory('security')}
              >
                Save Security Settings
              </Button>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Feature Settings */}
      <Accordion 
        expanded={expandedPanel === 'features'} 
        onChange={handlePanelChange('features')}
        sx={{ mb: 2 }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <FeaturesIcon color="primary" />
            <Box>
              <Typography variant="h6">Feature Settings</Typography>
              <Typography variant="body2" color="text.secondary">
                Enable or disable system features
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
                    checked={localSettings.features?.registrationEnabled || false}
                    onChange={(e) => handleSettingChange('features', 'registrationEnabled', e.target.checked)}
                  />
                }
                label="User Registration Enabled"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.features?.passwordResetEnabled || false}
                    onChange={(e) => handleSettingChange('features', 'passwordResetEnabled', e.target.checked)}
                  />
                }
                label="Password Reset Enabled"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.features?.emailVerificationRequired || false}
                    onChange={(e) => handleSettingChange('features', 'emailVerificationRequired', e.target.checked)}
                  />
                }
                label="Email Verification Required"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.features?.twoFactorAuthRequired || false}
                    onChange={(e) => handleSettingChange('features', 'twoFactorAuthRequired', e.target.checked)}
                  />
                }
                label="Two-Factor Auth Required"
              />
            </Grid>
            <Grid item xs={12}>
              <Button
                variant="contained"
                startIcon={<SaveIcon />}
                onClick={() => handleSaveCategory('features')}
              >
                Save Feature Settings
              </Button>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* System Limits */}
      <Accordion 
        expanded={expandedPanel === 'limits'} 
        onChange={handlePanelChange('limits')}
        sx={{ mb: 2 }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <LimitsIcon color="primary" />
            <Box>
              <Typography variant="h6">System Limits</Typography>
              <Typography variant="body2" color="text.secondary">
                Resource and usage limitations
              </Typography>
            </Box>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Max File Upload Size (MB)"
                type="number"
                value={localSettings.limits?.maxFileUploadSizeMB || 10}
                onChange={(e) => handleSettingChange('limits', 'maxFileUploadSizeMB', parseInt(e.target.value))}
                inputProps={{ min: 1, max: 100 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Max Users Per Account"
                type="number"
                value={localSettings.limits?.maxUsersPerAccount || 100}
                onChange={(e) => handleSettingChange('limits', 'maxUsersPerAccount', parseInt(e.target.value))}
                inputProps={{ min: 1, max: 1000 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="API Rate Limit (requests/hour)"
                type="number"
                value={localSettings.limits?.apiRateLimit || 1000}
                onChange={(e) => handleSettingChange('limits', 'apiRateLimit', parseInt(e.target.value))}
                inputProps={{ min: 100, max: 10000 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Data Retention (days)"
                type="number"
                value={localSettings.limits?.dataRetentionDays || 365}
                onChange={(e) => handleSettingChange('limits', 'dataRetentionDays', parseInt(e.target.value))}
                inputProps={{ min: 30, max: 3650 }}
              />
            </Grid>
            <Grid item xs={12}>
              <Button
                variant="contained"
                startIcon={<SaveIcon />}
                onClick={() => handleSaveCategory('limits')}
              >
                Save Limit Settings
              </Button>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Notification Settings */}
      <Accordion 
        expanded={expandedPanel === 'notifications'} 
        onChange={handlePanelChange('notifications')}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <NotificationsIcon color="primary" />
            <Box>
              <Typography variant="h6">System Notifications</Typography>
              <Typography variant="body2" color="text.secondary">
                Configure system-wide notification settings
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
                    checked={localSettings.notifications?.emailEnabled || false}
                    onChange={(e) => handleSettingChange('notifications', 'emailEnabled', e.target.checked)}
                  />
                }
                label="Email Notifications Enabled"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings.notifications?.smsEnabled || false}
                    onChange={(e) => handleSettingChange('notifications', 'smsEnabled', e.target.checked)}
                  />
                }
                label="SMS Notifications Enabled"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Admin Email"
                type="email"
                value={localSettings.notifications?.adminEmail || ''}
                onChange={(e) => handleSettingChange('notifications', 'adminEmail', e.target.value)}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Slack Webhook URL"
                value={localSettings.notifications?.slackWebhookUrl || ''}
                onChange={(e) => handleSettingChange('notifications', 'slackWebhookUrl', e.target.value)}
                placeholder="https://hooks.slack.com/services/..."
              />
            </Grid>
            <Grid item xs={12}>
              <Button
                variant="contained"
                startIcon={<SaveIcon />}
                onClick={() => handleSaveCategory('notifications')}
              >
                Save Notification Settings
              </Button>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Edit Setting Dialog */}
      <Dialog open={editDialog.open} onClose={() => setEditDialog({ ...editDialog, open: false })} maxWidth="sm" fullWidth>
        <DialogTitle>Edit System Setting</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              fullWidth
              label="Setting Key"
              value={`${editDialog.category}.${editDialog.key}`}
              disabled
              sx={{ mb: 2 }}
            />
            <TextField
              fullWidth
              label="Value"
              value={editDialog.value}
              onChange={(e) => setEditDialog({ ...editDialog, value: e.target.value })}
              sx={{ mb: 2 }}
            />
            <TextField
              fullWidth
              label="Description"
              value={editDialog.description}
              onChange={(e) => setEditDialog({ ...editDialog, description: e.target.value })}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialog({ ...editDialog, open: false })}>
            Cancel
          </Button>
          <Button onClick={handleSaveEdit} variant="contained">
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default SystemSettings;
