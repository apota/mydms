/**
 * User Management API Service for DMS
 * Handles interactions with LocalStack/backend for user operations
 */

const UserAPI = {
    baseUrl: localStorage.getItem('dms_localstack_endpoint') || 'http://localhost:4566',
    apiPath: '/dms/api/users',
    isAvailable: true,
    connectionRetries: 0,
    maxRetries: 2,
    
    /**
     * Check if User API is available
     */
    checkConnection: async function() {
        if (this.connectionRetries >= this.maxRetries) {
            console.log('Max connection retries reached for User API');
            this.isAvailable = false;
            return false;
        }
        
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 3000);
            
            const response = await fetch(`${this.baseUrl}${this.apiPath}`, {
                method: 'GET',
                signal: controller.signal,
                headers: {
                    'Content-Type': 'application/json'
                }
            }).catch(() => null);
            
            clearTimeout(timeoutId);
            
            if (response && (response.ok || response.status === 404)) {
                console.log('User API connection successful');
                this.isAvailable = true;
                this.connectionRetries = 0;
                return true;
            } else {
                this.connectionRetries++;
                this.isAvailable = false;
                console.warn(`User API connection failed (attempt ${this.connectionRetries}/${this.maxRetries})`);
                return false;
            }
        } catch (error) {
            this.connectionRetries++;
            this.isAvailable = false;
            console.warn(`User API connection error (attempt ${this.connectionRetries}/${this.maxRetries}):`, error.name);
            return false;
        }
    },
    
    /**
     * Initialize the User API/Database
     */
    initializeDatabase: async function() {
        try {
            const response = await fetch(`${this.baseUrl}${this.apiPath}/init`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.ok) {
                return { success: true };
            } else {
                return { success: false, error: `HTTP ${response.status}` };
            }
        } catch (error) {
            console.error('Error initializing User database:', error);
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Get all users
     */
    getUsers: async function() {
        try {
            const response = await fetch(`${this.baseUrl}${this.apiPath}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.ok) {
                const data = await response.json();
                return { success: true, data: data };
            } else {
                return { success: false, error: `HTTP ${response.status}: ${response.statusText}` };
            }
        } catch (error) {
            console.error('Error fetching users:', error);
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Add a new user
     */
    addUser: async function(user) {
        try {
            const response = await fetch(`${this.baseUrl}${this.apiPath}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    firstName: user.firstName,
                    lastName: user.lastName,
                    email: user.email,
                    role: user.role,
                    status: user.status || 'active',
                    password: user.password,
                    createdAt: new Date().toISOString(),
                    lastLogin: null
                })
            });
            
            if (response.ok) {
                const data = await response.json();
                return { success: true, data: data };
            } else {
                const errorText = await response.text();
                return { success: false, error: `HTTP ${response.status}: ${errorText}` };
            }
        } catch (error) {
            console.error('Error adding user:', error);
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Update an existing user
     */
    updateUser: async function(id, user) {
        try {
            const response = await fetch(`${this.baseUrl}${this.apiPath}/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    firstName: user.firstName,
                    lastName: user.lastName,
                    email: user.email,
                    role: user.role,
                    status: user.status,
                    password: user.password,
                    updatedAt: new Date().toISOString()
                })
            });
            
            if (response.ok) {
                const data = await response.json();
                return { success: true, data: data };
            } else {
                const errorText = await response.text();
                return { success: false, error: `HTTP ${response.status}: ${errorText}` };
            }
        } catch (error) {
            console.error('Error updating user:', error);
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Delete a user
     */
    deleteUser: async function(id) {
        try {
            const response = await fetch(`${this.baseUrl}${this.apiPath}/${id}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.ok) {
                return { success: true };
            } else {
                const errorText = await response.text();
                return { success: false, error: `HTTP ${response.status}: ${errorText}` };
            }
        } catch (error) {
            console.error('Error deleting user:', error);
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Reset user password
     */
    resetPassword: async function(id) {
        try {
            const response = await fetch(`${this.baseUrl}${this.apiPath}/${id}/reset-password`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.ok) {
                const data = await response.json();
                return { success: true, data: data };
            } else {
                const errorText = await response.text();
                return { success: false, error: `HTTP ${response.status}: ${errorText}` };
            }
        } catch (error) {
            console.error('Error resetting password:', error);
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Toggle user status (activate/deactivate)
     */
    toggleUserStatus: async function(id, status) {
        try {
            const response = await fetch(`${this.baseUrl}${this.apiPath}/${id}/status`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    status: status,
                    updatedAt: new Date().toISOString()
                })
            });
            
            if (response.ok) {
                const data = await response.json();
                return { success: true, data: data };
            } else {
                const errorText = await response.text();
                return { success: false, error: `HTTP ${response.status}: ${errorText}` };
            }
        } catch (error) {
            console.error('Error toggling user status:', error);
            return { success: false, error: error.message };
        }
    }
};

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = UserAPI;
}
