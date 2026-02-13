import type {BillingType} from "@app-types/tariff.ts";

export interface TariffWithTheme {
    tariffId: string;
    name: string;
    description?: string | null;
    pricePerMinute: number;
    billingType: BillingType;
    isActive: boolean;
    createdAt: string;
    lastModified: string;

    themeId?: string | null;
    themeName: string;
    themeEmoji?: string | null;
    themeColors?: string | null;
}