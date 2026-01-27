const normalizeBaseUrl = (url: string): string => {
    const trimmed = url.trim().replace(/\/+$/, "");

    // Локально YARP обычно слушает HTTP на :8000. Если кто-то задал https://localhost:8000
    // (без TLS), браузер/axios упадут с ERR_SSL_PROTOCOL_ERROR.
    const yarpHttpsWithoutTls = /^https:\/\/(localhost|127\.0\.0\.1):8000$/i;
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
