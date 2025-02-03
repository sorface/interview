const { createProxyMiddleware } = require('http-proxy-middleware');
module.exports = function (app) {
  app.use(
    '/api/copilot',
    createProxyMiddleware({
      target: 'http://localhost:4300/',
      changeOrigin: true,
    }),
  );

  app.use(
    '/api',
    createProxyMiddleware({
      target: process.env['VITE_PROXY_TARGET'] || 'http://localhost:5043/',
      changeOrigin: true,
    }),
  );
};
