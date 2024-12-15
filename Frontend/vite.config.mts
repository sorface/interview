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
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target:
          process.env['REACT_APP_PROXY_TARGET'] || 'http://localhost:5043/',
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
