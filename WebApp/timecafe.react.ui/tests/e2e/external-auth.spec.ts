import {expect, test} from "@playwright/test";
import {randomUUID} from "node:crypto";

const apiBaseUrl = process.env.VITE_API_BASE_URL ?? "http://localhost:8010";

const toBase64Url = (value: string): string => {
    return Buffer.from(value)
        .toString("base64")
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=+$/g, "");
};

const createFakeAccessToken = () => {
    const nowSec = Math.floor(Date.now() / 1000);
    const payload = {
        sub: randomUUID(),
        email: `e2e-${Date.now()}@timecafe.local`,
        role: "client",
        exp: nowSec + 3600,
        iat: nowSec,
    };

    const header = {alg: "none", typ: "JWT"};
    return `${toBase64Url(JSON.stringify(header))}.${toBase64Url(JSON.stringify(payload))}.`;
};

const mockAuthorizedHomeRequests = async (page: import("@playwright/test").Page) => {
    await page.route("**/auth/account/me", async (route) => {
        if (route.request().method() !== "GET") {
            await route.continue();
            return;
        }

        await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
                userId: randomUUID(),
                email: "e2e@timecafe.local",
                emailConfirmed: true,
                phoneNumberConfirmed: false,
                role: "client",
            }),
        });
    });

    await page.route("**/userprofile/profiles/*", async (route) => {
        if (route.request().method() !== "GET") {
            await route.continue();
            return;
        }

        await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
                userId: randomUUID(),
                firstName: "E2E",
                lastName: "Tester",
                middleName: null,
                birthDate: null,
                gender: 0,
                email: "e2e@timecafe.local",
                phoneNumber: "",
                profileStatus: 1,
            }),
        });
    });

    await page.route("**/venue/visits/has-active/*", async (route) => {
        if (route.request().method() !== "GET") {
            await route.continue();
            return;
        }

        await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({hasActiveVisit: false}),
        });
    });
};

test.describe("External auth", () => {
    test("Login page redirects to Google provider endpoint", async ({page}) => {
        await page.route("**/auth/authenticate/login/google**", async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "text/html",
                body: "ok",
            });
        });

        await page.goto("/login");

        const expectedReturnUrl = encodeURIComponent(new URL("/external-callback", page.url()).toString());

        const requestPromise = page.waitForRequest((request) => request.url().startsWith(`${apiBaseUrl}/auth/authenticate/login/google`));

        await page.getByRole("button", {name: "Google"}).click();

        const request = await requestPromise;
        expect(request.url()).toContain(`returnUrl=${expectedReturnUrl}`);
    });

    test("Login page redirects to Microsoft provider endpoint", async ({page}) => {
        await page.route("**/auth/authenticate/login/microsoft**", async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "text/html",
                body: "ok",
            });
        });

        await page.goto("/login");

        const expectedReturnUrl = encodeURIComponent(new URL("/external-callback", page.url()).toString());

        const requestPromise = page.waitForRequest((request) => request.url().startsWith(`${apiBaseUrl}/auth/authenticate/login/microsoft`));

        await page.getByRole("button", {name: "Microsoft"}).click();

        const request = await requestPromise;
        expect(request.url()).toContain(`returnUrl=${expectedReturnUrl}`);
    });

    test("External callback with token redirects to home", async ({page}) => {
        await mockAuthorizedHomeRequests(page);

        const token = createFakeAccessToken();
        await page.goto(`/external-callback#access_token=${encodeURIComponent(token)}&emailConfirmed=true`);

        await expect(page).toHaveURL(/\/home$/);
    });
});
