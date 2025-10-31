import axios from "axios";

const apiBase = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7057";

export interface ErrorTestResult {
    success: boolean;
    data?: unknown;
    error?: unknown;
}

export const errorTestApi = {
    testValidation: async (): Promise<ErrorTestResult> => {
        try {
            const res = await axios.get(`${apiBase}/error-test/validation`);
            return { success: true, data: res.data };
        } catch (error) {
            return { success: false, error };
        }
    },

    testNotFound: async (): Promise<ErrorTestResult> => {
        try {
            const res = await axios.get(`${apiBase}/error-test/not-found`);
            return { success: true, data: res.data };
        } catch (error) {
            return { success: false, error };
        }
    },

    testUnauthorized: async (): Promise<ErrorTestResult> => {
        try {
            const res = await axios.get(`${apiBase}/error-test/unauthorized`);
            return { success: true, data: res.data };
        } catch (error) {
            return { success: false, error };
        }
    },

    testForbidden: async (): Promise<ErrorTestResult> => {
        try {
            const res = await axios.get(`${apiBase}/error-test/forbidden`);
            return { success: true, data: res.data };
        } catch (error) {
            return { success: false, error };
        }
    },

    testConflict: async (): Promise<ErrorTestResult> => {
        try {
            const res = await axios.get(`${apiBase}/error-test/conflict`);
            return { success: true, data: res.data };
        } catch (error) {
            return { success: false, error };
        }
    },

    testRateLimit: async (): Promise<ErrorTestResult> => {
        try {
            const res = await axios.get(`${apiBase}/error-test/rate-limit`);
            return { success: true, data: res.data };
        } catch (error) {
            return { success: false, error };
        }
    },

    testBusinessLogic: async (): Promise<ErrorTestResult> => {
        try {
            const res = await axios.get(`${apiBase}/error-test/business-logic`);
            return { success: true, data: res.data };
        } catch (error) {
            return { success: false, error };
        }
    },

    testCritical: async (): Promise<ErrorTestResult> => {
        try {
            const res = await axios.get(`${apiBase}/error-test/critical`);
            return { success: true, data: res.data };
        } catch (error) {
            return { success: false, error };
        }
    },

    testMultipleValidation: async (): Promise<ErrorTestResult> => {
        try {
            const res = await axios.get(`${apiBase}/error-test/multiple-validation`);
            return { success: true, data: res.data };
        } catch (error) {
            return { success: false, error };
        }
    },

    testSuccess: async (): Promise<ErrorTestResult> => {
        try {
            const res = await axios.get(`${apiBase}/error-test/success`);
            return { success: true, data: res.data };
        } catch (error) {
            return { success: false, error };
        }
    }
};
