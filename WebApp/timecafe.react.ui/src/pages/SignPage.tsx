import {Button, Input, Link, Field, Subtitle1, Card} from '@fluentui/react-components';
import {useNavigate} from "react-router-dom";
import {faker} from '@faker-js/faker';
import {validateConfirmPassword, validateEmail, validatePassword, validateUsername} from "../utility/validate.ts";
import {registerUser} from "../api/auth.ts";
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";
import {useDispatch} from "react-redux";
import {useEffect, useState} from "react";

export const SignPage = () => {
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const {showToast, ToasterElement} = useProgressToast();

    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [email, setEmail] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [errors, setErrors] = useState({
        username: "",
        email: "",
        password: "",
        confirmPassword: "",
    });
    const [isSubmitting, setIsSubmitting] = useState(false);


    useEffect(() => {
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
            await registerUser({username, email, password}, dispatch);
            navigate("/home");
        } catch (err: any) {
            const newErrors = {email: "", password: "", username: "", confirmPassword: ""};

            if (Array.isArray(err)) {
                err.forEach((e: { code: string; description: string }) => {
                    const code = e.code.toLowerCase();
                    if (code.includes("email")) newErrors.email += e.description + " ";
                    else if (code.includes("password")) newErrors.password += e.description + " ";
                    else if (code.includes("username")) newErrors.username += e.description + " ";
                });
                setErrors(newErrors);
                showToast("Ошибка при регистрации. Проверьте введённые данные.", "error", "Ошибка");
            } else {
                const message = err instanceof Error ? err.message : String(err);
                showToast(message, "error", "Ошибка");
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    return (

        <Card className="auth_card">

            {ToasterElement}

            <Subtitle1 align={"center"}>Регистрация</Subtitle1>
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

        </Card>

    );
};
