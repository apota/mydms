/**
 * CRM API Service for DMS
 * Handles interactions with the CRM backend API
 */

const CrmAPI = {
    baseUrl: 'http://localhost:7001', // CRM API endpoint
    apiPath: '/api', // API path prefix
    isAvailable: true, // Flag to track if CRM API is available
    connectionRetries: 0, // Track connection attempts
    maxRetries: 2, // Maximum number of retries
    
    /**
     * Check if CRM API is running and available
     */
    checkConnection: async function() {
        if (this.connectionRetries >= this.maxRetries) {
            console.log('Max connection retries reached for CRM API');
            this.isAvailable = false;
            return false;
        }
        
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 3000); // 3 second timeout
            
            const response = await fetch(`${this.baseUrl}${this.apiPath}/customers`, {
                method: 'GET',
                signal: controller.signal,
                headers: {
                    'Content-Type': 'application/json'
                }
            }).catch(() => null);
            
            clearTimeout(timeoutId);
            
            if (response && (response.ok || response.status === 404)) {
                console.log('CRM API connection successful');
                this.isAvailable = true;
                this.connectionRetries = 0; // Reset retries on successful connection
                return true;
            } else {
                this.connectionRetries++;
                this.isAvailable = false;
                console.warn(`CRM API connection failed (attempt ${this.connectionRetries}/${this.maxRetries})`);
                return false;
            }
        } catch (error) {
            this.connectionRetries++;
            this.isAvailable = false;
            console.warn(`CRM API connection error (attempt ${this.connectionRetries}/${this.maxRetries}):`, error.name);
            return false;
        }
    },
    
    /**
     * Get all customers
     */
    getCustomers: async function() {
        try {
            const response = await fetch(`${this.baseUrl}${this.apiPath}/customers`, {
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
            console.error('Error fetching customers:', error);
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Add a new customer
     */
    addCustomer: async function(customer) {
        try {
            const response = await fetch(`${this.baseUrl}${this.apiPath}/customers`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    name: customer.name,
                    email: customer.email,
                    phone: customer.phone,
                    customerType: customer.type || 'Sales',
                    // Add any other required fields
                    address: customer.address || '',
                    city: customer.city || '',
                    state: customer.state || '',
                    zipCode: customer.zipCode || '',
                    country: customer.country || 'USA'
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
            console.error('Error adding customer:', error);
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Update an existing customer
     */
    updateCustomer: async function(id, customer) {
        try {
            const response = await fetch(`${this.baseUrl}${this.apiPath}/customers/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    name: customer.name,
                    email: customer.email,
                    phone: customer.phone,
                    customerType: customer.type || customer.customerType || 'Sales',
                    address: customer.address || '',
                    city: customer.city || '',
                    state: customer.state || '',
                    zipCode: customer.zipCode || '',
                    country: customer.country || 'USA'
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
            console.error('Error updating customer:', error);
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Delete a customer
     */
    deleteCustomer: async function(id) {
        try {
            const response = await fetch(`${this.baseUrl}${this.apiPath}/customers/${id}`, {
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
            console.error('Error deleting customer:', error);
            return { success: false, error: error.message };
        }
    }
};

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = CrmAPI;
}
