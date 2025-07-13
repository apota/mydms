# End-to-End Testing for Service Management Module

This directory contains end-to-end tests for the Service Management module using Cypress.

## Setup

1. Navigate to the e2e test directory:
```
cd tests/e2e
```

2. Install dependencies:
```
npm install
```

## Running Tests

### Open Cypress Test Runner (interactive mode)
```
npm run test:e2e
```

### Run All Tests Headless (CI mode)
```
npm run test:e2e:headless
```

### Run a Specific Test File
```
npm run test:e2e:single tests/e2e/specs/appointment-workflow.cy.js
```

## Test Structure

- **specs/** - Contains all test files
  - **appointment-workflow.cy.js** - Tests for appointment scheduling
  - **inspection-workflow.cy.js** - Tests for vehicle inspections
  - **service-job-workflow.cy.js** - Tests for service job creation and completion
  - **e2e-service-workflow.cy.js** - Full end-to-end flow from appointment to payment

- **fixtures/** - Contains mock data for tests
  - **customers.json** - Mock customer and vehicle data
  - **serviceData.json** - Mock data for appointments, inspections, and service jobs
  - Other fixtures are created dynamically by test interceptors

- **support/** - Contains Cypress utility functions and commands
  - **commands.js** - Custom Cypress commands for common operations
  - **e2e.js** - Cypress configuration and global setup

## Test Naming Conventions

Tests are named using behavior-driven development style:

```javascript
it('should allow a service advisor to schedule a new appointment', () => {
  // Test code
});
```

## Testing Strategy

1. **Unit Tests** - Test individual components (existing in `tests/` directory)
2. **Integration Tests** - Test interaction between components (existing in `tests/` directory)
3. **E2E Tests** - Test complete user workflows as included here

### Key Workflows Tested

1. **Appointment Scheduling** - Customer selection, vehicle selection, service details, scheduling
2. **Vehicle Inspection** - Creating and filling out inspection forms, adding recommendations
3. **Service Job Management** - Creating jobs, adding parts, assigning technicians, job completion
4. **Full Service Workflow** - Complete service process from appointment to payment

## Data Mocking

Tests use interceptors to mock API responses. This allows testing even if APIs are not available or to consistently test different scenarios.

Example:
```javascript
cy.intercept('GET', '**/api/customers*', { fixture: 'customers.json' }).as('getCustomers');
```

## Element Selection

Tests use `data-cy` attributes to select elements, making tests more resilient to UI changes:

```html
<button data-cy="submit-appointment">Schedule Appointment</button>
```

Then in tests:
```javascript
cy.get('[data-cy=submit-appointment]').click();
```
