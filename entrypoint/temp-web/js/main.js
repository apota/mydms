document.addEventListener('DOMContentLoaded', function() {
    // Check if user is logged in (for demo purposes)
    const isLoggedIn = sessionStorage.getItem('isLoggedIn') === 'true';
    
    if ((window.location.pathname.includes('index.html') || 
         window.location.pathname.endsWith('/') || 
         window.location.pathname.endsWith('temp-web') || 
         window.location.pathname.includes('modules/') || 
         window.location.pathname.includes('user-management.html') || 
         window.location.pathname.includes('settings.html')) && !isLoggedIn) {
        // Get the current path to redirect back after login
        const currentPath = window.location.pathname;
        sessionStorage.setItem('redirectAfterLogin', currentPath);
        
        // Redirect to dedicated login service
        window.location.href = 'http://localhost:3004/login';
    }
    
    // Handle logout
    const logoutButton = document.getElementById('logout-btn');
    if (logoutButton) {
        logoutButton.addEventListener('click', function() {
            // Clear login status
            sessionStorage.removeItem('isLoggedIn');
            sessionStorage.removeItem('currentUser');
            
            // Redirect to login
            const loginPath = window.location.pathname.includes('modules/') ? '../login.html' : 'login.html';
            window.location.href = loginPath;
        });
    }
    
    // Display current user
    const userDisplay = document.getElementById('current-user');
    if (userDisplay && isLoggedIn) {
        userDisplay.textContent = sessionStorage.getItem('currentUser') || 'User';
    }
    
    // Module navigation
    const moduleLinks = document.querySelectorAll('.module-link');
    moduleLinks.forEach(link => {
        // If the link has an href attribute, use that instead of showing the alert
        if (link.hasAttribute('href') && link.getAttribute('href') !== '#') {
            // No action needed as the natural link behavior will take place
        } else {
            link.addEventListener('click', function(e) {
                e.preventDefault();
                const module = this.getAttribute('data-module');
                
                // Determine the path to the module page
                const modulePath = window.location.pathname.includes('modules/') ? 
                    `${module}.html` : 
                    `modules/${module}.html`;
                
                // Navigate to the module page or show alert if page doesn't exist
                try {
                    window.location.href = modulePath;
                } catch (error) {
                    alert(`Navigating to ${module} module. This would connect to ${module}-api in a real implementation.`);
                }
            });
        }
    });
    
    // Modal functionality
    const modals = document.querySelectorAll('.modal');
    const modalTriggers = document.querySelectorAll('[data-modal]');
    const closeBtns = document.querySelectorAll('.close');
    
    modalTriggers.forEach(trigger => {
        trigger.addEventListener('click', function() {
            const modalId = this.getAttribute('data-modal');
            document.getElementById(modalId).style.display = 'block';
        });
    });
    
    closeBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            const modal = this.closest('.modal');
            modal.style.display = 'none';
        });
    });
    
    window.addEventListener('click', function(e) {
        modals.forEach(modal => {
            if (e.target === modal) {
                modal.style.display = 'none';
            }
        });
    });
    
    // Global Search Functionality
    const searchInput = document.getElementById('global-search');
    const searchBtn = document.getElementById('search-btn');
    const searchResultsModal = document.getElementById('searchResultsModal');
    
    if (searchBtn) {
        searchBtn.addEventListener('click', function() {
            performSearch();
        });
    }
    
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                performSearch();
            }
        });
    }
    
    function performSearch() {
        if (!searchInput || !searchInput.value.trim()) return;
        
        const query = searchInput.value.trim();
        
        // If we have a search results modal, show it
        if (searchResultsModal) {
            // Populate search results (demo data)
            const resultsContainer = searchResultsModal.querySelector('.search-results');
            if (resultsContainer) {
                resultsContainer.innerHTML = generateSearchResults(query);
            }
            
            searchResultsModal.style.display = 'block';
        } else {
            // Otherwise just alert
            alert(`Searching for: "${query}". In a real implementation, this would query the DMS API for results.`);
        }
    }
    
    function generateSearchResults(query) {
        // Demo data - in a real implementation, this would come from an API
        return `
            <div class="search-result-group">
                <h3>Customers (3 results)</h3>
                <div class="search-result-item">
                    <div class="result-title">John Smith</div>
                    <div class="result-details">Customer #12345 | Last Purchase: June 15, 2025</div>
                </div>
                <div class="search-result-item">
                    <div class="result-title">John Doe</div>
                    <div class="result-details">Customer #54321 | Last Service: June 20, 2025</div>
                </div>
                <div class="search-result-item">
                    <div class="result-title">Johnny Walker</div>
                    <div class="result-details">Lead | Status: New | Source: Website</div>
                </div>
            </div>
            
            <div class="search-result-group">
                <h3>Vehicles (2 results)</h3>
                <div class="search-result-item">
                    <div class="result-title">2023 SUV - VIN: 1HGCM82633A123456</div>
                    <div class="result-details">Status: In Stock | Location: Main Lot</div>
                </div>
                <div class="search-result-item">
                    <div class="result-title">2022 Sedan - VIN: 1HGCM82633A654321</div>
                    <div class="result-details">Status: Sold | Owner: John Smith</div>
                </div>
            </div>
        `;
    }
    
    // Rudy AI Assistant
    const rudyBtn = document.getElementById('rudy-btn');
    const rudyModal = document.getElementById('rudyModal');
    const rudyPrompt = document.getElementById('rudy-prompt');
    const rudySend = document.getElementById('rudy-send');
    const rudyConversation = document.querySelectorAll('.rudy-conversation');
    
    if (rudyBtn && rudyModal) {
        rudyBtn.addEventListener('click', function() {
            rudyModal.style.display = 'block';
            if (rudyPrompt) rudyPrompt.focus();
        });
    }
    
    function addRudyResponse(prompt, response) {
        rudyConversation.forEach(container => {
            // Add user message
            const userMessage = document.createElement('div');
            userMessage.className = 'rudy-message user-message';
            userMessage.innerHTML = `
                <div class="message-content user">
                    ${prompt}
                </div>
            `;
            container.appendChild(userMessage);
            
            // Add Rudy's response
            const rudyMessage = document.createElement('div');
            rudyMessage.className = 'rudy-message';
            rudyMessage.innerHTML = `
                <div class="rudy-avatar small">
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 640 512"><path d="M320 0c17.7 0 32 14.3 32 32V96H472c39.8 0 72 32.2 72 72V440c0 39.8-32.2 72-72 72H168c-39.8 0-72-32.2-72-72V168c0-39.8 32.2-72 72-72H288V32c0-17.7 14.3-32 32-32zM208 384c-8.8 0-16 7.2-16 16s7.2 16 16 16h32c8.8 0 16-7.2 16-16s-7.2-16-16-16H208zm96 0c-8.8 0-16 7.2-16 16s7.2 16 16 16h32c8.8 0 16-7.2 16-16s-7.2-16-16-16H304zm96 0c-8.8 0-16 7.2-16 16s7.2 16 16 16h32c8.8 0 16-7.2 16-16s-7.2-16-16-16H400zM264 256a40 40 0 1 0 -80 0 40 40 0 1 0 80 0zm152 40a40 40 0 1 0 0-80 40 40 0 1 0 0 80z"/></svg>
                </div>
                <div class="message-content">
                    ${response}
                </div>
            `;
            container.appendChild(rudyMessage);
            
            // Scroll to bottom
            container.scrollTop = container.scrollHeight;
        });
    }
    
    if (rudyPrompt && rudySend) {
        rudySend.addEventListener('click', sendRudyPrompt);
        rudyPrompt.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                sendRudyPrompt();
            }
        });
    }
    
    function sendRudyPrompt() {
        if (!rudyPrompt || !rudyPrompt.value.trim()) return;
        
        const prompt = rudyPrompt.value.trim();
        let response = "";
        
        // Simple demo responses based on keywords
        if (prompt.toLowerCase().includes('hello') || prompt.toLowerCase().includes('hi')) {
            response = "Hello! How can I assist you with the dealership management system today?";
        } else if (prompt.toLowerCase().includes('customer') || prompt.toLowerCase().includes('crm')) {
            response = "Our CRM module lets you manage all customer interactions. You can access customer records, track communication history, and manage leads. Would you like me to show you how to add a new customer?";
        } else if (prompt.toLowerCase().includes('inventory') || prompt.toLowerCase().includes('vehicle')) {
            response = "The Inventory Management module helps you track all vehicles in stock. You can view vehicle details, check availability, and manage inventory levels. Is there something specific about inventory management you'd like to know?";
        } else if (prompt.toLowerCase().includes('sales')) {
            response = "The Sales Management module handles the entire sales process from lead to closing. You can create quotes, manage deals, and track sales performance. Do you need help with a specific sales task?";
        } else if (prompt.toLowerCase().includes('service')) {
            response = "Our Service Management module helps schedule appointments, track repairs, and manage technician workload. Would you like to know how to create a new service order?";
        } else if (prompt.toLowerCase().includes('parts')) {
            response = "The Parts Management module tracks all parts inventory, handles orders, and manages supplier relationships. Is there a specific parts management task you need assistance with?";
        } else if (prompt.toLowerCase().includes('report')) {
            response = "The Reporting & Analytics module provides comprehensive business intelligence. You can generate reports on sales, inventory, service, and customer trends. What kind of report are you looking for?";
        } else if (prompt.toLowerCase().includes('login') || prompt.toLowerCase().includes('password')) {
            response = "For login issues, please use any username and password in the demo. In a production environment, you would contact your system administrator for access.";
        } else {
            response = "I'm here to help with any aspect of the Dealership Management System. You can ask me about customer management, inventory, sales, service, parts, or reporting functionalities. Is there something specific you'd like to know?";
        }
        
        addRudyResponse(prompt, response);
        rudyPrompt.value = '';
    }
    
    // Handle search and tab functionality in settings
    const searchTabs = document.querySelectorAll('.search-tab-btn');
    if (searchTabs) {
        searchTabs.forEach(tab => {
            tab.addEventListener('click', function() {
                const tabType = this.getAttribute('data-tab');
                
                // Update tab active state
                searchTabs.forEach(t => t.classList.remove('active'));
                this.classList.add('active');
                
                // Here you would filter results based on the tab
                alert(`Filtering results to show only ${tabType} matches. This would be implemented in a production version.`);
            });
        });
    }
});
