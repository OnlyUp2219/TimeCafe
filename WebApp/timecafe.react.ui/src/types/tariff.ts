export const BillingType = {
    Hourly: 1,
    PerMinute: 2,
} as const;

export type BillingType = (typeof BillingType)[keyof typeof BillingType];

export type CostBreakdown = {
    actualMinutes: number;
    billableMinutes: number;
    baseCost: number;
    finalCost: number;
    optimizationGain: number;
};

export type Tariff = {
    tariffId: string;
    name: string;
    description: string;
    billingType: BillingType;
    pricePerMinute: number;
    isActive: boolean;
    themeId?: string | null;

    accent?: "brand" | "green" | "pink" | "purple";
    recommended?: boolean;

    themeName?: string;
    themeEmoji?: string | null;
    themeColors?: string | null;
    colors?: string | null;

    summary?: string | null;
    features?: string[] | null;
    audienceTags?: string[] | null;
    minSessionMinutes?: number | null;
    roundingRule?: string | null;
    maxGuests?: number | null;
    cancellationPolicy?: string | null;
    isRecommended?: boolean;
    sortOrder?: number;
    calculationExamples?: CostBreakdown[] | null;
};
