import {Button, Input, Link, Field, Divider} from '@fluentui/react-components'
import {useNavigate} from "react-router-dom";
import './LoginPage.css'
import * as React from "react";
import {validateEmail, validatePassword} from "../utility/validate.ts";
import {loginUser} from "../api/auth.ts";

export const LoginPage = () => {

    const navigate = useNavigate();

    const [password, setPassword] = React.useState("");
    const [email, setEmail] = React.useState("");
    const [errors, setErrors] = React.useState({
        email: "",
        password: "",
    });
    const [isSubmitting, setIsSubmitting] = React.useState(false);

    const validate = () => {
        const emailError = validateEmail(email);
        const passwordError = validatePassword(password);
        setErrors({email: emailError, password: passwordError});
        return !emailError && !passwordError;
    };

    const handleSubmit = async () => {
        if (!validate()) return;

        setIsSubmitting(true);
        try {
            await loginUser({email, password});
            navigate("/home");
        } catch (err: any) {
            const newErrors = {email: "", password: ""};

            if (Array.isArray(err)) {
                err.forEach((e: { code: string; description: string }) => {
                    const code = e.code.toLowerCase();
                    if (code.includes("email")) newErrors.email += e.description + " ";
                    else if (code.includes("password")) newErrors.password += e.description + " ";
                    else newErrors.email += e.description + " ";
                });
            } else {
                const message = err instanceof Error ? err.message : String(err);
                newErrors.email = message;
            }
            setErrors(newErrors);
        } finally {
            setIsSubmitting(false);
            console.log("accessToken:", localStorage.getItem("accessToken"));
            console.log("refreshToken:", localStorage.getItem("refreshToken"));

        }
    };


    return (
        <div className="login_card">
            <h2>Вход</h2>
            <Field label="Почта"
                   required
                   validationState={errors.email ? "error" : undefined}
                   validationMessage={errors.email}
            >
                <Input
                    value={email}
                    placeholder="Введите почту"
                    type="email"
                    onChange={(_, data) => setEmail(data.value)}
                />
            </Field>

            <div>
                <Field label="Пароль"
                       required
                       validationState={errors.password ? "error" : undefined}
                       validationMessage={errors.password}>
                    <Input
                        value={password}
                        placeholder="Введите пароль"
                        type="password"
                        onChange={(_, data) => setPassword(data.value)}
                    />
                </Field>

                <Link onClick={() => navigate("/sign")}>Забыли пароль?</Link>
            </div>

            <Button appearance="primary" onClick={handleSubmit} disabled={isSubmitting} type="button">Войти</Button>

            <Link onClick={() => navigate("/sign")}>Зарегистрироваться</Link>

            <Divider appearance="brand" className="divider">или</Divider>

            <Button icon={<div className="icons8-google"></div>} appearance="outline"
                    onClick={() => navigate("/sign")}>Google</Button>
            <Button icon={<div className="icons8-microsoft"></div>} appearance="outline"
                    onClick={() => navigate("/sign")}>Microsoft</Button>
        </div>
    )
}
