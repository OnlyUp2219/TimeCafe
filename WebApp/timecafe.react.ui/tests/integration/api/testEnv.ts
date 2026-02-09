import {readFileSync, existsSync} from "node:fs";
import {resolve} from "node:path";

const parseEnv = (content: string) => {
    const result: Record<string, string> = {};
    const lines = content.split(/\r?\n/);
    for (const line of lines) {
        const trimmed = line.trim();
        if (!trimmed || trimmed.startsWith("#")) continue;
        const eq = trimmed.indexOf("=");
        if (eq <= 0) continue;
        const key = trimmed.slice(0, eq).trim();
        const value = trimmed.slice(eq + 1).trim();
        if (!key) continue;
        result[key] = value;
    }
    return result;
};

const loadDotEnv = () => {
    const envPath = resolve(process.cwd(), ".env");
    if (!existsSync(envPath)) return {};
    const content = readFileSync(envPath, "utf8");
    return parseEnv(content);
};

const fileEnv = loadDotEnv();

export const getEnvValue = (key: string, fallback?: string) => {
    return process.env[key] ?? fileEnv[key] ?? fallback;
};

export const getApiBaseUrl = () => {
    return getEnvValue("VITE_API_BASE_URL", "http://localhost:8010");
};

export const getMockEmailEnabled = () => {
    return String(getEnvValue("VITE_USE_MOCK_EMAIL", "true")).toLowerCase() === "true";
};

export const getMockSmsEnabled = () => {
    return String(getEnvValue("VITE_USE_MOCK_SMS", "true")).toLowerCase() === "true";
};

export const getOptionalEnvValue = (key: string) => {
    const value = process.env[key] ?? fileEnv[key];
    return value && value.trim() ? value.trim() : undefined;
};
