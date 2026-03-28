import {defineConfig} from "vitest/config";
import {sharedAliases} from "./vitest.shared";

export default defineConfig({
    resolve: {
        alias: sharedAliases,
    },
    test: {
        environment: "jsdom",
        globals: true,
        clearMocks: true,
        include: ["tests/unit/**/*.test.ts"],
    },
});
