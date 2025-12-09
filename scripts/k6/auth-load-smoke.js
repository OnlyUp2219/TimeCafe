import http from 'k6/http';
import { check, sleep } from 'k6';

// Smoke load: ~300 rps for 60s (approx 18_000 requests)
export const options = {
  scenarios: {
    smoke: {
      executor: 'constant-arrival-rate',
      rate: 300,
      timeUnit: '1s',
      duration: '30s',
      preAllocatedVUs: 100,
      maxVUs: 300,
      exec: 'default',
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<1000'],
    http_req_failed: ['rate<0.05'],
  },
  insecureSkipTLSVerify: true,
};

const BASE_URL = __ENV.BASE_URL || 'https://127.0.0.1:7058';
const USER_EMAIL = __ENV.USER_EMAIL || 'load.user@example.com';
const USER_PASSWORD = __ENV.USER_PASSWORD || 'P@ssw0rd!';
const ME_PATH = __ENV.ME_PATH || '/protected-test';

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
  const refreshCookie = (res.cookies && res.cookies['refresh_token'] && res.cookies['refresh_token'][0]) ? res.cookies['refresh_token'][0].value : null;

  return { accessToken: json?.accessToken, refreshCookie };
}

function me(accessToken) {
  const url = `${BASE_URL.replace(/\/$/, '')}${ME_PATH.startsWith('/') ? ME_PATH : '/' + ME_PATH}`;
  const res = http.get(url, {
    headers: { Authorization: `Bearer ${accessToken}` },
    timeout: '30s',
  });

  if (res.status !== 200) {
    console.error(`ME failed: url=${url} status=${res.status} body=${res.body}`);
  }

  check(res, { 'me status 200': (r) => r.status === 200, });
}

function refresh(refreshCookie) {
  const res = http.post(`${BASE_URL}/refresh-jwt-v2`, null, {
    headers: { 'Content-Type': 'application/json', 'Cookie': `refresh_token=${refreshCookie}` },
    timeout: '30s',
  });

  check(res, { 'refresh status 200': (r) => r.status === 200, });

  if (res.status !== 200 || !res.body) {
    return { accessToken: null, refreshCookie };
  }

  let json;
  try { json = res.json(); } catch { json = {}; }
  const newRefreshCookie = (res.cookies && res.cookies['refresh_token'] && res.cookies['refresh_token'][0]) ? res.cookies['refresh_token'][0].value : refreshCookie;

  return { accessToken: json?.accessToken, refreshCookie: newRefreshCookie };
}

export default function () {
  const { accessToken, refreshCookie } = login();
  if (accessToken) me(accessToken);
  if (refreshCookie) {
    const tokens = refresh(refreshCookie);
    if (tokens?.accessToken) me(tokens.accessToken);
  }
  sleep(1);
}
