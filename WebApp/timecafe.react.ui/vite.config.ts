import {defineConfig} from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import {fileURLToPath} from 'node:url';

export default defineConfig({
    plugins: [react(), tailwindcss()],
    resolve: {
        alias: {
            '@app': fileURLToPath(new URL('./src', import.meta.url)),
            '@components': fileURLToPath(new URL('./src/components', import.meta.url)),
            '@pages': fileURLToPath(new URL('./src/pages', import.meta.url)),
            '@shared': fileURLToPath(new URL('./src/shared', import.meta.url)),
            '@store': fileURLToPath(new URL('./src/store', import.meta.url)),
            '@hooks': fileURLToPath(new URL('./src/hooks', import.meta.url)),
            '@utility': fileURLToPath(new URL('./src/utility', import.meta.url)),
            '@assets': fileURLToPath(new URL('./src/assets', import.meta.url)),
            '@app-types': fileURLToPath(new URL('./src/types', import.meta.url)),
            '@layouts': fileURLToPath(new URL('./src/layouts', import.meta.url)),
            '@api': fileURLToPath(new URL('./src/shared/api', import.meta.url)),
            '@legacy': fileURLToPath(new URL('./src/legacy', import.meta.url)),
        },
    },
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