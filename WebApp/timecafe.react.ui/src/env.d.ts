/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL?: string;
  readonly VITE_USE_MOCK_SMS?: string;
  readonly VITE_USE_MOCK_EMAIL?: string;
  readonly VITE_SMS_RATE_LIMIT_SECONDS?: string;
  readonly VITE_EMAIL_RATE_LIMIT_SECONDS?: string;
  readonly VITE_RECAPTCHA_SITE_KEY?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
