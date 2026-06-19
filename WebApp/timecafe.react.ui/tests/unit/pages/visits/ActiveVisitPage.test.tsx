import { describe, it, expect, vi } from "vitest";
import { screen } from "@testing-library/react";
import { ActiveVisitPage } from "@pages/visits/ActiveVisitPage";
import { renderWithProviders } from "../../utils/test-utils";
import { VisitStatus } from "@app-types/visit";

global.ResizeObserver = class {
    observe = vi.fn();
    unobserve = vi.fn();
    disconnect = vi.fn();
};

// Mock hooks
vi.mock("@store/api/venueApi", () => ({
    useGetActiveVisitByUserQuery: () => ({
        data: {
            visitId: "visit-1",
            userId: "user-1",
            status: VisitStatus.Active,
            isFinishRequested: true, // This should trigger Grace Period
            entryTime: new Date().toISOString(),
            tariffId: "tariff-1",
            tariffName: "Base",
            tariffBillingType: 0,
            tariffPricePerMinute: 3,
            tariffMinSessionMinutes: 60,
            guestsCount: 1
        },
        isLoading: false,
        refetch: vi.fn()
    }),
    useRequestEndVisitMutation: vi.fn(() => [vi.fn(), { isLoading: false }]),
    useCancelVisitMutation: vi.fn(() => [vi.fn(), { isLoading: false }]),
    useGetAllPromotionsQuery: vi.fn(() => ({ data: [] })),
    useGetUserLoyaltyQuery: vi.fn(() => ({ data: null }))
}));

vi.mock("@store/api/profileApi", () => ({
    useGetProfileByUserIdQuery: vi.fn(() => ({ data: null }))
}));

vi.mock("@store/api/billingApi", () => {
    return {
        useGetBalanceQuery: vi.fn(() => ({ data: { currentBalance: 500 } })),
        useGetInvoiceByVisitIdQuery: vi.fn(() => ({ data: null, refetch: vi.fn(), isLoading: false })),
        usePayInvoiceMutation: vi.fn(() => [vi.fn(), { isLoading: false }]),
        useInitializeStripeInvoicePaymentMutation: vi.fn(() => [vi.fn(), { isLoading: false }]),
        useInitializeStripeCheckoutMutation: vi.fn(() => [vi.fn(), { isLoading: false }])
    };
});

vi.mock("@hooks/useComponentSize", () => ({
    useComponentSize: () => ({ sizes: { button: "medium", card: "medium" } })
}));

vi.mock("@shared/ui/progressToast", () => ({
    useProgressToast: () => ({ showToast: vi.fn(), ToasterElement: null })
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");
    return {
        ...actual as any,
        useNavigate: () => vi.fn()
    };
});

describe("ActiveVisitPage", () => {
    it("renders Grace Period state when isFinishRequested is true", async () => {
        renderWithProviders(<ActiveVisitPage />);

        // Verify that the Grace Period block is displayed
        const waitingText = await screen.findByText(/Ожидаем оплату/i);
        expect(waitingText).toBeDefined();

        const stripeBtn = screen.getByRole("button", { name: /Пополнить баланс картой/i });
        expect(stripeBtn).toBeDefined();

        const balanceBtn = screen.getByRole("button", { name: /Оплатить с баланса/i });
        expect(balanceBtn).toBeDefined();
    });
});
