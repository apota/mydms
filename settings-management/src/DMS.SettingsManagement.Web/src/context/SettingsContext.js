import React, { createContext, useContext, useReducer, useEffect } from 'react';
import axios from 'axios';

// API base URL - use /api for proxy routing
const API_BASE = '/api';

// Action types
const SET_USER_SETTINGS = 'SET_USER_SETTINGS';
const SET_SYSTEM_SETTINGS = 'SET_SYSTEM_SETTINGS';
const SET_LOADING = 'SET_LOADING';
const SET_ERROR = 'SET_ERROR';
const SET_CURRENT_USER = 'SET_CURRENT_USER';

// Initial state
const initialState = {
  userSettings: null,
  systemSettings: null,
  currentUserId: '1', // Default user ID for demo
  loading: false,
  error: null
};

// Reducer
const settingsReducer = (state, action) => {
  switch (action.type) {
    case SET_USER_SETTINGS:
      return { ...state, userSettings: action.payload };
    case SET_SYSTEM_SETTINGS:
      return { ...state, systemSettings: action.payload };
    case SET_LOADING:
      return { ...state, loading: action.payload };
    case SET_ERROR:
      return { ...state, error: action.payload };
    case SET_CURRENT_USER:
      return { ...state, currentUserId: action.payload };
    default:
      return state;
  }
};

// Create context
const SettingsContext = createContext();

// Custom hook to use settings context
export const useSettings = () => {
  const context = useContext(SettingsContext);
  if (!context) {
    throw new Error('useSettings must be used within a SettingsProvider');
  }
  return context;
};

// Provider component
export const SettingsProvider = ({ children }) => {
  const [state, dispatch] = useReducer(settingsReducer, initialState);

  // API Functions
  const api = {
    // User Settings
    getUserSettings: async (userId = state.currentUserId) => {
      dispatch({ type: SET_LOADING, payload: true });
      try {
        const response = await axios.get(`${API_BASE}/settings/user/${userId}`);
        dispatch({ type: SET_USER_SETTINGS, payload: response.data });
        return response.data;
      } catch (error) {
        dispatch({ type: SET_ERROR, payload: error.message });
        throw error;
      } finally {
        dispatch({ type: SET_LOADING, payload: false });
      }
    },

    updateUserSettings: async (settings, userId = state.currentUserId) => {
      dispatch({ type: SET_LOADING, payload: true });
      try {
        const response = await axios.put(`${API_BASE}/settings/user/${userId}`, { settings });
        await api.getUserSettings(userId); // Refresh settings
        return response.data;
      } catch (error) {
        dispatch({ type: SET_ERROR, payload: error.message });
        throw error;
      } finally {
        dispatch({ type: SET_LOADING, payload: false });
      }
    },

    patchUserSettings: async (partialSettings, userId = state.currentUserId) => {
      dispatch({ type: SET_LOADING, payload: true });
      try {
        const response = await axios.patch(`${API_BASE}/settings/user/${userId}`, { settings: partialSettings });
        await api.getUserSettings(userId); // Refresh settings
        return response.data;
      } catch (error) {
        dispatch({ type: SET_ERROR, payload: error.message });
        throw error;
      } finally {
        dispatch({ type: SET_LOADING, payload: false });
      }
    },

    updateCategorySettings: async (category, settings, userId = state.currentUserId) => {
      dispatch({ type: SET_LOADING, payload: true });
      try {
        const response = await axios.put(`${API_BASE}/settings/user/${userId}/category/${category}`, { settings });
        await api.getUserSettings(userId); // Refresh settings
        return response.data;
      } catch (error) {
        dispatch({ type: SET_ERROR, payload: error.message });
        throw error;
      } finally {
        dispatch({ type: SET_LOADING, payload: false });
      }
    },

    deleteUserSettings: async (userId) => {
      dispatch({ type: SET_LOADING, payload: true });
      try {
        const response = await axios.delete(`${API_BASE}/settings/user/${userId}`);
        return response.data;
      } catch (error) {
        dispatch({ type: SET_ERROR, payload: error.message });
        throw error;
      } finally {
        dispatch({ type: SET_LOADING, payload: false });
      }
    },

    // System Settings
    getSystemSettings: async () => {
      dispatch({ type: SET_LOADING, payload: true });
      try {
        const response = await axios.get(`${API_BASE}/settings/system`);
        dispatch({ type: SET_SYSTEM_SETTINGS, payload: response.data });
        return response.data;
      } catch (error) {
        dispatch({ type: SET_ERROR, payload: error.message });
        throw error;
      } finally {
        dispatch({ type: SET_LOADING, payload: false });
      }
    },

    updateSystemSetting: async (key, value, description = '', isPublic = false) => {
      dispatch({ type: SET_LOADING, payload: true });
      try {
        const response = await axios.put(`${API_BASE}/settings/system/${key}`, {
          value,
          description,
          isPublic
        });
        await api.getSystemSettings(); // Refresh system settings
        return response.data;
      } catch (error) {
        dispatch({ type: SET_ERROR, payload: error.message });
        throw error;
      } finally {
        dispatch({ type: SET_LOADING, payload: false });
      }
    },

    // Health check
    checkHealth: async () => {
      try {
        const response = await axios.get(`${API_BASE}/health`);
        return response.data;
      } catch (error) {
        throw error;
      }
    }
  };

  // Actions
  const actions = {
    setCurrentUser: (userId) => {
      dispatch({ type: SET_CURRENT_USER, payload: userId });
    },
    clearError: () => {
      dispatch({ type: SET_ERROR, payload: null });
    }
  };

  // Load initial data
  useEffect(() => {
    const loadInitialData = async () => {
      try {
        await Promise.all([
          api.getUserSettings(),
          api.getSystemSettings()
        ]);
      } catch (error) {
        console.error('Error loading initial data:', error);
        dispatch({ type: SET_ERROR, payload: 'Failed to load settings data' });
      }
    };

    loadInitialData();
  }, [state.currentUserId]);

  const value = {
    ...state,
    api,
    actions
  };

  return (
    <SettingsContext.Provider value={value}>
      {children}
    </SettingsContext.Provider>
  );
};
