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
    Active: 1,
    Completed: 2,
} as const;

export type VisitStatus = (typeof VisitStatus)[keyof typeof VisitStatus];