import { Button, Input, Link, Field, Divider } from '@fluentui/react-components'
import { useNavigate } from "react-router-dom";
import "./SignPage.css";

export const SignPage = () => {
    const navigate = useNavigate();
    return (
        <div className="signin_root">
            <div className="signin_card">
                <h2>Регистрация</h2>
                <Field label="Имя пользователя" required>
                    <Input placeholder="Введите имя пользователя" enterKeyHint="send"></Input>
                </Field>
                <Field label="Пароль" required>
                    <Input placeholder="Введите пароль"></Input>
                </Field>
                <Field label="Повторить пароль" required>
                    <Input placeholder="Повторите пароль"></Input>
                </Field>
                <Button appearance="primary" onClick={() => navigate("/login")}>Зарегистрироваться</Button>
                <Link onClick={() => navigate("/login")}>Войти</Link>
            </div>
        </div>
    )
}