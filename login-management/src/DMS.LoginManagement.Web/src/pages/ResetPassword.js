import React, { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  TextField,
  Button,
  Typography,
  Box,
  Alert,
  Link,
  Divider
} from '@mui/material';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { useAuth } from '../contexts/AuthContext';
import { Link as RouterLink, useSearchParams, useNavigate } from 'react-router-dom';

const validationSchema = Yup.object({
  password: Yup.string()
    .min(8, 'Password must be at least 8 characters')
    .matches(/[a-z]/, 'Password must contain at least one lowercase letter')
    .matches(/[A-Z]/, 'Password must contain at least one uppercase letter')
    .matches(/[0-9]/, 'Password must contain at least one number')
    .matches(/[^a-zA-Z0-9]/, 'Password must contain at least one special character')
    .required('Password is required'),
  confirmPassword: Yup.string()
    .oneOf([Yup.ref('password'), null], 'Passwords must match')
    .required('Confirm password is required')
});

const ResetPassword = () => {
  const { resetPassword } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [success, setSuccess] = useState(false);
  const [serverError, setServerError] = useState('');
  const [token, setToken] = useState('');

  useEffect(() => {
    const resetToken = searchParams.get('token');
    if (!resetToken) {
      setServerError('Invalid or missing reset token. Please request a new password reset.');
    } else {
      setToken(resetToken);
    }
  }, [searchParams]);

  const handleSubmit = async (values, { setSubmitting }) => {
    try {
      setServerError('');
      await resetPassword(token, values.password);
      setSuccess(true);
    } catch (error) {
      setServerError(error.message || 'Failed to reset password. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  if (success) {
    return (
      <Container component="main" maxWidth="sm">
        <Box
          sx={{
            marginTop: 8,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
          }}
        >
          <Paper elevation={3} sx={{ padding: 4, width: '100%' }}>
            <Box sx={{ textAlign: 'center', mb: 3 }}>
              <Typography component="h1" variant="h4" gutterBottom color="primary">
                Password Reset Successfully
              </Typography>
              <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
                Your password has been reset successfully. You can now sign in with your new password.
              </Typography>
            </Box>

            <Box sx={{ textAlign: 'center' }}>
              <Button
                component={RouterLink}
                to="/login"
                variant="contained"
                size="large"
              >
                Go to Login
              </Button>
            </Box>
          </Paper>
        </Box>
      </Container>
    );
  }

  if (!token) {
    return (
      <Container component="main" maxWidth="sm">
        <Box
          sx={{
            marginTop: 8,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
          }}
        >
          <Paper elevation={3} sx={{ padding: 4, width: '100%' }}>
            <Alert severity="error" sx={{ mb: 2 }}>
              Invalid or missing reset token. Please request a new password reset.
            </Alert>
            
            <Box sx={{ textAlign: 'center' }}>
              <Button
                component={RouterLink}
                to="/forgot-password"
                variant="contained"
                sx={{ mr: 2 }}
              >
                Request New Reset
              </Button>
              <Button
                component={RouterLink}
                to="/login"
                variant="outlined"
              >
                Back to Login
              </Button>
            </Box>
          </Paper>
        </Box>
      </Container>
    );
  }

  return (
    <Container component="main" maxWidth="sm">
      <Box
        sx={{
          marginTop: 8,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <Paper elevation={3} sx={{ padding: 4, width: '100%' }}>
          <Box sx={{ textAlign: 'center', mb: 3 }}>
            <Typography component="h1" variant="h4" gutterBottom>
              Reset Password
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Enter your new password below
            </Typography>
          </Box>

          {serverError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {serverError}
            </Alert>
          )}

          <Formik
            initialValues={{
              password: '',
              confirmPassword: ''
            }}
            validationSchema={validationSchema}
            onSubmit={handleSubmit}
          >
            {({ errors, touched, isSubmitting }) => (
              <Form>
                <Box sx={{ mb: 2 }}>
                  <Field
                    as={TextField}
                    name="password"
                    label="New Password"
                    type="password"
                    fullWidth
                    variant="outlined"
                    error={touched.password && !!errors.password}
                    helperText={touched.password && errors.password}
                  />
                </Box>

                <Box sx={{ mb: 3 }}>
                  <Field
                    as={TextField}
                    name="confirmPassword"
                    label="Confirm New Password"
                    type="password"
                    fullWidth
                    variant="outlined"
                    error={touched.confirmPassword && !!errors.confirmPassword}
                    helperText={touched.confirmPassword && errors.confirmPassword}
                  />
                </Box>

                <Button
                  type="submit"
                  fullWidth
                  variant="contained"
                  size="large"
                  disabled={isSubmitting}
                  sx={{ mb: 2 }}
                >
                  {isSubmitting ? 'Resetting...' : 'Reset Password'}
                </Button>
              </Form>
            )}
          </Formik>

          <Divider sx={{ my: 2 }} />

          <Box sx={{ textAlign: 'center' }}>
            <Typography variant="body2">
              Remember your password?{' '}
              <Link component={RouterLink} to="/login" underline="hover">
                Back to Login
              </Link>
            </Typography>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
};

export default ResetPassword;
