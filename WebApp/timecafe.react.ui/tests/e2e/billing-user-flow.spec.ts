import {expect, test} from "@playwright/test";
import {randomUUID} from "node:crypto";

const apiBaseUrl = process.env.VITE_API_BASE_URL ?? "http://127.0.0.1:8010";

const createRateLimitKey = () => `e2e-${Date.now()}-${randomUUID()}`;

const createTestCredentials = () => {
    const email = `test+e2e-billing-${Date.now()}-${randomUUID()}@timecafe.local`;
    const password = "Test1234!";
    return {email, password};
};

const registerAndConfirmUser = async (request: any, email: string, password: string) => {
    const headers = {
        "Content-Type": "application/json",
        "X-Test-RateLimit-Key": createRateLimitKey(),
    };

    const registerResponse = await request.post(`${apiBaseUrl}/auth/registerWithUsername-mock`, {
        headers,
        data: {username: email, email, password},
    });

    expect(registerResponse.ok()).toBeTruthy();

    const registerBody = await registerResponse.json();
    const callbackUrl = new URL(registerBody.callbackUrl);
    const userId = callbackUrl.searchParams.get("userId") ?? "";
    const token = (callbackUrl.searchParams.get("token") ?? "").replace(/ /g, "+");

    const confirmResponse = await request.post(`${apiBaseUrl}/auth/email/confirm`, {
        headers,
        data: {userId, token},
    });

    expect(confirmResponse.ok()).toBeTruthy();
    return userId;
};

test.describe("Billing User E2E Flow", () => {
    test("User view balance and redirects to Stripe checkout", async ({browser, request}) => {
        const {email, password} = createTestCredentials();
        await registerAndConfirmUser(request, email, password);

        const guestContext = await browser.newContext();
        const guestPage = await guestContext.newPage();

        await guestPage.goto("/login");
        await guestPage.getByLabel("Email").fill(email);
        await guestPage.getByLabel("Пароль").fill(password);
        await guestPage.getByTestId("login-submit").click();
        await guestPage.waitForURL(/\/home$/);

        const profileGateTitle = guestPage.getByTestId("profile-gate-title");
        await profileGateTitle.waitFor({ state: "visible", timeout: 4000 }).catch(() => {});
        if (await profileGateTitle.isVisible()) {
            await guestPage.getByTestId("profile-gate-last-name").fill("Клиентов");
            await guestPage.getByTestId("profile-gate-first-name").fill("Платежный");
            await guestPage.getByTestId("profile-gate-save").click();
            await expect(profileGateTitle).toBeHidden({timeout: 10000});

            const phoneModalTitle = guestPage.getByRole("heading", {name: "Подтверждение номера телефона"});
            await phoneModalTitle.waitFor({ state: "visible", timeout: 2000 }).catch(() => {});
            if (await phoneModalTitle.isVisible()) {
                await guestPage.getByRole("button", {name: "Отмена"}).click();
            }
        }

        await guestPage.goto("/billing");
        await guestPage.waitForURL(/\/billing$/);

        await expect(guestPage.getByText("Баланс и транзакции").first()).toBeVisible({timeout: 15000});

        await guestPage.getByPlaceholder("Введите сумму (₽)").fill("150");

        const checkoutRedirectPromise = guestPage.waitForResponse(response =>
            response.url().includes("/billing/payments/initialize-checkout") && response.request().method() === "POST",
            {timeout: 15000}
        );

        await guestPage.getByRole("button", {name: "Перейти к оплате Stripe"}).click();

        const checkoutResponse = await checkoutRedirectPromise;
        expect(checkoutResponse.ok()).toBeTruthy();

        await guestContext.close();
    });
});
