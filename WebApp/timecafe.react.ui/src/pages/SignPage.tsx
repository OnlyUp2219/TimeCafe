import { Button, Input, Link, Field } from '@fluentui/react-components';
import { useNavigate } from "react-router-dom";
import * as React from 'react';
import "./SignPage.css";
import { faker } from '@faker-js/faker';

export const SignPage = () => {
    const navigate = useNavigate();

    const [username, setUsername] = React.useState("");
    const [password, setPassword] = React.useState("");
    const [email, setEmail] = React.useState("");
    const [confirmPassword, setConfirmPassword] = React.useState("");

    React.useEffect(() => {
        setUsername(faker.internet.username());
        setEmail(faker.internet.email());
        setPassword(faker.internet.password({ length: 10 }));
    }, []);


    const [errors, setErrors] = React.useState({
        username: "",
        email: "",
        password: "",
        confirmPassword: "",
    });
    const [isSubmitting, setIsSubmitting] = React.useState(false);

    const validate = () => {
        const newErrors = {
            username: "",
            email: "",
            password: "",
            confirmPassword: "",
        };

        if (!username.trim())
            newErrors.username = "Введите имя пользователя.";
        if (!email.trim() || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
            newErrors.email = "Введите корректный email.";

        const passwordErrors = [];
        if (password.length < 6) {
            passwordErrors.push("Пароль должен содержать не менее 6 символов.");
        }
        if (!/\d/.test(password)) {
            passwordErrors.push("Пароль должен содержать хотя бы 1 цифру.");
        }
        if (!/[a-zа-яё]/i.test(password)) {
            passwordErrors.push("Пароль должен содержать хотя бы 1 букву.");
        }
        newErrors.password = passwordErrors.map((error, index) => `${index + 1}) ${error}`).join(" ");

        if (confirmPassword !== password) {
            newErrors.confirmPassword = "Пароли не совпадают.";
        }

        setErrors(newErrors);

        return !Object.values(newErrors).some(e => e);
    };

    const handleSubmit = async () => {
        if (!validate()) return;

        setIsSubmitting(true);
        try {
            const apiBase = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7057";
            const res = await fetch(`${apiBase}/registerWithUsername`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    userName: username,
                    email,
                    password
                })
            });


            if (res.ok) {
                console.log('register OK', res.status);
                navigate("/login");
                return;
            }

            let body: any = null;
            try { body = await res.json(); } catch (e) { console.warn('non-json response', e); }

            const newErrors = { username: "", email: "", password: "", confirmPassword: "" };
            if (body && body.errors) {
                for (const [key, value] of Object.entries(body.errors)) {
                    const arr = Array.isArray(value) ? value : [value];

                    arr.forEach((e: { code?: string; description?: string }) => {
                        const code = e.code?.toLowerCase();
                        const msg = e.description ?? String(e);

                        if (code?.includes('password')) newErrors.password += (newErrors.password ? ' ' : '') + msg;
                        else if (code?.includes('email')) newErrors.email += (newErrors.email ? ' ' : '') + msg;
                        else newErrors.username += (newErrors.username ? ' ' : '') + msg;
                    });
                }
            } else {
                newErrors.username = `Ошибка регистрации (${res.status})`;
            }




            setErrors(newErrors);
        } catch (err) {
            console.error('register error', err);
            setErrors(prev => ({ ...prev, username: 'Ошибка соединения с сервером' }));
        } finally {
            setIsSubmitting(false);
        }
    };

    return (

        <div className="signin_card">
            <h2>Регистрация</h2>

            <Field
                label="Имя пользователя"
                required
                validationState={errors.username ? "error" : undefined}
                validationMessage={errors.username}
            >
                <Input
                    value={username}
                    onChange={(_, data) => setUsername(data.value)}
                    placeholder="Введите имя пользователя"
                />
            </Field>

            <Field
                label="Email"
                required
                validationState={errors.email ? "error" : undefined}
                validationMessage={errors.email}>
                <Input
                    value={email}
                    onChange={(_, data) => setEmail(data.value)}
                    placeholder="Введите email" />
            </Field>

            <Field
                label="Пароль"
                required
                validationState={errors.password ? "error" : undefined}
                validationMessage={errors.password}
            >
                <Input
                    type="password"
                    value={password}
                    onChange={(_, data) => setPassword(data.value)}
                    placeholder="Введите пароль"
                />
            </Field>

            <Field
                label="Повторить пароль"
                required
                validationState={errors.confirmPassword ? "error" : undefined}
                validationMessage={errors.confirmPassword}
            >
                <Input
                    type="password"
                    value={confirmPassword}
                    onChange={(_, data) => setConfirmPassword(data.value)}
                    placeholder="Повторите пароль"
                />
            </Field>

            <Button appearance="primary" onClick={handleSubmit} disabled={isSubmitting} >
                Зарегистрироваться
            </Button>

            <Link onClick={() => navigate("/login")}>Войти</Link>
        </div>

    );
};
