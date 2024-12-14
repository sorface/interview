import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

// https://vitejs.dev/config/
export default defineConfig({
  base: '/',
  plugins: [react()],
  define: {
    global: {},
  },
  server: {
    proxy: {
      '/api': {
        target: process.env['REACT_APP_PROXY_TARGET'] || 'http://localhost:5043/',
        changeOrigin: true,
        secure: true,
        ws: true,
      }
    }
  }
});
