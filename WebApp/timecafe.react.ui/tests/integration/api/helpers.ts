import axios from "axios";
import {randomUUID} from "node:crypto";
import {mkdirSync, existsSync, appendFileSync} from "node:fs";
import {resolve} from "node:path";
import {getApiBaseUrl, getMockEmailEnabled, getMockSmsEnabled, getOptionalEnvValue} from "@tests/integration/api/testEnv";

export const apiBaseUrl = getApiBaseUrl();

export const createTestEmail = () => {
    const stamp = Date.now();
    return `test+${stamp}-${randomUUID()}@timecafe.local`;
};

export const createPassword = () => {
    return "Test1234!";
};

export const getRegisterEndpoint = () => {
    return getMockEmailEnabled() ? "/auth/registerWithUsername-mock" : "/auth/registerWithUsername";
};

export const getForgotPasswordEndpoint = () => {
    return getMockEmailEnabled() ? "/auth/forgot-password-link-mock" : "/auth/forgot-password-link";
};

export const getEmailChangeEndpoint = () => {
    return getMockEmailEnabled() ? "/auth/email/change-mock" : "/auth/email/change";
};

export const getResendEmailEndpoint = () => {
    return getMockEmailEnabled() ? "/auth/email/resend-mock" : "/auth/email/resend";
};

export const getPhoneGenerateEndpoint = () => {
    return getMockSmsEnabled() ? "/auth/twilio/generateSMS-mock" : "/auth/twilio/generateSMS";
};

export const getPhoneVerifyEndpoint = () => {
    return getMockSmsEnabled() ? "/auth/twilio/verifySMS-mock" : "/auth/twilio/verifySMS";
};

export const getAxiosClient = () => {
    return axios.create({
        baseURL: apiBaseUrl,
        withCredentials: true,
        validateStatus: () => true,
    });
};

export const createRateLimitKey = () => {
    return `test-${randomUUID()}`;
};

export const getAxiosClientWithRateLimitKey = (rateLimitKey: string) => {
    return axios.create({
        baseURL: apiBaseUrl,
        withCredentials: true,
        validateStatus: () => true,
        headers: {
            "X-Test-RateLimit-Key": rateLimitKey,
        },
    });
};

export const createTestClient = () => {
    const rateLimitKey = createRateLimitKey();
    const client = getAxiosClientWithRateLimitKey(rateLimitKey);
    return {client, rateLimitKey};
};

export const registerAndConfirm = async (client: ReturnType<typeof getAxiosClientWithRateLimitKey>, email: string, password: string) => {
    const registerRes = await registerUser(client, email, password);
    if (registerRes.status < 200 || registerRes.status >= 300) {
        return {registerRes, userId: "", token: "", callbackUrl: ""};
    }
    const callbackUrl = registerRes.data?.callbackUrl as string | undefined;
    if (!callbackUrl) return {registerRes, userId: "", token: "", callbackUrl: ""};
    const parsed = parseCallbackParams(callbackUrl);
    if (parsed.userId && parsed.token) {
        recordTestUser(parsed.userId);
        await client.post("/auth/email/confirm", {userId: parsed.userId, token: parsed.token});
    }
    return {registerRes, userId: parsed.userId, token: parsed.token, callbackUrl};
};

export const loginAndGetAccessToken = async (client: ReturnType<typeof getAxiosClientWithRateLimitKey>, email: string, password: string) => {
    const loginRes = await client.post("/auth/login-jwt-v2", {email, password});
    const token = loginRes.data?.accessToken as string | undefined;
    return {loginRes, token: token ?? ""};
};

export const loginAndGetRefreshToken = async (client: ReturnType<typeof getAxiosClientWithRateLimitKey>, email: string, password: string) => {
    const loginRes = await client.post("/auth/login-jwt-v2", {email, password});
    const refreshToken = extractRefreshTokenFromSetCookie(loginRes.headers?.["set-cookie"]);
    return {loginRes, refreshToken};
};

export const withAuthHeader = (_client: ReturnType<typeof getAxiosClientWithRateLimitKey>, token: string) => {
    const headers = token ? {Authorization: `Bearer ${token}`} : undefined;
    return {headers};
};

export const wait = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

export const registerUser = async (client: ReturnType<typeof getAxiosClient>, email: string, password: string) => {
    const endpoint = getRegisterEndpoint();
    const payload = {username: email, email, password};

    let lastStatus = 0;
    for (let attempt = 0; attempt < 10; attempt += 1) {
        const response = await client.post(endpoint, payload);
        lastStatus = response.status;
        if (response.status !== 429) return response;

        const retryAfter = Number(response.headers?.["retry-after"] ?? 0);
        const delayMs = Number.isFinite(retryAfter) && retryAfter > 0
            ? retryAfter * 1000
            : 3000 + attempt * 1000;
        await wait(delayMs);
    }

    throw new Error(`Регистрация ограничена по rate limit (status ${lastStatus})`);
};

export const parseCallbackParams = (url: string) => {
    const parsed = new URL(url, apiBaseUrl);
    return {
        userId: parsed.searchParams.get("userId") || "",
        token: (parsed.searchParams.get("token") || "").replace(/ /g, "+"),
        newEmail: (parsed.searchParams.get("newEmail") || "").replace(/ /g, "+"),
        email: (parsed.searchParams.get("email") || "").replace(/ /g, "+"),
        code: parsed.searchParams.get("code") || "",
    };
};

export const extractRefreshTokenFromSetCookie = (setCookie: string[] | string | undefined) => {
    if (!setCookie) return "";
    const list = Array.isArray(setCookie) ? setCookie : [setCookie];
    const cookie = list.find(value => value.startsWith("refresh_token="));
    if (!cookie) return "";
    const pair = cookie.split(";")[0] ?? "";
    const token = pair.split("=")[1] ?? "";
    return token;
};

export const buildRefreshCookieHeader = (refreshToken: string) => {
    if (!refreshToken) return "";
    return `refresh_token=${refreshToken}`;
};

export const canUseS3 = () => {
    return Boolean(getOptionalEnvValue("S3_SERVICE_URL"));
};

const artifactsDir = resolve(process.cwd(), "tests", ".artifacts");
const usersFile = resolve(artifactsDir, "test-users.txt");

export const recordTestUser = (userId: string) => {
    if (!userId) return;
    if (!existsSync(artifactsDir)) {
        mkdirSync(artifactsDir, {recursive: true});
    }
    appendFileSync(usersFile, `${userId}\n`, "utf8");
};
