import {defineConfig} from "vitest/config";
import {fileURLToPath} from "node:url";

export default defineConfig({
    resolve: {
        alias: {
            "@app": fileURLToPath(new URL("./src", import.meta.url)),
            "@components": fileURLToPath(new URL("./src/components", import.meta.url)),
            "@pages": fileURLToPath(new URL("./src/pages", import.meta.url)),
            "@shared": fileURLToPath(new URL("./src/shared", import.meta.url)),
            "@store": fileURLToPath(new URL("./src/store", import.meta.url)),
            "@hooks": fileURLToPath(new URL("./src/hooks", import.meta.url)),
            "@utility": fileURLToPath(new URL("./src/utility", import.meta.url)),
            "@assets": fileURLToPath(new URL("./src/assets", import.meta.url)),
            "@app-types": fileURLToPath(new URL("./src/types", import.meta.url)),
            "@layouts": fileURLToPath(new URL("./src/layouts", import.meta.url)),
            "@api": fileURLToPath(new URL("./src/shared/api", import.meta.url)),
            "@legacy": fileURLToPath(new URL("./src/legacy", import.meta.url)),
            "@tests": fileURLToPath(new URL("./tests", import.meta.url)),
        },
    },
    test: {
        environment: "node",
        globals: true,
        clearMocks: true,
        include: ["tests/integration/api/**/*.test.ts"],
        setupFiles: ["tests/integration/api/setup.ts"],
        testTimeout: 120000,
    },
});
