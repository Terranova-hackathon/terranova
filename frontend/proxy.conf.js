// Proxies /api to the mock (default) or the real backend. Set API_URL to switch.
const target = process.env.API_URL || 'http://localhost:4001';

module.exports = {
  '/api': { target, secure: false, changeOrigin: false },
};
