# DMS CRM Web UI

This is the frontend React application for the DMS CRM module. It provides a user interface for managing customers, interactions, campaigns, surveys, and analytics in the Automotive Dealership Management System.

## Features

- **Customer Management**: View, create, update, and search for customer records
- **Campaign Management**: Create marketing campaigns and track their performance
- **Customer Interactions**: Record and manage all customer touchpoints
- **Customer Surveys**: Create surveys and analyze customer feedback
- **Analytics Dashboard**: View key metrics and trends

## Getting Started

### Prerequisites

- Node.js (v14 or higher)
- npm (v7 or higher)

### Installation

1. Clone the repository
2. Navigate to the project directory
3. Install dependencies:

```bash
npm install
```

### Development

To start the development server:

```bash
npm start
```

This will run the app in development mode. Open [http://localhost:3000](http://localhost:3000) to view it in your browser.

### Building for Production

To build the app for production:

```bash
npm run build
```

This builds the app for production to the `build` folder.

## Project Structure

- `/src`: Source code for the application
  - `/components`: React components
  - `/services`: Services for API communication
  - `/hooks`: Custom React hooks
  - `/utils`: Utility functions
  - `/contexts`: React contexts for state management
  - `/pages`: Top-level page components

## Environment Variables

The following environment variables can be set:

- `REACT_APP_API_URL`: The URL of the backend API (default: https://localhost:7001/api)

## Technologies Used

- React
- React Router
- Axios for API calls
- Chart.js for data visualization
- Material-UI for UI components
- Formik for form handling
- Yup for form validation

## Available Scripts

- `npm start`: Runs the app in development mode
- `npm test`: Launches the test runner
- `npm run build`: Builds the app for production
- `npm run eject`: Ejects the configuration (one-way operation)
