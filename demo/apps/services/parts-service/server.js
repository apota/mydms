const createService = require('../service-template');

const sampleData = [
  { id: 1, name: 'Sample parts inventory item 1', status: 'Active' },
  { id: 2, name: 'Sample parts inventory item 2', status: 'Pending' }
];

const routes = {
  '/data': sampleData,
  '/search': (req, res) => {
    const { q } = req.query;
    const results = sampleData
      .filter(item => item.name.toLowerCase().includes(q.toLowerCase()))
      .map(item => ({ type: 'parts inventory', id: item.id, title: item.name, subtitle: item.status }));
    res.json({ results });
  }
};

createService('parts.ToUpper() Service', 8085, routes);
