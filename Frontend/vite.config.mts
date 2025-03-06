import { defineConfig, ViteUserConfig } from 'vitest/config';
import react from '@vitejs/plugin-react-swc';

// https://vitejs.dev/config/
export default defineConfig({
  base: '/',
  plugins: [react()] as ViteUserConfig['plugins'],
  resolve: {
    alias: {
      'simple-peer': 'simple-peer/simplepeer.min.js',
    },
  },
  build: {
    outDir: 'build',
  },
  server: {
    port: 3000,
    proxy: {
      '/ai-assistant': {
        target: 'http://localhost:3033',
        changeOrigin: true,
        secure: true,
        ws: true,
      },
      '/api': {
        target: process.env['VITE_PROXY_TARGET'] || 'http://localhost:5043/',
        changeOrigin: true,
        secure: true,
        ws: true,
      },
    },
  },
  test: {
    include: ['src/**/*.test.(ts|tsx)'],
    environment: 'jsdom',
    globals: true,
    setupFiles: './tests/setup.ts',
  },
});
