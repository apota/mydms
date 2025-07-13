describe('Vehicle Inspection Workflow', () => {
  beforeEach(() => {
    cy.login('technician', 'password123');
    cy.intercept('GET', '**/api/repair-orders/*', { fixture: 'repairOrder.json' }).as('getRepairOrder');
    cy.intercept('POST', '**/api/inspections', {
      statusCode: 201,
      body: {
        id: 'new-inspection-id',
        message: 'Inspection created successfully'
      }
    }).as('createInspection');
    cy.intercept('POST', '**/api/inspections/*/points/*/images', {
      statusCode: 201,
      body: {
        imageUrl: 'https://example.com/sample-image.jpg'
      }
    }).as('uploadImage');
  });
  
  it('should allow a technician to complete a vehicle inspection', () => {
    cy.fixture('serviceData').then((data) => {
      const inspection = data.inspections[0];
      const repairOrderId = inspection.repairOrderId;
      
      // Navigate to repair order detail page
      cy.visit(`/repair-orders/${repairOrderId}`);
      cy.wait('@getRepairOrder');
      
      // Start a new inspection
      cy.get('[data-cy=create-inspection]').click();
      
      // Complete inspection points
      inspection.points.forEach(point => {
        // Find and expand the category section if needed
        const [category, pointName] = point.id.split('-');
        cy.contains(category, { matchCase: false }).click({ force: true });
        
        // Select the result for the inspection point
        cy.get(`[data-cy=inspection-point-${point.id}]`).within(() => {
          cy.get('[data-cy=result-select]').click();
          cy.get(`[data-value="${point.result}"]`).click({ force: true });
          
          // Add notes if any
          if (point.notes) {
            cy.get('[data-cy=notes-input]').type(point.notes);
          }
          
          // Upload an image if needed (simulated)
          if (point.result === 'FAIL' || point.result === 'WARNING') {
            cy.get('[data-cy=upload-image]').click();
            cy.get('input[type="file"]').selectFile({
              contents: Cypress.Buffer.from('file contents'),
              fileName: 'test-image.jpg',
              mimeType: 'image/jpeg'
            }, { force: true });
            cy.wait('@uploadImage');
          }
        });
      });
      
      // Add recommended services
      inspection.services.forEach(service => {
        cy.get('[data-cy=service-description]').type(service.description);
        cy.get('[data-cy=service-urgency]').click();
        cy.contains(service.urgency).click();
        cy.get('[data-cy=service-price]').clear().type(service.price);
        cy.get('[data-cy=add-service]').click();
      });
      
      // Add general notes
      cy.get('[data-cy=general-notes]').type(inspection.generalNotes);
      
      // Submit the inspection
      cy.get('[data-cy=submit-inspection]').click();
      cy.wait('@createInspection');
      
      // Check success message
      cy.contains('Inspection completed successfully').should('be.visible');
    });
  });
  
  it('should validate required fields for failed inspection points', () => {
    const repairOrderId = 'ro001';
    
    // Navigate to repair order detail page
    cy.visit(`/repair-orders/${repairOrderId}`);
    cy.wait('@getRepairOrder');
    
    // Start a new inspection
    cy.get('[data-cy=create-inspection]').click();
    
    // Select a failure result without adding notes
    cy.contains('Brakes').click({ force: true });
    cy.get('[data-cy=inspection-point-brakes-front-pads]').within(() => {
      cy.get('[data-cy=result-select]').click();
      cy.get('[data-value="FAIL"]').click({ force: true });
    });
    
    // Try to submit without adding notes for the failed item
    cy.get('[data-cy=submit-inspection]').click();
    cy.contains('Notes are required for failed items').should('be.visible');
    
    // Add the required notes and try again
    cy.get('[data-cy=inspection-point-brakes-front-pads]').within(() => {
      cy.get('[data-cy=notes-input]').type('Front brake pads worn below minimum thickness');
    });
    
    // Validation should pass now and submission should work
    cy.get('[data-cy=submit-inspection]').click();
    cy.wait('@createInspection');
    cy.contains('Inspection completed successfully').should('be.visible');
  });
});
