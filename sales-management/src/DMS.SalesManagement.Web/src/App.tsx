// @ts-nocheck
import React, { Suspense } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';
import Layout from './components/layout/Layout';
import ProtectedRoute from './components/auth/ProtectedRoute';
import { AuthProvider } from './contexts/AuthContext';
import { NotificationProvider } from './contexts/NotificationContext';

// Lazy load components for better performance
const Dashboard = React.lazy(() => import('./pages/Dashboard'));
const Leads = React.lazy(() => import('./pages/leads/Leads'));
const LeadDetail = React.lazy(() => import('./pages/leads/LeadDetail'));
const CreateLead = React.lazy(() => import('./pages/leads/CreateLead'));
const Deals = React.lazy(() => import('./pages/deals/Deals'));
const DealDetail = React.lazy(() => import('./pages/deals/DealDetail'));
const DealCreate = React.lazy(() => import('./pages/deals/DealCreate'));
const DmvRegistration = React.lazy(() => import('./pages/deals/DmvRegistration'));
const Documents = React.lazy(() => import('./pages/documents/Documents'));
const Commissions = React.lazy(() => import('./pages/commissions/Commissions'));
const Login = React.lazy(() => import('./pages/Login'));
const NotFound = React.lazy(() => import('./pages/NotFound'));
const Unauthorized = React.lazy(() => import('./pages/Unauthorized'));

// Loading component
const Loading = () => (
  <Box 
    display="flex" 
    justifyContent="center" 
    alignItems="center" 
    height="calc(100vh - 64px)"
  >
    <CircularProgress />
  </Box>
);

function App() {
  return (
    <AuthProvider>
      <NotificationProvider>
        <Suspense fallback={<Loading />}>
          <Routes>
          {/* Public routes */}
          <Route path="/login" element={<Login />} />
          <Route path="/unauthorized" element={<Unauthorized />} />
          
          {/* Protected routes */}
          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/dashboard" element={<Dashboard />} />
                {/* Leads routes */}
              <Route path="/leads" element={<Leads />} />
              <Route path="/leads/create" element={<CreateLead />} />
              <Route path="/leads/:id" element={<LeadDetail />} />
                {/* Deals routes */}
              <Route path="/deals" element={<Deals />} />
              <Route path="/deals/create" element={<DealCreate />} />
              <Route path="/deals/:id" element={<DealDetail />} />
              <Route path="/deals/:id/dmv-registration" element={<DmvRegistration />} />
              
              {/* Other protected routes */}
              <Route path="/documents" element={<Documents />} />
              <Route path="/commissions" element={<Commissions />} />
            </Route>
          </Route>
          
          {/* Catch all */}
          <Route path="*" element={<NotFound />} />        </Routes>
        </Suspense>
      </NotificationProvider>
    </AuthProvider>
  );
}

export default App;
