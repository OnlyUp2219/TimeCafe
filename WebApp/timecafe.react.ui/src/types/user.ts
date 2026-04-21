export type User = {
    id: string;
    email: string;
    name?: string;
    role: string;
    status: string;
    emailConfirmed?: boolean;
    phoneNumberConfirmed?: boolean;
    phoneNumber?: string;
    profile?: {
        firstName: string;
        lastName: string;
        middleName?: string;
        photoUrl?: string;
        profileStatus: number;
    } | null;
    balance?: {
        currentBalance: number;
        debt: number;
    } | null;
};