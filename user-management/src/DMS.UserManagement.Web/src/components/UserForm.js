import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
} from '@mui/material';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { useCreateUser, useUpdateUser } from '../hooks/useUsers';

const UserForm = ({ open, onClose, user }) => {
  const isEdit = !!user;
  const createUserMutation = useCreateUser();
  const updateUserMutation = useUpdateUser();

  const initialValues = {
    firstName: user?.firstName || '',
    lastName: user?.lastName || '',
    email: user?.email || '',
    password: '',
    role: user?.role || 'user',
    status: user?.status || 'active',
    phone: user?.phone || '',
    department: user?.department || '',
  };

  const validationSchema = Yup.object({
    firstName: Yup.string().required('First name is required').max(100),
    lastName: Yup.string().required('Last name is required').max(100),
    email: Yup.string().email('Invalid email').required('Email is required').max(255),
    password: isEdit 
      ? Yup.string().min(8, 'Password must be at least 8 characters')
      : Yup.string().required('Password is required').min(8, 'Password must be at least 8 characters'),
    role: Yup.string().required('Role is required'),
    phone: Yup.string().max(255),
    department: Yup.string().max(255),
  });

  const handleSubmit = async (values, { setSubmitting }) => {
    try {
      const userData = { ...values };
      
      // If editing and password is empty, don't include it
      if (isEdit && !userData.password) {
        delete userData.password;
      }

      if (isEdit) {
        await updateUserMutation.mutateAsync({ id: user.id, data: userData });
      } else {
        await createUserMutation.mutateAsync(userData);
      }
      
      onClose();
    } catch (error) {
      // Error handling is done in the mutation hooks
    } finally {
      setSubmitting(false);
    }
  };

  const roles = [
    { value: 'admin', label: 'Administrator' },
    { value: 'manager', label: 'Manager' },
    { value: 'sales', label: 'Sales' },
    { value: 'service', label: 'Service' },
    { value: 'parts', label: 'Parts' },
    { value: 'user', label: 'User' },
  ];

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        {isEdit ? 'Edit User' : 'Add New User'}
      </DialogTitle>
      
      <Formik
        initialValues={initialValues}
        validationSchema={validationSchema}
        onSubmit={handleSubmit}
        enableReinitialize
      >
        {({ values, errors, touched, handleChange, handleBlur, isSubmitting }) => (
          <Form>
            <DialogContent>
              <Grid container spacing={3}>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    name="firstName"
                    label="First Name"
                    value={values.firstName}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.firstName && !!errors.firstName}
                    helperText={touched.firstName && errors.firstName}
                    required
                  />
                </Grid>
                
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    name="lastName"
                    label="Last Name"
                    value={values.lastName}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.lastName && !!errors.lastName}
                    helperText={touched.lastName && errors.lastName}
                    required
                  />
                </Grid>
                
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    name="email"
                    label="Email"
                    type="email"
                    value={values.email}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.email && !!errors.email}
                    helperText={touched.email && errors.email}
                    required
                  />
                </Grid>
                
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    name="password"
                    label="Password"
                    type="password"
                    value={values.password}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.password && !!errors.password}
                    helperText={
                      touched.password && errors.password || 
                      (isEdit ? 'Leave blank to keep current password' : '')
                    }
                    required={!isEdit}
                  />
                </Grid>
                
                <Grid item xs={12} sm={6}>
                  <FormControl fullWidth required>
                    <InputLabel>Role</InputLabel>
                    <Select
                      name="role"
                      value={values.role}
                      onChange={handleChange}
                      onBlur={handleBlur}
                      error={touched.role && !!errors.role}
                      label="Role"
                    >
                      {roles.map(role => (
                        <MenuItem key={role.value} value={role.value}>
                          {role.label}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
                
                <Grid item xs={12} sm={6}>
                  <FormControl fullWidth>
                    <InputLabel>Status</InputLabel>
                    <Select
                      name="status"
                      value={values.status}
                      onChange={handleChange}
                      onBlur={handleBlur}
                      label="Status"
                    >
                      <MenuItem value="active">Active</MenuItem>
                      <MenuItem value="inactive">Inactive</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    name="phone"
                    label="Phone"
                    value={values.phone}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.phone && !!errors.phone}
                    helperText={touched.phone && errors.phone}
                  />
                </Grid>
                
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    name="department"
                    label="Department"
                    value={values.department}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.department && !!errors.department}
                    helperText={touched.department && errors.department}
                  />
                </Grid>
              </Grid>
            </DialogContent>
            
            <DialogActions>
              <Button onClick={onClose}>Cancel</Button>
              <Button 
                type="submit" 
                variant="contained"
                disabled={isSubmitting}
              >
                {isSubmitting ? (
                  <CircularProgress size={20} />
                ) : (
                  isEdit ? 'Update' : 'Create'
                )}
              </Button>
            </DialogActions>
          </Form>
        )}
      </Formik>
    </Dialog>
  );
};

export default UserForm;
