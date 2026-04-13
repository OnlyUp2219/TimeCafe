export const Roles = {
    Admin: "Admin",
    Client: "Client",
} as const;

export type Role = (typeof Roles)[keyof typeof Roles];
