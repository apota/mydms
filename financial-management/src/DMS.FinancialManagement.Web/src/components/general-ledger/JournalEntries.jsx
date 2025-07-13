import React, { useState } from 'react';
import { Table, Form, Button, Card, Row, Col, Spinner, Badge, Modal } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faEye, faUndo, faSave, faSearch, faCheckCircle, faTimesCircle } from '@fortawesome/free-solid-svg-icons';
import PropTypes from 'prop-types';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';

const JournalEntries = ({ entries, isLoading, onDateRangeChange, onViewEntry, onReverseEntry }) => {
  const [startDate, setStartDate] = useState(() => {
    const date = new Date();
    date.setDate(date.getDate() - 30);
    return date;
  });
  const [endDate, setEndDate] = useState(new Date());
  const [showViewModal, setShowViewModal] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState(null);

  const handleDateRangeSubmit = (e) => {
    e.preventDefault();
    onDateRangeChange?.({ startDate, endDate });
  };

  const handleViewEntry = (entry) => {
    setSelectedEntry(entry);
    setShowViewModal(true);

    // If we have a callback, use it to get the full entry details
    if (onViewEntry) {
      onViewEntry(entry.id);
    }
  };

  const handleReverseEntry = (entry) => {
    if (window.confirm('Are you sure you want to reverse this journal entry? This action cannot be undone.')) {
      onReverseEntry?.(entry.id, {
        reversalDate: new Date(),
        reason: 'Manual reversal by user'
      });
    }
  };

  const getStatusBadge = (status) => {
    switch (status.toUpperCase()) {
      case 'DRAFT':
        return <Badge bg="secondary">Draft</Badge>;
      case 'POSTED':
        return <Badge bg="success">Posted</Badge>;
      case 'REVERSED':
        return <Badge bg="warning">Reversed</Badge>;
      default:
        return <Badge bg="info">{status}</Badge>;
    }
  };

  return (
    <div>
      <Card className="mb-4">
        <Card.Body>
          <Form onSubmit={handleDateRangeSubmit}>
            <Row className="align-items-end mb-3">
              <Col md={4}>
                <Form.Group>
                  <Form.Label>Start Date</Form.Label>
                  <DatePicker
                    selected={startDate}
                    onChange={date => setStartDate(date)}
                    selectsStart
                    startDate={startDate}
                    endDate={endDate}
                    className="form-control"
                  />
                </Form.Group>
              </Col>
              <Col md={4}>
                <Form.Group>
                  <Form.Label>End Date</Form.Label>
                  <DatePicker
                    selected={endDate}
                    onChange={date => setEndDate(date)}
                    selectsEnd
                    startDate={startDate}
                    endDate={endDate}
                    minDate={startDate}
                    className="form-control"
                  />
                </Form.Group>
              </Col>
              <Col md={4}>
                <Button variant="primary" type="submit">
                  <FontAwesomeIcon icon={faSearch} className="me-2" />
                  Search
                </Button>
              </Col>
            </Row>
          </Form>
        </Card.Body>
      </Card>

      <Card>
        <Card.Body className="p-0">
          {isLoading ? (
            <div className="text-center p-4">
              <Spinner animation="border" variant="primary" />
              <p className="mt-2">Loading journal entries...</p>
            </div>
          ) : (
            <Table hover responsive className="mb-0">
              <thead>
                <tr>
                  <th>Entry #</th>
                  <th>Date</th>
                  <th>Description</th>
                  <th>Status</th>
                  <th>Total Amount</th>
                  <th>Created By</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {entries.length === 0 ? (
                  <tr>
                    <td colSpan="7" className="text-center py-4">
                      No journal entries found for the selected date range
                    </td>
                  </tr>
                ) : (
                  entries.map(entry => (
                    <tr key={entry.id} className={entry.status === 'REVERSED' ? 'table-warning' : ''}>
                      <td>{entry.entryNumber}</td>
                      <td>{new Date(entry.entryDate).toLocaleDateString()}</td>
                      <td>{entry.description}</td>
                      <td>{getStatusBadge(entry.status)}</td>
                      <td className="text-end">
                        {entry.totalAmount?.toLocaleString('en-US', {
                          style: 'currency',
                          currency: 'USD',
                        })}
                      </td>
                      <td>{entry.createdBy}</td>
                      <td>
                        <Button
                          variant="outline-primary"
                          size="sm"
                          className="me-2"
                          onClick={() => handleViewEntry(entry)}
                        >
                          <FontAwesomeIcon icon={faEye} />
                        </Button>
                        {entry.status === 'POSTED' && (
                          <Button
                            variant="outline-warning"
                            size="sm"
                            onClick={() => handleReverseEntry(entry)}
                          >
                            <FontAwesomeIcon icon={faUndo} />
                          </Button>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </Table>
          )}
        </Card.Body>
      </Card>

      <JournalEntryViewModal
        show={showViewModal}
        onHide={() => setShowViewModal(false)}
        entry={selectedEntry}
      />
    </div>
  );
};

const JournalEntryViewModal = ({ show, onHide, entry }) => {
  if (!entry) return null;

  return (
    <Modal show={show} onHide={onHide} size="lg">
      <Modal.Header closeButton>
        <Modal.Title>Journal Entry #{entry.entryNumber}</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <Row className="mb-3">
          <Col md={6}>
            <strong>Date:</strong> {new Date(entry.entryDate).toLocaleDateString()}
          </Col>
          <Col md={6}>
            <strong>Status:</strong> {entry.status}
          </Col>
        </Row>

        <Row className="mb-3">
          <Col md={12}>
            <strong>Description:</strong> {entry.description}
          </Col>
        </Row>

        <hr />

        <h5 className="mb-3">Line Items</h5>
        <Table striped bordered hover>
          <thead>
            <tr>
              <th>Account</th>
              <th>Description</th>
              <th>Debit</th>
              <th>Credit</th>
            </tr>
          </thead>
          <tbody>
            {entry.lineItems?.map((item, index) => (
              <tr key={index}>
                <td>{item.accountNumber} - {item.accountName}</td>
                <td>{item.description}</td>
                <td className="text-end">
                  {item.debitAmount ? item.debitAmount.toLocaleString('en-US', {
                    style: 'currency',
                    currency: 'USD',
                  }) : '-'}
                </td>
                <td className="text-end">
                  {item.creditAmount ? item.creditAmount.toLocaleString('en-US', {
                    style: 'currency',
                    currency: 'USD',
                  }) : '-'}
                </td>
              </tr>
            ))}
            <tr className="table-secondary">
              <td colSpan="2"><strong>Totals</strong></td>
              <td className="text-end">
                <strong>
                  {entry.totalDebit?.toLocaleString('en-US', {
                    style: 'currency',
                    currency: 'USD',
                  })}
                </strong>
              </td>
              <td className="text-end">
                <strong>
                  {entry.totalCredit?.toLocaleString('en-US', {
                    style: 'currency',
                    currency: 'USD',
                  })}
                </strong>
              </td>
            </tr>
          </tbody>
        </Table>

        {entry.referenceNumber && (
          <Row className="mb-3">
            <Col md={12}>
              <strong>Reference:</strong> {entry.referenceNumber}
            </Col>
          </Row>
        )}

        {entry.postedBy && (
          <Row className="mb-3">
            <Col md={6}>
              <strong>Posted By:</strong> {entry.postedBy}
            </Col>
            <Col md={6}>
              <strong>Posted On:</strong> {new Date(entry.postedDate).toLocaleString()}
            </Col>
          </Row>
        )}

        {entry.reversalReason && (
          <Card className="mt-3 border-warning">
            <Card.Header className="bg-warning">Reversal Information</Card.Header>
            <Card.Body>
              <p><strong>Reason:</strong> {entry.reversalReason}</p>
              <p><strong>Reversed By:</strong> {entry.reversedBy}</p>
              <p><strong>Reversed On:</strong> {new Date(entry.reversedDate).toLocaleString()}</p>
              {entry.reversalEntryId && (
                <p><strong>Reversal Entry:</strong> #{entry.reversalEntryNumber}</p>
              )}
            </Card.Body>
          </Card>
        )}
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={onHide}>
          Close
        </Button>
      </Modal.Footer>
    </Modal>
  );
};

JournalEntries.propTypes = {
  entries: PropTypes.array,
  isLoading: PropTypes.bool,
  onDateRangeChange: PropTypes.func,
  onViewEntry: PropTypes.func,
  onReverseEntry: PropTypes.func
};

JournalEntries.defaultProps = {
  entries: [],
  isLoading: false
};

export default JournalEntries;
