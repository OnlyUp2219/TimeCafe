const normalizeBaseUrl = (url: string): string => {
    const trimmed = url.trim().replace(/\/+$/, "")
    const yarpHttpsWithoutTls = /^https:\/\/(localhost|127\.0\.0\.1):8010$/i;
    if (yarpHttpsWithoutTls.test(trimmed)) {
        return trimmed.replace(/^https:/i, "http:");
    }

    return trimmed;
};

export const getApiBaseUrl = (): string => {
    const envBaseUrl = import.meta.env.VITE_API_BASE_URL as string | undefined;
    const fallback = "https://localhost:7268";
    return normalizeBaseUrl(envBaseUrl ?? fallback);
};
