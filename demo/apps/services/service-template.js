const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const winston = require('winston');

// Generic microservice template
function createService(serviceName, port, routes = {}) {
  const app = express();
  const PORT = process.env.PORT || port;

  // Logger setup
  const logger = winston.createLogger({
    level: 'info',
    format: winston.format.combine(
      winston.format.timestamp(),
      winston.format.json()
    ),
    transports: [new winston.transports.Console()]
  });

  // Middleware
  app.use(helmet());
  app.use(cors());
  app.use(express.json());

  // Authentication middleware (simplified for demo)
  const authenticateToken = (req, res, next) => {
    const authHeader = req.headers['authorization'];
    if (!authHeader && !req.path.includes('/health')) {
      return res.status(401).json({ error: 'Access token required' });
    }
    req.user = { id: 1, role: 'demo' }; // Mock user for demo
    next();
  };

  app.use(authenticateToken);

  // Health check
  app.get('/health', (req, res) => {
    res.json({
      status: 'healthy',
      service: serviceName,
      timestamp: new Date().toISOString()
    });
  });

  // Default search endpoint
  app.get('/search', (req, res) => {
    const { q } = req.query;
    res.json({
      results: [{
        type: serviceName.toLowerCase(),
        id: 'demo-1',
        title: `${serviceName} search result for "${q}"`,
        subtitle: `Demo result from ${serviceName}`
      }]
    });
  });

  // Add custom routes
  Object.entries(routes).forEach(([path, handler]) => {
    if (typeof handler === 'function') {
      app.get(path, handler);
    } else {
      app.get(path, (req, res) => res.json(handler));
    }
  });

  // Error handling
  app.use((err, req, res, next) => {
    logger.error('Unhandled error:', err);
    res.status(500).json({ error: 'Internal server error' });
  });

  app.listen(PORT, '0.0.0.0', () => {
    logger.info(`${serviceName} running on port ${PORT}`);
  });

  return app;
}

module.exports = createService;
