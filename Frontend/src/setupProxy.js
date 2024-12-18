const { createProxyMiddleware } = require('http-proxy-middleware');
module.exports = function (app) {
  app.use(
    '/api',
    createProxyMiddleware({
      target: process.env['VITE_PROXY_TARGET'] || 'http://localhost:5043/',
      changeOrigin: true,
    }),
  );
};
