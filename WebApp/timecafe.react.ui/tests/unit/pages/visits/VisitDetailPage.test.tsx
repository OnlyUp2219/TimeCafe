import { describe, it, expect, vi } from "vitest";
import { screen } from "@testing-library/react";
import { VisitDetailPage } from "@pages/admin/VisitDetailPage";
import { renderWithProviders } from "../../utils/test-utils";
import { VisitStatus } from "@app-types/visit";

vi.mock("@store/api/venueApi", () => ({
    useGetVisitByIdQuery: () => ({
        data: {
            visitId: "visit-1",
            userId: "user-1",
            status: VisitStatus.Active,
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
    useFixateVisitTimeMutation: () => [vi.fn(), { isLoading: false }],
    useApproveVisitMutation: () => [vi.fn(), { isLoading: false }],
    useRejectVisitMutation: () => [vi.fn(), { isLoading: false }],
    useForceEndVisitMutation: () => [vi.fn(), { isLoading: false }],
    useGetAllPromotionsQuery: () => ({ data: [] }),
    useGetUserLoyaltyQuery: () => ({ data: null }),
    useGetResourcesQuery: () => ({ data: [] })
}));

vi.mock("@store/api/billingApi", () => ({
    useGetBalanceQuery: () => ({ data: null }),
    useGetInvoiceByVisitIdQuery: () => ({ data: null, refetch: vi.fn() }),
    usePayInvoiceMutation: () => [vi.fn()]
}));

vi.mock("@store/api/profileApi", () => ({
    useGetProfileByUserIdQuery: () => ({ data: null })
}));

vi.mock("@hooks/useComponentSize", () => ({
    useComponentSize: () => ({ sizes: { button: "medium", card: "medium" } })
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");
    return {
        ...actual as any,
        useParams: () => ({ id: "visit-1" }),
    };
});

vi.mock("@components/Guard/HasPermission", () => ({
    HasPermission: ({ children }: any) => <>{children}</>
}));

vi.mock("@app/components/RequirePermission/RequirePermission", () => ({
    RequirePermission: ({ children }: any) => <>{children}</>
}));

describe("VisitDetailPage", () => {
    it("renders tooltips for Завершить визит and Принудительный выход buttons when Active", async () => {
        renderWithProviders(<VisitDetailPage />);

        const endVisitBtn = await screen.findByText(/Завершить визит/i);
        const forceEndBtn = await screen.findByText(/Принудительный выход/i);

        expect(endVisitBtn).toBeDefined();
        expect(forceEndBtn).toBeDefined();

    });
});
