import {Button, Input, Link, Field, Divider} from '@fluentui/react-components'
import {useNavigate} from "react-router-dom";
import './LoginPage.css'
import * as React from "react";
import {validateEmail, validatePassword} from "../utility/validate.ts";

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

        navigate("/home")
        setIsSubmitting(false);
    }


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

            <Button appearance="primary" onClick={handleSubmit} disabled={isSubmitting}>Войти</Button>

            <Link onClick={() => navigate("/sign")}>Зарегистрироваться</Link>

            <Divider appearance="brand" className="divider">или</Divider>

            <Button icon={<div className="icons8-google"></div>} appearance="outline"
                    onClick={() => navigate("/sign")}>Google</Button>
            <Button icon={<div className="icons8-microsoft"></div>} appearance="outline"
                    onClick={() => navigate("/sign")}>Microsoft</Button>
        </div>
    )
}
