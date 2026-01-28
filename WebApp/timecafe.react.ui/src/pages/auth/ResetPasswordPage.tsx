import {
    Body2,
    Title3,
    Badge,
    Dialog,
    DialogContent,
    DialogBody,
    DialogSurface,
    DialogTitle,
    DialogActions
} from '@fluentui/react-components';
import {Spinner} from '@fluentui/react-components';
import {MailCheckmark20Filled} from '@fluentui/react-icons';
import {useState} from "react";
import {useNavigate} from "react-router-dom";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";
import {EmailInput} from "../../components/FormFields";
import {authFormContainerClassName} from "../../layouts/authLayout";
import {authApi} from "../../shared/api/auth/authApi";
import {MockCallbackLink} from "../../components/MockCallbackLink/MockCallbackLink";
import {TooltipButton} from "../../components/TooltipButton/TooltipButton";

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
    const USE_MOCK_EMAIL = import.meta.env.VITE_USE_MOCK_EMAIL === 'true';

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(true);
        if (errors.email || !email) return;

        setIsSubmitting(true);
        try {
            const res = await authApi.forgotPasswordLink({email});

            if (res.status === 429) {
                const retryAfter = res.headers.retryAfter;
                const msg = retryAfter
                    ? `Слишком часто. Повторите через ${retryAfter} сек.`
                    : "Слишком часто. Попробуйте позже.";
                showToast(msg, "error");
                return;
            }

            setCallbackUrl(res.data?.callbackUrl);
            setSentEmail(email);
            setOpenDialog(true);
        } catch (err: unknown) {
            if (err && typeof err === "object" && "message" in err && typeof (err as { message?: unknown }).message === "string") {
                showToast((err as { message: string }).message, "error");
                return;
            }

            if (err && typeof err === "object" && "errors" in err && Array.isArray((err as { errors?: unknown }).errors)) {
                const items = (err as { errors: Array<{ message?: string; description?: string }> }).errors;
                const message = items
                    .map(e => e.message || e.description)
                    .filter(Boolean)
                    .join(" ");
                showToast(message || "Ошибка. Попробуйте позже", "error");
                return;
            }

            showToast("Ошибка. Попробуйте позже", "error");
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
                            <div className="flex items-center gap-2">
                                <Badge appearance="tint" shape="rounded" size="extra-large" className="dark-green">
                                    <MailCheckmark20Filled />
                                </Badge>
                                <span>Письмо отправлено</span>
                            </div>
                        </DialogTitle>
                        <DialogContent>
                            <Body2 block className="mb-3">Мы отправили письмо на
                                почту <strong>{sentEmail}</strong></Body2>
                            <Body2 block>Перейдите на почту и нажмите ссылку для сброса пароля</Body2>
                            {USE_MOCK_EMAIL && callbackUrl && <MockCallbackLink url={callbackUrl}/>}
                        </DialogContent>
                        <DialogActions className="w-full">
                            <div className="grid w-full grid-cols-1 gap-[12px] sm:grid-cols-2">
                                {USE_MOCK_EMAIL && callbackUrl && (
                                    <TooltipButton
                                        appearance="primary"
                                        onClick={handleOpenCallbackUrl}
                                        tooltip="Открыть ссылку сброса пароля (mock)"
                                        label="Открыть ссылку"
                                        className="w-full order-1 sm:order-2"
                                    />
                                )}

                                <TooltipButton
                                    appearance={USE_MOCK_EMAIL && callbackUrl ? "secondary" : "primary"}
                                    onClick={handleGoToEmail}
                                    tooltip="Открыть почту"
                                    label="На почту"
                                    className={USE_MOCK_EMAIL && callbackUrl ? "w-full order-2 sm:order-1" : "w-full order-1 sm:order-2"}
                                />

                                <TooltipButton
                                    appearance="secondary"
                                    onClick={handleGoToLogin}
                                    tooltip="Перейти на страницу входа"
                                    label="К входу"
                                    className={USE_MOCK_EMAIL && callbackUrl ? "w-full order-3 sm:order-3" : "w-full order-2 sm:order-1"}
                                />
                            </div>
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

                            <div className="grid grid-cols-1 gap-[12px] mt-4 sm:grid-cols-2">
                                <TooltipButton
                                    appearance="primary"
                                    type="submit"
                                    disabled={isSubmitting || openDialog}
                                    className="w-full order-1 sm:order-2"
                                    icon={isSubmitting ? <Spinner size="tiny" /> : undefined}
                                    tooltip="Отправить письмо для сброса"
                                    label="Отправить код"
                                />

                                <TooltipButton
                                    appearance="secondary"
                                    type="button"
                                    disabled={isSubmitting || openDialog}
                                    className="w-full order-2 sm:order-1"
                                    onClick={() => navigate("/login")}
                                    tooltip="Вернуться на страницу входа"
                                    label="Вернуться к входу"
                                />
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </>
    );
};
