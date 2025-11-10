import {
    Button,
    Input,
    Field,
    Subtitle1,
    Card,
    MessageBar,
    MessageBarBody,
    MessageBarTitle
} from '@fluentui/react-components';
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
    const [registered, setRegistered] = useState(false);
    const [mockLink, setMockLink] = useState<string | undefined>(undefined);


    // useEffect(() => {
    //     setUsername(faker.internet.username());
    //     setEmail(faker.internet.email());
    //     const pwd =
    //         faker.string.alpha({length: 1, casing: "upper"}) +
    //         faker.string.alphanumeric({length: 4}) +
    //         faker.string.numeric({length: 1});
    //
    //     setPassword(pwd);
    //     setConfirmPassword(pwd);
    // }, []);

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
            const r = await registerUser({username, email, password}, dispatch);
            setRegistered(true);
            if (r.callbackUrl) setMockLink(r.callbackUrl);
        } catch (err: any) {
            const newErrors = {email: "", password: "", username: "", confirmPassword: ""};

            if (Array.isArray(err.errors)) {
                err.errors.forEach((e: { code: string; description: string }) => {
                    const code = e.code.toLowerCase();
                    if (code.includes("email")) newErrors.email += e.description + " ";
                    else if (code.includes("password")) newErrors.password += e.description + " ";
                    else if (code.includes("username")) newErrors.username += e.description + " ";
                });
                setErrors(newErrors);
            }
            if (err instanceof Error) {
                const message = err.message;
                showToast(message, "error", "Ошибка");
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <Card className="auth_card">
            {ToasterElement}
            {registered && mockLink && (
                <MessageBar intent="info" layout="multiline">
                    <MessageBarBody>
                        <MessageBarTitle>Mock ссылка:</MessageBarTitle>
                        <a href={mockLink} style={{wordBreak: 'break-all'}}>{mockLink.slice(0, 120)}...</a>
                    </MessageBarBody>
                </MessageBar>
            )}
            {registered && !mockLink && (
                <MessageBar intent="success">
                    <MessageBarBody>
                        <MessageBarTitle>Успех:</MessageBarTitle>
                        Перейдите на почту для подтверждения аккаунта
                    </MessageBarBody>
                </MessageBar>
            )}
            <Subtitle1 align={"center"}>Регистрация</Subtitle1>
            <Field label="Имя пользователя" required validationState={errors.username ? "error" : undefined}
                   validationMessage={errors.username}>
                <Input value={username} onChange={(_, d) => setUsername(d.value)}
                       placeholder="Введите имя пользователя" autoComplete="new-username"/>
            </Field>
            <Field label="Почта" required validationState={errors.email ? "error" : undefined}
                   validationMessage={errors.email}>
                <Input autoComplete="new-email" value={email} onChange={(_, d) => setEmail(d.value)}
                       placeholder="Введите почту"/>
            </Field>
            <Field label="Пароль" required validationState={errors.password ? "error" : undefined}
                   validationMessage={errors.password}>
                <Input type="password" value={password} onChange={(_, d) => setPassword(d.value)}
                       placeholder="Введите пароль" autoComplete="new-password"/>
            </Field>
            <Field label="Повторить пароль" required validationState={errors.confirmPassword ? "error" : undefined}
                   validationMessage={errors.confirmPassword}>
                <Input type="password" value={confirmPassword} onChange={(_, d) => setConfirmPassword(d.value)}
                       placeholder="Повторите пароль" autoComplete="new-password"/>
            </Field>

            <div className="button-action ">
                <Button as='a' appearance="subtle" onClick={() => navigate("/login")}>Войти</Button>
                <Button appearance="primary" onClick={handleSubmit} disabled={isSubmitting}>Зарегистрироваться</Button>
            </div>
        </Card>
    );
};
