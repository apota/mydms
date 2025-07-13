import { useQuery, useMutation, useQueryClient } from 'react-query';
import { userAPI } from '../services/api';
import toast from 'react-hot-toast';

// Query keys
export const QUERY_KEYS = {
  USERS: 'users',
  USER: 'user',
};

// Get all users
export const useUsers = () => {
  return useQuery(QUERY_KEYS.USERS, userAPI.getUsers, {
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

// Get single user
export const useUser = (id) => {
  return useQuery([QUERY_KEYS.USER, id], () => userAPI.getUser(id), {
    enabled: !!id,
  });
};

// Create user mutation
export const useCreateUser = () => {
  const queryClient = useQueryClient();
  
  return useMutation(userAPI.createUser, {
    onSuccess: () => {
      queryClient.invalidateQueries(QUERY_KEYS.USERS);
      toast.success('User created successfully!');
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create user');
    },
  });
};

// Update user mutation
export const useUpdateUser = () => {
  const queryClient = useQueryClient();
  
  return useMutation(({ id, data }) => userAPI.updateUser(id, data), {
    onSuccess: () => {
      queryClient.invalidateQueries(QUERY_KEYS.USERS);
      toast.success('User updated successfully!');
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update user');
    },
  });
};

// Delete user mutation
export const useDeleteUser = () => {
  const queryClient = useQueryClient();
  
  return useMutation(userAPI.deleteUser, {
    onSuccess: () => {
      queryClient.invalidateQueries(QUERY_KEYS.USERS);
      toast.success('User deleted successfully!');
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to delete user');
    },
  });
};

// Change password mutation
export const useChangePassword = () => {
  return useMutation(({ id, data }) => userAPI.changePassword(id, data), {
    onSuccess: () => {
      toast.success('Password changed successfully!');
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to change password');
    },
  });
};
