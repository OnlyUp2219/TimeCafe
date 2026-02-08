export type PhoneVerificationSessionV1 = {
    open: boolean;
    step: "input" | "verify";
    phoneNumber: string;
    mockToken?: string | null;
};

export const PHONE_VERIFICATION_SESSION_KEY = "tc_phone_verification_session_v1";

export const isPhoneVerificationSessionV1 = (value: unknown): value is PhoneVerificationSessionV1 => {
    if (!value || typeof value !== "object") return false;

    const v = value as Record<string, unknown>;

    const step = v.step;
    if (step !== "input" && step !== "verify") return false;

    const phoneNumber = v.phoneNumber;
    if (typeof phoneNumber !== "string") return false;

    const open = v.open;
    if (typeof open !== "boolean") return false;

    const mockToken = v.mockToken;
    if (!(mockToken == null || typeof mockToken === "string")) return false;

    return true;
};
