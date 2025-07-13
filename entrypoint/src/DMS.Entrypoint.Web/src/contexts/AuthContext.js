import React, { createContext, useContext, useState, useEffect } from 'react';
import axios from 'axios';

const AuthContext = createContext({});

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

// Configure axios defaults
axios.defaults.baseURL = 'http://localhost:8081'; // Direct connection to auth service for testing

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  // Setup axios interceptors
  useEffect(() => {
    // Request interceptor to add auth header
    const requestInterceptor = axios.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('accessToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor to handle token refresh
    const responseInterceptor = axios.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config;

        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true;

          try {
            const refreshToken = localStorage.getItem('refreshToken');
            if (refreshToken) {
              const response = await axios.post('/auth/refresh', {
                refreshToken,
              });

              const { accessToken } = response.data;
              localStorage.setItem('accessToken', accessToken);

              // Retry original request
              originalRequest.headers.Authorization = `Bearer ${accessToken}`;
              return axios(originalRequest);
            }
          } catch (refreshError) {
            // Refresh failed, redirect to login
            logout();
            return Promise.reject(refreshError);
          }
        }

        return Promise.reject(error);
      }
    );

    return () => {
      axios.interceptors.request.eject(requestInterceptor);
      axios.interceptors.response.eject(responseInterceptor);
    };
  }, []);

  // Initialize auth state
  useEffect(() => {
    const initAuth = async () => {
      console.log('=== INIT AUTH START ===');
      const token = localStorage.getItem('accessToken');
      console.log('Token from localStorage:', token);
      
      if (token && token.startsWith('mock-token')) {
        // Handle mock tokens
        const email = localStorage.getItem('userEmail') || 'demo@example.com';
        console.log('Found mock token, email:', email);
        const role = email.split('@')[0];
        const restoredUser = {
          id: 1,
          email: email,
          firstName: role.charAt(0).toUpperCase() + role.slice(1),
          lastName: 'User',
          role: role,
          permissions: ['all']
        };
        console.log('Restoring user from localStorage:', restoredUser);
        setUser(restoredUser);
      } else if (token) {
        try {
          console.log('Verifying token with backend...');
          const response = await axios.get('/verify');
          console.log('Token verification response:', response.data);
          setUser(response.data.user);
        } catch (error) {
          // Token is invalid, clear storage
          console.log('Token verification failed:', error);
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('userEmail');
          setUser(null);
        }
      } else {
        console.log('No token found, user remains null');
      }
      
      console.log('Setting loading to false');
      setLoading(false);
      console.log('=== INIT AUTH END ===');
    };

    initAuth();
  }, []);

  // Debug user state changes
  useEffect(() => {
    console.log('=== USER STATE CHANGED ===');
    console.log('New user state:', user);
    console.log('Loading state:', loading);
    console.log('========================');
  }, [user, loading]);

  const login = async (credentials) => {
    console.log('Login attempt started with:', credentials.email);
    
    try {
      // Simple mock login for testing
      if (credentials.email && credentials.password) {
        // Create mock user based on email
        const role = credentials.email.split('@')[0]; // extract role from email
        const mockUser = {
          id: 1,
          email: credentials.email,
          firstName: role.charAt(0).toUpperCase() + role.slice(1),
          lastName: 'User',
          role: role,
          permissions: ['all']
        };
        
        // Store mock tokens and email for persistence
        localStorage.setItem('accessToken', 'mock-token-' + Date.now());
        localStorage.setItem('refreshToken', 'mock-refresh-' + Date.now());
        localStorage.setItem('userEmail', credentials.email);
        
        setUser(mockUser);
        
        console.log('Mock login successful for:', credentials.email);
        
        return { success: true };
      }
      
      throw new Error('Email and password required');
      
    } catch (error) {
      console.error('Login error:', error);
      throw new Error(error.message || 'Login failed');
    }
  };

  const logout = async () => {
    try {
      await axios.post('/auth/logout');
    } catch (error) {
      // Logout failed, but we still want to clear local state
      console.error('Logout error:', error);
    } finally {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('userEmail');
      setUser(null);
      console.log('User logged out successfully');
    }
  };

  const register = async (userData) => {
    try {
      const response = await axios.post('/auth/register', userData);
      console.log('Registration successful! Please login.');
      return response.data;
    } catch (error) {
      const message = error.response?.data?.error || 'Registration failed';
      console.error('Registration error:', message);
      throw new Error(message);
    }
  };

  const enableMFA = async () => {
    try {
      const response = await axios.post('/auth/mfa/enable');
      return response.data;
    } catch (error) {
      const message = error.response?.data?.error || 'MFA setup failed';
      console.error('MFA setup error:', message);
      throw new Error(message);
    }
  };

  const confirmMFA = async (token) => {
    try {
      const response = await axios.post('/auth/mfa/confirm', { token });
      console.log('MFA enabled successfully!');
      return response.data;
    } catch (error) {
      const message = error.response?.data?.error || 'MFA confirmation failed';
      console.error('MFA confirmation error:', message);
      throw new Error(message);
    }
  };

  const value = {
    user,
    loading,
    login,
    logout,
    register,
    enableMFA,
    confirmMFA,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};
