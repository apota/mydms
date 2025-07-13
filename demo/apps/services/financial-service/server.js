const createService = require('../service-template');

const sampleData = [
  { id: 1, name: 'Sample financial records item 1', status: 'Active' },
  { id: 2, name: 'Sample financial records item 2', status: 'Pending' }
];

const routes = {
  '/data': sampleData,
  '/search': (req, res) => {
    const { q } = req.query;
    const results = sampleData
      .filter(item => item.name.toLowerCase().includes(q.toLowerCase()))
      .map(item => ({ type: 'financial records', id: item.id, title: item.name, subtitle: item.status }));
    res.json({ results });
  }
};

createService('financial.ToUpper() Service', 8087, routes);
