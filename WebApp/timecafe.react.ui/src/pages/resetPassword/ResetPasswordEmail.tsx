import {useNavigate} from "react-router-dom";
import * as React from "react";
import {Button, Field, Input, Subtitle1, Text} from "@fluentui/react-components";
import {validateEmail} from "../../utility/validate.ts";
import {forgotPassword} from "../../api/auth.ts";
import {useEffect} from "react";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";
import {parseErrorMessage} from "../../utility/errors.ts";


export const ResetPasswordEmail = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();

    const [email, setEmail] = React.useState("");
    const [errors, setErrors] = React.useState({
        email: "",
    });
    const [isSubmitting, setIsSubmitting] = React.useState(false);
    const [isSent, setIsSent] = React.useState(false);
    const [link, setLink] = React.useState("");


    useEffect(() => {
        setEmail("klimenkokov1@timecafesharp.ru");
    }, []);

    const validate = () => {
        const emailError = validateEmail(email);
        setErrors({email: emailError});
        return !emailError;
    };

    const handleSubmit = async () => {
        if (!validate()) return;

        setIsSubmitting(true);
        try {
            const res = await forgotPassword({email});
            setIsSent(true);
            setLink(res.callbackUrl);
        } catch (err: any) {
            const newErrors = {email: ""};

            if (Array.isArray(err)) {
                err.forEach((e: { code: string; description: string }) => {
                    const code = e.code.toLowerCase();
                    if (code.includes("email")) newErrors.email += e.description + " ";
                });
            } else {
                const message = parseErrorMessage(err);
                showToast(message, "error");
            }
            setErrors(newErrors);
        } finally {
            setIsSubmitting(false);
        }
    };

    if (isSent) {
        return (
            <div className="auth_card p-6">
                {ToasterElement}

                <Subtitle1 align="center" style={{marginBottom: 16}}>Сообщение отправлено!</Subtitle1>

                <Text>
                    Письмо для сброса пароля отправлено на <strong>{email}</strong>.
                </Text>

                <Text className="flex flex-wrap break-words">
                    <a className="break-all hyphens-auto" href={link}>Ссылка </a> для сброса пароля.
                </Text>

                <Text>
                    Проверьте свой почтовый ящик, включая папку "Спам". Ссылка для восстановления пароля действительна
                    ограниченное время.
                </Text>

                <div className="flex flex-col gap-[12px] mb-4">
                    <Button
                        appearance="primary"
                        onClick={() => window.open("https://mail.google.com", "_blank")}
                        type="button"
                    >
                        Перейти в Gmail
                    </Button>

                    <Button
                        appearance="secondary"
                        onClick={() => navigate("/login")}
                        type="button"
                    >
                        Продолжить в приложении
                    </Button>

                    <Button
                        appearance="transparent"
                        onClick={() => setIsSent(false)}
                        type="button"
                    >
                        Изменить email
                    </Button>
                </div>
            </div>
        );
    }


    if (!isSent) {
        return (
            <div className="auth_card">
                <Subtitle1 align={"center"}>Забыли пароль?</Subtitle1>

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

                <div className="flex w-full justify-between flex-wrap gap-x-[48px] gap-y-[16px]">
                    <Button className="flex-[1]" appearance="secondary" onClick={() => navigate(-1)}
                            type="button">Назад</Button>

                    <Button className="flex-[1.5]" appearance="primary" onClick={handleSubmit} disabled={isSubmitting}
                            type="button">Продолжить</Button>
                </div>

            </div>
        )
    }
}