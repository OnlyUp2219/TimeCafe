import {defineConfig} from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
    plugins: [react(), tailwindcss()],
    build: {
        sourcemap: true,
        minify: 'terser',
        terserOptions: {
            compress: {
                drop_console: true, // Удаляет console.log
                drop_debugger: true, // Удаляет debugger
                pure_funcs: ['console.info', 'console.debug', 'console.warn'], // Удаляет дополнительные console-вызовы
                passes: 3, // Многопроходная оптимизация
            },
            mangle: {
                toplevel: true, // Сжимает имена переменных на верхнем уровне
            },
        },
        rollupOptions: {
            output: {
                manualChunks: {
                    vendor: ['react', 'react-dom', 'react-router-dom', 'axios', 'react-redux', 'redux-persist'],
                    fluent: ['@fluentui/react-components'],
                },
            },
        },
    },
    server: {
        host: '127.0.0.1',
        port: 9301,
    },
});