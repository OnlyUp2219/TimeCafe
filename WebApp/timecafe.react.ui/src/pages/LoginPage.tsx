import { Button, Input,Link, Field, Divider } from '@fluentui/react-components'
import { useNavigate } from "react-router-dom";
import './LoginPage.css'


export const LoginPage = () => {

    const navigate = useNavigate();
    return (
            <div className="login_card">
                <h2>Вход</h2>
                <Field label="Имя пользователя" required>
                    <Input placeholder="Введите имя пользователя"></Input>
                </Field>
                <div>
                    <Field label="Пароль" required>
                        <Input placeholder="Введите пароль"></Input>
                    </Field>
                    <Link onClick={() => navigate("/sign")}>Забыли пароль?</Link>
                </div>
                <Button appearance="primary" onClick={() => navigate("/home")}>Войти</Button>
                <Link onClick={() => navigate("/sign")}>Зарегистрироваться</Link>
                <Divider appearance="brand" className="divider">или</Divider>

                <Button icon={<div className="icons8-google"></div>} appearance="outline" onClick={() => navigate("/sign")}>Google</Button>
                <Button icon={<div className="icons8-microsoft"></div>} appearance="outline" onClick={() => navigate("/sign")}>Microsoft</Button>
            </div>
    )
}
