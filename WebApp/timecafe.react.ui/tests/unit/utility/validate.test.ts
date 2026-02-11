import {describe, expect, it} from "vitest";
import {
    validateConfirmPassword,
    validateEmail,
    validatePassword,
    validatePhoneNumber,
    validateUsername,
} from "@utility/validate";

describe("validateEmail", () => {
    it("returns message for invalid email", () => {
        expect(validateEmail(" ")).toBe("Введите корректный email.");
    });

    it("returns empty string for valid email", () => {
        expect(validateEmail("user@example.com")).toBe("");
    });
});

describe("validateUsername", () => {
    it("returns message for empty username", () => {
        expect(validateUsername(" ")).toBe("Введите имя пользователя.");
    });

    it("returns empty string for valid username", () => {
        expect(validateUsername("user")).toBe("");
    });
});

describe("validateConfirmPassword", () => {
    it("returns message when passwords do not match", () => {
        expect(validateConfirmPassword("a", "b")).toBe("Пароли не совпадают.");
    });

    it("returns empty string when passwords match", () => {
        expect(validateConfirmPassword("pass", "pass")).toBe("");
    });
});

describe("validatePassword", () => {
    it("returns combined errors for weak password", () => {
        expect(validatePassword("abc")).toBe(
            "Пароль должен содержать не менее 6 символов. Пароль должен содержать хотя бы 1 цифру."
        );
    });

    it("returns empty string for strong password", () => {
        expect(validatePassword("abc123")).toBe("");
    });
});

describe("validatePhoneNumber", () => {
    it("returns message for empty phone", () => {
        expect(validatePhoneNumber(" ")).toBe("Введите номер телефона.");
    });

    it("returns message for too short phone", () => {
        expect(validatePhoneNumber("123")).toBe("Номер телефона должен содержать минимум 10 цифр.");
    });

    it("returns message for too long phone", () => {
        expect(validatePhoneNumber("+1234567890123456")).toBe("Номер телефона слишком длинный.");
    });

    it("returns message for invalid symbols", () => {
        expect(validatePhoneNumber("123#4567890")).toBe("Номер телефона содержит недопустимые символы.");
    });

    it("returns empty string for valid phone", () => {
        expect(validatePhoneNumber("+1 (234) 567-8901")).toBe("");
    });
});
