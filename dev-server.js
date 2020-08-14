/**
 * Development proxy server
 *
 * Serves static files from wwwroot and proxies /api requests to the ASP.NET
 * backend (default: http://localhost:5000), eliminating CORS issues during
 * local development.
 *
 * Usage:
 *   npm run dev          # start the dev proxy on http://localhost:3000
 *
 * Proxy rules are read from proxy.config.json.
 */

'use strict';

const express = require('express');
const { createProxyMiddleware } = require('http-proxy-middleware');
const path = require('path');
const fs = require('fs');

const PORT = process.env.PORT || 3000;
const proxyConfigPath = path.join(__dirname, 'proxy.config.json');
const proxyConfig = JSON.parse(fs.readFileSync(proxyConfigPath, 'utf8'));

const app = express();

for (const [context, options] of Object.entries(proxyConfig)) {
  app.use(context, createProxyMiddleware(options));
}

app.use(express.static(path.join(__dirname, 'wwwroot')));

app.listen(PORT, () => {
  console.log(`Dev server running at http://localhost:${PORT}`);
  console.log(`API requests proxied to ${proxyConfig['/api']?.target ?? 'configured target'}`);
});
