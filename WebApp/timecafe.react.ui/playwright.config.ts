import {defineConfig, devices} from "@playwright/test";

const baseUrl = process.env.E2E_BASE_URL ?? "http://127.0.0.1:9301";

export default defineConfig({
    testDir: "./tests/e2e",
    timeout: 90_000,
    expect: {
        timeout: 15_000,
    },
    fullyParallel: false,
    forbidOnly: !!process.env.CI,
    retries: process.env.CI ? 1 : 0,
    workers: 1,
    reporter: "list",
    use: {
        baseURL: baseUrl,
        trace: "on-first-retry",
        screenshot: "only-on-failure",
        video: "retain-on-failure",
    },
    webServer: {
        command: "npm run dev -- --host 127.0.0.1 --port 9301",
        url: baseUrl,
        timeout: 120_000,
        reuseExistingServer: true,
    },
    projects: [
        {
            name: "chromium",
            use: {...devices["Desktop Chrome"]},
        },
    ],
});