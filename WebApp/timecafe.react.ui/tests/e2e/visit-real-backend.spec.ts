import {expect, test} from "@playwright/test";
import type {APIRequestContext, Page} from "@playwright/test";
import {randomUUID} from "node:crypto";

const apiBaseUrl = process.env.VITE_API_BASE_URL ?? "http://127.0.0.1:8010";

const createRateLimitKey = () => `e2e-${Date.now()}-${randomUUID()}`;

const checkBackendHealth = async (request: APIRequestContext) => {
    try {
        const response = await request.get(`${apiBaseUrl}/health`);
        if (!response.ok()) {
            throw new Error(`Health check returned status ${response.status()}`);
        }
    } catch (error) {
        console.error("Backend health check failed:", error);
        throw new Error(
            "Бэкенд не запущен локально на порту 8010. Пожалуйста, запустите Docker-контейнеры и бэкенд перед прогоном E2E-тестов."
        );
    }
};

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
        const loginResponse = await request.post(`${apiBaseUrl}/auth/login-jwt`, {
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

const depositUserBalance = async (request: APIRequestContext, userId: string, amount: number) => {
    const loginResponse = await request.post(`${apiBaseUrl}/auth/login-jwt`, {
        headers: {
            "Content-Type": "application/json",
            "X-Test-RateLimit-Key": createRateLimitKey(),
        },
        data: {email: "klimAdmin@gmail.com", password: "Admin@12345"},
    });
    expect(loginResponse.ok()).toBeTruthy();
    const loginBody = await loginResponse.json() as { accessToken: string };
    const adminToken = loginBody.accessToken;
    expect(adminToken).toBeTruthy();

    const depositResponse = await request.post(`${apiBaseUrl}/billing/transactions`, {
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${adminToken}`,
            "X-Test-RateLimit-Key": createRateLimitKey(),
        },
        data: {
            userId,
            amount,
            type: 1, // Deposit
            source: 2, // Manual
            comment: "Пополнение баланса для E2E теста",
        },
    });
    expect(depositResponse.ok()).toBeTruthy();
};

const completeProfileGateIfNeeded = async (page: Page, firstName: string, lastName: string) => {
    const profileGateTitle = page.getByTestId("profile-gate-title");
    await profileGateTitle.waitFor({ state: "visible", timeout: 8000 }).catch(() => {});
    if (!(await profileGateTitle.isVisible())) return;

    await page.getByTestId("profile-gate-last-name").fill(lastName);
    await page.getByTestId("profile-gate-first-name").fill(firstName);
    await page.getByTestId("profile-gate-save").click();
    await expect(profileGateTitle).toBeHidden({timeout: 15000});

    const phoneModalTitle = page.getByRole("heading", {name: "Подтверждение номера телефона"});
    await phoneModalTitle.waitFor({ state: "visible", timeout: 12000 }).catch(() => {});
    if (await phoneModalTitle.isVisible()) {
        await page.locator(".phone-verification-modal").getByRole("button", {name: "Отмена"}).click();
        await expect(phoneModalTitle).toBeHidden({timeout: 10000});
    }
};

const setupAccountMeMock = async (page: Page) => {
    await page.route("**/auth/account/me", async (route) => {
        if (route.request().method() !== "GET") {
            await route.continue();
            return;
        }
        try {
            const response = await route.fetch();
            if (response.ok()) {
                const json = await response.json();
                json.phoneNumberConfirmed = true;
                await route.fulfill({
                    response,
                    json,
                });
            } else {
                await route.continue();
            }
        } catch (e) {
            await route.continue();
        }
    });
};

const ensureTariffsLoaded = async (page: Page) => {
    const startButton = page.getByTestId("visit-start-submit");

    for (let attempt = 0; attempt < 6; attempt += 1) {
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

test.describe("Real backend E2E flows", () => {
    test.beforeAll(async ({request}) => {
        await checkBackendHealth(request);
    });

    test("Visit Full Lifecycle: Start -> Pending -> Admin Approve -> Active -> Request Exit -> Admin Fixate & Complete", async ({browser, request}) => {
        const {email, password} = createTestCredentials();
        const guestUserId = await registerAndConfirmUser(request, email, password);
        await waitUntilLoginReady(request, email, password);
        await depositUserBalance(request, guestUserId, 5000);

        const guestContext = await browser.newContext();
        const guestPage = await guestContext.newPage();
        await guestPage.route("**/auth/account/me", async (route) => {
            if (route.request().method() !== "GET") {
                await route.continue();
                return;
            }
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    userId: guestUserId,
                    email: email,
                    emailConfirmed: true,
                    phoneNumberConfirmed: true,
                    role: "client",
                }),
            });
        });

        await guestPage.goto("/login");
        await guestPage.evaluate(() => localStorage.setItem("isE2E", "true"));
        await guestPage.getByLabel("Email").fill(email);
        await guestPage.getByLabel("Пароль").fill(password);
        await guestPage.getByTestId("login-submit").click();
        
        await guestPage.waitForURL(/\/home$/);
        await completeProfileGateIfNeeded(guestPage, "Тестов", "Е2Е");

        await guestPage.getByTestId("home-visit-action").click();
        await guestPage.waitForURL(/\/visit\/start$/);

        await ensureTariffsLoaded(guestPage);
        await expect(guestPage.getByTestId("visit-start-submit")).toBeEnabled();

        const createVisitResponsePromise = guestPage.waitForResponse(response =>
            response.url().includes("/venue/visits") && response.request().method() === "POST",
            {timeout: 15000}
        );

        await guestPage.getByTestId("visit-start-submit").click();

        const createVisitResponse = await createVisitResponsePromise;
        expect(createVisitResponse.ok()).toBeTruthy();
        const createVisitBody = await createVisitResponse.json() as { visitId?: string; visit?: { visitId: string } };
        const visitId = createVisitBody.visitId ?? createVisitBody.visit?.visitId;
        expect(visitId).toBeTruthy();

        // Даем кэшу Redis/HybridCache на бэкенде гарантированно инвалидироваться, затем переходим
        await guestPage.waitForTimeout(2500);
        await guestPage.goto("/visit/active");

        await guestPage.waitForURL(/\/visit\/active$/);
        await expect(guestPage.getByTestId("visit-active-page")).toBeVisible();
        await expect(guestPage.getByText("Ожидаем подтверждения").first()).toBeVisible();

        const adminContext = await browser.newContext();
        const adminPage = await adminContext.newPage();
        await setupAccountMeMock(adminPage);

        await adminPage.goto("/login");
        await adminPage.evaluate(() => localStorage.setItem("isE2E", "true"));
        await adminPage.getByLabel("Email").fill("klimAdmin@gmail.com");
        await adminPage.getByLabel("Пароль").fill("Admin@12345");
        await adminPage.getByTestId("login-submit").click();
        await adminPage.waitForURL(/\/home$/);

        await completeProfileGateIfNeeded(adminPage, "Администратор", "Системный");

        await adminPage.goto("/admin/visits/pending");
        await expect(adminPage.getByText("Ожидают подтверждения").first()).toBeVisible();

        const approveButtonSelector = adminPage.getByRole("button", { name: "Подтвердить" }).first();
        await expect(approveButtonSelector).toBeVisible({timeout: 15000});
        await approveButtonSelector.click();

        await expect(adminPage.getByText("Подтвердить визит").first()).toBeVisible();
        await adminPage.getByRole("dialog").getByRole("button", {name: "Подтвердить", exact: true}).click();
        await expect(adminPage.getByText("Подтвердить визит").first()).toBeHidden({timeout: 10000});

        await adminContext.close();

        await guestPage.bringToFront();
        await expect(guestPage.getByText("Активный визит").first()).toBeVisible({timeout: 25000});

        const closePhoneModal = async () => {
            const title = guestPage.getByRole("heading", {name: "Подтверждение номера телефона"});
            await title.waitFor({ state: "visible", timeout: 2500 }).catch(() => {});
            if (await title.isVisible()) {
                await guestPage.locator(".phone-verification-modal").getByRole("button", {name: "Отмена"}).click();
                await expect(title).toBeHidden({timeout: 5000});
            }
        };

        await closePhoneModal();
        await guestPage.getByTestId("visit-active-exit").click();

        await expect(guestPage.getByTestId("visit-end-dialog-title")).toBeVisible();
        await closePhoneModal();
        await guestPage.getByTestId("visit-end-confirm").click();

        await expect(guestPage.getByText("Запрос на выход отправлен администратору").first()).toBeVisible({timeout: 15000});

        const secondAdminContext = await browser.newContext();
        const secondAdminPage = await secondAdminContext.newPage();
        await setupAccountMeMock(secondAdminPage);

        await secondAdminPage.goto("/login");
        await secondAdminPage.evaluate(() => localStorage.setItem("isE2E", "true"));
        await secondAdminPage.getByLabel("Email").fill("klimAdmin@gmail.com");
        await secondAdminPage.getByLabel("Пароль").fill("Admin@12345");
        await secondAdminPage.getByTestId("login-submit").click();
        await secondAdminPage.waitForURL(/\/home$/);

        await secondAdminPage.goto(`/admin/visits/${visitId}`);
        await expect(secondAdminPage.getByText("Визит").first()).toBeVisible({timeout: 15000});

        const fixateButton = secondAdminPage.getByRole("button", {name: "Зафиксировать время (Запрошен выход)"});
        await expect(fixateButton).toBeVisible({timeout: 10000});
        await fixateButton.click();

        const cashPaymentButton = secondAdminPage.getByRole("button", {name: "Наличными (Cash)"});
        await expect(cashPaymentButton).toBeVisible({timeout: 15000});
        await cashPaymentButton.click();

        await expect(secondAdminPage.getByText("Завершен")).toBeVisible({timeout: 15000});

        await secondAdminContext.close();

        await guestPage.bringToFront();
        await expect(guestPage.getByText("Визит успешно оплачен!").first()).toBeVisible({timeout: 25000});

        await guestPage.getByRole("button", {name: "К выбору тарифа"}).click();
        await guestPage.waitForURL(/\/visit\/start$/);

        await guestContext.close();
    });
});
