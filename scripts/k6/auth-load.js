import http from 'k6/http';
import { check, sleep } from 'k6';

// Конфигурация нагрузки: регулируйте под свои цели
export const options = {
  // heavy load config: ~1_000_000 requests in 60s
  scenarios: {
    heavy: {
      executor: 'constant-arrival-rate',
      rate: 1000, // iterations per second (~100_000 total over duration)
      timeUnit: '1s',
      duration: '40',
      preAllocatedVUs: 200,
      maxVUs: 1000,
      exec: 'default',
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed: ['rate<0.02'],
  },
  insecureSkipTLSVerify: true,
};

// Базовый URL API; можно пробросить через переменные окружения
const BASE_URL = __ENV.BASE_URL || 'https://localhost:5001';

// Тестовые креды (заведите пользователя через сидинг/миграцию)
const USER_EMAIL = __ENV.USER_EMAIL || 'load.user@example.com';
const USER_PASSWORD = __ENV.USER_PASSWORD || 'P@ssw0rd!';

// Хелпер: логин и получение токенов (v2: refresh_cookie)
function login() {
  const res = http.post(`${BASE_URL}/login-jwt-v2`, JSON.stringify({
    email: USER_EMAIL,
    password: USER_PASSWORD,
  }), {
    headers: { 'Content-Type': 'application/json' },
    timeout: '30s',
  });

  check(res, {
    'login status 200': (r) => r.status === 200,
  });

  if (res.status !== 200 || !res.body) {
    return { accessToken: null, refreshCookie: null };
  }

  let json;
  try { json = res.json(); } catch { json = {}; }

  // refresh token is set in httpOnly cookie 'refresh_token' by server
  const refreshCookie = (res.cookies && res.cookies['refresh_token'] && res.cookies['refresh_token'][0]) ? res.cookies['refresh_token'][0].value : null;

  return {
    accessToken: json?.accessToken,
    refreshCookie,
  };
}

// Путь защищённого ресурса (по умолчанию /protected-test)
const ME_PATH = __ENV.ME_PATH || '/protected-test';

// Хелпер: вызов защищенного эндпоинта
function me(accessToken) {
  const url = `${BASE_URL.replace(/\/$/, '')}${ME_PATH.startsWith('/') ? ME_PATH : '/' + ME_PATH}`;
  const res = http.get(url, {
    headers: { Authorization: `Bearer ${accessToken}` },
    timeout: '30s',
  });

  if (res.status !== 200) {
    console.error(`ME failed: url=${url} status=${res.status} body=${res.body}`);
  }

  check(res, {
    'me status 200': (r) => r.status === 200,
  });
}

// Хелпер: рефреш токена
// Хелпер: рефреш токена (v2) — сервер читает refresh из httpOnly cookie
function refresh(refreshCookie) {
  // send cookie explicitly to be safe
  const res = http.post(`${BASE_URL}/refresh-jwt-v2`, null, {
    headers: { 'Content-Type': 'application/json', 'Cookie': `refresh_token=${refreshCookie}` },
    timeout: '30s',
  });

  check(res, {
    'refresh status 200': (r) => r.status === 200,
  });

  if (res.status !== 200 || !res.body) {
    return { accessToken: null, refreshCookie };
  }

  let json;
  try { json = res.json(); } catch { json = {}; }

  // server rotates cookie; capture new cookie if present
  const newRefreshCookie = (res.cookies && res.cookies['refresh_token'] && res.cookies['refresh_token'][0]) ? res.cookies['refresh_token'][0].value : refreshCookie;

  return {
    accessToken: json?.accessToken,
    refreshCookie: newRefreshCookie,
  };
}

export default function () {
  // 1) Логин
  const { accessToken, refreshCookie } = login();

  // 2) Защищенный вызов
  if (accessToken) {
    me(accessToken);
  }

  // 3) Рефреш
  if (refreshCookie) {
    const tokens = refresh(refreshCookie);
    if (tokens?.accessToken) {
      me(tokens.accessToken);
    }
  }

  // Имитация пользовательского "think time"
  sleep(1);
}
