export type JwtPayload = Record<string, unknown>;

const base64UrlDecode = (value: string): string => {
    const normalized = value.replace(/-/g, "+").replace(/_/g, "/");
    const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "=");
    const decoded = atob(padded);
    try {
        return decodeURIComponent(
            decoded
                .split("")
                .map(c => `%${("00" + c.charCodeAt(0).toString(16)).slice(-2)}`)
                .join("")
        );
    } catch {
        return decoded;
    }
};

export const decodeJwtPayload = (token: string): JwtPayload | null => {
    if (!token) return null;
    const parts = token.split(".");
    if (parts.length < 2) return null;

    try {
        const json = base64UrlDecode(parts[1]);
        const payload = JSON.parse(json);
        if (!payload || typeof payload !== "object") return null;
        return payload as JwtPayload;
    } catch {
        return null;
    }
};

const getFirstString = (value: unknown): string | null => {
    if (typeof value === "string") return value;
    if (Array.isArray(value)) {
        const first = value.find(v => typeof v === "string");
        return typeof first === "string" ? first : null;
    }
    return null;
};

export const getJwtUserId = (token: string): string | null => {
    const payload = decodeJwtPayload(token);
    if (!payload) return null;
    return getFirstString(payload.sub);
};

export const getJwtEmail = (token: string): string | null => {
    const payload = decodeJwtPayload(token);
    if (!payload) return null;
    const direct = getFirstString(payload.email);
    if (direct) return direct;
    return getFirstString(payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"]);
};

export const getJwtRole = (token: string): string | null => {
    const payload = decodeJwtPayload(token);
    if (!payload) return null;

    const direct = getFirstString(payload.role);
    if (direct) return direct;

    return getFirstString(payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]);
};

export const getJwtExp = (token: string): number | null => {
    const payload = decodeJwtPayload(token);
    if (!payload) return null;
    const exp = payload.exp;
    if (typeof exp === "number") return exp;
    const str = getFirstString(exp);
    if (!str) return null;
    const num = Number(str);
    return Number.isFinite(num) ? num : null;
};

export const isJwtExpired = (token: string, skewSeconds = 30): boolean => {
    const exp = getJwtExp(token);
    if (!exp) return false;
    const now = Math.floor(Date.now() / 1000);
    return now + skewSeconds >= exp;
};

export interface JwtInfo {
    userId: string | null;
    email: string | null;
    role: string | null;
    exp: number | null;
}

export const getJwtInfo = (token: string): JwtInfo => {
    return {
        userId: getJwtUserId(token),
        email: getJwtEmail(token),
        role: getJwtRole(token),
        exp: getJwtExp(token),
    };
};
