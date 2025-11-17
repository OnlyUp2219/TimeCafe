/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_API_BASE_URL?: string;
    readonly VITE_USE_MOCK_SMS?: string;
    readonly VITE_USE_MOCK_EMAIL?: string;
    readonly VITE_RECAPTCHA_SITE_KEY?: string;
    readonly VITE_FAKE_SIGN_AUTO_FILL?: string;
}

interface ImportMeta {
    readonly env: ImportMetaEnv;
}
