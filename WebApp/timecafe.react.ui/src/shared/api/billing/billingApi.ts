import {httpClient} from "@api/httpClient";
import {normalizeUnknownError} from "@api/errors/normalize";
import {
    type BillingBalance,
    type BillingPagination,
    type BillingTransaction,
} from "@app-types/billing";

export interface InitializeStripeCheckoutRequest {
    userId: string;
    amount: number;
    successUrl?: string;
    cancelUrl?: string;
    description?: string;
}

export interface InitializeStripeCheckoutResponse {
    paymentId: string;
    sessionId: string;
    checkoutUrl: string;
}

type GetBalanceResponse = {
    balance: BillingBalance;
};

type GetDebtResponse = {
    debt: number;
};

type GetTransactionHistoryResponse = {
    transactions: BillingTransaction[];
    pagination: BillingPagination;
};

export class BillingApi {
    static async getBalance(userId: string): Promise<BillingBalance> {
        try {
            const res = await httpClient.get<GetBalanceResponse>(`/billing/balance/${userId}`);
            return res.data.balance;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async getDebt(userId: string): Promise<number> {
        try {
            const res = await httpClient.get<GetDebtResponse>(`/billing/debt/${userId}`);
            return res.data.debt;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async getTransactionHistory(
        userId: string,
        page: number,
        pageSize: number,
    ): Promise<GetTransactionHistoryResponse> {
        try {
            const res = await httpClient.get<GetTransactionHistoryResponse>(`/billing/transactions/history/${userId}`, {
                params: {page, pageSize},
            });
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async initializeStripeCheckout(
        request: InitializeStripeCheckoutRequest,
    ): Promise<InitializeStripeCheckoutResponse> {
        try {
            const res = await httpClient.post<InitializeStripeCheckoutResponse>(
                "/billing/payments/initialize-checkout",
                request,
            );
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }
}
