// @ts-nocheck
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import AccessoriesSelector from '../../../src/DMS.SalesManagement.Web/src/components/accessories/AccessoriesSelector';
import * as integrationService from '../../../src/DMS.SalesManagement.Web/src/services/integrationService';

// Mock the integrationService
jest.mock('../../../src/DMS.SalesManagement.Web/src/services/integrationService');

const mockAccessories = [
  {
    id: 'acc-1',
    name: 'Roof Rack',
    description: 'Aluminum roof rack for all your adventure needs',
    price: 299.99,
    category: 'Exterior',
    isInstalled: false,
    imageUrl: 'https://example.com/images/roof-rack.jpg',
    installationTimeMinutes: 45
  },
  {
    id: 'acc-2',
    name: 'Floor Mats',
    description: 'All-weather floor mats with vehicle name embroidered',
    price: 129.99,
    category: 'Interior',
    isInstalled: false,
    imageUrl: 'https://example.com/images/floor-mats.jpg',
    installationTimeMinutes: 15
  }
];

describe('AccessoriesSelector Component', () => {
  beforeEach(() => {
    // Reset all mocks
    jest.clearAllMocks();
  });

  it('renders loading state initially', () => {
    // Mock the service to return a promise that doesn't resolve immediately
    integrationService.getVehicleAccessories.mockImplementation(() => new Promise(() => {}));
    
    render(
      <AccessoriesSelector 
        vehicleId="veh-123" 
        selectedAccessories={[]} 
        onChange={() => {}}
      />
    );
    
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('renders accessories when loaded', async () => {
    // Mock the service to return accessories
    integrationService.getVehicleAccessories.mockResolvedValue(mockAccessories);
    
    render(
      <AccessoriesSelector 
        vehicleId="veh-123" 
        selectedAccessories={[]} 
        onChange={() => {}}
      />
    );
    
    // Wait for accessories to load
    await waitFor(() => {
      expect(screen.getByText('Roof Rack')).toBeInTheDocument();
    });
    
    // Check if all accessories are rendered
    expect(screen.getByText('Roof Rack')).toBeInTheDocument();
    expect(screen.getByText('Floor Mats')).toBeInTheDocument();
    expect(screen.getByText('$299.99')).toBeInTheDocument();
    expect(screen.getByText('$129.99')).toBeInTheDocument();
  });

  it('shows error state when loading fails', async () => {
    // Mock the service to throw an error
    integrationService.getVehicleAccessories.mockRejectedValue(new Error('Failed to load accessories'));
    
    render(
      <AccessoriesSelector 
        vehicleId="veh-123" 
        selectedAccessories={[]} 
        onChange={() => {}}
      />
    );
    
    // Wait for error message to appear
    await waitFor(() => {
      expect(screen.getByText(/Failed to load vehicle accessories/i)).toBeInTheDocument();
    });
  });

  it('shows empty state when no accessories are available', async () => {
    // Mock the service to return an empty array
    integrationService.getVehicleAccessories.mockResolvedValue([]);
    
    render(
      <AccessoriesSelector 
        vehicleId="veh-123" 
        selectedAccessories={[]} 
        onChange={() => {}}
      />
    );
    
    // Wait for empty message to appear
    await waitFor(() => {
      expect(screen.getByText('No accessories available for this vehicle.')).toBeInTheDocument();
    });
  });

  it('calls onChange when accessory is selected', async () => {
    // Mock the service to return accessories
    integrationService.getVehicleAccessories.mockResolvedValue(mockAccessories);
    
    // Mock onChange function
    const handleChange = jest.fn();
    
    render(
      <AccessoriesSelector 
        vehicleId="veh-123" 
        selectedAccessories={[]} 
        onChange={handleChange}
      />
    );
    
    // Wait for accessories to load
    await waitFor(() => {
      expect(screen.getByText('Roof Rack')).toBeInTheDocument();
    });
    
    // Find the checkbox and click it
    const checkbox = screen.getAllByRole('checkbox')[0];
    fireEvent.click(checkbox);
    
    // Check if onChange was called with correct value
    expect(handleChange).toHaveBeenCalledWith(['acc-1']);
  });

  it('displays selected accessories correctly', async () => {
    // Mock the service to return accessories
    integrationService.getVehicleAccessories.mockResolvedValue(mockAccessories);
    
    render(
      <AccessoriesSelector 
        vehicleId="veh-123" 
        selectedAccessories={['acc-1']} 
        onChange={() => {}}
      />
    );
    
    // Wait for accessories to load
    await waitFor(() => {
      expect(screen.getByText('Roof Rack')).toBeInTheDocument();
    });
    
    // Find the checkbox for the selected accessory
    const checkboxes = screen.getAllByRole('checkbox');
    
    // Check if the first checkbox is checked
    expect(checkboxes[0]).toBeChecked();
    
    // Check if the second checkbox is not checked
    expect(checkboxes[1]).not.toBeChecked();
  });
});
