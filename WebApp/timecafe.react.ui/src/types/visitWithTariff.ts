import type {BillingType} from "@app-types/tariff";
import type {VisitStatus} from "@app-types/visit";

export type VisitWithTariff =
    {
        visitId: string;
        userId: string;
        tariffId: string;
        entryTime: string;
        exitTime: string | null;
        calculatedCost: number | null;
        status: VisitStatus;

        tariffName: string;
        tariffPricePerMinute: number;
        tariffDescription: string;
        tariffBillingType: BillingType
}