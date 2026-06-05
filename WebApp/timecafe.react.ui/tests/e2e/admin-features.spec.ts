import {expect, test} from "@playwright/test";

const ALL_MOCK_PERMISSIONS = [
    "userprofile.profile.create", "userprofile.profile.read", "userprofile.profile.update", "userprofile.profile.delete",
    "userprofile.additionalinfo.create", "userprofile.additionalinfo.read", "userprofile.additionalinfo.update", "userprofile.additionalinfo.delete",
    "userprofile.photo.create", "userprofile.photo.read", "userprofile.photo.delete",
    "billing.balance.read", "billing.debt.read", "billing.transaction.create", "billing.transaction.read",
    "billing.payment.initialize", "billing.payment.read", "billing.invoice.read", "billing.invoice.pay",
    "billing.invoice.admin.read", "billing.invoice.admin.write",
    "venue.tariff.create", "venue.tariff.read", "venue.tariff.update", "venue.tariff.delete",
    "venue.tariff.activate", "venue.tariff.deactivate",
    "venue.promotion.create", "venue.promotion.read", "venue.promotion.update", "venue.promotion.delete",
    "venue.promotion.activate", "venue.promotion.deactivate",
    "venue.theme.create", "venue.theme.read", "venue.theme.update", "venue.theme.delete",
    "venue.visit.create", "venue.visit.read", "venue.visit.update", "venue.visit.delete", "venue.visit.end",
    "visit.approve", "visit.reject", "visit.view.pending", "venue.loyalty.read",
    "venue.resource.create", "venue.resource.read", "venue.resource.update", "venue.resource.delete",
    "auth.account.self.read", "auth.account.admin.read", "auth.account.email.change", "auth.account.password.change",
    "auth.account.phone.save", "auth.account.phone.clear", "auth.account.phone.generate", "auth.account.phone.verify",
    "auth.account.phone.status.read",
    "auth.rbac.role.create", "auth.rbac.role.read", "auth.rbac.role.update", "auth.rbac.role.delete",
    "auth.rbac.role.claims.update", "auth.rbac.permission.read", "auth.rbac.userrole.assign", "auth.rbac.userrole.remove",
    "auth.rbac.superadmin",
    "audit.log.read", "audit.log.admin.read"
];

const generateMockToken = () => {
    const payload = {
        sub: "admin-user-id",
        role: "admin",
        email: "admin@timecafe.local",
        exp: Math.floor(Date.now() / 1000) + 3600 * 24
    };
    const base64Payload = Buffer.from(JSON.stringify(payload)).toString("base64")
        .replace(/=/g, "")
        .replace(/\+/g, "-")
        .replace(/\//g, "_");
    return `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.${base64Payload}.mocksignature`;
};

test.describe("Admin Features Mocked E2E Tests", () => {
    test.beforeEach(async ({page}) => {
        page.on("pageerror", (err) => {
            console.error("Browser JS error:", err.stack || err.message);
        });

        page.on("console", (msg) => {
            if (msg.type() === "error") {
                console.error("Browser console error:", msg.text());
            }
        });

        const mockToken = generateMockToken();

        await page.route(/\/auth\/login-jwt/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    accessToken: mockToken,
                    refreshToken: "mock-admin-refresh-token"
                })
            });
        });

        await page.route(/\/auth\/refresh-jwt/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    accessToken: mockToken
                })
            });
        });

        await page.route(/\/auth\/account\/me/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    userId: "admin-user-id",
                    email: "admin@timecafe.local",
                    emailConfirmed: true,
                    phoneNumberConfirmed: true,
                    role: "admin"
                })
            });
        });

        await page.route(/\/auth\/account\/my-permissions/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    permissions: ALL_MOCK_PERMISSIONS
                })
            });
        });

        await page.route(/\/userprofile\/profiles\/admin-user-id/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    userId: "admin-user-id",
                    firstName: "Администратор",
                    lastName: "Системный",
                    middleName: "",
                    birthDate: null,
                    gender: 0,
                    email: "admin@timecafe.local",
                    phoneNumber: "+79991112233",
                    profileStatus: 1
                })
            });
        });

        await page.route(/\/venue\/resource-groups/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify([
                    {
                        resourceGroupId: "group-1",
                        name: "Основной зал",
                        description: "Уютная зона с диванами"
                    },
                    {
                        resourceGroupId: "group-2",
                        name: "VIP комната",
                        description: "Комната с игровой приставкой"
                    }
                ])
            });
        });

        await page.route(/\/venue\/resources/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify([
                    {
                        resourceId: "res-1",
                        name: "Столик 1",
                        capacity: 4,
                        resourceGroupId: "group-1"
                    },
                    {
                        resourceId: "res-2",
                        name: "Столик 2",
                        capacity: 6,
                        resourceGroupId: "group-1"
                    },
                    {
                        resourceId: "res-3",
                        name: "Игровая зона VIP",
                        capacity: 8,
                        resourceGroupId: "group-2"
                    }
                ])
            });
        });

        await page.route(/\/venue\/visits\/active/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify([
                    {
                        visitId: "visit-active-1",
                        userId: "some-user-id",
                        resourceId: "res-1",
                        entryTime: new Date(Date.now() - 3600000).toISOString(),
                        status: 3,
                        tariffName: "Стандарт",
                        tariffPricePerMinute: 2.5,
                        calculatedCost: 150
                    }
                ])
            });
        });

        await page.route(/\/venue\/promotions/, async (route) => {
            const url = route.request().url();
            if (url.includes("/promotions/page")) {
                await route.fulfill({
                    status: 200,
                    contentType: "application/json",
                    body: JSON.stringify({
                        items: [
                            {
                                promotionId: "promo-1",
                                name: "Счастливые часы",
                                description: "Скидка 20% в обеденное время",
                                discountPercent: 20,
                                isActive: true,
                                isStackable: false,
                                startDate: "2026-01-01T00:00:00Z",
                                endDate: "2026-12-31T23:59:59Z",
                                validFrom: "2026-01-01T00:00:00Z",
                                validTo: "2026-12-31T23:59:59Z",
                                type: 1
                            }
                        ],
                        metadata: {
                            page: 1,
                            pageSize: 10,
                            totalCount: 1,
                            totalPages: 1
                        }
                    })
                });
            } else {
                await route.fulfill({
                    status: 200,
                    contentType: "application/json",
                    body: JSON.stringify([
                        {
                            promotionId: "promo-1",
                            name: "Счастливые часы",
                            description: "Скидка 20% в обеденное время",
                            discountPercent: 20,
                            isActive: true,
                            isStackable: false,
                            startDate: "2026-01-01T00:00:00Z",
                            endDate: "2026-12-31T23:59:59Z",
                            validFrom: "2026-01-01T00:00:00Z",
                            validTo: "2026-12-31T23:59:59Z",
                            type: 1
                        }
                    ])
                });
            }
        });

        await page.route(/\/audit\/logs/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    items: [
                        {
                            id: "audit-1",
                            eventType: "Security",
                            action: "UserLogin",
                            userName: "admin@timecafe.local",
                            machineName: "Localhost-PC",
                            duration: 120,
                            createdAt: new Date().toISOString()
                        },
                        {
                            id: "audit-2",
                            eventType: "Venue",
                            action: "TableCreated",
                            userName: "admin@timecafe.local",
                            machineName: "Localhost-PC",
                            duration: 80,
                            createdAt: new Date().toISOString()
                        }
                    ],
                    metadata: {
                        page: 1,
                        pageSize: 10,
                        totalCount: 2,
                        totalPages: 1
                    }
                })
            });
        });

        await page.route(/\/auth\/admin\/users\/some-user-id/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    user: {
                        id: "some-user-id",
                        email: "ivan@timecafe.local",
                        name: "Иван",
                        role: "client",
                        status: "active",
                        emailConfirmed: true,
                        phoneNumberConfirmed: true,
                        phoneNumber: "+79998887766",
                        profile: {
                            firstName: "Иван",
                            lastName: "Иванов",
                            middleName: "Иванович",
                            profileStatus: 1
                        },
                        balance: {
                            currentBalance: 3200,
                            debt: 0
                        }
                    }
                })
            });
        });

        await page.route(/\/userprofile\/profiles\/some-user-id/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    userId: "some-user-id",
                    firstName: "Иван",
                    lastName: "Иванов",
                    middleName: "Иванович",
                    birthDate: "1995-05-15",
                    gender: 1,
                    email: "ivan@timecafe.local",
                    phoneNumber: "+79998887766",
                    profileStatus: 1
                })
            });
        });

        await page.route(/\/billing\/balance\/some-user-id/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    balance: {
                        userId: "some-user-id",
                        currentBalance: 3200,
                        totalDeposited: 3200,
                        totalSpent: 0,
                        debt: 0
                    }
                })
            });
        });

        await page.route(/\/billing\/transactions/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    items: [
                        {
                            transactionId: "tx-1",
                            userId: "some-user-id",
                            amount: 1500,
                            type: 1,
                            source: 2,
                            status: 2,
                            createdAt: new Date().toISOString(),
                            comment: "Пополнение через администратора",
                            balanceAfter: 1500
                        }
                    ],
                    metadata: {
                        page: 1,
                        pageSize: 10,
                        totalCount: 1,
                        totalPages: 1
                    }
                })
            });
        });

        await page.route(/\/venue\/tariffs\/active/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify([])
            });
        });

        await page.route(/\/venue\/tariffs/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify([
                    {
                        tariffId: "tariff-1",
                        name: "Ночной",
                        pricePerMinute: 3,
                        billingType: 1
                    }
                ])
            });
        });

        await page.route(/\/auth\/users\/some-user-id\/roles/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify(["client"])
            });
        });

        await page.route(/\/auth\/roles/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify(["admin", "client", "employee"])
            });
        });

        await page.route(/\/auth\/rbac\/permissions/, async (route) => {
            await route.fulfill({
                status: 200,
                contentType: "application/json",
                body: JSON.stringify({
                    permissions: ALL_MOCK_PERMISSIONS
                })
            });
        });
    });

    test("Tables and Resources Interactive Map Flow", async ({page}) => {
        await page.goto("/login");
        await page.evaluate(() => localStorage.setItem("isE2E", "true"));
        await page.getByLabel("Email").fill("admin@timecafe.local");
        await page.getByLabel("Пароль").fill("Admin@12345");
        await page.getByTestId("login-submit").click();
        await page.waitForURL(/\/home$/);

        await page.goto("/admin/resources");
        await page.waitForURL(/\/admin\/resources$/);

        await expect(page.getByText("Интерактивная карта столов").first()).toBeVisible({timeout: 10000});

        await expect(page.getByText("Основной зал").first()).toBeVisible();
        await expect(page.getByText("VIP комната").first()).toBeVisible();

        await expect(page.getByText("Столик 1").first()).toBeVisible();
        await expect(page.getByText("Занят").first()).toBeVisible();

        await expect(page.getByText("Столик 2").first()).toBeVisible();
        await expect(page.getByText("Свободен").first()).toBeVisible();

        const seatButton = page.getByRole("button", {name: "Посадить"}).first();
        await expect(seatButton).toBeVisible();
        await seatButton.click();

        await expect(page.getByText("Быстрая посадка гостя").first()).toBeVisible();
        await page.getByRole("button", {name: "Отмена"}).click();
    });

    test("Promotions Administration Flow", async ({page}) => {
        await page.goto("/login");
        await page.evaluate(() => localStorage.setItem("isE2E", "true"));
        await page.getByLabel("Email").fill("admin@timecafe.local");
        await page.getByLabel("Пароль").fill("Admin@12345");
        await page.getByTestId("login-submit").click();
        await page.waitForURL(/\/home$/);

        await page.goto("/admin/promotions");
        await page.waitForURL(/\/admin\/promotions$/);

        await expect(page.getByText("Акции").first()).toBeVisible({timeout: 10000});
        await expect(page.getByText("Счастливые часы").first()).toBeVisible();

        const addPromoButton = page.getByRole("button", {name: "Добавить акцию"});
        await expect(addPromoButton).toBeVisible();
        await addPromoButton.click();

        await expect(page.getByRole("heading", {name: "Новая акция"}).first()).toBeVisible();
        await page.getByRole("button", {name: "Отмена"}).click();
    });

    test("Audit Logs Security View Flow", async ({page}) => {
        await page.goto("/login");
        await page.evaluate(() => localStorage.setItem("isE2E", "true"));
        await page.getByLabel("Email").fill("admin@timecafe.local");
        await page.getByLabel("Пароль").fill("Admin@12345");
        await page.getByTestId("login-submit").click();
        await page.waitForURL(/\/home$/);

        await page.goto("/admin/audit-logs");
        await page.waitForURL(/\/admin\/audit-logs$/);

        await expect(page.getByText("Аудит-логи").first()).toBeVisible({timeout: 10000});
        await expect(page.getByText("UserLogin").first()).toBeVisible();
        await expect(page.getByText("TableCreated").first()).toBeVisible();
    });

    test("User Card and Transactions Flow", async ({page}) => {
        await page.goto("/login");
        await page.evaluate(() => localStorage.setItem("isE2E", "true"));
        await page.getByLabel("Email").fill("admin@timecafe.local");
        await page.getByLabel("Пароль").fill("Admin@12345");
        await page.getByTestId("login-submit").click();
        await page.waitForURL(/\/home$/);

        await page.goto("/admin/users/some-user-id");
        await page.waitForURL(/\/admin\/users\/some-user-id$/);

        await expect(page.getByText("Иван Иванов").first()).toBeVisible({timeout: 10000});
        await expect(page.getByText("3200").first()).toBeVisible();
        await expect(page.getByText("Пополнение через администратора").first()).toBeVisible();
    });
});
