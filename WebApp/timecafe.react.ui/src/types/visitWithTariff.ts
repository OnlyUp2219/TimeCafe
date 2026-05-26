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
        approvedByUserId: string | null;
        approvedAt: string | null;
        rejectionReason: string | null;

        tariffName: string;
        tariffPricePerMinute: number;
        tariffDescription: string;
        tariffBillingType: BillingType
}