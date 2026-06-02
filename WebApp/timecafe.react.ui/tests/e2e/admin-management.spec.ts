import {expect, test} from "@playwright/test";

test.describe("Admin Management E2E Flow", () => {
    test("Users list filtering and basic admin navigation", async ({browser}) => {
        const adminContext = await browser.newContext();
        const adminPage = await adminContext.newPage();

        await adminPage.goto("/login");
        await adminPage.getByLabel("Email").fill("klimAdmin@gmail.com");
        await adminPage.getByLabel("Пароль").fill("Admin@12345");
        await adminPage.getByTestId("login-submit").click();
        await adminPage.waitForURL(/\/home$/);

        const profileGateTitle = adminPage.getByTestId("profile-gate-title");
        await profileGateTitle.waitFor({ state: "visible", timeout: 5000 }).catch(() => {});
        if (await profileGateTitle.isVisible()) {
            await adminPage.getByTestId("profile-gate-last-name").fill("Системный");
            await adminPage.getByTestId("profile-gate-first-name").fill("Администратор");
            await adminPage.getByTestId("profile-gate-save").click();
            await expect(profileGateTitle).toBeHidden({timeout: 10000});

            const phoneModalTitle = adminPage.getByRole("heading", {name: "Подтверждение номера телефона"});
            await phoneModalTitle.waitFor({ state: "visible", timeout: 2000 }).catch(() => {});
            if (await phoneModalTitle.isVisible()) {
                await adminPage.getByRole("button", {name: "Отмена"}).click();
            }
        }

        await adminPage.goto("/admin/users");
        await adminPage.waitForURL(/\/admin\/users$/);
        await expect(adminPage.getByText("Пользователи").first()).toBeVisible({timeout: 15000});

        const statusSelect = adminPage.locator("select").first();
        await expect(statusSelect).toBeVisible();

        await statusSelect.selectOption("active");
        await adminPage.waitForTimeout(1000);
        await expect(adminPage.getByText("зарегистрированных пользователей").first()).toBeVisible();

        await statusSelect.selectOption("inactive");
        await adminPage.waitForTimeout(1000);
        await expect(adminPage.getByText("зарегистрированных пользователей").first()).toBeVisible();

        await statusSelect.selectOption("");
        await adminPage.waitForTimeout(1000);

        await adminPage.goto("/admin/tariffs");
        await adminPage.waitForURL(/\/admin\/tariffs$/);
        await expect(adminPage.getByText("Тарифы").first()).toBeVisible({timeout: 15000});

        await adminContext.close();
    });
});
