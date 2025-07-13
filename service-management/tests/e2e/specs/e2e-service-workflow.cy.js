describe('End-to-End Service Process', () => {
  beforeEach(() => {
    cy.login('service-advisor', 'password123');
    cy.intercept('GET', '**/api/customers*', { fixture: 'customers.json' }).as('getCustomers');
    cy.intercept('POST', '**/api/appointments', {
      statusCode: 201,
      body: {
        id: 'appt001',
        message: 'Appointment created successfully'
      }
    }).as('createAppointment');
    cy.intercept('POST', '**/api/repair-orders', {
      statusCode: 201,
      body: {
        id: 'ro001',
        message: 'Repair order created successfully'
      }
    }).as('createRepairOrder');
    cy.intercept('POST', '**/api/inspections', {
      statusCode: 201,
      body: {
        id: 'insp001',
        message: 'Inspection created successfully'
      }
    }).as('createInspection');
    cy.intercept('POST', '**/api/service/jobs', {
      statusCode: 201,
      body: {
        id: 'sj001',
        message: 'Service job created successfully'
      }
    }).as('createServiceJob');
    cy.intercept('POST', '**/api/service/jobs/*/complete', {
      statusCode: 200,
      body: {
        message: 'Job completed successfully'
      }
    }).as('completeJob');
    cy.intercept('POST', '**/api/financial/invoices', {
      statusCode: 201,
      body: {
        id: 'inv001',
        message: 'Invoice created successfully'
      }
    }).as('createInvoice');
    cy.intercept('POST', '**/api/financial/payments', {
      statusCode: 200,
      body: {
        id: 'pay001',
        message: 'Payment processed successfully'
      }
    }).as('processPayment');
  });
  
  it('should complete a full service process from appointment to payment', () => {
    cy.fixture('serviceData').then((data) => {
      const appointment = data.appointments[0];
      const inspection = data.inspections[0];
      const serviceJob = data.serviceJobs[0];
      const completion = data.jobCompletion;
      
      // Step 1: Schedule Appointment
      cy.visit('/appointments/new');
      cy.wait('@getCustomers');
      cy.createAppointment(appointment);
      cy.wait('@createAppointment');
      
      // Step 2: Check-in and Create Repair Order
      cy.visit('/appointments');
      cy.contains(appointment.customerConcerns).click();
      cy.get('[data-cy=check-in-button]').click();
      
      // Fill in check-in details
      cy.get('[data-cy=current-mileage]').type('25500');
      cy.get('[data-cy=confirm-concerns]').click();
      cy.get('[data-cy=authorize-inspection]').click();
      cy.get('[data-cy=create-repair-order]').click();
      cy.wait('@createRepairOrder');
      
      // Step 3: Perform Inspection
      const repairOrderId = 'ro001'; // From intercept
      cy.visit(`/repair-orders/${repairOrderId}`);
      
      // Create inspection
      cy.get('[data-cy=create-inspection]').click();
      
      // Fill out inspection as technician
      cy.contains('Logging in as Technician').should('be.visible');
      
      // Complete inspection points
      inspection.points.forEach(point => {
        const [category, pointId] = point.id.split('-');
        cy.contains(category).click({ force: true });
        
        cy.get(`[data-cy=inspection-point-${point.id}]`).within(() => {
          cy.get('[data-cy=result-select]').click();
          cy.get(`[data-value="${point.result}"]`).click({ force: true });
          
          if (point.notes) {
            cy.get('[data-cy=notes-input]').type(point.notes);
          }
        });
      });
      
      // Add recommended services
      inspection.services.forEach(service => {
        cy.get('[data-cy=service-description]').type(service.description);
        cy.get('[data-cy=service-urgency]').click();
        cy.contains(service.urgency).click({ force: true });
        cy.get('[data-cy=service-price]').clear().type(service.price);
        cy.get('[data-cy=add-service]').click();
      });
      
      cy.get('[data-cy=general-notes]').type(inspection.generalNotes);
      cy.get('[data-cy=submit-inspection]').click();
      cy.wait('@createInspection');
      
      // Step 4: Review results with customer (service advisor)
      cy.visit(`/repair-orders/${repairOrderId}`);
      cy.get('[data-cy=review-inspection]').click();
      cy.get('[data-cy=call-customer]').click();
      cy.contains('Customer notified').should('be.visible');
      
      // Step 5: Create Service Jobs
      cy.get('[data-cy=add-service-job]').click();
      
      // Fill in job details
      cy.get('[data-cy=job-description]').type(serviceJob.description);
      cy.get('[data-cy=job-type]').click();
      cy.contains(serviceJob.jobType).click();
      cy.get('[data-cy=labor-code]').type(serviceJob.laborCode);
      cy.get('[data-cy=est-hours]').type(serviceJob.estHours);
      cy.get('[data-cy=technician-select]').click();
      cy.contains(serviceJob.technician).click();
      
      // Add parts
      serviceJob.parts.forEach(part => {
        cy.get('[data-cy=add-part]').click();
        cy.get('[data-cy=part-search]').type(part.partNumber);
        cy.get('[data-cy=part-select]').click({ force: true });
        cy.get('[data-cy=part-quantity]').clear().type(part.quantity);
        cy.get('[data-cy=save-part]').click();
      });
      
      cy.get('[data-cy=save-job]').click();
      cy.wait('@createServiceJob');
      
      // Step 6: Complete Service Job
      const serviceJobId = 'sj001'; // From intercept
      cy.visit(`/service-jobs/${serviceJobId}`);
      cy.get('[data-cy=start-job]').click();
      cy.contains('Job started').should('be.visible');
      
      // Complete the job
      cy.get('[data-cy=complete-job]').click();
      cy.get('[data-cy=actual-hours]').clear().type(completion.actualHours);
      cy.get('[data-cy=completion-notes]').type(completion.notes);
      cy.get('[data-cy=submit-completion]').click();
      cy.wait('@completeJob');
      
      // Step 7: Generate Invoice
      cy.visit(`/repair-orders/${repairOrderId}`);
      cy.get('[data-cy=create-invoice]').click();
      cy.get('[data-cy=confirm-invoice]').click();
      cy.wait('@createInvoice');
      
      // Step 8: Process Payment
      const invoiceId = 'inv001'; // From intercept
      cy.visit(`/invoices/${invoiceId}`);
      cy.get('[data-cy=process-payment]').click();
      
      cy.get('[data-cy=payment-method]').click();
      cy.contains('Credit Card').click();
      cy.get('[data-cy=payment-amount]').should('have.value', '479.98');
      cy.get('[data-cy=submit-payment]').click();
      cy.wait('@processPayment');
      
      // Final verification
      cy.contains('Payment processed successfully').should('be.visible');
      cy.visit(`/repair-orders/${repairOrderId}`);
      cy.get('[data-cy=status]').should('contain', 'Paid');
    });
  });
});
