import type {BillingType} from "@app-types/tariff";

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

    summary?: string | null;
    features?: string[] | null;
    audienceTags?: string[] | null;
    minSessionMinutes?: number | null;
    roundingRule?: string | null;
    maxGuests?: number | null;
    cancellationPolicy?: string | null;
    isRecommended?: boolean;
    sortOrder?: number;
}
