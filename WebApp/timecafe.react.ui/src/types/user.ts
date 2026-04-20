export type User = {
    id: string;
    email: string;
    name?: string;
    role: string;
    status: string;
    emailConfirmed?: boolean;
    phoneNumberConfirmed?: boolean;
};