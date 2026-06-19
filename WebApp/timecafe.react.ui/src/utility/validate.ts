// src/utils/validation.ts
export const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function validateEmail(email: string): string {
    if (!email.trim() || !EMAIL_REGEX.test(email))
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

export const PASSWORD_RULES = {
    minLength: (pwd: string) => pwd.length >= 6,
    hasDigit: (pwd: string) => /\d/.test(pwd),
    hasLetter: (pwd: string) => /[a-zа-яё]/i.test(pwd),
};

export function validatePassword(password: string): string {
    const errors: string[] = [];
    if (!PASSWORD_RULES.minLength(password))
        errors.push("Пароль должен содержать не менее 6 символов.");
    if (!PASSWORD_RULES.hasDigit(password))
        errors.push("Пароль должен содержать хотя бы 1 цифру.");
    if (!PASSWORD_RULES.hasLetter(password))
        errors.push("Пароль должен содержать хотя бы 1 букву.");
    return errors.join(" ");
}

export const PHONE_ALLOWED_REGEX = /^[+]?[0-9\s\-()]+$/;

export function validatePhoneNumber(phoneNumber: string): string {
    const cleaned = phoneNumber.replace(/\D/g, "");

    if (!phoneNumber.trim())
        return "Введите номер телефона.";

    if (cleaned.length < 10)
        return "Номер телефона должен содержать минимум 10 цифр.";

    if (cleaned.length > 15)
        return "Номер телефона слишком длинный.";

    if (!PHONE_ALLOWED_REGEX.test(phoneNumber))
        return "Номер телефона содержит недопустимые символы.";

    return "";
}

export const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export function isUuid(str: string): boolean {
    return UUID_REGEX.test(str);
}
