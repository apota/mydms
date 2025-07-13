const createService = require('../service-template');

const sampleData = [
  { id: 1, name: 'Sample service appointments item 1', status: 'Active' },
  { id: 2, name: 'Sample service appointments item 2', status: 'Pending' }
];

const routes = {
  '/data': sampleData,
  '/search': (req, res) => {
    const { q } = req.query;
    const results = sampleData
      .filter(item => item.name.toLowerCase().includes(q.toLowerCase()))
      .map(item => ({ type: 'service appointments', id: item.id, title: item.name, subtitle: item.status }));
    res.json({ results });
  }
};

createService('service.ToUpper() Service', 8084, routes);
