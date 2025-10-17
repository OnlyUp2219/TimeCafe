import {defineConfig} from 'vite';
import plugin from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin(), tailwindcss()],
    build: {
        sourcemap: true,
    },
    server: {
        host: '127.0.0.1',
        port: 9301,
    }
})
