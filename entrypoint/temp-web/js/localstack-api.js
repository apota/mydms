/**
 * LocalStack API Service for DMS
 * Handles interactions with the LocalStack backend for database operations
 */

const LocalStackAPI = {
    // Use stored endpoint from localStorage if available, otherwise default
    baseUrl: localStorage.getItem('dms_localstack_endpoint') || 'http://localhost:4566',
    apiPath: '/dms/api', // API path prefix
    isAvailable: true, // Flag to track if LocalStack is available
    connectionRetries: 0, // Track connection attempts
    maxRetries: 2, // Maximum number of retries
    _hasShownResetMessage: false, // Flag to track whether reset message has been shown
    _hasShownDockerMessage: false, // Flag to track whether Docker message has been shown
    _hasShownTmpBusyMessage: false, // Flag to track whether tmp busy message has been shown
    _hasShownCorsMessage: false, // Flag to track whether CORS message has been shown
    _hasShownCorsMessage: false, // Flag to track whether CORS message has been shown
    
    /**
     * Set LocalStack endpoint URL
     */
    setEndpoint: function(url) {
        this.baseUrl = url;
        localStorage.setItem('dms_localstack_endpoint', url);
        // Reset connection tracking when endpoint changes
        this.connectionRetries = 0;
        this.isAvailable = true;
        console.log(`LocalStack endpoint updated to: ${url}`);
    },
    
    /**
     * Check if LocalStack is running and available with enhanced diagnostics
     */
    checkConnection: async function() {
        if (this.connectionRetries >= this.maxRetries) {
            console.log('Max connection retries reached, not attempting further connections');
            this.isAvailable = false;
            return false;
        }
        
        // Check if Docker is running first
        if (this.connectionRetries === 0) {
            await this.checkDockerRunning();
        }
        
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 3000); // Increased timeout
            
            console.log(`Attempting LocalStack connection to ${this.baseUrl}/health...`);
            
            // Add CORS-friendly headers
            const response = await fetch(`${this.baseUrl}/health`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Access-Control-Request-Method': 'GET',
                    'Access-Control-Request-Headers': 'Content-Type'
                },
                signal: controller.signal,
                mode: 'cors' // Explicitly request CORS
            }).catch((error) => {
                console.warn('Fetch error:', error.name, error.message);
                return null;
            });
            
            clearTimeout(timeoutId);
            
            if (response && response.ok) {
                console.log('✅ LocalStack connection successful');
                this.isAvailable = true;
                this.connectionRetries = 0; // Reset retries on successful connection
                
                // Try to get health details
                try {
                    const healthData = await response.json();
                    console.log('LocalStack services:', Object.keys(healthData.services || {}).join(', '));
                } catch (e) {
                    console.log('LocalStack is responding (health data not parseable)');
                }
                
                return true;
            } else {
                this.connectionRetries++;
                this.isAvailable = false;
                
                // Provide specific error messages
                if (!response) {
                    console.warn(`❌ LocalStack connection failed - no response (attempt ${this.connectionRetries}/${this.maxRetries})`);
                    console.warn('Possible causes: LocalStack not running, Docker not started, port blocked');
                } else if (response.status === 0) {
                    console.warn(`❌ LocalStack CORS error (attempt ${this.connectionRetries}/${this.maxRetries})`);
                    this.suggestCorsSetup();
                } else {
                    console.warn(`❌ LocalStack HTTP error ${response.status} (attempt ${this.connectionRetries}/${this.maxRetries})`);
                }
                return false;
            }
        } catch (error) {
            this.connectionRetries++;
            this.isAvailable = false;
            
            // Enhanced error diagnosis
            if (error.name === 'AbortError') {
                console.warn(`❌ LocalStack connection timeout (attempt ${this.connectionRetries}/${this.maxRetries})`);
                console.warn('LocalStack may be starting up - try again in 30 seconds');
            } else if (error.name === 'TypeError' && error.message.includes('Failed to fetch')) {
                console.warn(`❌ LocalStack connection refused (attempt ${this.connectionRetries}/${this.maxRetries})`);
                console.warn('Check: 1) Docker running, 2) LocalStack container started, 3) Port 4566 not blocked');
            } else if (error.message.includes('CORS')) {
                console.warn(`❌ LocalStack CORS error (attempt ${this.connectionRetries}/${this.maxRetries}):`, error.message);
                this.suggestCorsSetup();
            } else {
                console.warn(`❌ LocalStack connection error (attempt ${this.connectionRetries}/${this.maxRetries}):`, error.name, error.message);
            }
            
            // If we've reached max retries, suggest solutions
            if (this.connectionRetries >= this.maxRetries) {
                this.suggestTroubleshooting();
            }
            return false;
        }
    },
    
    /**
     * Check if Docker appears to be running
     * This is a very simple check that tries to fetch a common Docker port
     */
    checkDockerRunning: async function() {
        try {
            // Use controller to abort the request after 500ms if not responding
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 500);
            
            // Try to connect to Docker daemon port 2375 (only works if Docker exposes this port)
            // This is just a basic connection check that will fail fast
            const response = await fetch('http://localhost:2375', {
                signal: controller.signal
            }).catch(() => null);
            
            clearTimeout(timeoutId);
            
            // The response will likely be an error, but if we get any response,
            // that suggests some service is running on that port
            if (response) {
                console.log('Docker connection test: Service detected on port 2375');
                return true;
            }
            
            // Try alternate Docker port
            const controller2 = new AbortController();
            const timeoutId2 = setTimeout(() => controller2.abort(), 500);
            
            const response2 = await fetch('http://localhost:2376', {
                signal: controller2.signal
            }).catch(() => null);
            
            clearTimeout(timeoutId2);
            
            if (response2) {
                console.log('Docker connection test: Service detected on port 2376');
                return true;
            }
            
            // Check for LocalStack port itself as another indicator
            const controller3 = new AbortController();
            const timeoutId3 = setTimeout(() => controller3.abort(), 500);
            
            // Check if something responds on the LocalStack port (even if it returns an error)
            const response3 = await fetch(this.baseUrl, {
                signal: controller3.signal
            }).catch((e) => {
                // If it's a CORS error, that indicates something is running
                if (e.message && e.message.includes('CORS')) {
                    return { corsError: true };
                }
                return null;
            });
            
            clearTimeout(timeoutId3);
            
            if (response3) {
                console.log('Docker connection test: Service detected on LocalStack port');
                return true;
            }
            
            console.warn('Docker connection test: No services detected on common Docker ports');
            this.showDockerNotRunningMessage();
            return false;
        } catch (error) {
            console.warn('Docker connection test error:', error.name);
            return false;
        }
    },
    
    /**
     * Show a message indicating Docker doesn't appear to be running
     */
    showDockerNotRunningMessage: function() {
        // Only show once per session
        if (this._hasShownDockerMessage) {
            return;
        }
        this._hasShownDockerMessage = true;
        
        // Create Docker not running notification
        const notification = document.createElement('div');
        notification.className = 'notification error';
        notification.innerHTML = `
            <strong>Docker May Not Be Running</strong>
            <p>LocalStack requires Docker, but Docker doesn't appear to be running.</p>
            <p>Please:</p>
            <ol>
                <li>Start Docker Desktop or Docker service</li>
                <li>Ensure LocalStack is properly installed</li>
                <li>Restart the application</li>
            </ol>
            <a href="#" class="notification-close">&times;</a>
        `;
        document.body.appendChild(notification);
        
        // Add close button functionality
        notification.querySelector('.notification-close').addEventListener('click', function() {
            notification.remove();
        });
        
        // Auto-hide after 30 seconds
        setTimeout(() => {
            if (document.body.contains(notification)) {
                notification.style.opacity = '0';
                setTimeout(() => notification.remove(), 500);
            }
        }, 30000);
    },
    
    /**
     * Initialize the customer database in LocalStack
     * Creates the necessary resources if they don't exist
     */
    initializeDatabase: async function() {
        // Check if LocalStack is available first
        if (!await this.checkConnection()) {
            return { success: false, error: 'LocalStack is not available' };
        }
        
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 3000); // 3 second timeout
            
            const response = await fetch(`${this.baseUrl}${this.apiPath}/init`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                signal: controller.signal
            });
            
            clearTimeout(timeoutId);
            
            if (!response.ok) {
                throw new Error(`Failed to initialize database: ${response.status}`);
            }
            
            return await response.json();
        } catch (error) {
            console.error('Error initializing LocalStack database:', error);
            this.isAvailable = false;
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Get all customers from LocalStack database
     */
    getCustomers: async function() {
        // Skip if LocalStack is known to be unavailable
        if (!this.isAvailable) {
            return { success: false, error: 'LocalStack is not available', data: [] };
        }
        
        // Check connection first
        if (!await this.checkConnection()) {
            return { success: false, error: 'LocalStack connection failed', data: [] };
        }
        
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 3000);
            
            const response = await fetch(`${this.baseUrl}${this.apiPath}/customers`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                signal: controller.signal
            });
            
            clearTimeout(timeoutId);
            
            if (!response.ok) {
                throw new Error(`Failed to get customers: ${response.status}`);
            }
            
            return await response.json();
        } catch (error) {
            // If it's an abort error, it's a timeout
            if (error.name === 'AbortError') {
                console.error('Timeout getting customers from LocalStack');
                this.isAvailable = false;
            } else {
                console.error('Error getting customers from LocalStack:', error);
            }
            return { success: false, error: error.message, data: [] };
        }
    },
    
    /**
     * Add a new customer to LocalStack database
     */
    addCustomer: async function(customerData) {
        // Skip if LocalStack is known to be unavailable
        if (!this.isAvailable) {
            return { success: false, error: 'LocalStack is not available' };
        }
        
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 3000);
            
            const response = await fetch(`${this.baseUrl}${this.apiPath}/customers`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Access-Control-Request-Method': 'POST',
                    'Access-Control-Request-Headers': 'Content-Type'
                },
                body: JSON.stringify(customerData),
                signal: controller.signal,
                mode: 'cors'
            });
            
            clearTimeout(timeoutId);
            
            if (!response.ok) {
                if (response.status === 0) {
                    throw new Error('CORS error - LocalStack is blocking cross-origin requests');
                }
                throw new Error(`Failed to add customer: ${response.status}`);
            }
            
            return await response.json();
        } catch (error) {
            // If it's an abort error, it's a timeout
            if (error.name === 'AbortError') {
                console.error('Timeout adding customer to LocalStack');
                this.isAvailable = false;
            } else if (error.message.includes('CORS')) {
                console.error('CORS error adding customer to LocalStack:', error.message);
                this.suggestCorsSetup();
                this.isAvailable = false;
            } else {
                console.error('Error adding customer to LocalStack:', error);
            }
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Update an existing customer in LocalStack database
     */
    updateCustomer: async function(id, customerData) {
        // Skip if LocalStack is known to be unavailable
        if (!this.isAvailable) {
            return { success: false, error: 'LocalStack is not available' };
        }
        
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 3000);
            
            const response = await fetch(`${this.baseUrl}${this.apiPath}/customers/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(customerData),
                signal: controller.signal
            });
            
            clearTimeout(timeoutId);
            
            if (!response.ok) {
                throw new Error(`Failed to update customer: ${response.status}`);
            }
            
            return await response.json();
        } catch (error) {
            // If it's an abort error, it's a timeout
            if (error.name === 'AbortError') {
                console.error('Timeout updating customer in LocalStack');
                this.isAvailable = false;
            } else {
                console.error('Error updating customer in LocalStack:', error);
            }
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Delete a customer from LocalStack database
     */
    deleteCustomer: async function(id) {
        // Skip if LocalStack is known to be unavailable
        if (!this.isAvailable) {
            return { success: false, error: 'LocalStack is not available' };
        }
        
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 3000);
            
            const response = await fetch(`${this.baseUrl}${this.apiPath}/customers/${id}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                },
                signal: controller.signal
            });
            
            clearTimeout(timeoutId);
            
            if (!response.ok) {
                throw new Error(`Failed to delete customer: ${response.status}`);
            }
            
            return await response.json();
        } catch (error) {
            // If it's an abort error, it's a timeout
            if (error.name === 'AbortError') {
                console.error('Timeout deleting customer from LocalStack');
                this.isAvailable = false;
            } else {
                console.error('Error deleting customer from LocalStack:', error);
            }
            return { success: false, error: error.message };
        }
    },
    
    /**
     * Display a notification to the user suggesting Docker reset
     */
    suggestDockerReset: function() {
        // Only suggest once per session
        if (this._hasShownResetMessage) {
            return;
        }
        this._hasShownResetMessage = true;
        
        // Create reset suggestion notification
        const notification = document.createElement('div');
        notification.className = 'notification error';
        notification.innerHTML = `
            <strong>LocalStack Connection Error</strong>
            <p>Unable to connect to LocalStack. This could be due to the "Device or resource busy" error.</p>
            <p>Try running the reset script in the /localstack folder:</p>
            <ul>
                <li>Windows: <code>reset_localstack.bat</code> or <code>start_localstack_compose.bat</code></li>
                <li>Linux/Mac: <code>./reset_localstack.sh</code> or <code>./start_localstack_compose.sh</code></li>
            </ul>
            <a href="#" class="notification-close">&times;</a>
        `;
        document.body.appendChild(notification);
        
        // Add close button functionality
        notification.querySelector('.notification-close').addEventListener('click', function() {
            notification.remove();
        });
        
        // Auto-hide after 20 seconds
        setTimeout(() => {
            if (document.body.contains(notification)) {
                notification.style.opacity = '0';
                setTimeout(() => notification.remove(), 500);
            }
        }, 20000);
    },
    
    /**
     * Set custom endpoint for LocalStack (useful for different Docker setups)
     */
    setEndpoint: function(url) {
        if (url && typeof url === 'string') {
            this.baseUrl = url.replace(/\/+$/, ''); // Remove trailing slashes
            console.log(`LocalStack endpoint updated to: ${this.baseUrl}`);
            
            // Reset connection status
            this.isAvailable = true;
            this.connectionRetries = 0;
            
            // Try connection with new endpoint
            this.checkConnection();
            return true;
        }
        return false;
    },
    
    /**
     * Check and handle the "/tmp/localstack busy device" error
     * This error occurs when the LocalStack data directory is mounted as a volume
     * and the container is unable to access it due to permission issues.
     */
    checkTmpBusyError: function() {
        // Only check once per session
        if (this._hasShownTmpBusyMessage) {
            return;
        }
        this._hasShownTmpBusyMessage = true;
        
        // Create tmp busy error notification
        const notification = document.createElement('div');
        notification.className = 'notification error';
        notification.innerHTML = `
            <strong>LocalStack Initialization Error</strong>
            <p>LocalStack data directory may be busy or inaccessible.</p>
            <p>If you have recently run LocalStack, ensure it is completely stopped before restarting.</p>
            <p>Check Docker for any running LocalStack containers and stop them:</p>
            <pre>docker ps -a | grep localstack</pre>
            <p>Then try restarting LocalStack.</p>
            <a href="#" class="notification-close">&times;</a>
        `;
        document.body.appendChild(notification);
        
        // Add close button functionality
        notification.querySelector('.notification-close').addEventListener('click', function() {
            notification.remove();
        });
        
        // Auto-hide after 30 seconds
        setTimeout(() => {
            if (document.body.contains(notification)) {
                notification.style.opacity = '0';
                setTimeout(() => notification.remove(), 500);
            }
        }, 30000);
    },
    
    /**
     * Specifically checks if the error is related to the "/tmp/localstack busy device" issue
     * and provides targeted guidance to resolve it
     * @param {Error} error - The error to check
     * @returns {boolean} - True if error was handled, false otherwise
     */
    checkForTmpBusyError: function(error) {
        // Check if error message includes indicators of the "Device or resource busy: '/tmp/localstack'" error
        const errorStr = error.toString().toLowerCase();
        const responseStr = error.response ? JSON.stringify(error.response).toLowerCase() : '';
        
        if ((errorStr.includes('device or resource busy') && errorStr.includes('/tmp/localstack')) || 
            (responseStr.includes('device or resource busy') && responseStr.includes('/tmp/localstack'))) {
            
            // Only show the message once per session
            if (!this._hasShownTmpBusyMessage) {
                this._hasShownTmpBusyMessage = true;
                
                const message = `
                    <div class="alert-heading">LocalStack "/tmp/localstack" Mount Error Detected</div>
                    <p>The system has detected the common "Device or resource busy: '/tmp/localstack'" error.</p>
                    <p>This happens when Docker can't properly unmount the LocalStack volume.</p>
                    <p><strong>To fix this issue:</strong></p>
                    <ol>
                        <li>Run the provided <code>fix_tmp_localstack_error</code> script in the LocalStack folder</li>
                        <li>Or run <code>fix_busy_device_error</code> script for a targeted fix</li>
                        <li>Then restart LocalStack using <code>start_localstack_compose</code></li>
                    </ol>
                    <p>See TROUBLESHOOTING.md for more details.</p>
                `;
                
                this.showNotification(message, 'error', true);
                
                // Suggest checking Docker and LocalStack status
                this.checkDockerStatus();
                return true;
            }
        }
        
        return false;
    },
    
    /**
     * Tests the connection to LocalStack with specific error detection
     * for the '/tmp/localstack' busy device error
     * @returns {Promise<boolean>} - True if connection is successful
     */
    healthCheck: async function() {
        try {
            // Try to hit the LocalStack health endpoint
            const url = `${this.baseUrl}/health`;
            console.log('Testing LocalStack connection at:', url);
            
            this.showLoading(true, 'Testing LocalStack connection...');
            
            const response = await fetch(url, { 
                method: 'GET',
                timeout: 3000
            });
            
            this.showLoading(false);
            
            if (response.ok) {
                const data = await response.json();
                console.log('LocalStack health status:', data);
                
                // Check if services are running
                const services = data.services || {};
                const servicesRunning = Object.values(services).some(s => s === 'running');
                
                if (servicesRunning) {
                    // Reset connection status
                    this.isAvailable = true;
                    this.connectionRetries = 0;
                    this._hasShownResetMessage = false;
                    this._hasShownDockerMessage = false;
                    this._hasShownTmpBusyMessage = false;
                    
                    this.showNotification('LocalStack connection successful. Services are running.', 'success');
                    return true;
                } else {
                    this.showNotification('LocalStack is responding but no services are running yet. Please wait for services to start.', 'warning');
                    return false;
                }
            } else {
                throw new Error(`Health check failed with status: ${response.status}`);
            }
        } catch (error) {
            this.showLoading(false);
            console.error('LocalStack health check failed:', error);
            
            // Check specifically for the /tmp/localstack busy error
            if (this.checkForTmpBusyError(error)) {
                return false;
            }
            
            // Check for other Docker/LocalStack issues
            this.isAvailable = false;
            this.checkDockerStatus();
            
            return false;
        }
    },
    
    /**
     * Makes an API request to LocalStack
     * @param {string} endpoint - API endpoint to call
     * @param {string} method - HTTP method (GET, POST, etc.)
     * @param {object} data - Data to send (for POST, PUT)
     * @returns {Promise} - Promise resolving to the API response
     */
    apiRequest: async function(endpoint, method = 'GET', data = null) {
        if (!this.isAvailable && this.connectionRetries >= this.maxRetries) {
            console.warn('LocalStack API not available, using localStorage fallback');
            throw new Error('LocalStack not available');
        }
        
        try {
            const url = `${this.baseUrl}${this.apiPath}${endpoint}`;
            const timeout = 3000; // 3 second timeout
            
            const options = {
                method: method,
                headers: {
                    'Content-Type': 'application/json'
                },
                timeout: timeout
            };
            
            if (data && (method === 'POST' || method === 'PUT')) {
                options.body = JSON.stringify(data);
            }
            
            console.log(`Making ${method} request to ${url}`);
            
            // Show loading indicator for long operations
            if (method !== 'GET') this.showLoading(true);
            
            const response = await Promise.race([
                fetch(url, options),
                new Promise((_, reject) => 
                    setTimeout(() => reject(new Error('LocalStack request timeout')), timeout)
                )
            ]);
            
            // Hide loading indicator
            if (method !== 'GET') this.showLoading(false);
            
            if (!response.ok) {
                throw new Error(`LocalStack API error: ${response.status}`);
            }
            
            const result = await response.json();
            
            // Reset connection status on successful request
            this.isAvailable = true;
            this.connectionRetries = 0;
            this._hasShownResetMessage = false;
            
            return result;
        } catch (error) {
            console.error('LocalStack API request failed:', error);
            
            // Hide loading indicator if it's still showing
            this.showLoading(false);
            
            // First, check specifically for the /tmp/localstack busy device error
            if (this.checkForTmpBusyError(error)) {
                // Error was handled by the specific handler
                this.isAvailable = false;
                throw error;
            }
            
            // Then check for other LocalStack/Docker availability issues
            this.connectionRetries++;
            
            if (this.connectionRetries >= this.maxRetries && !this._hasShownResetMessage) {
                this._hasShownResetMessage = true;
                this.isAvailable = false;
                this.checkDockerStatus();
            }
            
            throw error;
        }
    },
    
    /**
     * Suggest CORS setup to user
     */
    suggestCorsSetup: function() {
        if (this._hasShownCorsMessage) return;
        this._hasShownCorsMessage = true;
        
        console.warn('🚨 CORS Configuration Required for LocalStack');
        console.log('📋 To fix CORS issues:');
        console.log('1. Stop LocalStack if running');
        console.log('2. Use start-localstack-cors.cmd instead of start-localstack.cmd');
        console.log('3. Or restart LocalStack with CORS enabled:');
        console.log('   docker run --rm -it -p 4566:4566 -e CORS=* -e EXTRA_CORS_ALLOWED_ORIGINS=http://localhost:3000,file:// localstack/localstack');
        
        // Show user-friendly message in browser
        const notification = document.createElement('div');
        notification.className = 'notification error';
        notification.style.cssText = `
            position: fixed;
            top: 80px;
            right: 20px;
            max-width: 400px;
            z-index: 10000;
            padding: 15px;
            background: #f8d7da;
            border: 1px solid #f5c6cb;
            border-radius: 6px;
            color: #721c24;
            font-family: Arial, sans-serif;
            font-size: 14px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        `;
        notification.innerHTML = `
            <strong>🚨 CORS Configuration Required</strong><br>
            LocalStack is blocking browser requests. To fix this:<br>
            <ol style="margin: 10px 0; padding-left: 20px;">
                <li>Stop LocalStack</li>
                <li>Run <code>start-localstack-cors.cmd</code></li>
                <li>Refresh this page</li>
            </ol>
            <a href="#" class="notification-close" style="float: right; color: #721c24; text-decoration: none; font-weight: bold;">&times;</a>
        `;
        
        document.body.appendChild(notification);
        
        // Add close functionality
        notification.querySelector('.notification-close').addEventListener('click', function(e) {
            e.preventDefault();
            notification.remove();
        });
        
        // Auto-remove after 15 seconds
        setTimeout(() => {
            if (document.body.contains(notification)) {
                notification.remove();
            }
        }, 15000);
    },

    /**
     * Shows a notification message to the user
     * @param {string} message - Message to display
     * @param {string} type - Type of notification (success, error, warning, info)
     * @param {boolean} persistent - Whether the notification should stay until dismissed
     */
    showNotification: function(message, type = 'info', persistent = false) {
        // Create notification element if it doesn't exist
        const notification = document.createElement('div');
        notification.className = `notification ${type}`;
        notification.innerHTML = `
            ${message}
            <a href="#" class="notification-close">&times;</a>
        `;
        document.body.appendChild(notification);
        
        // Add close button functionality
        notification.querySelector('.notification-close').addEventListener('click', function() {
            notification.style.opacity = '0';
            setTimeout(() => notification.remove(), 500);
        });
        
        // Auto-hide if not persistent
        if (!persistent) {
            setTimeout(() => {
                if (document.body.contains(notification)) {
                    notification.style.opacity = '0';
                    setTimeout(() => notification.remove(), 500);
                }
            }, 7000);
        }
    },
    
    /**
     * Shows or hides a loading indicator
     * @param {boolean} show - Whether to show or hide the loading indicator
     * @param {string} message - Message to display with the loading indicator
     */
    showLoading: function(show, message = 'Loading...') {
        // Remove existing loaders first
        const existingLoader = document.getElementById('localstack-loader');
        if (existingLoader) {
            existingLoader.remove();
        }
        
        if (show) {
            // Create and show loader
            const loader = document.createElement('div');
            loader.id = 'localstack-loader';
            loader.className = 'loading-overlay';
            loader.innerHTML = `
                <div class="loading-spinner"></div>
                <div class="loading-message">${message}</div>
            `;
            document.body.appendChild(loader);
        }
    },
    
    /**
     * Check overall Docker and LocalStack status
     */
    checkDockerStatus: function() {
        // First check if Docker appears to be running
        this.checkDockerRunning().then(dockerRunning => {
            if (!dockerRunning) {
                // Docker not running, show Docker message
                this.showDockerNotRunningMessage();
            } else {
                // Docker running, but LocalStack may have issues
                // Check if it's the specific /tmp/localstack busy error
                this._hasShownTmpBusyMessage = false; // Reset so we can check again
                this.checkForTmpBusyError({
                    message: 'Checking for tmp/localstack busy error',
                    response: { 
                        detail: "Device or resource busy: '/tmp/localstack'" 
                    }
                });
                
                // If no specific error detected, show general reset message
                if (!this._hasShownTmpBusyMessage && !this._hasShownResetMessage) {
                    this.suggestDockerReset();
                }
            }
        });
    },
};
