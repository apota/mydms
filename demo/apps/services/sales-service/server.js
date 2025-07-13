const createService = require('../service-template');

const sampleDeals = [
  { id: 1, customer: 'John Smith', vehicle: '2023 Honda Accord', amount: 28500, status: 'Negotiating', salesperson: 'Sarah Wilson' },
  { id: 2, customer: 'Mike Johnson', vehicle: '2022 Ford F-150', amount: 42500, status: 'Closed', salesperson: 'Bob Miller' }
];

const routes = {
  '/deals': sampleDeals,
  '/leads': [
    { id: 1, customer: 'Jane Doe', interest: '2024 Toyota Camry', status: 'Hot', source: 'Website' },
    { id: 2, customer: 'Tom Brown', interest: 'Used SUV', status: 'Warm', source: 'Referral' }
  ],
  '/search': (req, res) => {
    const { q } = req.query;
    const results = sampleDeals
      .filter(deal => deal.customer.toLowerCase().includes(q.toLowerCase()) || deal.vehicle.toLowerCase().includes(q.toLowerCase()))
      .map(deal => ({ type: 'deal', id: deal.id, title: `${deal.customer} - ${deal.vehicle}`, subtitle: `$${deal.amount.toLocaleString()} - ${deal.status}` }));
    res.json({ results });
  }
};

createService('Sales Service', 8083, routes);
