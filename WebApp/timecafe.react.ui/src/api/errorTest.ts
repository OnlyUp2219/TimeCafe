import axios from "axios";

const apiBase = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7057";

export const errorTestApi = {
    testValidation: () => axios.get(`${apiBase}/error-test/validation`),
    testNotFound: () => axios.get(`${apiBase}/error-test/not-found`),
    testUnauthorized: () => axios.get(`${apiBase}/error-test/unauthorized`),
    testForbidden: () => axios.get(`${apiBase}/error-test/forbidden`),
    testConflict: () => axios.get(`${apiBase}/error-test/conflict`),
    testRateLimit: () => axios.get(`${apiBase}/error-test/rate-limit`),
    testBusinessLogic: () => axios.get(`${apiBase}/error-test/business-logic`),
    testCritical: () => axios.get(`${apiBase}/error-test/critical`),
    testMultipleValidation: () => axios.get(`${apiBase}/error-test/multiple-validation`),
    testSuccess: () => axios.get(`${apiBase}/error-test/success`)
};
