// @ts-nocheck
import React from 'react';
import { Box, Button, Typography, Container } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const Unauthorized: React.FC = () => {
  const navigate = useNavigate();

  return (
    <Container maxWidth="md">
      <Box
        display="flex"
        flexDirection="column"
        alignItems="center"
        justifyContent="center"
        minHeight="80vh"
        textAlign="center"
      >
        <Typography variant="h1" sx={{ fontWeight: 'bold', mb: 2 }}>
          403
        </Typography>
        <Typography variant="h4" sx={{ mb: 3 }}>
          Access Denied
        </Typography>
        <Typography variant="body1" color="textSecondary" sx={{ mb: 4, maxWidth: 500 }}>
          You do not have permission to access this resource.
          Please contact your administrator if you believe this is in error.
        </Typography>
        <Button 
          variant="contained" 
          color="primary"
          onClick={() => navigate('/dashboard')}
        >
          Back to Dashboard
        </Button>
      </Box>
    </Container>
  );
};

export default Unauthorized;
