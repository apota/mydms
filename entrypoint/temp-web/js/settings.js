document.addEventListener('DOMContentLoaded', function() {
    // Settings page functionality
    const tabBtns = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');
    const settingsForms = document.querySelectorAll('form[id$="-form"]');
    
    // Tab switching functionality
    if (tabBtns.length > 0) {
        tabBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                const tabId = this.getAttribute('data-tab');
                
                // Update active tab button
                tabBtns.forEach(b => b.classList.remove('active'));
                this.classList.add('active');
                
                // Show selected tab content, hide others
                tabContents.forEach(content => {
                    if (content.id === tabId + '-tab') {
                        content.classList.remove('hidden');
                    } else {
                        content.classList.add('hidden');
                    }
                });
            });
        });
    }
    
    // Form submission handling
    if (settingsForms.length > 0) {
        settingsForms.forEach(form => {
            form.addEventListener('submit', function(e) {
                e.preventDefault();
                
                // Get form ID to know which settings we're saving
                const formId = this.id;
                const formData = new FormData(this);
                const settings = {};
                
                // Convert form data to object
                for (let [key, value] of formData.entries()) {
                    settings[key] = value;
                }
                
                // In a real implementation, this would save to the server
                // For demo purposes, just show alert with the settings
                const formType = formId.replace('-settings-form', '').replace('-form', '');
                saveSettings(formType, settings);
            });
        });
    }
    
    // Test SMTP connection button
    const testSmtpBtn = document.getElementById('test-smtp');
    if (testSmtpBtn) {
        testSmtpBtn.addEventListener('click', function(e) {
            e.preventDefault();
            // Demo functionality
            alert('SMTP connection test: Success! Email settings are working correctly.');
        });
    }
    
    // Test API connection button
    const testApiBtn = document.getElementById('test-api-connection');
    if (testApiBtn) {
        testApiBtn.addEventListener('click', function(e) {
            e.preventDefault();
            // Demo functionality
            alert('API connection test: Success! Integration settings are working correctly.');
        });
    }
    
    // Generate backup button
    const generateBackupBtn = document.getElementById('generate-backup');
    if (generateBackupBtn) {
        generateBackupBtn.addEventListener('click', function(e) {
            e.preventDefault();
            // Demo functionality
            alert('Generating backup... This would download a backup file in a production environment.');
        });
    }
    
    // Upload logo functionality
    const logoUpload = document.getElementById('logo-upload');
    const logoPreview = document.getElementById('logo-preview');
    
    if (logoUpload && logoPreview) {
        logoUpload.addEventListener('change', function() {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    logoPreview.src = e.target.result;
                    logoPreview.style.display = 'block';
                };
                reader.readAsDataURL(file);
            }
        });
    }
    
    // Functions
    function saveSettings(type, settings) {
        // Demo functionality - would connect to API in production
        console.log(`Saving ${type} settings:`, settings);
        
        // Show success message
        alert(`${type.charAt(0).toUpperCase() + type.slice(1)} settings saved successfully.`);
    }
});
