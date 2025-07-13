import React, { useState } from 'react';
import { Table, Form, Button, Modal, Spinner, Row, Col, Card } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faEdit, faPlus, faTrash, faTimes, faCheck } from '@fortawesome/free-solid-svg-icons';
import PropTypes from 'prop-types';

const AccountTypeLabels = {
  ASSET: 'Asset',
  LIABILITY: 'Liability',
  EQUITY: 'Equity',
  REVENUE: 'Revenue',
  EXPENSE: 'Expense'
};

const ChartOfAccounts = ({ accounts, isLoading, onEditAccount, onCreateAccount, onDeactivateAccount, onActivateAccount }) => {
  const [showAccountModal, setShowAccountModal] = useState(false);
  const [currentAccount, setCurrentAccount] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterType, setFilterType] = useState('ALL');
  const [showInactive, setShowInactive] = useState(false);

  const filteredAccounts = accounts.filter(account => {
    // Filter by search term (account number or name)
    const matchesSearch = !searchTerm || 
      account.accountNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      account.name.toLowerCase().includes(searchTerm.toLowerCase());
    
    // Filter by account type
    const matchesType = filterType === 'ALL' || account.accountType === filterType;
    
    // Filter by active status
    const matchesActive = showInactive || account.isActive;
    
    return matchesSearch && matchesType && matchesActive;
  });

  const handleEditAccount = (account) => {
    setCurrentAccount(account);
    setShowAccountModal(true);
  };

  const handleCreateAccount = () => {
    setCurrentAccount(null);
    setShowAccountModal(true);
  };

  const handleSaveAccount = (accountData) => {
    if (currentAccount) {
      onEditAccount?.(currentAccount.id, accountData);
    } else {
      onCreateAccount?.(accountData);
    }
    setShowAccountModal(false);
  };

  const handleToggleStatus = (account) => {
    if (account.isActive) {
      onDeactivateAccount?.(account.id);
    } else {
      onActivateAccount?.(account.id);
    }
  };

  return (
    <div>
      <Row className="mb-4">
        <Col md={4}>
          <Form.Group>
            <Form.Control
              type="text"
              placeholder="Search by account number or name"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </Form.Group>
        </Col>
        <Col md={3}>
          <Form.Group>
            <Form.Select
              value={filterType}
              onChange={(e) => setFilterType(e.target.value)}
            >
              <option value="ALL">All Account Types</option>
              <option value="ASSET">Assets</option>
              <option value="LIABILITY">Liabilities</option>
              <option value="EQUITY">Equity</option>
              <option value="REVENUE">Revenue</option>
              <option value="EXPENSE">Expenses</option>
            </Form.Select>
          </Form.Group>
        </Col>
        <Col md={3}>
          <Form.Check
            type="checkbox"
            id="show-inactive"
            label="Show Inactive Accounts"
            checked={showInactive}
            onChange={(e) => setShowInactive(e.target.checked)}
          />
        </Col>
        <Col md={2} className="text-end">
          <Button variant="primary" onClick={handleCreateAccount}>
            <FontAwesomeIcon icon={faPlus} className="me-2" />
            New Account
          </Button>
        </Col>
      </Row>

      <Card className="mb-4">
        <Card.Body className="p-0">
          {isLoading ? (
            <div className="text-center p-4">
              <Spinner animation="border" variant="primary" />
              <p className="mt-2">Loading accounts...</p>
            </div>
          ) : (
            <Table hover responsive className="mb-0">
              <thead>
                <tr>
                  <th>Account #</th>
                  <th>Name</th>
                  <th>Type</th>
                  <th>Parent Account</th>
                  <th>Status</th>
                  <th>Balance</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredAccounts.length === 0 ? (
                  <tr>
                    <td colSpan="7" className="text-center py-4">
                      No accounts found matching your criteria
                    </td>
                  </tr>
                ) : (
                  filteredAccounts.map(account => (
                    <tr key={account.id} className={!account.isActive ? 'table-secondary' : ''}>
                      <td>{account.accountNumber}</td>
                      <td>{account.name}</td>
                      <td>{AccountTypeLabels[account.accountType]}</td>
                      <td>{account.parentAccountName || '-'}</td>
                      <td>
                        <span className={`badge ${account.isActive ? 'bg-success' : 'bg-secondary'}`}>
                          {account.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td className="text-end">
                        {account.currentBalance?.toLocaleString('en-US', {
                          style: 'currency',
                          currency: 'USD',
                        })}
                      </td>
                      <td>
                        <Button
                          variant="outline-primary"
                          size="sm"
                          className="me-2"
                          onClick={() => handleEditAccount(account)}
                        >
                          <FontAwesomeIcon icon={faEdit} />
                        </Button>
                        <Button
                          variant={account.isActive ? 'outline-danger' : 'outline-success'}
                          size="sm"
                          onClick={() => handleToggleStatus(account)}
                        >
                          <FontAwesomeIcon icon={account.isActive ? faTimes : faCheck} />
                        </Button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </Table>
          )}
        </Card.Body>
      </Card>

      <AccountModal
        show={showAccountModal}
        onHide={() => setShowAccountModal(false)}
        onSave={handleSaveAccount}
        account={currentAccount}
        accounts={accounts}
      />
    </div>
  );
};

const AccountModal = ({ show, onHide, onSave, account, accounts }) => {
  const isEditMode = !!account;
  const [formData, setFormData] = useState(
    account || {
      accountNumber: '',
      name: '',
      description: '',
      accountType: 'ASSET',
      parentAccountId: '',
      isActive: true
    }
  );

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onSave(formData);
  };

  return (
    <Modal show={show} onHide={onHide} backdrop="static" size="lg">
      <Modal.Header closeButton>
        <Modal.Title>{isEditMode ? 'Edit Account' : 'Create New Account'}</Modal.Title>
      </Modal.Header>
      <Form onSubmit={handleSubmit}>
        <Modal.Body>
          <Row>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>Account Number</Form.Label>
                <Form.Control
                  type="text"
                  name="accountNumber"
                  value={formData.accountNumber}
                  onChange={handleChange}
                  required
                />
              </Form.Group>
            </Col>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>Account Type</Form.Label>
                <Form.Select
                  name="accountType"
                  value={formData.accountType}
                  onChange={handleChange}
                  required
                >
                  <option value="ASSET">Asset</option>
                  <option value="LIABILITY">Liability</option>
                  <option value="EQUITY">Equity</option>
                  <option value="REVENUE">Revenue</option>
                  <option value="EXPENSE">Expense</option>
                </Form.Select>
              </Form.Group>
            </Col>
          </Row>

          <Form.Group className="mb-3">
            <Form.Label>Account Name</Form.Label>
            <Form.Control
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              required
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Description</Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              name="description"
              value={formData.description}
              onChange={handleChange}
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Parent Account</Form.Label>
            <Form.Select
              name="parentAccountId"
              value={formData.parentAccountId}
              onChange={handleChange}
            >
              <option value="">No Parent (Top Level)</option>
              {accounts
                .filter(a => a.accountType === formData.accountType && a.id !== formData.id)
                .map(account => (
                  <option key={account.id} value={account.id}>
                    {account.accountNumber} - {account.name}
                  </option>
                ))}
            </Form.Select>
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Check
              type="checkbox"
              id="isActive"
              name="isActive"
              label="Active"
              checked={formData.isActive}
              onChange={handleChange}
            />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={onHide}>
            Cancel
          </Button>
          <Button variant="primary" type="submit">
            {isEditMode ? 'Update' : 'Create'}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
};

ChartOfAccounts.propTypes = {
  accounts: PropTypes.array.isRequired,
  isLoading: PropTypes.bool,
  onEditAccount: PropTypes.func,
  onCreateAccount: PropTypes.func,
  onDeactivateAccount: PropTypes.func,
  onActivateAccount: PropTypes.func
};

ChartOfAccounts.defaultProps = {
  accounts: [],
  isLoading: false
};

export default ChartOfAccounts;
