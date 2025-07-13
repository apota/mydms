import React, { useState, useEffect } from 'react';
import { Modal, Button, Form, Table, Row, Col, Alert } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPlus, faTrash, faSave } from '@fortawesome/free-solid-svg-icons';
import PropTypes from 'prop-types';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';

const initialLineItem = {
  accountId: '',
  description: '',
  debitAmount: '',
  creditAmount: ''
};

const CreateJournalEntry = ({ show, onHide, onSave, accounts }) => {
  const [entryDate, setEntryDate] = useState(new Date());
  const [description, setDescription] = useState('');
  const [referenceNumber, setReferenceNumber] = useState('');
  const [lineItems, setLineItems] = useState([{ ...initialLineItem }]);
  const [error, setError] = useState('');
  const [isBalanced, setIsBalanced] = useState(false);
  const [totals, setTotals] = useState({ debit: 0, credit: 0 });

  useEffect(() => {
    // Calculate totals and check if balanced
    const debitTotal = lineItems.reduce((sum, item) => sum + (parseFloat(item.debitAmount) || 0), 0);
    const creditTotal = lineItems.reduce((sum, item) => sum + (parseFloat(item.creditAmount) || 0), 0);
    
    setTotals({
      debit: debitTotal,
      credit: creditTotal
    });
    
    setIsBalanced(Math.abs(debitTotal - creditTotal) < 0.01);
  }, [lineItems]);

  // Reset form when the modal is opened
  useEffect(() => {
    if (show) {
      setEntryDate(new Date());
      setDescription('');
      setReferenceNumber('');
      setLineItems([{ ...initialLineItem }]);
      setError('');
    }
  }, [show]);

  const handleAddLineItem = () => {
    setLineItems([...lineItems, { ...initialLineItem }]);
  };

  const handleRemoveLineItem = (index) => {
    if (lineItems.length <= 1) {
      return; // Always keep at least one line item
    }
    const newItems = [...lineItems];
    newItems.splice(index, 1);
    setLineItems(newItems);
  };

  const handleLineItemChange = (index, field, value) => {
    const newItems = [...lineItems];
    newItems[index] = { ...newItems[index], [field]: value };

    // If entering a debit amount, clear the credit amount and vice versa
    if (field === 'debitAmount' && value) {
      newItems[index].creditAmount = '';
    } else if (field === 'creditAmount' && value) {
      newItems[index].debitAmount = '';
    }

    setLineItems(newItems);
  };

  const validateForm = () => {
    if (!description.trim()) {
      setError('Please enter a description for this journal entry');
      return false;
    }

    if (lineItems.some(item => !item.accountId)) {
      setError('All line items must have an account selected');
      return false;
    }

    if (lineItems.some(item => !item.debitAmount && !item.creditAmount)) {
      setError('All line items must have either a debit or credit amount');
      return false;
    }

    if (!isBalanced) {
      setError('Journal entry must be balanced (total debits must equal total credits)');
      return false;
    }

    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!validateForm()) {
      return;
    }

    const journalEntry = {
      entryDate: entryDate.toISOString(),
      description,
      referenceNumber,
      lineItems: lineItems.map(item => ({
        accountId: item.accountId,
        description: item.description,
        debitAmount: parseFloat(item.debitAmount) || 0,
        creditAmount: parseFloat(item.creditAmount) || 0
      }))
    };

    try {
      const result = await onSave(journalEntry);
      if (result) {
        onHide();
      }
    } catch (err) {
      setError(`Failed to save journal entry: ${err.message}`);
    }
  };

  return (
    <Modal show={show} onHide={onHide} size="xl" backdrop="static">
      <Modal.Header closeButton>
        <Modal.Title>Create New Journal Entry</Modal.Title>
      </Modal.Header>
      <Form onSubmit={handleSubmit}>
        <Modal.Body>
          {error && <Alert variant="danger">{error}</Alert>}

          <Row className="mb-3">
            <Col md={6}>
              <Form.Group>
                <Form.Label>Entry Date</Form.Label>
                <DatePicker
                  selected={entryDate}
                  onChange={date => setEntryDate(date)}
                  className="form-control"
                  dateFormat="MM/dd/yyyy"
                  required
                />
              </Form.Group>
            </Col>
            <Col md={6}>
              <Form.Group>
                <Form.Label>Reference Number (Optional)</Form.Label>
                <Form.Control
                  type="text"
                  value={referenceNumber}
                  onChange={e => setReferenceNumber(e.target.value)}
                />
              </Form.Group>
            </Col>
          </Row>

          <Form.Group className="mb-3">
            <Form.Label>Description</Form.Label>
            <Form.Control
              as="textarea"
              rows={2}
              value={description}
              onChange={e => setDescription(e.target.value)}
              required
            />
          </Form.Group>

          <hr />
          <h5>Line Items</h5>
          <Table striped bordered>
            <thead>
              <tr>
                <th>Account</th>
                <th>Description</th>
                <th>Debit</th>
                <th>Credit</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {lineItems.map((item, index) => (
                <tr key={index}>
                  <td>
                    <Form.Select
                      value={item.accountId}
                      onChange={e => handleLineItemChange(index, 'accountId', e.target.value)}
                      required
                    >
                      <option value="">Select Account</option>
                      {Object.entries(accounts.reduce((acc, account) => {
                        if (!acc[account.accountType]) {
                          acc[account.accountType] = [];
                        }
                        acc[account.accountType].push(account);
                        return acc;
                      }, {})).map(([type, accounts]) => (
                        <optgroup key={type} label={type}>
                          {accounts.map(account => (
                            <option key={account.id} value={account.id}>
                              {account.accountNumber} - {account.name}
                            </option>
                          ))}
                        </optgroup>
                      ))}
                    </Form.Select>
                  </td>
                  <td>
                    <Form.Control
                      type="text"
                      value={item.description}
                      onChange={e => handleLineItemChange(index, 'description', e.target.value)}
                      placeholder="Line description"
                    />
                  </td>
                  <td>
                    <Form.Control
                      type="number"
                      step="0.01"
                      min="0"
                      value={item.debitAmount}
                      onChange={e => handleLineItemChange(index, 'debitAmount', e.target.value)}
                      disabled={!!item.creditAmount}
                      placeholder="0.00"
                    />
                  </td>
                  <td>
                    <Form.Control
                      type="number"
                      step="0.01"
                      min="0"
                      value={item.creditAmount}
                      onChange={e => handleLineItemChange(index, 'creditAmount', e.target.value)}
                      disabled={!!item.debitAmount}
                      placeholder="0.00"
                    />
                  </td>
                  <td className="text-center">
                    <Button
                      variant="outline-danger"
                      size="sm"
                      onClick={() => handleRemoveLineItem(index)}
                      disabled={lineItems.length <= 1}
                    >
                      <FontAwesomeIcon icon={faTrash} />
                    </Button>
                  </td>
                </tr>
              ))}
              <tr>
                <td colSpan="2" className="text-end">
                  <strong>Totals</strong>
                </td>
                <td>
                  <strong>
                    {totals.debit.toLocaleString('en-US', {
                      style: 'currency',
                      currency: 'USD',
                    })}
                  </strong>
                </td>
                <td>
                  <strong>
                    {totals.credit.toLocaleString('en-US', {
                      style: 'currency',
                      currency: 'USD',
                    })}
                  </strong>
                </td>
                <td></td>
              </tr>
              {!isBalanced && (
                <tr className="table-danger">
                  <td colSpan="2" className="text-end">
                    <strong>Difference</strong>
                  </td>
                  <td colSpan="2">
                    <strong>
                      {Math.abs(totals.debit - totals.credit).toLocaleString('en-US', {
                        style: 'currency',
                        currency: 'USD',
                      })}
                    </strong>
                  </td>
                  <td></td>
                </tr>
              )}
            </tbody>
          </Table>

          <div className="text-center">
            <Button variant="outline-primary" onClick={handleAddLineItem}>
              <FontAwesomeIcon icon={faPlus} className="me-2" />
              Add Line Item
            </Button>
          </div>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={onHide}>
            Cancel
          </Button>
          <Button
            variant="primary"
            type="submit"
            disabled={!isBalanced}
          >
            <FontAwesomeIcon icon={faSave} className="me-2" />
            Save Journal Entry
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
};

CreateJournalEntry.propTypes = {
  show: PropTypes.bool.isRequired,
  onHide: PropTypes.func.isRequired,
  onSave: PropTypes.func.isRequired,
  accounts: PropTypes.array
};

CreateJournalEntry.defaultProps = {
  accounts: []
};

export default CreateJournalEntry;
