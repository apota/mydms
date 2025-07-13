// ***********************************************
// This commands file contains custom Cypress commands
// ***********************************************

// Login command
Cypress.Commands.add('login', (username, password) => {
  cy.session([username, password], () => {
    cy.visit('/login');
    cy.get('[data-cy=username-input]').type(username);
    cy.get('[data-cy=password-input]').type(password);
    cy.get('[data-cy=login-button]').click();
    cy.url().should('not.include', '/login');
  });
});

// Create an appointment
Cypress.Commands.add('createAppointment', (appointmentDetails) => {
  cy.visit('/appointments/new');
  cy.get('[data-cy=customer-select]').click();
  cy.contains(appointmentDetails.customerName).click();
  cy.get('[data-cy=vehicle-select]').click();
  cy.contains(appointmentDetails.vehicleInfo).click();
  cy.get('[data-cy=appointment-type]').click();
  cy.contains(appointmentDetails.appointmentType).click();
  cy.get('[data-cy=date-picker]').click();
  cy.get(`.MuiPickersDay-root[aria-label*="${appointmentDetails.date}"]`).click();
  cy.get('[data-cy=time-select]').click();
  cy.contains(appointmentDetails.time).click();
  cy.get('[data-cy=customer-concerns]').type(appointmentDetails.customerConcerns);
  cy.get('[data-cy=transportation-type]').click();
  cy.contains(appointmentDetails.transportationType).click();
  cy.get('[data-cy=contact-phone]').type(appointmentDetails.contactPhone);
  cy.get('[data-cy=contact-email]').type(appointmentDetails.contactEmail);
  cy.get('[data-cy=submit-appointment]').click();
  cy.contains('Appointment created successfully').should('be.visible');
});

// Create an inspection
Cypress.Commands.add('createInspection', (repairOrderId, inspectionDetails) => {
  cy.visit(`/repair-orders/${repairOrderId}`);
  cy.get('[data-cy=create-inspection]').click();

  // Fill out inspection points
  inspectionDetails.points.forEach(point => {
    cy.get(`[data-cy=inspection-point-${point.id}] [data-cy=result-select]`).click();
    cy.contains(point.result).click();
    
    if (point.notes) {
      cy.get(`[data-cy=inspection-point-${point.id}] [data-cy=notes-input]`).type(point.notes);
    }
  });

  // Add recommended services
  inspectionDetails.services.forEach(service => {
    cy.get('[data-cy=service-description]').type(service.description);
    cy.get('[data-cy=service-urgency]').click();
    cy.contains(service.urgency).click();
    cy.get('[data-cy=service-price]').clear().type(service.price);
    cy.get('[data-cy=add-service]').click();
  });

  // Add general notes
  if (inspectionDetails.generalNotes) {
    cy.get('[data-cy=general-notes]').type(inspectionDetails.generalNotes);
  }

  // Submit the inspection
  cy.get('[data-cy=submit-inspection]').click();
  cy.contains('Inspection completed successfully').should('be.visible');
});

// Create a service job
Cypress.Commands.add('createServiceJob', (repairOrderId, jobDetails) => {
  cy.visit(`/repair-orders/${repairOrderId}`);
  cy.get('[data-cy=add-service-job]').click();
  cy.get('[data-cy=job-description]').type(jobDetails.description);
  cy.get('[data-cy=job-type]').click();
  cy.contains(jobDetails.jobType).click();
  cy.get('[data-cy=labor-code]').type(jobDetails.laborCode);
  cy.get('[data-cy=est-hours]').type(jobDetails.estHours);
  
  // Select technician if provided
  if (jobDetails.technician) {
    cy.get('[data-cy=technician-select]').click();
    cy.contains(jobDetails.technician).click();
  }
  
  // Add parts if provided
  if (jobDetails.parts && jobDetails.parts.length > 0) {
    jobDetails.parts.forEach(part => {
      cy.get('[data-cy=add-part]').click();
      cy.get('[data-cy=part-search]').type(part.partNumber);
      cy.get('[data-cy=part-select]').click();
      cy.get('[data-cy=part-quantity]').clear().type(part.quantity);
      cy.get('[data-cy=save-part]').click();
    });
  }
  
  cy.get('[data-cy=save-job]').click();
  cy.contains('Service job created successfully').should('be.visible');
});

// Complete a service job
Cypress.Commands.add('completeServiceJob', (serviceJobId, completionDetails) => {
  cy.visit(`/service-jobs/${serviceJobId}`);
  cy.get('[data-cy=complete-job]').click();
  cy.get('[data-cy=actual-hours]').clear().type(completionDetails.actualHours);
  
  if (completionDetails.notes) {
    cy.get('[data-cy=completion-notes]').type(completionDetails.notes);
  }
  
  if (completionDetails.signature) {
    // Simulate signature on the signature pad
    cy.get('[data-cy=signature-pad]').then($canvas => {
      const canvas = $canvas[0];
      const ctx = canvas.getContext('2d');
      ctx.moveTo(50, 50);
      ctx.lineTo(200, 50);
      ctx.stroke();
    });
  }
  
  cy.get('[data-cy=submit-completion]').click();
  cy.contains('Job completed successfully').should('be.visible');
});
