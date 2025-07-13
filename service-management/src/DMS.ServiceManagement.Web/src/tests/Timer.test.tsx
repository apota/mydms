import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import Timer from '../components/Timer';

// Mock timer functions
jest.useFakeTimers();

describe('Timer Component', () => {
  test('renders with initial time of 0:00:00', () => {
    const mockSetActive = jest.fn();
    render(<Timer isActive={false} setIsActive={mockSetActive} />);
    
    expect(screen.getByText('00:00:00')).toBeInTheDocument();
  });

  test('renders with provided initial seconds', () => {
    const mockSetActive = jest.fn();
    render(<Timer 
      isActive={false} 
      setIsActive={mockSetActive} 
      initialSeconds={3665} // 1 hour, 1 minute, 5 seconds
    />);
    
    expect(screen.getByText('01:01:05')).toBeInTheDocument();
  });

  test('starts counting when isActive is true', () => {
    const mockSetActive = jest.fn();
    render(<Timer isActive={true} setIsActive={mockSetActive} />);
    
    // Initial time
    expect(screen.getByText('00:00:00')).toBeInTheDocument();
    
    // Advance timer by 3 seconds
    act(() => {
      jest.advanceTimersByTime(3000);
    });
    
    // Time should be updated
    expect(screen.getByText('00:00:03')).toBeInTheDocument();
  });

  test('stops counting when isActive becomes false', () => {
    const mockSetActive = jest.fn();
    const { rerender } = render(<Timer isActive={true} setIsActive={mockSetActive} />);
    
    // Advance timer by 5 seconds
    act(() => {
      jest.advanceTimersByTime(5000);
    });
    
    // Time should be updated
    expect(screen.getByText('00:00:05')).toBeInTheDocument();
    
    // Make timer inactive
    rerender(<Timer isActive={false} setIsActive={mockSetActive} initialSeconds={5} />);
    
    // Advance timer more
    act(() => {
      jest.advanceTimersByTime(5000);
    });
    
    // Time should not change
    expect(screen.getByText('00:00:05')).toBeInTheDocument();
  });

  test('handles pause button click', () => {
    const mockSetActive = jest.fn();
    render(<Timer isActive={true} setIsActive={mockSetActive} />);
    
    // Find and click the pause button
    fireEvent.click(screen.getByText('Pause'));
    
    // setIsActive should be called with false
    expect(mockSetActive).toHaveBeenCalledWith(false);
  });

  test('handles start button click', () => {
    const mockSetActive = jest.fn();
    render(<Timer isActive={false} setIsActive={mockSetActive} />);
    
    // Find and click the start button
    fireEvent.click(screen.getByText('Start'));
    
    // setIsActive should be called with true
    expect(mockSetActive).toHaveBeenCalledWith(true);
  });

  test('handles reset button click', () => {
    const mockSetActive = jest.fn();
    render(<Timer isActive={true} setIsActive={mockSetActive} initialSeconds={10} />);
    
    // Find and click the reset button
    fireEvent.click(screen.getByText('Reset'));
    
    // setIsActive should be called with false
    expect(mockSetActive).toHaveBeenCalledWith(false);
    
    // Time should be reset to 00:00:00
    expect(screen.getByText('00:00:00')).toBeInTheDocument();
  });

  test('calls onTime callback when time changes', () => {
    const mockSetActive = jest.fn();
    const mockOnTime = jest.fn();
    
    render(
      <Timer 
        isActive={true} 
        setIsActive={mockSetActive} 
        onTime={mockOnTime}
      />
    );
    
    // Advance timer by 1 second
    act(() => {
      jest.advanceTimersByTime(1000);
    });
    
    // onTime callback should be called with 1
    expect(mockOnTime).toHaveBeenCalledWith(1);
    
    // Advance timer by 1 more second
    act(() => {
      jest.advanceTimersByTime(1000);
    });
    
    // onTime callback should be called with 2
    expect(mockOnTime).toHaveBeenCalledWith(2);
  });

  test('applies className prop to container', () => {
    const mockSetActive = jest.fn();
    const { container } = render(
      <Timer 
        isActive={false} 
        setIsActive={mockSetActive} 
        className="test-class"
      />
    );
    
    expect(container.firstChild).toHaveClass('test-class');
  });
});
