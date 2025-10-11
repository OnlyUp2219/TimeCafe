export interface RegisterRequest {
    userName: string;
    email: string;
    password: string;
}

export interface ApiError {
    [key: string]: string[] | string;
}

export async function registerUser(data: RegisterRequest): Promise<void> {
    const apiBase = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7057";

    const res = await fetch(`${apiBase}/registerWithUsername`, {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify(data)
    });

    if (res.ok) return;

    let body: any = null;
    try {
        body = await res.json();
    } catch {
    }

    if (body?.errors) {
        const errors: ApiError = body.errors;
        throw errors;
    }

    throw new Error(`Ошибка регистрации (${res.status})`);
}
