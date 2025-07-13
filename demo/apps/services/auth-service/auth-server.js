const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const bcrypt = require('bcryptjs');
const jwt = require('jsonwebtoken');
const { Pool } = require('pg');
const winston = require('winston');

const app = express();
const PORT = process.env.PORT || 8081;

// Logger setup
const logger = winston.createLogger({
  level: 'info',
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.json()
  ),
  transports: [new winston.transports.Console()]
});

// Database connection
const pool = new Pool({
  host: process.env.DB_HOST || 'postgres',
  port: process.env.DB_PORT || 5432,
  database: process.env.DB_NAME || 'dms_auth',
  user: process.env.DB_USER || 'dms_user',
  password: process.env.DB_PASSWORD || 'dms_password',
});

// Middleware
app.use(helmet());
app.use(cors());
app.use(express.json());

const JWT_SECRET = process.env.JWT_SECRET || 'demo-secret-key';

// Demo users
const demoUsers = [
  { id: 1, email: 'manager@dms-demo.com', password: '$2a$10$demo.hash', firstName: 'John', lastName: 'Manager', role: 'manager', permissions: ['all'] },
  { id: 2, email: 'sales@dms-demo.com', password: '$2a$10$demo.hash', firstName: 'Sarah', lastName: 'Sales', role: 'sales', permissions: ['inventory', 'sales', 'crm'] },
  { id: 3, email: 'service@dms-demo.com', password: '$2a$10$demo.hash', firstName: 'Mike', lastName: 'Service', role: 'service', permissions: ['service', 'parts', 'crm'] },
  { id: 4, email: 'parts@dms-demo.com', password: '$2a$10$demo.hash', firstName: 'Lisa', lastName: 'Parts', role: 'parts', permissions: ['parts', 'inventory'] },
  { id: 5, email: 'admin@dms-demo.com', password: '$2a$10$demo.hash', firstName: 'Admin', lastName: 'User', role: 'admin', permissions: ['all', 'users', 'settings'] }
];

// Health check
app.get('/health', (req, res) => {
  res.json({ status: 'healthy', service: 'Auth Service' });
});

// Login endpoint
app.post('/login', async (req, res) => {
  try {
    const { email, password, mfaCode, captchaToken } = req.body;

    // Find user (demo implementation)
    const user = demoUsers.find(u => u.email.toLowerCase() === email.toLowerCase());
    
    if (!user) {
      return res.status(401).json({ error: 'Invalid credentials' });
    }

    // For demo, accept any password that starts with 'Demo' or 'Admin'
    const passwordValid = password.startsWith('Demo') || password.startsWith('Admin');
    
    if (!passwordValid) {
      return res.status(401).json({ error: 'Invalid credentials' });
    }

    // Demo MFA check
    if (user.role === 'admin' && !mfaCode) {
      return res.json({ requiresMFA: true });
    }

    if (user.role === 'admin' && mfaCode && mfaCode !== '123456') {
      return res.status(401).json({ error: 'Invalid MFA code' });
    }

    // Generate tokens
    const accessToken = jwt.sign(
      { id: user.id, email: user.email, role: user.role },
      JWT_SECRET,
      { expiresIn: '24h' }
    );

    const refreshToken = jwt.sign(
      { id: user.id },
      JWT_SECRET,
      { expiresIn: '7d' }
    );

    res.json({
      accessToken,
      refreshToken,
      user: {
        id: user.id,
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
        role: user.role,
        permissions: user.permissions
      }
    });

  } catch (error) {
    logger.error('Login error:', error);
    res.status(500).json({ error: 'Login failed' });
  }
});

// Token verification
app.get('/verify', (req, res) => {
  try {
    const authHeader = req.headers.authorization;
    const token = authHeader && authHeader.split(' ')[1];

    if (!token) {
      return res.status(401).json({ error: 'Token required' });
    }

    const decoded = jwt.verify(token, JWT_SECRET);
    const user = demoUsers.find(u => u.id === decoded.id);

    if (!user) {
      return res.status(401).json({ error: 'User not found' });
    }

    res.json({
      user: {
        id: user.id,
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
        role: user.role,
        permissions: user.permissions
      }
    });

  } catch (error) {
    logger.error('Token verification error:', error);
    res.status(401).json({ error: 'Invalid token' });
  }
});

// Token verification (POST for backwards compatibility)
app.post('/verify', (req, res) => {
  try {
    const authHeader = req.headers.authorization;
    const token = authHeader && authHeader.split(' ')[1];

    if (!token) {
      return res.status(401).json({ error: 'Token required' });
    }

    const decoded = jwt.verify(token, JWT_SECRET);
    const user = demoUsers.find(u => u.id === decoded.id);

    if (!user) {
      return res.status(401).json({ error: 'User not found' });
    }

    res.json({
      user: {
        id: user.id,
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
        role: user.role,
        permissions: user.permissions
      }
    });

  } catch (error) {
    logger.error('Token verification error:', error);
    res.status(401).json({ error: 'Invalid token' });
  }
});

// Logout endpoint
app.post('/logout', (req, res) => {
  // In a real implementation, you'd invalidate the token
  res.json({ message: 'Logged out successfully' });
});

// Refresh token endpoint
app.post('/refresh', (req, res) => {
  try {
    const { refreshToken } = req.body;

    if (!refreshToken) {
      return res.status(401).json({ error: 'Refresh token required' });
    }

    const decoded = jwt.verify(refreshToken, JWT_SECRET);
    const user = demoUsers.find(u => u.id === decoded.id);

    if (!user) {
      return res.status(401).json({ error: 'User not found' });
    }

    const accessToken = jwt.sign(
      { id: user.id, email: user.email, role: user.role },
      JWT_SECRET,
      { expiresIn: '24h' }
    );

    res.json({ accessToken });

  } catch (error) {
    logger.error('Token refresh error:', error);
    res.status(401).json({ error: 'Invalid refresh token' });
  }
});

// Search endpoint
app.get('/search', (req, res) => {
  const { q } = req.query;
  const results = demoUsers
    .filter(user => 
      user.firstName.toLowerCase().includes(q.toLowerCase()) ||
      user.lastName.toLowerCase().includes(q.toLowerCase()) ||
      user.email.toLowerCase().includes(q.toLowerCase())
    )
    .map(user => ({
      type: 'user',
      id: user.id,
      title: `${user.firstName} ${user.lastName}`,
      subtitle: `${user.email} - ${user.role}`
    }));

  res.json({ results });
});

// Error handling
app.use((err, req, res, next) => {
  logger.error('Unhandled error:', err);
  res.status(500).json({ error: 'Internal server error' });
});

app.listen(PORT, '0.0.0.0', () => {
  logger.info(`Auth Service running on port ${PORT}`);
});
