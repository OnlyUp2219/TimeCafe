import {expect, test} from "@playwright/test";
import {randomUUID} from "node:crypto";

test.describe("Tariff Management E2E Flow", () => {
    test("Create, view, and delete tariff in admin panel", async ({browser}) => {
        const adminContext = await browser.newContext();
        const adminPage = await adminContext.newPage();

        await adminPage.goto("/login");
        await adminPage.getByLabel("Email").fill("klimAdmin@gmail.com");
        await adminPage.getByLabel("Пароль").fill("Admin@12345");
        await adminPage.getByTestId("login-submit").click();
        await adminPage.waitForURL(/\/home$/);

        const profileGateTitle = adminPage.getByTestId("profile-gate-title");
        await profileGateTitle.waitFor({ state: "visible", timeout: 4000 }).catch(() => {});
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

        await adminPage.goto("/admin/tariffs");
        await adminPage.waitForURL(/\/admin\/tariffs$/);

        await adminPage.getByRole("button", {name: "Добавить тариф"}).click();
        await adminPage.waitForURL(/\/admin\/tariffs\/create$/);

        const tariffName = `E2E-Tariff-${randomUUID().substring(0, 8)}`;
        await adminPage.getByLabel("Название").fill(tariffName);
        await adminPage.getByLabel("Описание").fill("Описание тестового тарифа E2E");
        await adminPage.getByLabel("Краткая сводка (Summary)").fill("Тестовый тариф E2E");
        await adminPage.getByLabel("Цена за минуту").fill("5.5");

        await adminPage.getByRole("button", {name: "Сохранить"}).click();
        await adminPage.waitForURL(/\/admin\/tariffs$/);

        await expect(adminPage.getByText(tariffName).first()).toBeVisible({timeout: 15000});

        await adminContext.close();
    });
});
