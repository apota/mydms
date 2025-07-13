describe('Service Job Workflow', () => {
  beforeEach(() => {
    cy.login('service-advisor', 'password123');
    cy.intercept('GET', '**/api/repair-orders/*', { fixture: 'repairOrder.json' }).as('getRepairOrder');
    cy.intercept('GET', '**/api/service/jobs/*', { fixture: 'serviceJob.json' }).as('getServiceJob');
    cy.intercept('GET', '**/api/parts/search*', { fixture: 'partsSearch.json' }).as('searchParts');
    cy.intercept('POST', '**/api/service/jobs', {
      statusCode: 201,
      body: {
        id: 'new-job-id',
        message: 'Service job created successfully'
      }
    }).as('createServiceJob');
    cy.intercept('PUT', '**/api/service/jobs/*', {
      statusCode: 200,
      body: {
        id: 'job-id',
        message: 'Service job updated successfully'
      }
    }).as('updateServiceJob');
    cy.intercept('POST', '**/api/service/jobs/*/complete', {
      statusCode: 200,
      body: {
        id: 'job-id',
        status: 'Completed',
        message: 'Job completed successfully'
      }
    }).as('completeServiceJob');
  });
  
  it('should allow creating and assigning a service job', () => {
    cy.fixture('serviceData').then((data) => {
      const job = data.serviceJobs[0];
      const repairOrderId = job.repairOrderId;
      
      // Navigate to repair order detail page
      cy.visit(`/repair-orders/${repairOrderId}`);
      cy.wait('@getRepairOrder');
      
      // Create a new service job
      cy.get('[data-cy=add-service-job]').click();
      
      // Fill out job details
      cy.get('[data-cy=job-description]').type(job.description);
      cy.get('[data-cy=job-type]').click();
      cy.contains(job.jobType).click();
      cy.get('[data-cy=labor-code]').type(job.laborCode);
      cy.get('[data-cy=est-hours]').type(job.estHours);
      
      // Assign technician
      cy.get('[data-cy=technician-select]').click();
      cy.contains(job.technician).click();
      
      // Add parts
      job.parts.forEach(part => {
        cy.get('[data-cy=add-part]').click();
        cy.get('[data-cy=part-search]').type(part.partNumber);
        cy.wait('@searchParts');
        cy.get('[data-cy=part-select]').click();
        cy.get('[data-cy=part-quantity]').clear().type(part.quantity);
        cy.get('[data-cy=save-part]').click();
      });
      
      // Save the job
      cy.get('[data-cy=save-job]').click();
      cy.wait('@createServiceJob');
      
      // Check success message
      cy.contains('Service job created successfully').should('be.visible');
    });
  });
  
  it('should validate required fields when creating a service job', () => {
    const repairOrderId = 'ro001';
    
    // Navigate to repair order detail page
    cy.visit(`/repair-orders/${repairOrderId}`);
    cy.wait('@getRepairOrder');
    
    // Create a new service job
    cy.get('[data-cy=add-service-job]').click();
    
    // Try to save without required fields
    cy.get('[data-cy=save-job]').click();
    
    // Check that validation errors are shown
    cy.contains('Description is required').should('be.visible');
    cy.contains('Job type is required').should('be.visible');
    cy.contains('Labor operation code is required').should('be.visible');
    cy.contains('Estimated hours is required').should('be.visible');
    
    // Fill in the fields and try again
    cy.get('[data-cy=job-description]').type('Oil Change');
    cy.get('[data-cy=job-type]').click();
    cy.contains('Maintenance').click();
    cy.get('[data-cy=labor-code]').type('M-001');
    cy.get('[data-cy=est-hours]').type('0.5');
    
    // Save should work now
    cy.get('[data-cy=save-job]').click();
    cy.wait('@createServiceJob');
    cy.contains('Service job created successfully').should('be.visible');
  });
  
  it('should allow completing a service job', () => {
    cy.fixture('serviceData').then((data) => {
      const completion = data.jobCompletion;
      const serviceJobId = 'sj001';
      
      // Navigate to service job detail page
      cy.visit(`/service-jobs/${serviceJobId}`);
      cy.wait('@getServiceJob');
      
      // Complete the job
      cy.get('[data-cy=complete-job]').click();
      
      // Fill in completion details
      cy.get('[data-cy=actual-hours]').clear().type(completion.actualHours);
      cy.get('[data-cy=completion-notes]').type(completion.notes);
      
      // Signature pad (simulated)
      cy.get('[data-cy=signature-pad]').then($canvas => {
        const canvas = $canvas[0];
        const ctx = canvas.getContext('2d');
        ctx.moveTo(50, 50);
        ctx.lineTo(200, 50);
        ctx.stroke();
      });
      
      // Submit completion
      cy.get('[data-cy=submit-completion]').click();
      cy.wait('@completeServiceJob');
      
      // Check success message
      cy.contains('Job completed successfully').should('be.visible');
    });
  });
});
