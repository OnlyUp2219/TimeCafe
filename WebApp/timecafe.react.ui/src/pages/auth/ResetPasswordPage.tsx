import {
    Button,
    Link,
    Body2,
    Caption1,
    Title3,
    Dialog,
    DialogContent,
    DialogBody,
    DialogSurface,
    DialogTitle,
    DialogActions
} from '@fluentui/react-components';
import {MailCheckmark20Filled} from '@fluentui/react-icons';
import {useState} from "react";
import {useNavigate} from "react-router-dom";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";
import {EmailInput} from "../../components/FormFields";
import {authFormContainerClassName} from "../../layouts/authLayout";
import {authApi} from "../../shared/api/auth/authApi";
import {MockCallbackLink} from "../../components/MockCallbackLink/MockCallbackLink";

export const ResetPasswordPage = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();

    const [email, setEmail] = useState("");
    const [errors, setErrors] = useState({email: ""});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [openDialog, setOpenDialog] = useState(false);
    const [sentEmail, setSentEmail] = useState("");
    const [submitted, setSubmitted] = useState(false);
    const [callbackUrl, setCallbackUrl] = useState<string | undefined>(undefined);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(true);
        if (errors.email || !email) return;

        setIsSubmitting(true);
        try {
            const res = await authApi.forgotPasswordLink({email});
            setCallbackUrl(res.callbackUrl);
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

    const handleOpenCallbackUrl = () => {
        if (callbackUrl) {
            window.open(callbackUrl, "_blank");
        }
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
                                <MailCheckmark20Filled style={{color: '#107C10'}}/>
                                Письмо отправлено
                            </div>
                        </DialogTitle>
                        <DialogContent>
                            <Body2 block className="mb-3">Мы отправили письмо на
                                почту <strong>{sentEmail}</strong></Body2>
                            <Body2 block>Перейдите на почту и нажмите ссылку для сброса пароля</Body2>
                            <MockCallbackLink url={callbackUrl}/>
                        </DialogContent>
                        <DialogActions>
                            {callbackUrl ? (
                                <Button
                                    appearance="primary"
                                    onClick={handleOpenCallbackUrl}
                                >
                                    Открыть ссылку
                                </Button>
                            ) : (
                            <Button
                                appearance="primary"
                                onClick={handleGoToEmail}
                            >
                                На почту
                            </Button>
                            )}
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

            <div className="!grid grid-cols-1 items-center justify-center
                 sm:grid-cols-2 sm:justify-stretch sm:items-stretch">

                {/* Hero Section - Left Side (Desktop Only) */}
                <div id="Left Side" className="relative hidden sm:block bg-[url(/src/assets/abstract_bg.svg)] bg-left bg-cover bg-no-repeat">
                    <div className="absolute inset-0 bg-black/40 pointer-events-none" />
                </div>

                {/* Reset Form */}
                <div id="Form"
                     className={authFormContainerClassName}>
                    <div className="flex flex-col w-full max-w-md gap-[12px]">

                        <div className="flex flex-col items-center">
                            <Title3 block>Восстановление пароля</Title3>
                            <Body2 block>Введите почту вашего аккаунта</Body2>
                        </div>

                        <form onSubmit={handleSubmit}>
                            <EmailInput
                                value={email}
                                onChange={setEmail}
                                disabled={isSubmitting || openDialog}
                                onValidationChange={(error) => setErrors({email: error})}
                                shouldValidate={submitted}
                            />

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
                            <Caption1>
                                <Link
                                    onClick={() => navigate("/login")}
                                >
                                    Вернуться к входу
                                </Link>
                            </Caption1>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
};
