// CRM Module Interactivity

document.addEventListener('DOMContentLoaded', function() {
    // Initialize loading state elements
    let loadingOverlay = document.createElement('div');
    loadingOverlay.className = 'loading-overlay';
    loadingOverlay.innerHTML = '<div class="loading-spinner"></div><p>Loading...</p>';
    loadingOverlay.style.display = 'none';
    document.body.appendChild(loadingOverlay);
    
    // Function to show/hide loading overlay
    function toggleLoading(show) {
        loadingOverlay.style.display = show ? 'flex' : 'none';
    }
    
    // Function to show connection status
    function showConnectionStatus() {
        const statusIndicator = document.createElement('div');
        statusIndicator.className = 'connection-status';
        statusIndicator.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 10px 15px;
            border-radius: 6px;
            color: white;
            font-weight: bold;
            z-index: 1000;
            font-size: 14px;
        `;
        
        // Check LocalStack connection
        LocalStackAPI.checkConnection().then(isConnected => {
            if (isConnected) {
                statusIndicator.style.background = 'linear-gradient(135deg, #4CAF50, #45a049)';
                statusIndicator.innerHTML = '🟢 LocalStack Connected';
                console.log('LocalStack database is available for customer storage');
            } else {
                statusIndicator.style.background = 'linear-gradient(135deg, #f44336, #da190b)';
                statusIndicator.innerHTML = '🔴 LocalStack Disconnected';
                console.warn('LocalStack database is not available - using local storage only');
            }
            
            document.body.appendChild(statusIndicator);
            
            // Auto-hide after 4 seconds
            setTimeout(() => {
                if (document.body.contains(statusIndicator)) {
                    statusIndicator.style.opacity = '0';
                    statusIndicator.style.transition = 'opacity 0.5s ease';
                    setTimeout(() => statusIndicator.remove(), 500);
                }
            }, 4000);
        });
    }
    
    // Show connection status on page load
    showConnectionStatus();

    // Data store initialization with LocalStack integration
    const customerStore = {
        customers: JSON.parse(localStorage.getItem('dms_customers')) || [
            { id: 1, name: 'John Smith', phone: '(555) 123-4567', email: 'john.smith@example.com', type: 'Sales', lastContact: 'June 25, 2025' },
            { id: 2, name: 'Sarah Johnson', phone: '(555) 987-6543', email: 'sarah.johnson@example.com', type: 'Service', lastContact: 'June 26, 2025' },
            { id: 3, name: 'Michael Brown', phone: '(555) 555-1234', email: 'michael.brown@example.com', type: 'Parts', lastContact: 'June 24, 2025' },
            { id: 4, name: 'Emily Wilson', phone: '(555) 777-8888', email: 'emily.wilson@example.com', type: 'Lead', lastContact: 'June 27, 2025' }
        ],
        
        // Initialize store and sync with CRM API (preferred) or LocalStack (fallback)
        init: async function() {
            try {
                toggleLoading(true);
                
                // First try CRM API
                let crmApiAvailable = false;
                if (typeof CrmAPI !== 'undefined') {
                    crmApiAvailable = await CrmAPI.checkConnection();
                }
                
                if (crmApiAvailable) {
                    console.log('Using CRM API for customer data');
                    // Fetch customers from CRM API
                    const response = await CrmAPI.getCustomers();
                    
                    if (response.success && response.data && response.data.length > 0) {
                        // Transform CRM API data to our format
                        this.customers = response.data.map(customer => ({
                            id: customer.id,
                            name: customer.name,
                            phone: customer.phone,
                            email: customer.email,
                            type: customer.customerType || customer.type || 'Sales',
                            lastContact: customer.lastContact || new Date().toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' })
                        }));
                        localStorage.setItem('dms_customers', JSON.stringify(this.customers));
                        console.log('Loaded customer data from CRM API');
                        return;
                    }
                }
                
                // Fallback to LocalStack if CRM API is not available
                const isLocalStackAvailable = await LocalStackAPI.checkConnection();
                
                if (isLocalStackAvailable) {
                    console.log('Falling back to LocalStack for customer data');
                    // Initialize the backend database first
                    const initResult = await LocalStackAPI.initializeDatabase();
                    
                    if (initResult.success) {
                        // Fetch customers from LocalStack
                        const response = await LocalStackAPI.getCustomers();
                        
                        if (response.success && response.data && response.data.length > 0) {
                            // If data was successfully fetched from LocalStack, use it
                            this.customers = response.data;
                            localStorage.setItem('dms_customers', JSON.stringify(this.customers));
                            console.log('Loaded customer data from LocalStack');
                        } else {
                            // If no data in LocalStack, sync our local data to LocalStack
                            await this.syncToLocalStack();
                        }
                    }
                } else {
                    // Neither CRM API nor LocalStack is available
                    console.warn('Neither CRM API nor LocalStack is available. Operating in localStorage-only mode.');
                    
                    // Add a notification to the UI
                    const notification = document.createElement('div');
                    notification.className = 'notification warning';
                    notification.innerHTML = `
                        <strong>Notice:</strong> CRM API and LocalStack database connections failed. 
                        Customer data is being stored in browser storage only. 
                        <a href="#" class="notification-close">&times;</a>
                    `;
                    document.body.appendChild(notification);
                    
                    // Add close button functionality
                    notification.querySelector('.notification-close').addEventListener('click', function() {
                        notification.remove();
                    });
                    
                    // Auto-hide after 10 seconds
                    setTimeout(() => {
                        if (document.body.contains(notification)) {
                            notification.style.opacity = '0';
                            setTimeout(() => notification.remove(), 500);
                        }
                    }, 10000);
                }
            } catch (error) {
                console.error("Error initializing customer store:", error);
                // If both APIs fail, just use localStorage data
            } finally {
                toggleLoading(false);
            }
        },
        
        // Sync all local customers to LocalStack (bulk upload)
        syncToLocalStack: async function() {
            try {
                for (const customer of this.customers) {
                    await LocalStackAPI.addCustomer(customer);
                }
                return true;
            } catch (error) {
                console.error("Error syncing to LocalStack:", error);
                return false;
            }
        },
        
        // Add customer to CRM API (preferred) or LocalStack (fallback) and localStorage
        addCustomer: async function(customer) {
            try {
                // Generate a new ID
                const newId = this.customers.length > 0 ? Math.max(...this.customers.map(c => c.id)) + 1 : 1;
                
                // Add the new customer with ID and current date
                const newCustomer = { 
                    id: newId, 
                    name: customer.name,
                    phone: customer.phone,
                    email: customer.email,
                    type: customer.type,
                    notes: customer.notes || '',
                    lastContact: new Date().toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' }),
                    createdAt: new Date().toISOString()
                };
                
                let response = { success: false };
                let apiUsed = 'none';
                
                // Try CRM API first
                if (typeof CrmAPI !== 'undefined') {
                    console.log('Attempting to save customer to CRM API...');
                    response = await CrmAPI.addCustomer(newCustomer);
                    
                    if (response.success) {
                        console.log('Customer added to CRM API successfully');
                        apiUsed = 'CRM';
                        
                        // Use the data returned from CRM API if available
                        const serverCustomer = response.data ? {
                            id: response.data.id,
                            name: response.data.name,
                            phone: response.data.phone,
                            email: response.data.email,
                            type: response.data.customerType || response.data.type || customer.type,
                            notes: response.data.notes || customer.notes || '',
                            lastContact: response.data.lastContact || newCustomer.lastContact,
                            createdAt: response.data.createdAt || newCustomer.createdAt
                        } : newCustomer;
                        
                        this.customers.unshift(serverCustomer);
                        localStorage.setItem('dms_customers', JSON.stringify(this.customers));
                        console.log(`Customer saved successfully using ${apiUsed} API`);
                        return { success: true, data: serverCustomer, apiUsed };
                    } else {
                        console.log('CRM API failed:', response.error);
                    }
                }
                
                // Fallback to LocalStack if CRM API failed
                if (!response.success) {
                    console.log('CRM API unavailable or failed, trying LocalStack...');
                    response = await LocalStackAPI.addCustomer(newCustomer);
                    
                    if (response.success) {
                        console.log('Customer added to LocalStack successfully');
                        apiUsed = 'LocalStack';
                        // Use the ID and data returned from LocalStack if available
                        const serverCustomer = response.data || newCustomer;
                        this.customers.unshift(serverCustomer);
                        localStorage.setItem('dms_customers', JSON.stringify(this.customers));
                        console.log(`Customer saved successfully using ${apiUsed}`);
                        return { success: true, data: serverCustomer, apiUsed };
                    } else {
                        console.log('LocalStack failed:', response.error);
                    }
                }
                
                // Both APIs failed, add to localStorage only
                if (!response.success) {
                    console.warn('Both CRM API and LocalStack failed, adding to localStorage only');
                    this.customers.unshift(newCustomer);
                    localStorage.setItem('dms_customers', JSON.stringify(this.customers));
                    apiUsed = 'localStorage';
                    return { success: true, data: newCustomer, apiUsed, warning: 'Saved locally only - database connection failed' };
                }
                
            } catch (error) {
                console.error("Error adding customer:", error);
                
                // Fallback: add to localStorage only
                const newId = this.customers.length > 0 ? Math.max(...this.customers.map(c => c.id)) + 1 : 1;
                const newCustomer = { 
                    id: newId, 
                    name: customer.name,
                    phone: customer.phone,
                    email: customer.email,
                    type: customer.type,
                    notes: customer.notes || '',
                    lastContact: new Date().toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' }),
                    createdAt: new Date().toISOString()
                };
                this.customers.unshift(newCustomer);
                localStorage.setItem('dms_customers', JSON.stringify(this.customers));
                
                return { success: true, data: newCustomer, apiUsed: 'localStorage', warning: 'Saved locally only due to database error' };
            }
        },
        
        getCustomers: function() {
            return this.customers;
        },
        
        updateCustomer: async function(id, data) {
            try {
                toggleLoading(true);
                
                // Update in LocalStack first
                const response = await LocalStackAPI.updateCustomer(id, data);
                
                // Update in localStorage
                const index = this.customers.findIndex(c => c.id === id);
                if (index !== -1) {
                    this.customers[index] = { ...this.customers[index], ...data };
                    localStorage.setItem('dms_customers', JSON.stringify(this.customers));
                    return true;
                }
                return false;
            } catch (error) {
                console.error("Error updating customer:", error);
                
                // Fallback: update in localStorage only
                const index = this.customers.findIndex(c => c.id === id);
                if (index !== -1) {
                    this.customers[index] = { ...this.customers[index], ...data };
                    localStorage.setItem('dms_customers', JSON.stringify(this.customers));
                    return true;
                }
                return false;
            } finally {
                toggleLoading(false);
            }
        },
        
        deleteCustomer: async function(id) {
            try {
                toggleLoading(true);
                
                // Delete from LocalStack first
                const response = await LocalStackAPI.deleteCustomer(id);
                
                // Delete from localStorage
                const index = this.customers.findIndex(c => c.id === id);
                if (index !== -1) {
                    this.customers.splice(index, 1);
                    localStorage.setItem('dms_customers', JSON.stringify(this.customers));
                    return true;
                }
                return false;
            } catch (error) {
                console.error("Error deleting customer:", error);
                
                // Fallback: delete from localStorage only
                const index = this.customers.findIndex(c => c.id === id);
                if (index !== -1) {
                    this.customers.splice(index, 1);
                    localStorage.setItem('dms_customers', JSON.stringify(this.customers));
                    return true;
                }
                return false;
            } finally {
                toggleLoading(false);
            }
        }
    };
    
    // Initialize the store and load data
    customerStore.init().then(() => {
        // Populate customer table initially
        populateCustomerTable(customerStore.getCustomers().slice(0, 4)); // Show only first 4 for "Recent Customers"
    });
    
    // New Customer Modal
    const newCustomerBtn = document.querySelector('.module-actions .btn-primary');
    const customerModal = document.getElementById('newCustomerModal');
    
    if (newCustomerBtn && customerModal) {
        newCustomerBtn.addEventListener('click', function() {
            customerModal.style.display = 'block';
        });
    }
    
    if (customerModal) {
        customerModal.addEventListener('click', function(e) {
            if (e.target.classList.contains('close') || e.target.classList.contains('close-modal')) {
                customerModal.style.display = 'none';
            }
        });
    }
    document.getElementById('new-customer-form')?.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        // Get form values
        const name = document.getElementById('customer-name').value.trim();
        const phone = document.getElementById('customer-phone').value.trim();
        const email = document.getElementById('customer-email').value.trim();
        const type = document.getElementById('customer-type').value;
        const notes = document.getElementById('customer-notes').value.trim();
        
        // Validate required fields
        if (!name || !phone || !email || !type) {
            const notification = document.createElement('div');
            notification.className = 'notification error';
            notification.innerHTML = `
                <strong>Validation Error:</strong> Please fill in all required fields (Name, Phone, Email, Type).
                <a href="#" class="notification-close">&times;</a>
            `;
            document.body.appendChild(notification);
            
            notification.querySelector('.notification-close').addEventListener('click', function() {
                notification.remove();
            });
            
            setTimeout(() => {
                if (document.body.contains(notification)) {
                    notification.remove();
                }
            }, 5000);
            return;
        }
        
        // Validate email format
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            const notification = document.createElement('div');
            notification.className = 'notification error';
            notification.innerHTML = `
                <strong>Validation Error:</strong> Please enter a valid email address.
                <a href="#" class="notification-close">&times;</a>
            `;
            document.body.appendChild(notification);
            
            notification.querySelector('.notification-close').addEventListener('click', function() {
                notification.remove();
            });
            
            setTimeout(() => {
                if (document.body.contains(notification)) {
                    notification.remove();
                }
            }, 5000);
            return;
        }
        
        // Show saving feedback
        const saveBtn = this.querySelector('button[type="submit"]');
        const originalText = saveBtn.textContent;
        saveBtn.textContent = 'Saving...';
        saveBtn.disabled = true;
        
        try {
            // Add new customer to data store
            const result = await customerStore.addCustomer({
                name,
                phone,
                email,
                type,
                notes
            });
            
            if (result.success) {
                // Clear and reload customers table
                const customersTable = document.querySelector('.module-section:first-of-type .data-table tbody');
                if (customersTable) {
                    // Clear table first
                    customersTable.innerHTML = '';
                    
                    // Repopulate with first 4 customers
                    populateCustomerTable(customerStore.getCustomers().slice(0, 4));
                    
                    // Increment counter for total customers
                    const totalCustomers = document.querySelector('.stat-card:first-child .number');
                    if (totalCustomers) {
                        const currentCount = parseInt(totalCustomers.textContent.replace(/,/g, ''));
                        totalCustomers.textContent = (currentCount + 1).toLocaleString();
                    }
                }
                
                // Show appropriate success message based on which API was used
                const notification = document.createElement('div');
                notification.className = 'notification success';
                let message = `<strong>Success!</strong> Customer "${name}" has been saved`;
                
                if (result.apiUsed === 'CRM') {
                    message += ' to the CRM database.';
                } else if (result.apiUsed === 'LocalStack') {
                    message += ' to the LocalStack database.';
                } else if (result.apiUsed === 'localStorage') {
                    message += ' locally. Database connection failed.';
                    notification.className = 'notification warning';
                }
                
                notification.innerHTML = `
                    ${message}
                    <a href="#" class="notification-close">&times;</a>
                `;
                document.body.appendChild(notification);
                
                // Add close button functionality
                notification.querySelector('.notification-close').addEventListener('click', function() {
                    notification.remove();
                });
                
                // Auto-hide after 5 seconds
                setTimeout(() => {
                    if (document.body.contains(notification)) {
                        notification.remove();
                    }
                }, 5000);
                
                customerModal.style.display = 'none';
                
                // Reset the form
                document.getElementById('new-customer-form').reset();
            } else {
                throw new Error(result.error || 'Unknown error occurred');
            }
        } catch (error) {
            console.error('Error saving customer:', error);
            
            // Show error message
            const notification = document.createElement('div');
            notification.className = 'notification error';
            notification.innerHTML = `
                <strong>Error:</strong> There was an issue saving the customer to the database. 
                Please check that LocalStack is running and try again.
                <a href="#" class="notification-close">&times;</a>
            `;
            document.body.appendChild(notification);
            
            // Add close button functionality
            notification.querySelector('.notification-close').addEventListener('click', function() {
                notification.remove();
            });
            
            // Auto-hide after 8 seconds
            setTimeout(() => {
                if (document.body.contains(notification)) {
                    notification.remove();
                }
            }, 8000);
        } finally {
            // Reset button state
            saveBtn.textContent = originalText;
            saveBtn.disabled = false;
        }
    });

    // Import Data
    const importBtn = document.querySelector('.module-actions .btn-secondary');
    let importModal = document.getElementById('importModal');
    if (!importModal) {
        importModal = document.createElement('div');
        importModal.id = 'importModal';
        importModal.className = 'modal';
        importModal.innerHTML = `
            <div class="modal-content">
                <span class="close">&times;</span>
                <h2>Import Customer Data</h2>
                <form id="import-form">
                    <div class="form-group">
                        <label for="import-file">Select CSV File</label>
                        <input type="file" id="import-file" accept=".csv" required>
                    </div>
                    <div class="form-actions">
                        <button type="button" class="btn btn-secondary cancel-btn">Cancel</button>
                        <button type="submit" class="btn btn-primary">Import</button>
                    </div>
                </form>
            </div>
        `;
        document.body.appendChild(importModal);
    }
    if (importBtn) {
        importBtn.addEventListener('click', function() {
            importModal.style.display = 'block';
        });
    }
    importModal.addEventListener('click', function(e) {
        if (e.target.classList.contains('close') || e.target.classList.contains('cancel-btn')) {
            importModal.style.display = 'none';
        }
    });
    document.getElementById('import-form')?.addEventListener('submit', async function(e) {
        e.preventDefault();
        const fileInput = document.getElementById('import-file');
        
        if (fileInput.files.length > 0) {
            // Show importing feedback
            const importBtn = this.querySelector('button[type="submit"]');
            const originalText = importBtn.textContent;
            importBtn.textContent = 'Importing...';
            importBtn.disabled = true;
            
            try {
                // Demo import - add sample customers
                const sampleCustomers = [
                    { name: 'Robert Williams', phone: '(555) 222-3333', email: 'robert.williams@example.com', type: 'Sales' },
                    { name: 'Amanda Garcia', phone: '(555) 444-5555', email: 'amanda.garcia@example.com', type: 'Lead' },
                    { name: 'David Lee', phone: '(555) 666-7777', email: 'david.lee@example.com', type: 'Service' }
                ];
                
                // Add sample customers to store
                let successCount = 0;
                for (const customer of sampleCustomers) {
                    try {
                        await customerStore.addCustomer(customer);
                        successCount++;
                    } catch (err) {
                        console.error(`Error importing customer ${customer.name}:`, err);
                    }
                }
                
                // Update stats
                const totalCustomers = document.querySelector('.stat-card:first-child .number');
                if (totalCustomers) {
                    const currentCount = parseInt(totalCustomers.textContent.replace(/,/g, ''));
                    totalCustomers.textContent = (currentCount + successCount).toLocaleString();
                }
                
                // Reload customer table
                const customersTable = document.querySelector('.module-section:first-of-type .data-table tbody');
                if (customersTable) {
                    customersTable.innerHTML = '';
                    populateCustomerTable(customerStore.getCustomers().slice(0, 4));
                }
                
                const message = successCount === sampleCustomers.length 
                    ? `Import successful! Added ${successCount} new customers to both LocalStack and browser storage.`
                    : `Partially successful import. Added ${successCount} of ${sampleCustomers.length} customers.`;
                    
                alert(message);
                importModal.style.display = 'none';
            } catch (error) {
                console.error('Import error:', error);
                alert('There was an error during import. Some customers may have been added.');
            } finally {
                // Reset button state
                importBtn.textContent = originalText;
                importBtn.disabled = false;
            }
        } else {
            alert('Please select a CSV file.');
        }
    });

    // View All links
    document.querySelectorAll('.view-all').forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            alert('Showing all items (Demo). In production, this would navigate to a full list.');
        });
    });

    // Complete and Reschedule
    document.querySelectorAll('.task-card .action-btn[title="Complete"]').forEach(btn => {
        btn.addEventListener('click', function() {
            btn.closest('.task-card').style.opacity = '0.5';
            btn.closest('.task-card').querySelector('h4').textContent += ' (Completed)';
            alert('Task marked as complete! (Demo)');
        });
    });
    document.querySelectorAll('.task-card .action-btn[title="Reschedule"]').forEach(btn => {
        btn.addEventListener('click', function() {
            const newTime = prompt('Enter new date/time for this task:','2025-07-01 10:00 AM');
            if (newTime) {
                btn.closest('.task-card').querySelector('.task-time').textContent = newTime;
                alert('Task rescheduled! (Demo)');
            }
        });
    });

    // Helper function to populate customer table
    function populateCustomerTable(customers) {
        const customersTable = document.querySelector('.module-section:first-of-type .data-table tbody');
        if (!customersTable) return;
        
        customers.forEach(customer => {
            const row = document.createElement('tr');
            row.setAttribute('data-id', customer.id);
            
            row.innerHTML = `
                <td>${customer.name}</td>
                <td>${customer.phone}</td>
                <td>${customer.email}</td>
                <td>${customer.type}</td>
                <td>${customer.lastContact}</td>
                <td class="actions">
                    <button class="action-btn" title="View">
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 576 512"><path d="M288 32c-80.8 0-145.5 36.8-192.6 80.6C48.6 156 17.3 208 2.5 243.7c-3.3 7.9-3.3 16.7 0 24.6C17.3 304 48.6 356 95.4 399.4C142.5 443.2 207.2 480 288 480s145.5-36.8 192.6-80.6c46.8-43.5 78.1-95.4 93-131.1c3.3-7.9 3.3-16.7 0-24.6c-14.9-35.7-46.2-87.7-93-131.1C433.5 68.8 368.8 32 288 32zM144 256a144 144 0 1 1 288 0 144 144 0 1 1 -288 0zm144-64c0 35.3-28.7 64-64 64c-7.1 0-13.9-1.2-20.3-3.3c-5.5-1.8-11.9 1.6-11.7 7.4c.3 6.9 1.3 13.8 3.2 20.7c13.7 51.2 66.4 81.6 117.6 67.9s81.6-66.4 67.9-117.6c-11.1-41.5-47.8-69.4-88.6-71.1c-5.8-.2-9.2 6.1-7.4 11.7c2.1 6.4 3.3 13.2 3.3 20.3z"/></svg>
                    </button>
                    <button class="action-btn" title="Edit">
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><path d="M471.6 21.7c-21.9-21.9-57.3-21.9-79.2 0L362.3 51.7l97.9 97.9 30.1-30.1c21.9-21.9 21.9-57.3 0-79.2L471.6 21.7zm-299.2 220c-6.1 6.1-10.8 13.6-13.5 21.9l-29.6 88.8c-2.9 8.6-.6 18.1 5.8 24.6s15.9 8.7 24.6 5.8l88.8-29.6c8.2-2.7 15.7-7.4 21.9-13.5L437.7 172.3 339.7 74.3 172.4 241.7zM96 64C43 64 0 107 0 160V416c0 53 43 96 96 96H352c53 0 96-43 96-96V320c0-17.7-14.3-32-32-32s-32 14.3-32 32v96c0 17.7-14.3 32-32 32H96c-17.7 0-32-14.3-32-32V160c0-17.7 14.3-32 32-32h96c17.7 0 32-14.3 32-32s-14.3-32-32-32H96z"/></svg>
                    </button>
                    <button class="action-btn" title="Contact">
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><path d="M164.9 24.6c-7.7-18.6-28-28.5-47.4-23.2l-88 24C12.1 30.2 0 46 0 64C0 311.4 200.6 512 448 512c18 0 33.8-12.1 38.6-29.5l24-88c5.3-19.4-4.6-39.7-23.2-47.4l-96-40c-16.3-6.8-35.2-2.1-46.3 11.6L304.7 368C234.3 334.7 177.3 277.7 144 207.3L193.3 167c13.7-11.2 18.4-30 11.6-46.3l-40-96z"/></svg>
                    </button>
                </td>
            `;
            
            customersTable.appendChild(row);
        });
        
        // Add event listeners after populating
        addCustomerActionListeners();
    }

    // Customer details View/Edit - add event listeners
    function addCustomerActionListeners() {
        document.querySelectorAll('.data-table .action-btn[title="View"]').forEach(btn => {
            btn.addEventListener('click', function() {
                const row = btn.closest('tr');
                const customerId = parseInt(row.getAttribute('data-id'));
                const customer = customerStore.getCustomers().find(c => c.id === customerId) || {
                    name: row.cells[0].textContent,
                    phone: row.cells[1].textContent,
                    email: row.cells[2].textContent,
                    type: row.cells[3].textContent
                };
                
                showCustomerDetails('view', customer);
            });
        });
        
        document.querySelectorAll('.data-table .action-btn[title="Edit"]').forEach(btn => {
            btn.addEventListener('click', function() {
                const row = btn.closest('tr');
                const customerId = parseInt(row.getAttribute('data-id'));
                const customer = customerStore.getCustomers().find(c => c.id === customerId) || {
                    name: row.cells[0].textContent,
                    phone: row.cells[1].textContent,
                    email: row.cells[2].textContent,
                    type: row.cells[3].textContent
                };
                
                showCustomerDetails('edit', customer);
            });
        });
    }

    function showCustomerDetails(mode, customer = null) {
        // Default customer data if not provided
        customer = customer || {
            id: null,
            name: "John Smith", 
            phone: "(555) 123-4567", 
            email: "john.smith@example.com", 
            type: "Sales"
        };
        
        // Create or update modal
        let detailsModal = document.getElementById('customerDetailsModal');
        if (detailsModal) {
            // If modal exists, update form values
            document.getElementById('detail-name').value = customer.name;
            document.getElementById('detail-phone').value = customer.phone;
            document.getElementById('detail-email').value = customer.email;
            document.getElementById('detail-type').value = customer.type;
            
            // Update read-only status
            const inputs = detailsModal.querySelectorAll('input');
            inputs.forEach(input => {
                if (mode === 'view') {
                    input.setAttribute('readonly', '');
                } else {
                    input.removeAttribute('readonly');
                }
            });
            
            // Update save button
            const saveBtn = detailsModal.querySelector('.btn.btn-primary');
            if (saveBtn) {
                saveBtn.style.display = mode === 'edit' ? 'inline-block' : 'none';
            }
        } else {
            // Create new modal
            detailsModal = document.createElement('div');
            detailsModal.id = 'customerDetailsModal';
            detailsModal.className = 'modal';
            detailsModal.innerHTML = `
                <div class="modal-content">
                    <span class="close">&times;</span>
                    <h2>Customer Details</h2>
                    <form id="customer-details-form">
                        <input type="hidden" id="detail-id" value="${customer.id || ''}">
                        <div class="form-group">
                            <label>Name</label>
                            <input type="text" id="detail-name" value="${customer.name}" ${mode==='view'?'readonly':''}>
                        </div>
                        <div class="form-group">
                            <label>Phone</label>
                            <input type="text" id="detail-phone" value="${customer.phone}" ${mode==='view'?'readonly':''}>
                        </div>
                        <div class="form-group">
                            <label>Email</label>
                            <input type="email" id="detail-email" value="${customer.email}" ${mode==='view'?'readonly':''}>
                        </div>
                        <div class="form-group">
                            <label>Type</label>
                            <input type="text" id="detail-type" value="${customer.type}" ${mode==='view'?'readonly':''}>
                        </div>
                        <div class="form-actions">
                            <button type="button" class="btn btn-secondary cancel-btn">Close</button>
                            ${mode==='edit'?'<button type="submit" class="btn btn-primary">Save</button>':''}
                        </div>
                    </form>
                </div>
            `;
            document.body.appendChild(detailsModal);
            
            // Add event listeners to new modal
            detailsModal.addEventListener('click', function(e) {
                if (e.target.classList.contains('close') || e.target.classList.contains('cancel-btn')) {
                    detailsModal.style.display = 'none';
                }
            });
            
            document.getElementById('customer-details-form')?.addEventListener('submit', async function(e) {
                e.preventDefault();
                
                // Get updated data
                const updatedCustomer = {
                    id: document.getElementById('detail-id').value ? parseInt(document.getElementById('detail-id').value) : null,
                    name: document.getElementById('detail-name').value,
                    phone: document.getElementById('detail-phone').value,
                    email: document.getElementById('detail-email').value,
                    type: document.getElementById('detail-type').value
                };
                
                // Show saving feedback
                const saveBtn = this.querySelector('.btn.btn-primary');
                const originalText = saveBtn.textContent;
                saveBtn.textContent = 'Saving...';
                saveBtn.disabled = true;
                
                try {
                    // Update in data store if ID exists
                    if (updatedCustomer.id) {
                        await customerStore.updateCustomer(updatedCustomer.id, updatedCustomer);
                        
                        // Refresh the table
                        const customersTable = document.querySelector('.module-section:first-of-type .data-table tbody');
                        if (customersTable) {
                            customersTable.innerHTML = '';
                            populateCustomerTable(customerStore.getCustomers().slice(0, 4));
                        }
                        
                        alert('Customer details saved to both LocalStack and browser storage!');
                        detailsModal.style.display = 'none';
                    } else {
                        alert('Error: Customer ID not found.');
                    }
                } catch (error) {
                    console.error('Error updating customer:', error);
                    alert('There was an error updating the customer in LocalStack. Changes saved to browser storage only.');
                    detailsModal.style.display = 'none';
                } finally {
                    // Reset button state
                    saveBtn.textContent = originalText;
                    saveBtn.disabled = false;
                }
            });
        }
        
        // Show modal
        detailsModal.style.display = 'block';
    }
});
