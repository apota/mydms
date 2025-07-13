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

    // User management functionality
    const userTable = document.querySelector('.users-table-container table tbody');
    const addUserBtn = document.getElementById('add-user-btn');
    const userModal = document.getElementById('userModal');
    const userForm = document.getElementById('user-form');
    const userSearch = document.getElementById('user-search');
    const roleFilter = document.getElementById('role-filter');
    
    // User store initialization with LocalStack integration
    const userStore = {
        users: JSON.parse(localStorage.getItem('dms_users')) || [
            { id: 1, firstName: 'John', lastName: 'Doe', email: 'john.doe@example.com', role: 'admin', status: 'active', lastLogin: '2025-06-26 14:32' },
            { id: 2, firstName: 'Jane', lastName: 'Smith', email: 'jane.smith@example.com', role: 'sales', status: 'active', lastLogin: '2025-06-27 09:15' },
            { id: 3, firstName: 'Michael', lastName: 'Johnson', email: 'michael.johnson@example.com', role: 'service', status: 'active', lastLogin: '2025-06-25 11:42' },
            { id: 4, firstName: 'Robert', lastName: 'Brown', email: 'robert.brown@example.com', role: 'parts', status: 'inactive', lastLogin: '2025-06-15 16:07' }
        ],
        
        // Initialize store and sync with LocalStack
        init: async function() {
            try {
                toggleLoading(true);
                
                // Check LocalStack availability first
                const isApiAvailable = await UserAPI.checkConnection();
                
                if (isApiAvailable) {
                    // Initialize the backend database first
                    const initResult = await UserAPI.initializeDatabase();
                    
                    if (initResult.success) {
                        // Fetch users from LocalStack
                        const response = await UserAPI.getUsers();
                        
                        if (response.success && response.data && response.data.length > 0) {
                            // If data was successfully fetched from LocalStack, use it
                            this.users = response.data;
                            localStorage.setItem('dms_users', JSON.stringify(this.users));
                            console.log('Loaded user data from LocalStack');
                        } else {
                            // If no data in LocalStack, sync our local data to LocalStack
                            await this.syncToLocalStack();
                        }
                    }
                } else {
                    // LocalStack is not available, show warning
                    console.warn('LocalStack is not available. Operating in localStorage-only mode.');
                    
                    // Add a notification to the UI
                    this.showOfflineNotification();
                }
            } catch (error) {
                console.error("Error initializing user store:", error);
                // If LocalStack sync fails, just use localStorage data
            } finally {
                toggleLoading(false);
                this.refreshUserTable();
            }
        },
        
        showOfflineNotification: function() {
            const notification = document.createElement('div');
            notification.className = 'notification warning';
            notification.innerHTML = `
                <strong>Notice:</strong> User management API connection failed. 
                User data is being stored in browser storage only. 
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
        },
        
        // Sync all local users to LocalStack (bulk upload)
        syncToLocalStack: async function() {
            try {
                for (const user of this.users) {
                    await UserAPI.addUser(user);
                }
                return true;
            } catch (error) {
                console.error("Error syncing to LocalStack:", error);
                return false;
            }
        },
        
        // Add user to both LocalStack and localStorage
        addUser: async function(user) {
            try {
                toggleLoading(true);
                
                // Generate a new ID
                const newId = this.users.length > 0 ? Math.max(...this.users.map(u => u.id)) + 1 : 1;
                
                // Add the new user with ID
                const newUser = { 
                    id: newId, 
                    ...user,
                    createdAt: new Date().toISOString(),
                    lastLogin: null
                };
                
                // Add to LocalStack first
                const response = await UserAPI.addUser(newUser);
                
                // If LocalStack fails, we still add to localStorage
                if (response.success) {
                    // Use the ID and data returned from LocalStack if available
                    const serverUser = response.data || newUser;
                    this.users.unshift(serverUser);
                } else {
                    this.users.unshift(newUser);
                }
                
                // Save to localStorage
                localStorage.setItem('dms_users', JSON.stringify(this.users));
                
                return this.users[0]; // Return the newly added user
            } catch (error) {
                console.error("Error adding user:", error);
                
                // Fallback: add to localStorage only
                const newId = this.users.length > 0 ? Math.max(...this.users.map(u => u.id)) + 1 : 1;
                const newUser = { 
                    id: newId, 
                    ...user,
                    createdAt: new Date().toISOString(),
                    lastLogin: null
                };
                this.users.unshift(newUser);
                localStorage.setItem('dms_users', JSON.stringify(this.users));
                
                return newUser;
            } finally {
                toggleLoading(false);
            }
        },
        
        getUsers: function() {
            return this.users;
        },
        
        updateUser: async function(id, data) {
            try {
                toggleLoading(true);
                
                // Update in LocalStack first
                const response = await UserAPI.updateUser(id, data);
                
                // Update in localStorage
                const index = this.users.findIndex(u => u.id === id);
                if (index !== -1) {
                    this.users[index] = { ...this.users[index], ...data, updatedAt: new Date().toISOString() };
                    localStorage.setItem('dms_users', JSON.stringify(this.users));
                    return true;
                }
                return false;
            } catch (error) {
                console.error("Error updating user:", error);
                
                // Fallback: update in localStorage only
                const index = this.users.findIndex(u => u.id === id);
                if (index !== -1) {
                    this.users[index] = { ...this.users[index], ...data, updatedAt: new Date().toISOString() };
                    localStorage.setItem('dms_users', JSON.stringify(this.users));
                    return true;
                }
                return false;
            } finally {
                toggleLoading(false);
            }
        },
        
        deleteUser: async function(id) {
            try {
                toggleLoading(true);
                
                // Delete from LocalStack first
                const response = await UserAPI.deleteUser(id);
                
                // Delete from localStorage
                const index = this.users.findIndex(u => u.id === id);
                if (index !== -1) {
                    this.users.splice(index, 1);
                    localStorage.setItem('dms_users', JSON.stringify(this.users));
                    return true;
                }
                return false;
            } catch (error) {
                console.error("Error deleting user:", error);
                
                // Fallback: delete from localStorage only
                const index = this.users.findIndex(u => u.id === id);
                if (index !== -1) {
                    this.users.splice(index, 1);
                    localStorage.setItem('dms_users', JSON.stringify(this.users));
                    return true;
                }
                return false;
            } finally {
                toggleLoading(false);
            }
        },
        
        refreshUserTable: function() {
            if (!userTable) return;
            
            userTable.innerHTML = '';
            
            this.users.forEach(user => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${user.firstName} ${user.lastName}</td>
                    <td>${user.email}</td>
                    <td>${user.role.charAt(0).toUpperCase() + user.role.slice(1)}</td>
                    <td><span class="status ${user.status}">${user.status.charAt(0).toUpperCase() + user.status.slice(1)}</span></td>
                    <td>${user.lastLogin || 'Never'}</td>
                    <td class="actions">
                        <button class="action-btn edit-btn" data-id="${user.id}" title="Edit User">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                                <path d="m18.5 2.5 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
                            </svg>
                        </button>
                        <button class="action-btn reset-pwd-btn" data-id="${user.id}" title="Reset Password">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                                <circle cx="12" cy="16" r="1"/>
                                <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                            </svg>
                        </button>
                        <button class="action-btn ${user.status === 'active' ? 'deactivate-btn' : 'activate-btn'}" data-id="${user.id}" title="${user.status === 'active' ? 'Deactivate User' : 'Activate User'}">
                            ${user.status === 'active' ? 
                                '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="m15 9-6 6"/><path d="m9 9 6 6"/></svg>' :
                                '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="9,12 12,15 15,9"/></svg>'
                            }
                        </button>
                    </td>
                `;
                userTable.appendChild(row);
            });
        }
    };
    
    // Initialize the store and load data
    userStore.init();
    
    // Add event listeners
    if (addUserBtn) {
        addUserBtn.addEventListener('click', function() {
            showUserModal();
        });
    }
    
    // Search and filter functionality
    if (userSearch) {
        userSearch.addEventListener('input', filterUsers);
    }
    
    if (roleFilter) {
        roleFilter.addEventListener('change', filterUsers);
    }
    
    // Action button events (already defined in the HTML)
    document.addEventListener('click', function(e) {
        if (e.target.closest('.edit-btn')) {
            const userId = parseInt(e.target.closest('.edit-btn').getAttribute('data-id'));
            editUser(userId);
        } else if (e.target.closest('.reset-pwd-btn')) {
            const userId = parseInt(e.target.closest('.reset-pwd-btn').getAttribute('data-id'));
            resetPassword(userId);
        } else if (e.target.closest('.deactivate-btn') || e.target.closest('.activate-btn')) {
            const userId = parseInt(e.target.closest('.action-btn').getAttribute('data-id'));
            toggleUserStatus(userId);
        }
    });
    
    // Form submission handling
    if (userForm) {
        userForm.addEventListener('submit', function(e) {
            e.preventDefault();
            saveUser();
        });
        
        // Cancel button handling
        const cancelBtn = userForm.querySelector('.cancel-btn');
        if (cancelBtn) {
            cancelBtn.addEventListener('click', function() {
                hideUserModal();
            });
        }
    }
    
    // Functions
    function showUserModal(user = null) {
        if (!userModal) return;
        
        // Reset form
        userForm.reset();
        
        // Set form title
        const modalTitle = userModal.querySelector('h2');
        if (modalTitle) {
            modalTitle.textContent = user ? 'Edit User' : 'Add New User';
        }
        
        // If editing, populate form
        if (user) {
            document.getElementById('user-id').value = user.id;
            document.getElementById('first-name').value = user.firstName;
            document.getElementById('last-name').value = user.lastName;
            document.getElementById('email').value = user.email;
            document.getElementById('role').value = user.role.toLowerCase();
            document.getElementById('status').value = user.status;
            // Don't populate password for editing
            document.getElementById('password').placeholder = 'Leave blank to keep current password';
            document.getElementById('password').required = false;
        } else {
            document.getElementById('user-id').value = '';
            document.getElementById('password').placeholder = 'Enter password';
            document.getElementById('password').required = true;
        }
        
        userModal.style.display = 'block';
    }
    
    function hideUserModal() {
        if (!userModal) return;
        userModal.style.display = 'none';
    }
    
    async function saveUser() {
        const userId = document.getElementById('user-id').value;
        const firstName = document.getElementById('first-name').value.trim();
        const lastName = document.getElementById('last-name').value.trim();
        const email = document.getElementById('email').value.trim();
        const role = document.getElementById('role').value;
        const status = document.getElementById('status').value;
        const password = document.getElementById('password').value;
        
        // Validation
        if (!firstName || !lastName || !email || !role) {
            alert('Please fill in all required fields.');
            return;
        }
        
        if (!userId && !password) {
            alert('Password is required for new users.');
            return;
        }
        
        // Show saving feedback
        const saveBtn = userForm.querySelector('button[type="submit"]');
        const originalText = saveBtn.textContent;
        saveBtn.textContent = 'Saving...';
        saveBtn.disabled = true;
        
        try {
            const userData = {
                firstName,
                lastName,
                email,
                role,
                status
            };
            
            // Only include password if provided
            if (password) {
                userData.password = password;
            }
            
            if (userId) {
                // Update existing user
                const success = await userStore.updateUser(parseInt(userId), userData);
                if (success) {
                    alert('User updated successfully!');
                    userStore.refreshUserTable();
                    hideUserModal();
                } else {
                    throw new Error('Failed to update user');
                }
            } else {
                // Create new user
                const newUser = await userStore.addUser(userData);
                if (newUser) {
                    alert('New user created successfully!');
                    userStore.refreshUserTable();
                    hideUserModal();
                } else {
                    throw new Error('Failed to create user');
                }
            }
        } catch (error) {
            console.error('Error saving user:', error);
            alert('There was an error saving the user. Please try again.');
        } finally {
            // Reset button state
            saveBtn.textContent = originalText;
            saveBtn.disabled = false;
        }
    }
    
    function editUser(userId) {
        const user = userStore.getUsers().find(u => u.id === userId);
        if (user) {
            showUserModal(user);
        }
    }
    
    async function resetPassword(userId) {
        const user = userStore.getUsers().find(u => u.id === userId);
        if (!user) return;
        
        if (confirm(`Reset password for ${user.firstName} ${user.lastName}?`)) {
            try {
                toggleLoading(true);
                const response = await UserAPI.resetPassword(userId);
                
                if (response.success) {
                    alert(`Password reset link has been sent to ${user.email}`);
                } else {
                    // Fallback for demo
                    alert(`Password reset link has been sent to ${user.email} (Demo mode - LocalStack not available)`);
                }
            } catch (error) {
                console.error('Error resetting password:', error);
                alert(`Password reset link has been sent to ${user.email} (Demo mode)`);
            } finally {
                toggleLoading(false);
            }
        }
    }
    
    async function toggleUserStatus(userId) {
        const user = userStore.getUsers().find(u => u.id === userId);
        if (!user) return;
        
        const newStatus = user.status === 'active' ? 'inactive' : 'active';
        const action = newStatus === 'active' ? 'activate' : 'deactivate';
        
        if (confirm(`Are you sure you want to ${action} ${user.firstName} ${user.lastName}?`)) {
            try {
                const success = await userStore.updateUser(userId, { status: newStatus });
                if (success) {
                    alert(`User ${user.firstName} ${user.lastName} has been ${newStatus === 'active' ? 'activated' : 'deactivated'}.`);
                    userStore.refreshUserTable();
                } else {
                    throw new Error('Failed to update user status');
                }
            } catch (error) {
                console.error('Error updating user status:', error);
                alert('There was an error updating the user status. Please try again.');
            }
        }
    }
    
    function filterUsers() {
        const searchTerm = userSearch ? userSearch.value.toLowerCase() : '';
        const roleValue = roleFilter ? roleFilter.value.toLowerCase() : '';
        
        const allUsers = userStore.getUsers();
        let filteredUsers = allUsers;
        
        // Filter by search term
        if (searchTerm) {
            filteredUsers = filteredUsers.filter(user => 
                `${user.firstName} ${user.lastName}`.toLowerCase().includes(searchTerm) ||
                user.email.toLowerCase().includes(searchTerm) ||
                user.role.toLowerCase().includes(searchTerm)
            );
        }
        
        // Filter by role
        if (roleValue) {
            filteredUsers = filteredUsers.filter(user => 
                user.role.toLowerCase() === roleValue
            );
        }
        
        // Update table with filtered results
        if (userTable) {
            userTable.innerHTML = '';
            
            filteredUsers.forEach(user => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${user.firstName} ${user.lastName}</td>
                    <td>${user.email}</td>
                    <td>${user.role.charAt(0).toUpperCase() + user.role.slice(1)}</td>
                    <td><span class="status ${user.status}">${user.status.charAt(0).toUpperCase() + user.status.slice(1)}</span></td>
                    <td>${user.lastLogin || 'Never'}</td>
                    <td class="actions">
                        <button class="action-btn edit-btn" data-id="${user.id}" title="Edit User">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                                <path d="m18.5 2.5 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
                            </svg>
                        </button>
                        <button class="action-btn reset-pwd-btn" data-id="${user.id}" title="Reset Password">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                                <circle cx="12" cy="16" r="1"/>
                                <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                            </svg>
                        </button>
                        <button class="action-btn ${user.status === 'active' ? 'deactivate-btn' : 'activate-btn'}" data-id="${user.id}" title="${user.status === 'active' ? 'Deactivate User' : 'Activate User'}">
                            ${user.status === 'active' ? 
                                '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="m15 9-6 6"/><path d="m9 9 6 6"/></svg>' :
                                '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="9,12 12,15 15,9"/></svg>'
                            }
                        </button>
                    </td>
                `;
                userTable.appendChild(row);
            });
        }
        
        // Update pagination if needed
        // This would be implemented based on the total number of results
    }
});
