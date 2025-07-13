// @ts-nocheck
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import DocumentSigner from '../../src/DMS.SalesManagement.Web/src/components/documents/DocumentSigner';
import { NotificationProvider, useNotification } from '../../src/DMS.SalesManagement.Web/src/contexts/NotificationContext';

// Mock the notification context
const mockShowSuccess = jest.fn();
const mockShowError = jest.fn();
const mockShowWarning = jest.fn();
const mockShowInfo = jest.fn();
const mockShowNotification = jest.fn();
const mockHideNotification = jest.fn();

jest.mock('../../src/DMS.SalesManagement.Web/src/contexts/NotificationContext', () => {
  return {
    useNotification: jest.fn().mockReturnValue({
      showSuccess: mockShowSuccess,
      showError: mockShowError,
      showWarning: mockShowWarning,
      showInfo: mockShowInfo,
      showNotification: mockShowNotification,
      hideNotification: mockHideNotification
    }),
    NotificationProvider: ({ children }) => <div>{children}</div>
  };
});

// Mock SignatureCanvas
jest.mock('react-signature-canvas', () => {
  return function MockSignatureCanvas() {
    return <div data-testid="signature-canvas">Signature Canvas</div>;
  };
});

describe('DocumentSigner Component', () => {
  const mockOnSignComplete = jest.fn().mockResolvedValue(undefined);

  beforeEach(() => {
    jest.clearAllMocks();
  });
  it('renders the sign document button', () => {
    render(
      <DocumentSigner 
        documentId="doc123" 
        documentName="Test Document" 
        onSignComplete={mockOnSignComplete}
      />
    );
    
    expect(screen.getByText('Sign Document')).toBeInTheDocument();
  });

  it('opens dialog when button is clicked', () => {
    render(
      <DocumentSigner 
        documentId="doc123" 
        documentName="Test Document" 
        onSignComplete={mockOnSignComplete}
      />
    );
    
    fireEvent.click(screen.getByText('Sign Document'));
    
    expect(screen.getByText('Sign Document: Test Document')).toBeInTheDocument();
    expect(screen.getByText(/By signing below/)).toBeInTheDocument();
  });

  it('displays signer name when provided', () => {
    render(
      <DocumentSigner 
        documentId="doc123" 
        documentName="Test Document" 
        onSignComplete={mockOnSignComplete}
        fullName="John Doe"
      />
    );
    
    fireEvent.click(screen.getByText('Sign Document'));
    
    expect(screen.getByText('Signing as:')).toBeInTheDocument();
    expect(screen.getByText('John Doe')).toBeInTheDocument();
  });
  it('calls onSignComplete when signing is completed', async () => {
    // In a real test we would test the actual signature functionality
    // but for simplicity we'll just mock the call
    
    // Create a mock signature canvas ref with needed methods
    const mockRef = {
      current: {
        isEmpty: jest.fn().mockReturnValue(false),
        clear: jest.fn(),
        toDataURL: jest.fn().mockReturnValue('data:image/png;base64,signaturedata')
      }
    };
    
    // Save the original useRef
    const originalUseRef = React.useRef;
    
    // Mock useRef for our test
    React.useRef = jest.fn().mockReturnValue(mockRef);
    
    try {
      render(
        <DocumentSigner 
          documentId="doc123" 
          documentName="Test Document" 
          onSignComplete={mockOnSignComplete}
        />
      );
      
      fireEvent.click(screen.getByText('Sign Document'));
        // Now click the sign button in the dialog
      const dialogSignButton = screen.getAllByText('Sign Document')[1];
      fireEvent.click(dialogSignButton);
      
      await waitFor(() => {
        expect(mockOnSignComplete).toHaveBeenCalledWith('data:image/png;base64,signaturedata');
        expect(mockShowSuccess).toHaveBeenCalledWith('Document signed successfully!');
      });
    } finally {
      // Restore original useRef
      React.useRef = originalUseRef;
    }
  });
  
  it('shows error when signature canvas is empty', async () => {
    // Create a mock signature canvas ref with isEmpty returning true
    const mockEmptyRef = {
      current: {
        isEmpty: jest.fn().mockReturnValue(true),
        clear: jest.fn(),
        toDataURL: jest.fn()
      }
    };
    
    // Save the original useRef
    const originalUseRef = React.useRef;
    
    // Mock useRef for our test
    React.useRef = jest.fn().mockReturnValue(mockEmptyRef);
    
    try {
      render(
        <DocumentSigner 
          documentId="doc123" 
          documentName="Test Document" 
          onSignComplete={mockOnSignComplete}
        />
      );
      
      fireEvent.click(screen.getByText('Sign Document'));
        // Now click the sign button in the dialog
      const dialogSignButton = screen.getAllByText('Sign Document')[1];
      fireEvent.click(dialogSignButton);
      
      expect(mockShowError).toHaveBeenCalledWith('Please provide a signature');
      expect(mockOnSignComplete).not.toHaveBeenCalled();
    } finally {
      // Restore original useRef
      React.useRef = originalUseRef;
    }
  });

  it('shows error when onSignComplete fails', async () => {
    // Create a mock signature canvas ref for a valid signature
    const mockRef = {
      current: {
        isEmpty: jest.fn().mockReturnValue(false),
        clear: jest.fn(),
        toDataURL: jest.fn().mockReturnValue('data:image/png;base64,signaturedata')
      }
    };
    
    // Mock onSignComplete to reject with an error
    const mockErrorOnSignComplete = jest.fn().mockRejectedValue(new Error('API Error'));
    
    // Save the original useRef
    const originalUseRef = React.useRef;
    
    // Mock useRef for our test
    React.useRef = jest.fn().mockReturnValue(mockRef);
    
    try {
      render(
        <DocumentSigner 
          documentId="doc123" 
          documentName="Test Document" 
          onSignComplete={mockErrorOnSignComplete}
        />
      );
      
      fireEvent.click(screen.getByText('Sign Document'));
        // Now click the sign button in the dialog
      const dialogSignButton = screen.getAllByText('Sign Document')[1];
      fireEvent.click(dialogSignButton);
      
      await waitFor(() => {
        expect(mockErrorOnSignComplete).toHaveBeenCalled();
        expect(mockShowError).toHaveBeenCalledWith('Error signing document: API Error');
      });
    } finally {
      // Restore original useRef
      React.useRef = originalUseRef;
    }
  });
});
