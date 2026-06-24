import type {BillingType} from "@app-types/tariff";
import type {VisitStatus} from "@app-types/visit";

export type VisitWithTariff =
    {
        visitId: string;
        userId: string | null;
        tariffId: string;
        resourceId: string | null;
        resourceMaxGuests: number | null;
        entryTime: string;
        exitTime: string | null;
        calculatedCost: number | null;
        status: VisitStatus;
        approvedByUserId: string | null;
        approvedAt: string | null;
        rejectionReason: string | null;
        isFinishRequested: boolean;
        finishRequestedAt: string | null;

        tariffName: string;
        tariffPricePerMinute: number;
        tariffDescription: string;
        tariffBillingType: BillingType;
        plannedMinutes: number | null;
        guestsCount: number;
        tariffMinSessionMinutes: number | null;
        tariffRoundingRule: string | null;
}
