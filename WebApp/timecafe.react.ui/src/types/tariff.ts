export const BillingType = {
    Hourly: 1,
    PerMinute: 2,
} as const;

export type BillingType = (typeof BillingType)[keyof typeof BillingType];

export type Tariff = {
    tariffId: string;
    name: string;
    description: string;
    billingType: BillingType;
    pricePerMinute: number;
    isActive: boolean;

    accent?: "brand" | "green" | "pink" | "purple";
    recommended?: boolean;

    themeName?: string;
    themeEmoji?: string | null;
};
