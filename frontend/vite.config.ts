import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Forward API calls to the ASP.NET Core backend so no CORS setup is needed in dev.
    proxy: {
      '/api': 'http://localhost:5120',
    },
  },
})
