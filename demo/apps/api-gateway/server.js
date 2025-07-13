const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const morgan = require('morgan');
const compression = require('compression');
const rateLimit = require('express-rate-limit');
const { createProxyMiddleware } = require('http-proxy-middleware');
const jwt = require('jsonwebtoken');
const Redis = require('redis');
const winston = require('winston');

const app = express();
const PORT = process.env.PORT || 8080;

// Logger setup
const logger = winston.createLogger({
  level: 'info',
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.json()
  ),
  transports: [
    new winston.transports.Console(),
    new winston.transports.File({ filename: 'error.log', level: 'error' }),
    new winston.transports.File({ filename: 'combined.log' })
  ]
});

// Redis client
const redis = Redis.createClient({
  url: process.env.REDIS_URL || 'redis://localhost:6379'
});

redis.on('error', (err) => logger.error('Redis Client Error', err));
redis.connect();

// Rate limiting
const limiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 1000, // limit each IP to 1000 requests per windowMs
  message: 'Too many requests from this IP, please try again later.',
  standardHeaders: true,
  legacyHeaders: false,
});

// Middleware
app.use(helmet({
  crossOriginEmbedderPolicy: false,
  contentSecurityPolicy: {
    directives: {
      defaultSrc: ["'self'"],
      styleSrc: ["'self'", "'unsafe-inline'"],
      scriptSrc: ["'self'"],
      imgSrc: ["'self'", "data:", "https:"],
    },
  },
}));

app.use(cors({
  origin: [
    'http://localhost:3000',
    'http://localhost:3001',
    'http://localhost:4566'
  ],
  credentials: true
}));

app.use(compression());
app.use(limiter);
app.use(morgan('combined', { stream: { write: message => logger.info(message.trim()) } }));
app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true, limit: '10mb' }));

// Authentication middleware
const authenticateToken = async (req, res, next) => {
  const authHeader = req.headers['authorization'];
  const token = authHeader && authHeader.split(' ')[1];

  if (!token) {
    // Allow public endpoints - note: paths here are AFTER the service prefix is stripped
    const publicPaths = ['/login', '/register', '/verify', '/health', '/docs'];
    if (publicPaths.includes(req.path) || publicPaths.some(path => req.path.startsWith(path + '/'))) {
      return next();
    }
    return res.status(401).json({ error: 'Access token required' });
  }

  try {
    // Verify token with auth service
    const response = await fetch(`${process.env.AUTH_SERVICE_URL}/verify`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      }
    });

    if (!response.ok) {
      return res.status(401).json({ error: 'Invalid token' });
    }

    const user = await response.json();
    req.user = user;
    next();
  } catch (error) {
    logger.error('Token verification error:', error);
    return res.status(401).json({ error: 'Token verification failed' });
  }
};

// Service URLs
const serviceMap = {
  '/auth': process.env.AUTH_SERVICE_URL || 'http://localhost:8081',
  '/inventory': process.env.INVENTORY_SERVICE_URL || 'http://localhost:8082',
  '/sales': process.env.SALES_SERVICE_URL || 'http://localhost:8083',
  '/service': process.env.SERVICE_SERVICE_URL || 'http://localhost:8084',
  '/parts': process.env.PARTS_SERVICE_URL || 'http://localhost:8085',
  '/crm': process.env.CRM_SERVICE_URL || 'http://localhost:8086',
  '/financial': process.env.FINANCIAL_SERVICE_URL || 'http://localhost:8087',
  '/ai': process.env.AI_SERVICE_URL || 'http://localhost:8088',
  '/users': process.env.USER_SERVICE_URL || 'http://localhost:8089',
  '/settings': process.env.SETTINGS_SERVICE_URL || 'http://localhost:8090'
};

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({
    status: 'healthy',
    timestamp: new Date().toISOString(),
    uptime: process.uptime(),
    services: Object.keys(serviceMap)
  });
});

// Global search endpoint
app.get('/search', authenticateToken, async (req, res) => {
  try {
    const { q, type } = req.query;
    
    if (!q) {
      return res.status(400).json({ error: 'Search query required' });
    }

    // Search across all services
    const searchPromises = Object.entries(serviceMap).map(async ([path, url]) => {
      try {
        const response = await fetch(`${url}/search?q=${encodeURIComponent(q)}&type=${type || ''}`, {
          headers: {
            'Authorization': req.headers.authorization
          },
          timeout: 5000
        });
        
        if (response.ok) {
          const results = await response.json();
          return { service: path.substring(1), results };
        }
      } catch (error) {
        logger.warn(`Search failed for ${path}:`, error.message);
      }
      return { service: path.substring(1), results: [] };
    });

    const searchResults = await Promise.all(searchPromises);
    
    res.json({
      query: q,
      results: searchResults.filter(result => result.results.length > 0)
    });
  } catch (error) {
    logger.error('Global search error:', error);
    res.status(500).json({ error: 'Search failed' });
  }
});

// Proxy configuration
Object.entries(serviceMap).forEach(([path, target]) => {
  app.use(
    path,
    authenticateToken,
    createProxyMiddleware({
      target,
      changeOrigin: true,
      pathRewrite: (reqPath) => reqPath.replace(path, ''),
      onError: (err, req, res) => {
        logger.error(`Proxy error for ${path}:`, err);
        res.status(502).json({ error: 'Service unavailable' });
      },
      onProxyReq: (proxyReq, req) => {
        // Add user context to headers
        if (req.user) {
          proxyReq.setHeader('X-User-ID', req.user.id);
          proxyReq.setHeader('X-User-Role', req.user.role);
          proxyReq.setHeader('X-User-Permissions', JSON.stringify(req.user.permissions || []));
        }
      }
    })
  );
});

// 404 handler
app.use('*', (req, res) => {
  res.status(404).json({ error: 'Endpoint not found' });
});

// Error handler
app.use((err, req, res, next) => {
  logger.error('Unhandled error:', err);
  res.status(500).json({ error: 'Internal server error' });
});

// Graceful shutdown
process.on('SIGTERM', async () => {
  logger.info('SIGTERM received, shutting down gracefully');
  await redis.quit();
  process.exit(0);
});

app.listen(PORT, '0.0.0.0', () => {
  logger.info(`API Gateway running on port ${PORT}`);
  logger.info('Service mappings:', serviceMap);
});

module.exports = app;
