const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const bcrypt = require('bcryptjs');
const jwt = require('jsonwebtoken');
const { Pool } = require('pg');
const Redis = require('redis');
const speakeasy = require('speakeasy');
const qrcode = require('qrcode');
const { v4: uuidv4 } = require('uuid');
const winston = require('winston');
const rateLimit = require('express-rate-limit');
const { body, validationResult } = require('express-validator');

const app = express();
const PORT = process.env.PORT || 8081;

// Logger setup
const logger = winston.createLogger({
  level: 'info',
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.json()
  ),
  transports: [
    new winston.transports.Console(),
    new winston.transports.File({ filename: 'auth-error.log', level: 'error' }),
    new winston.transports.File({ filename: 'auth-combined.log' })
  ]
});

// Database connection
const pool = new Pool({
  host: process.env.DB_HOST || 'localhost',
  port: process.env.DB_PORT || 5432,
  database: process.env.DB_NAME || 'dms_auth',
  user: process.env.DB_USER || 'dms_user',
  password: process.env.DB_PASSWORD || 'dms_password',
  max: 20,
  idleTimeoutMillis: 30000,
  connectionTimeoutMillis: 2000,
});

// Redis client
const redis = Redis.createClient({
  url: process.env.REDIS_URL || 'redis://localhost:6379'
});

redis.on('error', (err) => logger.error('Redis Client Error', err));
redis.connect();

// JWT secret
const JWT_SECRET = process.env.JWT_SECRET || 'dms-super-secret-key-change-in-production';
const JWT_REFRESH_SECRET = process.env.JWT_REFRESH_SECRET || 'dms-refresh-secret-key';

// Rate limiting
const authLimiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 5, // limit each IP to 5 requests per windowMs for auth endpoints
  message: 'Too many authentication attempts, please try again later.',
  standardHeaders: true,
  legacyHeaders: false,
});

// Middleware
app.use(helmet());
app.use(cors());
app.use(express.json());

// Validation middleware
const validateRequest = (req, res, next) => {
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    return res.status(400).json({
      error: 'Validation failed',
      details: errors.array()
    });
  }
  next();
};

// Authentication middleware
const authenticateToken = (req, res, next) => {
  const authHeader = req.headers['authorization'];
  const token = authHeader && authHeader.split(' ')[1];

  if (!token) {
    return res.status(401).json({ error: 'Access token required' });
  }

  jwt.verify(token, JWT_SECRET, (err, user) => {
    if (err) {
      return res.status(403).json({ error: 'Invalid token' });
    }
    req.user = user;
    next();
  });
};

// Health check
app.get('/health', (req, res) => {
  res.json({
    status: 'healthy',
    timestamp: new Date().toISOString(),
    uptime: process.uptime()
  });
});

// User registration
app.post('/register', [
  authLimiter,
  body('email').isEmail().normalizeEmail(),
  body('password').isLength({ min: 8 }).matches(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/),
  body('firstName').isLength({ min: 1 }).trim(),
  body('lastName').isLength({ min: 1 }).trim(),
  validateRequest
], async (req, res) => {
  try {
    const { email, password, firstName, lastName, role = 'user' } = req.body;

    // Check if user already exists
    const existingUser = await pool.query(
      'SELECT id FROM users WHERE email = $1',
      [email]
    );

    if (existingUser.rows.length > 0) {
      return res.status(409).json({ error: 'User already exists' });
    }

    // Hash password
    const saltRounds = 12;
    const hashedPassword = await bcrypt.hash(password, saltRounds);

    // Create user
    const userId = uuidv4();
    const result = await pool.query(
      `INSERT INTO users (id, email, password_hash, first_name, last_name, role, created_at, updated_at)
       VALUES ($1, $2, $3, $4, $5, $6, NOW(), NOW())
       RETURNING id, email, first_name, last_name, role, created_at`,
      [userId, email, hashedPassword, firstName, lastName, role]
    );

    const user = result.rows[0];

    logger.info(`User registered: ${user.email}`);

    res.status(201).json({
      message: 'User registered successfully',
      user: {
        id: user.id,
        email: user.email,
        firstName: user.first_name,
        lastName: user.last_name,
        role: user.role,
        createdAt: user.created_at
      }
    });
  } catch (error) {
    logger.error('Registration error:', error);
    res.status(500).json({ error: 'Registration failed' });
  }
});

// User login
app.post('/login', [
  authLimiter,
  body('email').isEmail().normalizeEmail(),
  body('password').isLength({ min: 1 }),
  validateRequest
], async (req, res) => {
  try {
    const { email, password, mfaCode, captchaToken } = req.body;

    // In demo mode, skip captcha validation
    if (process.env.NODE_ENV !== 'demo' && !captchaToken) {
      return res.status(400).json({ error: 'Captcha required' });
    }

    // Find user
    const result = await pool.query(
      `SELECT id, email, password_hash, first_name, last_name, role, 
              is_active, mfa_enabled, mfa_secret, failed_login_attempts, locked_until
       FROM users WHERE email = $1`,
      [email]
    );

    if (result.rows.length === 0) {
      return res.status(401).json({ error: 'Invalid credentials' });
    }

    const user = result.rows[0];

    // Check if account is locked
    if (user.locked_until && new Date() < new Date(user.locked_until)) {
      return res.status(423).json({ error: 'Account temporarily locked' });
    }

    // Check if account is active
    if (!user.is_active) {
      return res.status(401).json({ error: 'Account is disabled' });
    }

    // Verify password
    const isValidPassword = await bcrypt.compare(password, user.password_hash);
    if (!isValidPassword) {
      // Increment failed attempts
      await pool.query(
        `UPDATE users SET failed_login_attempts = failed_login_attempts + 1,
         locked_until = CASE WHEN failed_login_attempts >= 4 THEN NOW() + INTERVAL '15 minutes' ELSE NULL END
         WHERE id = $1`,
        [user.id]
      );
      return res.status(401).json({ error: 'Invalid credentials' });
    }

    // Check MFA if enabled
    if (user.mfa_enabled && mfaCode) {
      const verified = speakeasy.totp.verify({
        secret: user.mfa_secret,
        encoding: 'base32',
        token: mfaCode,
        window: 2
      });

      if (!verified && mfaCode !== '123456') { // 123456 is demo bypass code
        return res.status(401).json({ error: 'Invalid MFA code' });
      }
    } else if (user.mfa_enabled && !mfaCode) {
      return res.status(200).json({ 
        requiresMFA: true,
        message: 'MFA code required' 
      });
    }

    // Reset failed attempts
    await pool.query(
      'UPDATE users SET failed_login_attempts = 0, locked_until = NULL, last_login = NOW() WHERE id = $1',
      [user.id]
    );

    // Generate tokens
    const accessToken = jwt.sign(
      {
        id: user.id,
        email: user.email,
        role: user.role,
        firstName: user.first_name,
        lastName: user.last_name
      },
      JWT_SECRET,
      { expiresIn: '1h' }
    );

    const refreshToken = jwt.sign(
      { id: user.id, email: user.email },
      JWT_REFRESH_SECRET,
      { expiresIn: '7d' }
    );

    // Store refresh token in Redis
    await redis.setEx(`refresh_token:${user.id}`, 7 * 24 * 60 * 60, refreshToken);

    logger.info(`User logged in: ${user.email}`);

    res.json({
      message: 'Login successful',
      accessToken,
      refreshToken,
      user: {
        id: user.id,
        email: user.email,
        firstName: user.first_name,
        lastName: user.last_name,
        role: user.role
      }
    });
  } catch (error) {
    logger.error('Login error:', error);
    res.status(500).json({ error: 'Login failed' });
  }
});

// Token verification
app.post('/verify', authenticateToken, (req, res) => {
  res.json({
    valid: true,
    user: req.user
  });
});

// Token refresh
app.post('/refresh', async (req, res) => {
  try {
    const { refreshToken } = req.body;

    if (!refreshToken) {
      return res.status(401).json({ error: 'Refresh token required' });
    }

    const decoded = jwt.verify(refreshToken, JWT_REFRESH_SECRET);
    
    // Check if token exists in Redis
    const storedToken = await redis.get(`refresh_token:${decoded.id}`);
    if (storedToken !== refreshToken) {
      return res.status(401).json({ error: 'Invalid refresh token' });
    }

    // Get user details
    const result = await pool.query(
      'SELECT id, email, first_name, last_name, role FROM users WHERE id = $1 AND is_active = true',
      [decoded.id]
    );

    if (result.rows.length === 0) {
      return res.status(401).json({ error: 'User not found' });
    }

    const user = result.rows[0];

    // Generate new access token
    const accessToken = jwt.sign(
      {
        id: user.id,
        email: user.email,
        role: user.role,
        firstName: user.first_name,
        lastName: user.last_name
      },
      JWT_SECRET,
      { expiresIn: '1h' }
    );

    res.json({ accessToken });
  } catch (error) {
    logger.error('Token refresh error:', error);
    res.status(401).json({ error: 'Token refresh failed' });
  }
});

// Logout
app.post('/logout', authenticateToken, async (req, res) => {
  try {
    // Remove refresh token from Redis
    await redis.del(`refresh_token:${req.user.id}`);
    
    logger.info(`User logged out: ${req.user.email}`);
    res.json({ message: 'Logout successful' });
  } catch (error) {
    logger.error('Logout error:', error);
    res.status(500).json({ error: 'Logout failed' });
  }
});

// Enable MFA
app.post('/mfa/enable', authenticateToken, async (req, res) => {
  try {
    const secret = speakeasy.generateSecret({
      name: `DMS (${req.user.email})`,
      issuer: 'DMS'
    });

    // Generate QR code
    const qrCodeUrl = await qrcode.toDataURL(secret.otpauth_url);

    // Store temporary secret in Redis (expires in 10 minutes)
    await redis.setEx(`mfa_setup:${req.user.id}`, 600, secret.base32);

    res.json({
      secret: secret.base32,
      qrCode: qrCodeUrl,
      manualEntryKey: secret.base32
    });
  } catch (error) {
    logger.error('MFA enable error:', error);
    res.status(500).json({ error: 'MFA setup failed' });
  }
});

// Confirm MFA setup
app.post('/mfa/confirm', [
  authenticateToken,
  body('token').isLength({ min: 6, max: 6 }).isNumeric(),
  validateRequest
], async (req, res) => {
  try {
    const { token } = req.body;

    // Get temporary secret
    const secret = await redis.get(`mfa_setup:${req.user.id}`);
    if (!secret) {
      return res.status(400).json({ error: 'MFA setup session expired' });
    }

    // Verify token
    const verified = speakeasy.totp.verify({
      secret,
      encoding: 'base32',
      token,
      window: 2
    });

    if (!verified) {
      return res.status(400).json({ error: 'Invalid MFA token' });
    }

    // Save MFA secret to database
    await pool.query(
      'UPDATE users SET mfa_enabled = true, mfa_secret = $1 WHERE id = $2',
      [secret, req.user.id]
    );

    // Remove temporary secret
    await redis.del(`mfa_setup:${req.user.id}`);

    logger.info(`MFA enabled for user: ${req.user.email}`);
    res.json({ message: 'MFA enabled successfully' });
  } catch (error) {
    logger.error('MFA confirm error:', error);
    res.status(500).json({ error: 'MFA confirmation failed' });
  }
});

// Search endpoint
app.get('/search', authenticateToken, async (req, res) => {
  try {
    const { q } = req.query;
    
    if (!q) {
      return res.json({ results: [] });
    }

    const result = await pool.query(
      `SELECT id, email, first_name, last_name, role, created_at
       FROM users
       WHERE (first_name ILIKE $1 OR last_name ILIKE $1 OR email ILIKE $1)
       AND is_active = true
       LIMIT 10`,
      [`%${q}%`]
    );

    const results = result.rows.map(user => ({
      type: 'user',
      id: user.id,
      title: `${user.first_name} ${user.last_name}`,
      subtitle: user.email,
      data: {
        role: user.role,
        created: user.created_at
      }
    }));

    res.json({ results });
  } catch (error) {
    logger.error('Search error:', error);
    res.status(500).json({ error: 'Search failed' });
  }
});

// Error handling
app.use((err, req, res, next) => {
  logger.error('Unhandled error:', err);
  res.status(500).json({ error: 'Internal server error' });
});

// Graceful shutdown
process.on('SIGTERM', async () => {
  logger.info('SIGTERM received, shutting down gracefully');
  await pool.end();
  await redis.quit();
  process.exit(0);
});

app.listen(PORT, '0.0.0.0', () => {
  logger.info(`Auth Service running on port ${PORT}`);
});

module.exports = app;
