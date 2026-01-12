import {
    Button,
    Input,
    Field,
    Link,
    Text,
    Title3,
    Dialog,
    DialogContent,
    DialogBody,
    DialogSurface,
    DialogTitle,
    DialogTrigger, DialogActions
} from '@fluentui/react-components';
import {MailCheckmark20Filled} from '@fluentui/react-icons';
import {useState} from "react";
import {useNavigate} from "react-router-dom";
import {validateEmail} from "../utility/validate.ts";
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";

export const ResetPasswordPage = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();

    const [email, setEmail] = useState("");
    const [errors, setErrors] = useState({email: ""});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [openDialog, setOpenDialog] = useState(false);
    const [sentEmail, setSentEmail] = useState("");

    const validate = () => {
        const emailError = validateEmail(email);
        setErrors({email: emailError});
        return !emailError;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!validate()) return;

        setIsSubmitting(true);
        try {
            // TODO: STUB - отправка кода восстановления (подключить бек)
            // await sendResetCode({email});
            setSentEmail(email);
            setOpenDialog(true);
        } catch (err: unknown) {
            if (err && typeof err === 'object' && 'errors' in err && Array.isArray((err as {
                errors: Array<{ description: string }>
            }).errors)) {
                const message = (err as {
                    errors: Array<{ description: string }>
                }).errors.map(e => e.description).join(" ");
                showToast(message, "error");
            } else {
                showToast("Ошибка. Попробуйте позже", "error");
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleGoToEmail = () => {
        window.open("https://mail.google.com", "_blank");
    };

    const handleGoToLogin = () => {
        navigate("/login", {replace: true});
    };

    return (
        <>
            {ToasterElement}
            <Dialog open={openDialog}>
                <DialogSurface>
                    <DialogBody>
                        <DialogTitle>
                            <div>
                                <MailCheckmark20Filled style={{color: '#107C10'}} />
                                Письмо отправлено
                            </div>
                        </DialogTitle>
                        <DialogContent>
                            <Text block className="mb-3">Мы отправили письмо на почту <strong>{sentEmail}</strong></Text>
                            <Text block>Перейдите на почту и нажмите ссылку для сброса пароля</Text>
                        </DialogContent>
                        <DialogActions>
                            <Button
                                appearance="primary"
                                onClick={handleGoToEmail}
                            >
                                На почту
                            </Button>
                            <Button
                                appearance="secondary"
                                onClick={handleGoToLogin}
                            >
                                К входу
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>

            <div
                className="!grid grid-cols-1 items-center justify-center
                 sm:grid-cols-2 sm:justify-stretch sm:items-stretch">

                {/* Hero Section - Left Side (Desktop Only) */}
                <div id="Left Side" className="hidden sm:block bg-sky-400">
                </div>

                {/* Reset Form */}
                <div id="Form"
                     className="flex flex-col flex-wrap items-center w-full
                     sm:w-auto sm:justify-center sm:p-8">
                    <div className="flex flex-col w-full max-w-md gap-[12px]">

                        <div className="flex flex-col items-center">
                            <Title3 block>Восстановление пароля</Title3>
                            <Text block>Введите почту вашего аккаунта</Text>
                        </div>

                        <form onSubmit={handleSubmit}>
                            <Field
                                label="Email"
                                required
                                validationState={errors.email ? "error" : undefined}
                                validationMessage={errors.email}
                            >
                                <Input
                                    type="email"
                                    value={email}
                                    onChange={(_, data) => setEmail(data.value)}
                                    placeholder="example@timecafe.ru"
                                    disabled={isSubmitting || openDialog}
                                    className="w-full"
                                />
                            </Field>

                            <Button
                                appearance="primary"
                                type="submit"
                                disabled={isSubmitting || openDialog}
                                className="w-full mt-4"
                            >
                                {isSubmitting ? "Отправка..." : "Отправить код"}
                            </Button>
                        </form>

                        <div>
                            <Text size={300}>
                                <Link
                                    onClick={() => navigate("/login")}
                                >
                                    Вернуться к входу
                                </Link>
                            </Text>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
};
