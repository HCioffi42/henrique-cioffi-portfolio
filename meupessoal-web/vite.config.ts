import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

/**
 * Vite configuration.
 * Registers the Tailwind CSS v4 plugin to handle @import "tailwindcss" in CSS files.
 */
export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
  ],
});

// https://vite.dev/config/