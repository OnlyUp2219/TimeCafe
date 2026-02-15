import {expect, test} from "@playwright/test";
import type {APIRequestContext} from "@playwright/test";
import type {Page} from "@playwright/test";
import {randomUUID} from "node:crypto";

const apiBaseUrl = process.env.VITE_API_BASE_URL ?? "http://localhost:8010";

const createRateLimitKey = () => `e2e-${Date.now()}-${randomUUID()}`;

const createTestCredentials = () => {
    const email = `test+e2e-${Date.now()}-${randomUUID()}@timecafe.local`;
    const password = "Test1234!";
    return {email, password};
};

const registerAndConfirmUser = async (request: APIRequestContext, email: string, password: string) => {
    const headers = {
        "Content-Type": "application/json",
        "X-Test-RateLimit-Key": createRateLimitKey(),
    };

    const registerResponse = await request.post(`${apiBaseUrl}/auth/registerWithUsername-mock`, {
        headers,
        data: {username: email, email, password},
    });

    expect(registerResponse.ok()).toBeTruthy();

    const registerBody = await registerResponse.json() as { callbackUrl?: string };
    expect(registerBody.callbackUrl).toBeTruthy();

    const callbackUrl = new URL(registerBody.callbackUrl!);
    const userId = callbackUrl.searchParams.get("userId") ?? "";
    const token = (callbackUrl.searchParams.get("token") ?? "").replace(/ /g, "+");

    expect(userId).toBeTruthy();
    expect(token).toBeTruthy();

    const confirmResponse = await request.post(`${apiBaseUrl}/auth/email/confirm`, {
        headers,
        data: {userId, token},
    });

    expect(confirmResponse.ok()).toBeTruthy();
    return userId;
};

const waitUntilLoginReady = async (request: APIRequestContext, email: string, password: string) => {
    for (let attempt = 0; attempt < 10; attempt += 1) {
        const loginResponse = await request.post(`${apiBaseUrl}/auth/login-jwt-v2`, {
            headers: {
                "Content-Type": "application/json",
                "X-Test-RateLimit-Key": createRateLimitKey(),
            },
            data: {email, password},
        });

        if (loginResponse.ok()) {
            const body = await loginResponse.json() as { accessToken?: string };
            if (body.accessToken) {
                return;
            }
        }

        await new Promise(resolve => setTimeout(resolve, 700));
    }

    throw new Error("Пользователь не готов к входу после подтверждения email");
};

const setupFlowApiMocks = async (page: Page, userId: string) => {
    const tariffId = randomUUID();
    const visitId = randomUUID();
    let activeVisit: null | {
        entryTime: string;
    } = null;

    await page.route("**/userprofile/profiles/*", async (route) => {
        if (route.request().method() !== "GET") {
            await route.continue();
            return;
        }

        await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
                userId,
                firstName: "Тестов",
                lastName: "Е2Е",
                middleName: null,
                birthDate: null,
                gender: 0,
                email: "",
                phoneNumber: "",
                profileStatus: 1,
            }),
        });
    });

    await page.route("**/venue/tariffs/active", async (route) => {
        if (route.request().method() !== "GET") {
            await route.continue();
            return;
        }

        await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
                tariffs: [
                    {
                        tariffId,
                        name: "E2E Тариф",
                        description: "Тестовый тариф",
                        pricePerMinute: 3,
                        billingType: 1,
                        isActive: true,
                        createdAt: new Date().toISOString(),
                        lastModified: new Date().toISOString(),
                        themeId: null,
                        themeName: "Default",
                        themeEmoji: "☕",
                        themeColors: null,
                    },
                ],
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
            body: JSON.stringify({
                hasActiveVisit: !!activeVisit,
            }),
        });
    });

    await page.route("**/venue/visits/active/*", async (route) => {
        if (route.request().method() !== "GET") {
            await route.continue();
            return;
        }

        if (!activeVisit) {
            await route.fulfill({
                status: 404,
                contentType: "application/json",
                body: JSON.stringify({message: "Visit not found"}),
            });
            return;
        }

        await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
                visit: {
                    visitId,
                    userId,
                    tariffId,
                    entryTime: activeVisit.entryTime,
                    exitTime: null,
                    calculatedCost: null,
                    status: 1,
                    tariffName: "E2E Тариф",
                    tariffPricePerMinute: 3,
                    tariffDescription: "Тестовый тариф",
                    tariffBillingType: 1,
                },
            }),
        });
    });

    await page.route("**/venue/visits/end", async (route) => {
        if (route.request().method() !== "POST") {
            await route.continue();
            return;
        }

        const now = new Date().toISOString();
        activeVisit = null;

        await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
                message: "Визит завершён",
                visit: {
                    visitId,
                    userId,
                    tariffId,
                    entryTime: now,
                    exitTime: now,
                    calculatedCost: 15,
                    status: 2,
                },
                calculatedCost: 15,
            }),
        });
    });

    await page.route("**/venue/visits", async (route) => {
        if (route.request().method() !== "POST") {
            await route.continue();
            return;
        }

        const entryTime = new Date().toISOString();
        activeVisit = {entryTime};

        await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
                message: "Визит создан",
                visit: {
                    visitId,
                    userId,
                    tariffId,
                    entryTime,
                    exitTime: null,
                    calculatedCost: null,
                    status: 1,
                },
            }),
        });
    });
};

const completeProfileGateIfNeeded = async (page: Page) => {
    const profileGateTitle = page.getByRole("heading", {name: "Заполните профиль"});
    const isVisible = await profileGateTitle.isVisible({timeout: 8000}).catch(() => false);
    if (!isVisible) return;

    await page.getByLabel("Фамилия").fill("Тестов");
    await page.getByLabel("Имя").fill("Е2Е");
    await page.getByRole("button", {name: "Сохранить"}).first().click();
    await expect(profileGateTitle).toBeHidden({timeout: 15000});

    const phoneModalTitle = page.getByRole("heading", {name: "Подтверждение номера телефона"});
    const phoneModalVisible = await phoneModalTitle.isVisible({timeout: 1500}).catch(() => false);
    if (phoneModalVisible) {
        await page.getByRole("button", {name: "Отмена"}).click();
    }
};

const ensureTariffsLoaded = async (page: Page) => {
    const startButton = page.getByTestId("visit-start-submit");

    for (let attempt = 0; attempt < 6; attempt += 1) {
        await completeProfileGateIfNeeded(page);

        const enabled = await startButton.isEnabled().catch(() => false);
        if (enabled) return;

        const retryButton = page.getByTestId("visit-start-retry");
        const hasRetry = await retryButton.isVisible({timeout: 1500}).catch(() => false);
        if (hasRetry) {
            await retryButton.click();
        }

        await page.waitForTimeout(1500);
    }

    await expect(startButton).toBeEnabled({timeout: 15000});
};

test("Visit flow start-active-end", async ({page, request}) => {
    const {email, password} = createTestCredentials();
    const userId = await registerAndConfirmUser(request, email, password);
    await waitUntilLoginReady(request, email, password);
    await setupFlowApiMocks(page, userId);

    await page.goto("/login");
    await page.getByLabel("Email").fill(email);
    await page.getByLabel("Пароль").fill(password);
    await page.getByTestId("login-submit").click();
    await expect(page).toHaveURL(/\/home$/);
    await completeProfileGateIfNeeded(page);

    await page.getByTestId("home-visit-action").click();
    await expect(page).toHaveURL(/\/visit\/start$/);
    await expect(page.getByTestId("visit-start-page")).toBeVisible();

    await ensureTariffsLoaded(page);
    await expect(page.getByTestId("visit-start-submit")).toBeEnabled();
    await page.getByTestId("visit-start-submit").click();

    await expect(page).toHaveURL(/\/visit\/active$/);
    await expect(page.getByTestId("visit-active-page")).toBeVisible();

    await page.getByTestId("visit-active-exit").click();
    await expect(page.getByTestId("visit-end-dialog-title")).toBeVisible();
    await page.getByTestId("visit-end-confirm").click();

    await expect(page).toHaveURL(/\/visit\/start$/);
    await expect(page.getByTestId("visit-start-page")).toBeVisible();
});