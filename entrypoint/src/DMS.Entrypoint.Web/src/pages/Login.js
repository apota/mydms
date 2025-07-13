import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Avatar,
  Checkbox,
  FormControlLabel,
  Divider,
  Alert,
  InputAdornment,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
  Grid,
} from '@mui/material';
import {
  LockOutlined,
  Visibility,
  VisibilityOff,
  Security,
  Google,
} from '@mui/icons-material';
import { useFormik } from 'formik';
import * as yup from 'yup';
import { useAuth } from '../contexts/AuthContext';
import QRCode from 'qrcode.react';

const validationSchema = yup.object({
  email: yup.string().email('Enter a valid email').required('Email is required'),
  password: yup.string().min(8, 'Password should be of minimum 8 characters length').required('Password is required'),
});

const demoCredentials = [
  { role: 'Manager', email: 'manager@dms-demo.com', password: 'Demo123!' },
  { role: 'Sales', email: 'sales@dms-demo.com', password: 'Demo123!' },
  { role: 'Service', email: 'service@dms-demo.com', password: 'Demo123!' },
  { role: 'Parts', email: 'parts@dms-demo.com', password: 'Demo123!' },
  { role: 'Admin', email: 'admin@dms-demo.com', password: 'Admin123!' },
];

export default function Login() {
  const [showPassword, setShowPassword] = useState(false);
  const [mfaRequired, setMfaRequired] = useState(false);
  const [mfaCode, setMfaCode] = useState('');
  const [showRegister, setShowRegister] = useState(false);
  const [captchaValue, setCaptchaValue] = useState('');
  const [captchaQuestion] = useState('7 + 3 = ?'); // Simple math captcha for demo
  const [error, setError] = useState('');
  
  const { login } = useAuth();

  const formik = useFormik({
    initialValues: {
      email: '',
      password: '',
      rememberMe: false,
    },
    validationSchema: validationSchema,
    onSubmit: async (values, { setSubmitting }) => {
      try {
        console.log('Form submission started');
        setError('');
        
        // Validate captcha (demo implementation)
        if (captchaValue !== '10') {
          console.log('Captcha validation failed:', captchaValue);
          setError('Please solve the captcha correctly');
          setSubmitting(false);
          return;
        }

        const result = await login({
          ...values,
          mfaCode: mfaRequired ? mfaCode : undefined,
          captchaToken: 'demo-captcha-token',
        });

        if (result.requiresMFA) {
          setMfaRequired(true);
        } else if (result.success) {
          console.log('Login successful, redirecting to dashboard');
          setError('');
        } else {
          setError('Login failed - unexpected response');
        }
        
        setSubmitting(false);
      } catch (err) {
        console.error('Form submission error:', err);
        setError(err.message);
        setSubmitting(false);
      }
    },
  });

  const handleDemoLogin = (credentials) => {
    formik.setFieldValue('email', credentials.email);
    formik.setFieldValue('password', credentials.password);
    setCaptchaValue('10'); // Auto-solve captcha for demo
  };

  const handleMfaSubmit = async () => {
    try {
      const result = await login({
        email: formik.values.email,
        password: formik.values.password,
        mfaCode,
        captchaToken: 'demo-captcha-token',
      });
      
      if (result.success) {
        setMfaRequired(false);
      }
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(135deg, #1976d2 0%, #42a5f5 100%)',
        padding: 2,
      }}
    >
      <Card sx={{ maxWidth: 480, width: '100%', borderRadius: 2, boxShadow: 4 }}>
        <CardContent sx={{ p: 4 }}>
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mb: 3 }}>
            <Avatar sx={{ m: 1, bgcolor: 'primary.main', width: 56, height: 56 }}>
              <LockOutlined fontSize="large" />
            </Avatar>
            <Typography component="h1" variant="h4" fontWeight="bold">
              DMS Login
            </Typography>
            <Typography variant="body2" color="text.secondary" align="center" sx={{ mt: 1 }}>
              Dealership Management System
            </Typography>
          </Box>

          {/* Demo Credentials */}
          <Box sx={{ mb: 3 }}>
            <Typography variant="subtitle2" gutterBottom>
              Demo Credentials (Click to use):
            </Typography>
            <Grid container spacing={1}>
              {demoCredentials.map((cred, index) => (
                <Grid item xs={6} sm={4} key={index}>
                  <Chip
                    label={cred.role}
                    variant="outlined"
                    size="small"
                    clickable
                    onClick={() => handleDemoLogin(cred)}
                    sx={{ width: '100%' }}
                  />
                </Grid>
              ))}
            </Grid>
          </Box>

          <Divider sx={{ mb: 3 }} />

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <form onSubmit={formik.handleSubmit}>
            <TextField
              fullWidth
              id="email"
              name="email"
              label="Email Address"
              value={formik.values.email}
              onChange={formik.handleChange}
              error={formik.touched.email && Boolean(formik.errors.email)}
              helperText={formik.touched.email && formik.errors.email}
              margin="normal"
              autoComplete="email"
            />

            <TextField
              fullWidth
              id="password"
              name="password"
              label="Password"
              type={showPassword ? 'text' : 'password'}
              value={formik.values.password}
              onChange={formik.handleChange}
              error={formik.touched.password && Boolean(formik.errors.password)}
              helperText={formik.touched.password && formik.errors.password}
              margin="normal"
              autoComplete="current-password"
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      aria-label="toggle password visibility"
                      onClick={() => setShowPassword(!showPassword)}
                      edge="end"
                    >
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />

            {/* Simple Captcha */}
            <TextField
              fullWidth
              label={`Captcha: ${captchaQuestion}`}
              value={captchaValue}
              onChange={(e) => setCaptchaValue(e.target.value)}
              margin="normal"
              placeholder="Enter the answer"
              helperText="Solve the math problem above"
            />

            <FormControlLabel
              control={
                <Checkbox
                  checked={formik.values.rememberMe}
                  onChange={formik.handleChange}
                  name="rememberMe"
                  color="primary"
                />
              }
              label="Remember me"
              sx={{ mt: 1 }}
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2, py: 1.5 }}
              disabled={formik.isSubmitting}
            >
              {formik.isSubmitting ? 'Signing In...' : 'Sign In'}
            </Button>

            <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
              <Button variant="text" size="small">
                Forgot Password?
              </Button>
              <Button 
                variant="text" 
                size="small"
                onClick={() => setShowRegister(true)}
              >
                Create Account
              </Button>
            </Box>
          </form>

          {/* SSO Options */}
          <Divider sx={{ my: 3 }}>
            <Typography variant="body2" color="text.secondary">
              OR
            </Typography>
          </Divider>

          <Button
            fullWidth
            variant="outlined"
            startIcon={<Google />}
            sx={{ mb: 1 }}
            disabled
          >
            Sign in with Google SSO (Demo)
          </Button>

          <Button
            fullWidth
            variant="outlined"
            startIcon={<Security />}
            disabled
          >
            Sign in with Azure AD (Demo)
          </Button>
        </CardContent>
      </Card>

      {/* MFA Dialog */}
      <Dialog open={mfaRequired} onClose={() => setMfaRequired(false)}>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Security />
            Multi-Factor Authentication
          </Box>
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            Enter the 6-digit code from your authenticator app, or use 123456 for demo.
          </Typography>
          <TextField
            autoFocus
            fullWidth
            label="MFA Code"
            value={mfaCode}
            onChange={(e) => setMfaCode(e.target.value)}
            inputProps={{ maxLength: 6 }}
            placeholder="123456"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setMfaRequired(false)}>Cancel</Button>
          <Button onClick={handleMfaSubmit} variant="contained">
            Verify
          </Button>
        </DialogActions>
      </Dialog>

      {/* Register Dialog */}
      <Dialog open={showRegister} onClose={() => setShowRegister(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Create Account</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            This is a demo environment. Use the demo credentials above to explore the system.
          </Typography>
          <Alert severity="info">
            Registration is disabled in demo mode. Please use one of the demo accounts.
          </Alert>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowRegister(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
