import React from 'react';
import { Container } from 'react-bootstrap';
import './Footer.css';

const Footer = () => {
  return (
    <footer className="footer">
      <Container fluid>
        <div className="footer-content">
          <p className="mb-0">
            &copy; {new Date().getFullYear()} Dealership Management System | Inventory Module
          </p>
          <p className="mb-0">
            <a href="/help" className="footer-link">Help</a>
            <span className="mx-2">|</span>
            <a href="/support" className="footer-link">Support</a>
            <span className="mx-2">|</span>
            <a href="/privacy" className="footer-link">Privacy Policy</a>
          </p>
        </div>
      </Container>
    </footer>
  );
};

export default Footer;
