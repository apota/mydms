describe('Appointment Scheduling Workflow', () => {
  beforeEach(() => {
    cy.login('service-advisor', 'password123');
    cy.intercept('GET', '**/api/customers*', { fixture: 'customers.json' }).as('getCustomers');
    cy.intercept('POST', '**/api/appointments', {
      statusCode: 201,
      body: {
        id: 'new-appointment-id',
        message: 'Appointment created successfully'
      }
    }).as('createAppointment');
  });
  
  it('should allow a service advisor to schedule a new appointment', () => {
    cy.fixture('serviceData').then((data) => {
      const appointment = data.appointments[0];
      
      // Navigate to new appointment page
      cy.visit('/appointments/new');
      cy.wait('@getCustomers');
      
      // Step 1: Customer Selection
      cy.get('[data-cy=customer-select]').click();
      cy.contains('John Smith').click();
      cy.get('[data-cy=customer-details]').should('contain', '555-123-4567');
      cy.get('[data-cy=next-step]').click();
      
      // Step 2: Vehicle Selection
      cy.get('[data-cy=vehicle-select]').click();
      cy.contains('Honda Accord').click();
      cy.get('[data-cy=vehicle-details]').should('contain', '1HGCV2F34LA012345');
      cy.get('[data-cy=next-step]').click();
      
      // Step 3: Service Details
      cy.get('[data-cy=appointment-type]').click();
      cy.contains(appointment.appointmentType).click();
      
      cy.get('[data-cy=date-picker]').click();
      cy.get('.MuiPickersDay-root:not(.Mui-disabled)').first().click();
      
      cy.get('[data-cy=time-select]').click();
      cy.contains('09:30 AM').click();
      
      cy.get('[data-cy=duration-select]').click();
      cy.contains('60 minutes').click();
      
      cy.get('[data-cy=customer-concerns]')
        .type(appointment.customerConcerns);
        
      cy.get('[data-cy=next-step]').click();
      
      // Step 4: Transportation & Contact
      cy.get('[data-cy=transportation-type]').click();
      cy.contains(appointment.transportationType).click();
      
      cy.get('[data-cy=contact-phone]')
        .clear()
        .type(appointment.contactPhone);
        
      cy.get('[data-cy=contact-email]')
        .clear()
        .type(appointment.contactEmail);
        
      // Submit appointment
      cy.get('[data-cy=submit-appointment]').click();
      cy.wait('@createAppointment');
      
      // Check success message
      cy.contains('Appointment created successfully').should('be.visible');
      cy.url().should('include', '/appointments');
    });
  });
  
  it('should validate required fields when scheduling an appointment', () => {
    // Navigate to new appointment page
    cy.visit('/appointments/new');
    cy.wait('@getCustomers');
    
    // Try to navigate through steps with empty fields
    cy.get('[data-cy=next-step]').click();
    cy.contains('Customer is required').should('be.visible');
    
    // Select a customer but try to proceed without a vehicle
    cy.get('[data-cy=customer-select]').click();
    cy.contains('John Smith').click();
    cy.get('[data-cy=next-step]').click();
    cy.get('[data-cy=next-step]').click();
    cy.contains('Vehicle is required').should('be.visible');
    
    // Try to submit without required service details
    cy.get('[data-cy=vehicle-select]').click();
    cy.contains('Honda Accord').click();
    cy.get('[data-cy=next-step]').click();
    cy.get('[data-cy=next-step]').click();
    cy.contains('Service type is required').should('be.visible');
    
    // Try to submit with missing contact information
    cy.get('[data-cy=appointment-type]').click();
    cy.contains('Oil Change').click();
    
    cy.get('[data-cy=date-picker]').click();
    cy.get('.MuiPickersDay-root:not(.Mui-disabled)').first().click();
    
    cy.get('[data-cy=customer-concerns]')
      .type('Need oil change');
      
    cy.get('[data-cy=next-step]').click();
    cy.get('[data-cy=submit-appointment]').click();
    cy.contains('Contact phone is required').should('be.visible');
  });
});
