import axios from "axios";

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface EmailRequest {
    email: string;
}

export interface ApiError {
    [key: string]: string[] | string;
}

const apiBase = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7057";

export async function registerUser(data: RegisterRequest): Promise<void> {
    try {
        await axios.post(`${apiBase}/registerWithUsername`, data, {
            headers: {"Content-Type": "application/json"},
        });
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;

            if (res?.data?.errors) {
                const errors: ApiError = res.data.errors;
                throw errors;
            }

            throw new Error(`Ошибка регистрации (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при регистрации");
    }
}

export async function loginUser(data: LoginRequest): Promise<void> {
    try {
        const res = await axios.post(`${apiBase}/login-jwt`, data, {
            headers: {"Content-Type": "application/json"},
        })

        const tokens = res.data as { accessToken: string; refreshToken: string };
        localStorage.setItem("accessToken", tokens.accessToken);
        localStorage.setItem("refreshToken", tokens.refreshToken);

    } catch (error) {
        if (axios.isAxiosError(error)) {

            const res = error.response;

            if (res?.data?.errors) {
                const errors: ApiError = res.data.errors;
                throw errors;
            }

            throw new Error(`Ошибка входа (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при входе");
    }
}


export async function refreshToken(): Promise<void> {
    const refreshToken = localStorage.getItem("refreshToken");
    if (!refreshToken) throw new Error("Нет refresh токена");

    try {
        const res = await axios.post(`${apiBase}/refresh-token-jwt`, {refreshToken}, {
            headers: {"Content-Type": "application/json"},
        });

        const tokens = res.data as { accessToken: string; refreshToken: string };
        localStorage.setItem("accessToken", tokens.accessToken);
        localStorage.setItem("refreshToken", tokens.refreshToken);

    } catch {
        // Todo Если refresh не удался — нужно разлогинить пользователя
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        
        throw new Error("Не удалось обновить токен");
    }
}

export async function forgotPassword(data: EmailRequest): Promise<void> {
    try {
        await axios.post(`${apiBase}/forgotPassword`, data, {
            headers: {"Content-Type": "application/json"},
        })
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;
            if (res?.data?.errors) {
                const errors: ApiError = res.data.errors;
                throw errors;
            }
            throw new Error(`Ошибка отправки (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при попытке отправить сообщение на почту");
    }
}



