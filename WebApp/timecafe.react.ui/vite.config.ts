import {defineConfig} from 'vite';
import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    build: {
        sourcemap: true,
    },
    server: {
        host: '127.0.0.1',
        port: 9301,
    }
})
