export type BillingType = "Hourly" | "PerMinute";

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
