const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const winston = require('winston');
const axios = require('axios');

const app = express();
const PORT = process.env.PORT || 8088;

// Logger setup
const logger = winston.createLogger({
  level: 'info',
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.json()
  ),
  transports: [
    new winston.transports.Console(),
  ]
});

// Middleware
app.use(helmet());
app.use(cors());
app.use(express.json());

// Authentication middleware (simplified for demo)
const authenticateToken = (req, res, next) => {
  const authHeader = req.headers['authorization'];
  if (!authHeader) {
    return res.status(401).json({ error: 'Access token required' });
  }
  // In a real implementation, verify the token here
  req.user = { id: 1, name: 'Demo User' }; // Mock user for demo
  next();
};

// Health check
app.get('/health', (req, res) => {
  res.json({
    status: 'healthy',
    timestamp: new Date().toISOString(),
    service: 'AI Assistant (Rudy)'
  });
});

// Chat endpoint
app.post('/chat', authenticateToken, async (req, res) => {
  try {
    const { message, userId, conversationId } = req.body;
    
    logger.info(`Chat request from user ${userId}: ${message}`);
    
    // Simple AI responses based on keywords (in production, this would use actual AI)
    const response = await generateResponse(message.toLowerCase());
    
    res.json({
      response: response.text,
      actions: response.actions || [],
      timestamp: new Date().toISOString()
    });
  } catch (error) {
    logger.error('Chat error:', error);
    res.status(500).json({ error: 'Failed to process chat message' });
  }
});

// Search endpoint for AI context
app.get('/search', authenticateToken, async (req, res) => {
  try {
    const { q } = req.query;
    
    // Mock search results for AI assistant
    const results = [
      {
        type: 'ai_response',
        id: 'ai-1',
        title: 'AI Assistant Available',
        subtitle: `Ask Rudy about "${q}"`,
        data: { query: q }
      }
    ];

    res.json({ results });
  } catch (error) {
    logger.error('AI search error:', error);
    res.status(500).json({ error: 'Search failed' });
  }
});

// Generate AI response based on message content
async function generateResponse(message) {
  const responses = {
    // Inventory queries
    'vehicles': {
      text: "I can help you with vehicle inventory! We currently have 146 vehicles in stock. Would you like me to show you available vehicles, search by specific criteria, or check inventory aging reports?",
      actions: [
        { label: "View Available Vehicles", action: "navigate", target: "/inventory" },
        { label: "Aging Report", action: "report", target: "inventory_aging" }
      ]
    },
    'inventory': {
      text: "Our current inventory includes 67 new vehicles and 79 used vehicles. The average days in inventory is 45 days. Would you like detailed analytics or specific vehicle information?",
      actions: [
        { label: "View Inventory", action: "navigate", target: "/inventory" },
        { label: "Analytics Dashboard", action: "navigate", target: "/dashboard" }
      ]
    },
    
    // Sales queries
    'sales': {
      text: "This month we've sold 67 vehicles with $268K in revenue - that's 12% above last month! Our top performer is Sarah with 15 sales. How can I help with sales management?",
      actions: [
        { label: "View Sales Dashboard", action: "navigate", target: "/sales" },
        { label: "Performance Report", action: "report", target: "sales_performance" }
      ]
    },
    'leads': {
      text: "We have 23 active leads in the pipeline. 8 are hot leads requiring immediate follow-up. Would you like me to show you the lead details or help schedule follow-ups?",
      actions: [
        { label: "View Leads", action: "navigate", target: "/sales" },
        { label: "Schedule Follow-ups", action: "schedule", target: "follow_ups" }
      ]
    },
    
    // Service queries
    'service': {
      text: "The service department has 89 pending work orders and 12 appointments scheduled for today. Average wait time is 2.5 hours. How can I assist with service operations?",
      actions: [
        { label: "View Service Queue", action: "navigate", target: "/service" },
        { label: "Schedule Appointment", action: "create", target: "service_appointment" }
      ]
    },
    'appointment': {
      text: "I can help you schedule a service appointment! What type of service is needed? I can check technician availability and suggest optimal time slots.",
      actions: [
        { label: "New Appointment", action: "create", target: "service_appointment" },
        { label: "View Calendar", action: "navigate", target: "/service" }
      ]
    },
    
    // Customer queries
    'customer': {
      text: "I can help you find customer information! We have 1,247 active customers in our database. Would you like me to search for a specific customer or show recent interactions?",
      actions: [
        { label: "Search Customers", action: "search", target: "customers" },
        { label: "Recent Activity", action: "navigate", target: "/crm" }
      ]
    },
    
    // Parts queries
    'parts': {
      text: "Our parts department has 2,156 items in stock. 15 items are below reorder level and need immediate attention. How can I help with parts management?",
      actions: [
        { label: "View Parts Inventory", action: "navigate", target: "/parts" },
        { label: "Reorder Report", action: "report", target: "parts_reorder" }
      ]
    },
    
    // Financial queries
    'revenue': {
      text: "Current month revenue is $268K (8% above target). Vehicle sales: $201K, Service: $45K, Parts: $22K. Would you like detailed financial reports?",
      actions: [
        { label: "Financial Dashboard", action: "navigate", target: "/financial" },
        { label: "Detailed Reports", action: "report", target: "financial_summary" }
      ]
    },
    'profit': {
      text: "Gross profit margin this month is 18.5%. New vehicle margin: 6.2%, Used vehicle margin: 12.8%, Service margin: 65%, Parts margin: 40%. Any specific analysis needed?",
      actions: [
        { label: "Profit Analysis", action: "report", target: "profit_analysis" }
      ]
    },
    
    // General help
    'help': {
      text: "I'm Rudy, your AI assistant! I can help you with:\n• Vehicle inventory management\n• Sales performance tracking\n• Service scheduling\n• Customer information\n• Parts inventory\n• Financial reporting\n\nJust ask me anything about your dealership operations!",
      actions: []
    }
  };

  // Check for specific keywords
  for (const [keyword, response] of Object.entries(responses)) {
    if (message.includes(keyword)) {
      return response;
    }
  }

  // Handle specific questions
  if (message.includes('john smith')) {
    return {
      text: "I found John Smith in our customer database! He purchased a 2020 Honda Civic in March 2023 and is due for his next service appointment. His last service was an oil change 3 months ago. Would you like me to schedule a follow-up?",
      actions: [
        { label: "View Customer Profile", action: "view", target: "customer_1" },
        { label: "Schedule Service", action: "create", target: "service_appointment" }
      ]
    };
  }

  if (message.includes('60 days') || message.includes('aging')) {
    return {
      text: "I found 8 vehicles that have been in inventory for over 60 days:\n• 2023 Honda Accord (78 days)\n• 2022 Ford F-150 (65 days)\n• 2023 Toyota Camry (62 days)\n\nThese vehicles may benefit from pricing adjustments or targeted marketing. Would you like me to generate an aging report?",
      actions: [
        { label: "Full Aging Report", action: "report", target: "inventory_aging" },
        { label: "Suggest Pricing", action: "analyze", target: "pricing_suggestions" }
      ]
    };
  }

  if (message.includes('performance') && message.includes('month')) {
    return {
      text: "This month's performance summary:\n• Sales: 67 vehicles (+12% vs last month)\n• Revenue: $268K (+8% vs last month)\n• Service ROs: 234 (+5% vs last month)\n• Customer satisfaction: 4.8/5\n\nOverall, we're performing excellently! Any specific metrics you'd like me to analyze?",
      actions: [
        { label: "Detailed Analytics", action: "navigate", target: "/dashboard" },
        { label: "Comparison Report", action: "report", target: "monthly_comparison" }
      ]
    };
  }

  // Default response
  return {
    text: "I understand you're asking about your dealership operations. I can help you with inventory, sales, service, customers, parts, and financial information. Could you be more specific about what you'd like to know? For example, you could ask about 'vehicle inventory', 'sales performance', or 'service appointments'.",
    actions: [
      { label: "View Dashboard", action: "navigate", target: "/dashboard" },
      { label: "Help Guide", action: "help", target: "ai_guide" }
    ]
  };
}

// Error handling
app.use((err, req, res, next) => {
  logger.error('Unhandled error:', err);
  res.status(500).json({ error: 'Internal server error' });
});

app.listen(PORT, '0.0.0.0', () => {
  logger.info(`AI Service (Rudy) running on port ${PORT}`);
});
