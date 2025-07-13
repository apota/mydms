import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Button,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  Card,
  CardContent,
  CardActions,
  Chip,
  IconButton,
  Tooltip,
  Stack,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';
import { DataGrid } from '@mui/x-data-grid';
import {
  Person as PersonIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
  Search as SearchIcon,
  Refresh as RefreshIcon,
  Settings as SettingsIcon,
  Visibility as ViewIcon
} from '@mui/icons-material';
import { useSettings } from '../context/SettingsContext';

const UsersManagement = ({ showNotification }) => {
  const { users, loading, api, actions } = useSettings();
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState('all');
  const [selectedUser, setSelectedUser] = useState(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogType, setDialogType] = useState('view'); // 'view', 'edit', 'delete'
  const [userSettings, setUserSettings] = useState(null);
  const [loadingUserSettings, setLoadingUserSettings] = useState(false);

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      await api.getUsers();
    } catch (error) {
      showNotification('Unable to load users. Please refresh the page.', 'error');
    }
  };

  const handleViewUser = async (userId) => {
    setLoadingUserSettings(true);
    try {
      const response = await api.getUserSettings(userId);
      setUserSettings(response.settings);
      setSelectedUser({ user_id: userId });
      setDialogType('view');
      setDialogOpen(true);
    } catch (error) {
      showNotification('Unable to load user settings. Please try again.', 'error');
    } finally {
      setLoadingUserSettings(false);
    }
  };

  const handleEditUser = async (userId) => {
    setLoadingUserSettings(true);
    try {
      const response = await api.getUserSettings(userId);
      setUserSettings(response.settings);
      setSelectedUser({ user_id: userId });
      setDialogType('edit');
      setDialogOpen(true);
    } catch (error) {
      showNotification('Unable to load user settings. Please try again.', 'error');
    } finally {
      setLoadingUserSettings(false);
    }
  };

  const handleDeleteUser = (userId) => {
    setSelectedUser({ user_id: userId });
    setDialogType('delete');
    setDialogOpen(true);
  };

  const handleSwitchToUser = (userId) => {
    actions.setCurrentUser(userId);
    showNotification(`Switched to user ${userId}`, 'info');
  };

  const confirmDeleteUser = async () => {
    try {
      await api.deleteUserSettings(selectedUser.user_id);
      showNotification('User settings deleted successfully', 'success');
      setDialogOpen(false);
      loadUsers();
    } catch (error) {
      showNotification('Failed to delete user settings: ' + error.message, 'error');
    }
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setSelectedUser(null);
    setUserSettings(null);
  };

  // Filter users based on search term
  const filteredUsers = users.filter(user => {
    const matchesSearch = user.user_id.toString().includes(searchTerm);
    return matchesSearch;
  });

  // DataGrid columns
  const columns = [
    {
      field: 'user_id',
      headerName: 'User ID',
      width: 120,
      renderCell: (params) => (
        <Chip 
          label={params.value} 
          variant="outlined" 
          color="primary" 
          size="small"
        />
      )
    },
    {
      field: 'created_at',
      headerName: 'Created',
      width: 180,
      renderCell: (params) => (
        new Date(params.value).toLocaleDateString()
      )
    },
    {
      field: 'updated_at',
      headerName: 'Last Updated',
      width: 180,
      renderCell: (params) => (
        new Date(params.value).toLocaleDateString()
      )
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      renderCell: (params) => {
        const isRecent = new Date(params.row.updated_at) > new Date(Date.now() - 7 * 24 * 60 * 60 * 1000);
        return (
          <Chip 
            label={isRecent ? 'Active' : 'Inactive'} 
            color={isRecent ? 'success' : 'default'}
            size="small"
          />
        );
      }
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 200,
      sortable: false,
      renderCell: (params) => (
        <Stack direction="row" spacing={1}>
          <Tooltip title="View Settings">
            <IconButton 
              size="small" 
              onClick={() => handleViewUser(params.row.user_id)}
              disabled={loadingUserSettings}
            >
              <ViewIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Edit Settings">
            <IconButton 
              size="small" 
              onClick={() => handleEditUser(params.row.user_id)}
              disabled={loadingUserSettings}
            >
              <EditIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Switch to User">
            <IconButton 
              size="small" 
              onClick={() => handleSwitchToUser(params.row.user_id)}
            >
              <PersonIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete Settings">
            <IconButton 
              size="small" 
              color="error"
              onClick={() => handleDeleteUser(params.row.user_id)}
            >
              <DeleteIcon />
            </IconButton>
          </Tooltip>
        </Stack>
      )
    }
  ];

  return (
    <Box>
      {/* Header */}
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h4" gutterBottom>
            Users Management
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Manage user settings and access control
          </Typography>
        </Box>
        <Button
          variant="outlined"
          startIcon={<RefreshIcon />}
          onClick={loadUsers}
          disabled={loading}
        >
          Refresh
        </Button>
      </Box>

      {/* Filters */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} md={6}>
            <TextField
              fullWidth
              placeholder="Search by User ID..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon />
                  </InputAdornment>
                ),
              }}
            />
          </Grid>
          <Grid item xs={12} md={3}>
            <FormControl fullWidth>
              <InputLabel>Status Filter</InputLabel>
              <Select
                value={filterStatus}
                label="Status Filter"
                onChange={(e) => setFilterStatus(e.target.value)}
              >
                <MenuItem value="all">All Users</MenuItem>
                <MenuItem value="active">Active</MenuItem>
                <MenuItem value="inactive">Inactive</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={3}>
            <Typography variant="body2" color="text.secondary">
              {filteredUsers.length} user(s) found
            </Typography>
          </Grid>
        </Grid>
      </Paper>

      {/* Data Grid */}
      <Paper sx={{ height: 600, width: '100%' }}>
        <DataGrid
          rows={filteredUsers}
          columns={columns}
          getRowId={(row) => row.user_id}
          initialState={{
            pagination: {
              paginationModel: { page: 0, pageSize: 10 },
            },
          }}
          pageSizeOptions={[5, 10, 25]}
          loading={loading}
          disableRowSelectionOnClick
        />
      </Paper>

      {/* View/Edit User Dialog */}
      <Dialog 
        open={dialogOpen && (dialogType === 'view' || dialogType === 'edit')} 
        onClose={handleCloseDialog}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <SettingsIcon />
            {dialogType === 'view' ? 'View' : 'Edit'} User Settings - ID: {selectedUser?.user_id}
          </Box>
        </DialogTitle>
        <DialogContent>
          {loadingUserSettings ? (
            <Box sx={{ p: 3, textAlign: 'center' }}>
              <Typography>Loading user settings...</Typography>
            </Box>
          ) : userSettings ? (
            <Grid container spacing={2} sx={{ mt: 1 }}>
              {/* Account Settings */}
              <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>Account Settings</Typography>
              </Grid>
              <Grid item xs={6}>
                <TextField
                  fullWidth
                  label="Language"
                  value={userSettings.language || 'en'}
                  disabled={dialogType === 'view'}
                  size="small"
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  fullWidth
                  label="Timezone"
                  value={userSettings.timezone || 'UTC'}
                  disabled={dialogType === 'view'}
                  size="small"
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  fullWidth
                  label="Theme"
                  value={userSettings.theme || 'light'}
                  disabled={dialogType === 'view'}
                  size="small"
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  fullWidth
                  label="Date Format"
                  value={userSettings.dateFormat || 'MM/DD/YYYY'}
                  disabled={dialogType === 'view'}
                  size="small"
                />
              </Grid>

              {/* Security Settings */}
              <Grid item xs={12} sx={{ mt: 2 }}>
                <Typography variant="h6" gutterBottom>Security Settings</Typography>
              </Grid>
              <Grid item xs={6}>
                <Stack direction="row" spacing={1} alignItems="center">
                  <Typography>Two-Factor Auth:</Typography>
                  <Chip 
                    label={userSettings.twoFactorAuth ? 'Enabled' : 'Disabled'}
                    color={userSettings.twoFactorAuth ? 'success' : 'default'}
                    size="small"
                  />
                </Stack>
              </Grid>
              <Grid item xs={6}>
                <Stack direction="row" spacing={1} alignItems="center">
                  <Typography>Login Alerts:</Typography>
                  <Chip 
                    label={userSettings.loginAlerts ? 'Enabled' : 'Disabled'}
                    color={userSettings.loginAlerts ? 'success' : 'default'}
                    size="small"
                  />
                </Stack>
              </Grid>

              {/* Notification Settings */}
              <Grid item xs={12} sx={{ mt: 2 }}>
                <Typography variant="h6" gutterBottom>Notifications</Typography>
              </Grid>
              <Grid item xs={6}>
                <Stack direction="row" spacing={1} alignItems="center">
                  <Typography>Email:</Typography>
                  <Chip 
                    label={userSettings.notifications?.emailNotifications ? 'On' : 'Off'}
                    color={userSettings.notifications?.emailNotifications ? 'success' : 'default'}
                    size="small"
                  />
                </Stack>
              </Grid>
              <Grid item xs={6}>
                <Stack direction="row" spacing={1} alignItems="center">
                  <Typography>SMS:</Typography>
                  <Chip 
                    label={userSettings.notifications?.smsNotifications ? 'On' : 'Off'}
                    color={userSettings.notifications?.smsNotifications ? 'success' : 'default'}
                    size="small"
                  />
                </Stack>
              </Grid>

              {/* Privacy Settings */}
              <Grid item xs={12} sx={{ mt: 2 }}>
                <Typography variant="h6" gutterBottom>Privacy</Typography>
              </Grid>
              <Grid item xs={6}>
                <Stack direction="row" spacing={1} alignItems="center">
                  <Typography>Profile Visible:</Typography>
                  <Chip 
                    label={userSettings.privacy?.profileVisible ? 'Yes' : 'No'}
                    color={userSettings.privacy?.profileVisible ? 'success' : 'default'}
                    size="small"
                  />
                </Stack>
              </Grid>
              <Grid item xs={6}>
                <Stack direction="row" spacing={1} alignItems="center">
                  <Typography>Activity Visible:</Typography>
                  <Chip 
                    label={userSettings.privacy?.activityVisible ? 'Yes' : 'No'}
                    color={userSettings.privacy?.activityVisible ? 'success' : 'default'}
                    size="small"
                  />
                </Stack>
              </Grid>
            </Grid>
          ) : (
            <Typography>No settings found for this user.</Typography>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>
            {dialogType === 'view' ? 'Close' : 'Cancel'}
          </Button>
          {dialogType === 'edit' && (
            <Button variant="contained" onClick={handleCloseDialog}>
              Save Changes
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog 
        open={dialogOpen && dialogType === 'delete'} 
        onClose={handleCloseDialog}
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <DeleteIcon color="error" />
            Confirm Deletion
          </Box>
        </DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            This action cannot be undone!
          </Alert>
          <Typography>
            Are you sure you want to delete all settings for User ID: <strong>{selectedUser?.user_id}</strong>?
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            The user will be reset to default settings on their next login.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>
            Cancel
          </Button>
          <Button 
            variant="contained" 
            color="error" 
            onClick={confirmDeleteUser}
          >
            Delete Settings
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default UsersManagement;
