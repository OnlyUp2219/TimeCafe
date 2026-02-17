export const TransactionType = {
    Deposit: 1,
    Withdrawal: 2,
    Adjustment: 3,
} as const;

export type TransactionType = (typeof TransactionType)[keyof typeof TransactionType];

export const TransactionSource = {
    Visit: 1,
    Manual: 2,
    Payment: 3,
    Refund: 4,
} as const;

export type TransactionSource = (typeof TransactionSource)[keyof typeof TransactionSource];

export const TransactionStatus = {
    Pending: 1,
    Completed: 2,
    Failed: 3,
    PartialCompleted: 4,
} as const;

export type TransactionStatus = (typeof TransactionStatus)[keyof typeof TransactionStatus];

export type BillingBalance = {
    userId: string;
    currentBalance: number;
    totalDeposited: number;
    totalSpent: number;
    debt: number;
};

export type BillingTransaction = {
    transactionId: string;
    userId: string;
    amount: number;
    type: TransactionType;
    source: TransactionSource;
    status: TransactionStatus;
    comment?: string | null;
    createdAt: string;
    balanceAfter: number;
};

export type BillingActivityPoint = {
    date: Date;
    depositsRub: number;
    withdrawalsRub: number;
};

export type BillingFilters = {
    period: "week" | "month" | "all";
    onlyCredits?: boolean;
    onlyDebits?: boolean;
};

export type BillingPagination = {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
};
