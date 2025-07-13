import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Container,
  Alert,
  Button,
  Card,
  CardContent,
  Grid,
  TextField,
  Switch,
  FormControlLabel,
  Tabs,
  Tab,
  CircularProgress,
  Snackbar,
  Divider,
  Chip,
  Select,
  MenuItem,
  FormControl,
  InputLabel
} from '@mui/material';
import {
  Settings as SettingsIcon,
  Save as SaveIcon,
  Refresh as RefreshIcon,
  Palette as PaletteIcon,
  Notifications as NotificationsIcon,
  Security as SecurityIcon,
  Computer as SystemIcon
} from '@mui/icons-material';

const Settings = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [settings, setSettings] = useState({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [notification, setNotification] = useState({ open: false, message: '', severity: 'info' });
  const [apiAvailable, setApiAvailable] = useState(false);

  const API_BASE = 'http://localhost:8090/api';

  useEffect(() => {
    checkApiAndLoadSettings();
  }, []);

  const checkApiAndLoadSettings = async () => {
    try {
      const healthResponse = await fetch('http://localhost:8090/health');
      if (healthResponse.ok) {
        setApiAvailable(true);
        await loadSettings();
      } else {
        setApiAvailable(false);
      }
    } catch (error) {
      console.error('API not available:', error);
      setApiAvailable(false);
    } finally {
      setLoading(false);
    }
  };

  const loadSettings = async () => {
    try {
      const response = await fetch(`${API_BASE}/settings`);
      if (response.ok) {
        const settingsArray = await response.json();
        const settingsMap = {};
        settingsArray.forEach(setting => {
          settingsMap[setting.key] = setting;
        });
        setSettings(settingsMap);
      }
    } catch (error) {
      console.error('Error loading settings:', error);
      showNotification('Error loading settings', 'error');
    }
  };

  const updateSetting = async (key, value) => {
    try {
      const setting = settings[key];
      if (!setting) return;

      const updateData = {
        value: value.toString(),
        description: setting.description,
        category: setting.category,
        dataType: setting.dataType,
        isUserEditable: setting.isUserEditable
      };

      const response = await fetch(`${API_BASE}/settings/${key}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(updateData),
      });

      if (response.ok) {
        const updatedSetting = await response.json();
        setSettings(prev => ({
          ...prev,
          [key]: updatedSetting
        }));
        showNotification('Setting updated successfully', 'success');
      } else {
        showNotification('Error updating setting', 'error');
      }
    } catch (error) {
      console.error('Error updating setting:', error);
      showNotification('Error updating setting', 'error');
    }
  };

  const handleSettingChange = (key, value) => {
    setSettings(prev => ({
      ...prev,
      [key]: {
        ...prev[key],
        value: value.toString()
      }
    }));
  };

  const saveSetting = async (key) => {
    setSaving(true);
    await updateSetting(key, settings[key]?.value);
    setSaving(false);
  };

  const showNotification = (message, severity = 'info') => {
    setNotification({ open: true, message, severity });
  };

  const renderSettingControl = (setting) => {
    const key = setting.key;
    const value = setting.value;

    switch (setting.dataType) {
      case 'boolean':
        return (
          <FormControlLabel
            control={
              <Switch
                checked={value === 'true'}
                onChange={(e) => handleSettingChange(key, e.target.checked)}
                disabled={!setting.isUserEditable}
              />
            }
            label={setting.description || key}
          />
        );
      case 'number':
        return (
          <TextField
            fullWidth
            type="number"
            label={setting.description || key}
            value={value}
            onChange={(e) => handleSettingChange(key, e.target.value)}
            disabled={!setting.isUserEditable}
            size="small"
          />
        );
      case 'string':
        if (key === 'theme') {
          return (
            <FormControl fullWidth size="small">
              <InputLabel>{setting.description || key}</InputLabel>
              <Select
                value={value}
                label={setting.description || key}
                onChange={(e) => handleSettingChange(key, e.target.value)}
                disabled={!setting.isUserEditable}
              >
                <MenuItem value="light">Light</MenuItem>
                <MenuItem value="dark">Dark</MenuItem>
                <MenuItem value="auto">Auto</MenuItem>
              </Select>
            </FormControl>
          );
        }
        return (
          <TextField
            fullWidth
            label={setting.description || key}
            value={value}
            onChange={(e) => handleSettingChange(key, e.target.value)}
            disabled={!setting.isUserEditable}
            size="small"
          />
        );
      default:
        return (
          <TextField
            fullWidth
            label={setting.description || key}
            value={value}
            onChange={(e) => handleSettingChange(key, e.target.value)}
            disabled={!setting.isUserEditable}
            size="small"
          />
        );
    }
  };

  const getSettingsByCategory = (category) => {
    return Object.values(settings).filter(setting => setting.category === category);
  };

  const getCategoryIcon = (category) => {
    switch (category) {
      case 'Appearance': return <PaletteIcon />;
      case 'Notifications': return <NotificationsIcon />;
      case 'Security': return <SecurityIcon />;
      case 'System': return <SystemIcon />;
      default: return <SettingsIcon />;
    }
  };

  if (loading) {
    return (
      <Container maxWidth="lg">
        <Box sx={{ mt: 4, mb: 4, textAlign: 'center' }}>
          <CircularProgress />
          <Typography variant="h6" sx={{ mt: 2 }}>
            Loading Settings...
          </Typography>
        </Box>
      </Container>
    );
  }

  if (!apiAvailable) {
    return (
      <Container maxWidth="lg">
        <Box sx={{ mt: 4, mb: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
            <SettingsIcon sx={{ mr: 2 }} />
            Settings
          </Typography>
          <Alert severity="error" sx={{ mb: 3 }}>
            Settings API is not available. Please ensure the settings service is running on port 8090.
          </Alert>
          <Button 
            variant="contained" 
            startIcon={<RefreshIcon />}
            onClick={checkApiAndLoadSettings}
          >
            Retry Connection
          </Button>
        </Box>
      </Container>
    );
  }

  const categories = ['Appearance', 'Notifications', 'Security', 'System'];

  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
          <SettingsIcon sx={{ mr: 2 }} />
          Settings
        </Typography>

        <Alert severity="info" sx={{ mb: 3 }}>
          Configure system and user preferences below. Changes are saved immediately.
        </Alert>

        <Card>
          <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tabs value={activeTab} onChange={(e, newValue) => setActiveTab(newValue)}>
              {categories.map((category, index) => (
                <Tab 
                  key={category}
                  label={category}
                  icon={getCategoryIcon(category)}
                  iconPosition="start"
                />
              ))}
            </Tabs>
          </Box>

          <CardContent>
            {categories.map((category, index) => (
              <Box key={category} hidden={activeTab !== index}>
                <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                  {getCategoryIcon(category)}
                  <Box sx={{ ml: 1 }}>{category} Settings</Box>
                </Typography>

                <Grid container spacing={3}>
                  {getSettingsByCategory(category).map((setting) => (
                    <Grid item xs={12} md={6} key={setting.key}>
                      <Card variant="outlined">
                        <CardContent>
                          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                            <Typography variant="subtitle2" color="text.secondary">
                              {setting.key}
                            </Typography>
                            <Chip 
                              label={setting.isUserEditable ? "Editable" : "Read-only"} 
                              size="small" 
                              color={setting.isUserEditable ? "primary" : "default"}
                            />
                          </Box>
                          
                          {renderSettingControl(setting)}
                          
                          {setting.isUserEditable && (
                            <Box sx={{ mt: 2, textAlign: 'right' }}>
                              <Button
                                size="small"
                                startIcon={<SaveIcon />}
                                onClick={() => saveSetting(setting.key)}
                                disabled={saving}
                              >
                                Save
                              </Button>
                            </Box>
                          )}
                        </CardContent>
                      </Card>
                    </Grid>
                  ))}
                  
                  {getSettingsByCategory(category).length === 0 && (
                    <Grid item xs={12}>
                      <Box sx={{ textAlign: 'center', py: 4 }}>
                        <Typography color="text.secondary">
                          No settings available in this category.
                        </Typography>
                      </Box>
                    </Grid>
                  )}
                </Grid>
              </Box>
            ))}
          </CardContent>
        </Card>

        <Snackbar
          open={notification.open}
          autoHideDuration={6000}
          onClose={() => setNotification({ ...notification, open: false })}
          message={notification.message}
        />
      </Box>
    </Container>
  );
};

export default Settings;
