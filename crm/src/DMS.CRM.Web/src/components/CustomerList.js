import React, { useState, useEffect } from 'react';
import { CustomerService } from '../services/api-services';

const CustomerList = () => {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 10;

  useEffect(() => {
    const fetchCustomers = async () => {
      try {
        setLoading(true);
        const skip = (currentPage - 1) * pageSize;
        const data = await CustomerService.getAll(skip, pageSize);
        setCustomers(data);
        // Assuming total count is returned in headers or response metadata
        // This would need to be adjusted based on actual API implementation
        setTotalPages(Math.ceil(100 / pageSize)); // Placeholder logic
        setLoading(false);
      } catch (err) {
        setError('Failed to fetch customers');
        setLoading(false);
        console.error('Error fetching customers:', err);
      }
    };

    fetchCustomers();
  }, [currentPage]);

  const handlePageChange = (newPage) => {
    setCurrentPage(newPage);
  };

  if (loading) return <div className="loading">Loading customers...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
    <div className="customer-list-container">
      <h2>Customer List</h2>
      
      <table className="customer-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Phone</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {customers.map(customer => (
            <tr key={customer.id}>
              <td>{customer.firstName} {customer.lastName}</td>
              <td>{customer.email}</td>
              <td>{customer.phoneNumber}</td>
              <td>{customer.status}</td>
              <td>
                <button className="view-btn">View</button>
                <button className="edit-btn">Edit</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="pagination">
        <button 
          disabled={currentPage === 1} 
          onClick={() => handlePageChange(currentPage - 1)}
        >
          Previous
        </button>
        <span>Page {currentPage} of {totalPages}</span>
        <button 
          disabled={currentPage === totalPages} 
          onClick={() => handlePageChange(currentPage + 1)}
        >
          Next
        </button>
      </div>
    </div>
  );
};

export default CustomerList;
