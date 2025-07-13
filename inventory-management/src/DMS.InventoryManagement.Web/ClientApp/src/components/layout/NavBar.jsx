import React from 'react';
import { Navbar, Nav, NavDropdown, Container } from 'react-bootstrap';
import { Link, NavLink } from 'react-router-dom';
import './NavBar.css';

const NavBar = () => {
  return (
    <Navbar bg="dark" variant="dark" expand="lg" className="navbar-custom" sticky="top">
      <Container fluid>
        <Navbar.Brand as={Link} to="/">
          <img
            src="/logo.png"
            width="30"
            height="30"
            className="d-inline-block align-top me-2"
            alt="DMS Logo"
          />
          DMS Inventory Management
        </Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">          <Nav className="me-auto">
            <Nav.Link as={NavLink} to="/inventory">Dashboard</Nav.Link>
            <Nav.Link as={NavLink} to="/inventory/search">Search Inventory</Nav.Link>
            <Nav.Link as={NavLink} to="/inventory/marketplace">Marketplace</Nav.Link>
            
            <NavDropdown title="Workflows" id="workflows-dropdown">
              <NavDropdown.Item as={NavLink} to="/inventory/workflows">Workflow Management</NavDropdown.Item>
              <NavDropdown.Item as={NavLink} to="/inventory/acquisition">Acquisition</NavDropdown.Item>
            </NavDropdown>
            
            <NavDropdown title="Analytics" id="analytics-dropdown">
              <NavDropdown.Item as={NavLink} to="/inventory/analytics/aging">Aging Heat Map</NavDropdown.Item>
              <NavDropdown.Item as={NavLink} to="/inventory/analytics/price-competitiveness">Price Competitiveness</NavDropdown.Item>
              <NavDropdown.Item as={NavLink} to="/inventory/analytics/mix">Inventory Mix Analysis</NavDropdown.Item>
              <NavDropdown.Item as={NavLink} to="/inventory/analytics/turnover">Turnover Metrics</NavDropdown.Item>
            </NavDropdown>
            
            <NavDropdown title="Administration" id="admin-dropdown">
              <NavDropdown.Item as={NavLink} to="/inventory/admin/locations">Manage Locations</NavDropdown.Item>
              <NavDropdown.Item as={NavLink} to="/inventory/admin/users">User Access</NavDropdown.Item>
              <NavDropdown.Item as={NavLink} to="/inventory/admin/import">Batch Import</NavDropdown.Item>
              <NavDropdown.Item as={NavLink} to="/inventory/admin/settings">System Settings</NavDropdown.Item>
            </NavDropdown>
          </Nav>
          
          <Nav>
            <NavDropdown title="Other Modules" id="modules-dropdown" align="end">
              <NavDropdown.Item as={Link} to="/crm">CRM</NavDropdown.Item>
              <NavDropdown.Item as={Link} to="/sales">Sales</NavDropdown.Item>
              <NavDropdown.Item as={Link} to="/service">Service</NavDropdown.Item>
              <NavDropdown.Item as={Link} to="/parts">Parts</NavDropdown.Item>
              <NavDropdown.Item as={Link} to="/financial">Financial</NavDropdown.Item>
            </NavDropdown>
            <NavDropdown title="Jane Doe" id="user-dropdown" align="end">
              <NavDropdown.Item>Profile</NavDropdown.Item>
              <NavDropdown.Item>Preferences</NavDropdown.Item>
              <NavDropdown.Divider />
              <NavDropdown.Item>Logout</NavDropdown.Item>
            </NavDropdown>
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default NavBar;
