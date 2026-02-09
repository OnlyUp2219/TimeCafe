import {defineConfig} from "vitest/config";

export default defineConfig({
    test: {
        environment: "node",
        globals: true,
        clearMocks: true,
        include: ["tests/integration/api/**/*.test.ts"],
        setupFiles: ["tests/integration/api/setup.ts"],
        testTimeout: 120000,
    },
});
