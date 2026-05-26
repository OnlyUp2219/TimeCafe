export type Visit = {
    visitId: string;
    userId: string;
    tariffId: string;

    entryTime: string;
    exitTime: string | null;

    calculatedCost: number | null;

    status: VisitStatus;
}

export const VisitStatus = {
    Pending: 0,
    Approved: 1,
    Rejected: 2,
    Active: 3,
    Completed: 4,
    Cancelled: 5,
} as const;

export type VisitStatus = (typeof VisitStatus)[keyof typeof VisitStatus];