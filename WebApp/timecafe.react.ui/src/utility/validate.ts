// src/utils/validation.ts
export function validateEmail(email: string): string {
    if (!email.trim() || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
        return "Введите корректный email.";
    return "";
}

export function validateUsername(username: string): string {
    if (!username.trim())
        return "Введите имя пользователя.";
    return "";
}

export function validateConfirmPassword(confirmPassword: string, password: string): string {
    if (confirmPassword !== password)
        return "Пароли не совпадают.";
    return "";
}

export function validatePassword(password: string): string {
    const errors: string[] = [];
    if (password.length < 6)
        errors.push("Пароль должен содержать не менее 6 символов.");
    if (!/\d/.test(password))
        errors.push("Пароль должен содержать хотя бы 1 цифру.");
    if (!/[a-zа-яё]/i.test(password))
        errors.push("Пароль должен содержать хотя бы 1 букву.");
    return errors.join(" ");
}
