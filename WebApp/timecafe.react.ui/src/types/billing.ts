export type BillingTransactionStatus = "Pending" | "Completed" | "Failed" | "Cancelled";

export type BillingTransaction = {
    transactionId: string;
    createdAtMs: number;
    title: string;
    subtitle: string;
    amountRub: number;
    status: BillingTransactionStatus;
};

export type BillingFilters = {
    period: "week" | "month" | "all";
    onlyCredits?: boolean;
    onlyDebits?: boolean;
};

export type BillingPagination = {
    page: number;
    pageSize: number;
};
