import {Button, Input, Link, Field} from '@fluentui/react-components';
import {useNavigate} from "react-router-dom";
import * as React from 'react';
import "./SignPage.css";
import {faker} from '@faker-js/faker';
import {validateConfirmPassword, validateEmail, validatePassword, validateUsername} from "../utility/validate.ts";
import {registerUser} from "../api/auth.ts";

export const SignPage = () => {
    const navigate = useNavigate();

    const [username, setUsername] = React.useState("");
    const [password, setPassword] = React.useState("");
    const [email, setEmail] = React.useState("");
    const [confirmPassword, setConfirmPassword] = React.useState("");
    const [errors, setErrors] = React.useState({
        username: "",
        email: "",
        password: "",
        confirmPassword: "",
    });
    const [isSubmitting, setIsSubmitting] = React.useState(false);

    React.useEffect(() => {
        setUsername(faker.internet.username());
        setEmail(faker.internet.email());
        const pwd =
            faker.string.alpha({length: 1, casing: "upper"}) +
            faker.string.alphanumeric({length: 4}) +
            faker.string.numeric({length: 1});

        setPassword(pwd);
        setConfirmPassword(pwd);
    }, []);

    const validate = () => {
        const usernameError = validateUsername(username);
        const emailError = validateEmail(email);
        const passwordError = validatePassword(password);
        const confirmPasswordError = validateConfirmPassword(confirmPassword, password);
        setErrors({
            username: usernameError,
            email: emailError,
            password: passwordError,
            confirmPassword: confirmPasswordError
        });
        return !emailError && !passwordError && !confirmPasswordError && !usernameError;
    };

    const handleSubmit = async () => {
        if (!validate()) return;

        setIsSubmitting(true);
        try {
            await registerUser({username, email, password});
            navigate("/login");
        } catch (err: any) {
            const newErrors = {email: "", password: "", username: "", confirmPassword: ""};

            if (Array.isArray(err)) {
                // err - массив IdentityError { Code, Description }
                err.forEach((e: { code: string; description: string }) => {
                    const code = e.code.toLowerCase();
                    if (code.includes("email")) newErrors.email += e.description + " ";
                    else if (code.includes("password")) newErrors.password += e.description + " ";
                    else if (code.includes("username")) newErrors.username += e.description + " ";
                });
                setErrors(newErrors);
            } else {
                const message = err instanceof Error ? err.message : String(err);
                newErrors.username = message;
                setErrors(newErrors);
            }
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
                label="Почта"
                required
                validationState={errors.email ? "error" : undefined}
                validationMessage={errors.email}>
                <Input
                    value={email}
                    onChange={(_, data) => setEmail(data.value)}
                    placeholder="Введите почту"/>
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

            <Button appearance="primary" onClick={handleSubmit} disabled={isSubmitting}>
                Зарегистрироваться
            </Button>

            <Link onClick={() => navigate("/login")}>Войти</Link>
        </div>

    );
};
