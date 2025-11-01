import {Button, Input, Link, Field, Subtitle1, Card} from '@fluentui/react-components';
import {useNavigate} from "react-router-dom";
import {faker} from '@faker-js/faker';
import {validateConfirmPassword, validateEmail, validatePassword, validateUsername} from "../utility/validate.ts";
import {registerUser} from "../api/auth.ts";
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";
import {useEffect, useState} from "react";
import {useErrorHandler} from "../hooks/useErrorHandler.ts";

export const SignPage = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();
    const {fieldErrors, handleError, clearAllErrors} = useErrorHandler(showToast);

    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [email, setEmail] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [localErrors, setLocalErrors] = useState({
        username: "",
        email: "",
        password: "",
        confirmPassword: "",
    });
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        setUsername(faker.internet.username());
        setEmail("klimenkokov1@timecafesharp.ru");
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
        
        setLocalErrors({
            username: usernameError,
            email: emailError,
            password: passwordError,
            confirmPassword: confirmPasswordError
        });
        
        return !emailError && !passwordError && !confirmPasswordError && !usernameError;
    };

    const handleSubmit = async () => {
        if (!validate()) return;

        clearAllErrors();
        setIsSubmitting(true);
        
        try {
            const confirmLink = await registerUser({username, email, password});

            if (confirmLink) {
                showToast(`Регистрация успешна! Ссылка: ${confirmLink}`, "success", "Успех");
                setTimeout(() => window.location.href = confirmLink, 2000);
            } else {
                showToast("Регистрация успешна! Проверьте почту для подтверждения email.", "success", "Успех");
                navigate("/login");
            }
        } catch (error) {
            handleError(error);
        } finally {
            setIsSubmitting(false);
        }
    };

    const allErrors = {...localErrors, ...fieldErrors};

    return (
        <Card className="auth_card">
            {ToasterElement}

            <Subtitle1 align={"center"}>Регистрация</Subtitle1>
            
            <Field
                label="Имя пользователя"
                required
                validationState={allErrors.username ? "error" : undefined}
                validationMessage={allErrors.username}
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
                validationState={allErrors.email ? "error" : undefined}
                validationMessage={allErrors.email}>
                <Input
                    value={email}
                    onChange={(_, data) => setEmail(data.value)}
                    placeholder="Введите почту"/>
            </Field>

            <Field
                label="Пароль"
                required
                validationState={allErrors.password ? "error" : undefined}
                validationMessage={allErrors.password}
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
                validationState={allErrors.confirmPassword ? "error" : undefined}
                validationMessage={allErrors.confirmPassword}
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
        </Card>
    );
};
