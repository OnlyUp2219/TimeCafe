export type Visit = {
    visitId: string;
    userId: string | null;
    tariffId: string;
    resourceId: string | null;

    entryTime: string;
    exitTime: string | null;

    calculatedCost: number | null;

    status: VisitStatus;
    plannedMinutes: number | null;
    guestsCount: number;
    isFinishRequested: boolean;
}

export const VisitStatus = {
    Pending: 0,
    Approved: 1,
    Rejected: 2,
    Active: 3,
    Completed: 4,
    Cancelled: 5,
    WaitingForPayment: 6,
} as const;

export type VisitStatus = (typeof VisitStatus)[keyof typeof VisitStatus];
