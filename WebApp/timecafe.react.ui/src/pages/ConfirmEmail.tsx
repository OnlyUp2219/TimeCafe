import {useEffect, useRef, useState} from "react";
import {useNavigate, useSearchParams} from "react-router-dom";
import {Button, Card, Spinner, Subtitle1, Text} from "@fluentui/react-components";
import {CheckmarkCircle24Filled, DismissCircle24Filled} from "@fluentui/react-icons";
import {confirmEmail} from "../api/auth.ts";

export const ConfirmEmail = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const [status, setStatus] = useState<"loading" | "confirmed" | "already_confirmed" | "error">("loading");
    const [errorMessage, setErrorMessage] = useState<string>("");
    const hasCalledRef = useRef(false);

    const userId = searchParams.get("userId");
    const token = searchParams.get("token");

    useEffect(() => {
        if (hasCalledRef.current) return;

        const confirm = async () => {
            if (!userId || !token) {
                setStatus("error");
                setErrorMessage("Неверная ссылка подтверждения");
                return;
            }

            hasCalledRef.current = true;

            try {
                const data = await confirmEmail(userId, token);

                if (data.status === "confirmed") {
                    setStatus("confirmed");
                } else if (data.status === "already_confirmed") {
                    setStatus("already_confirmed");
                }
            } catch (err) {
                setStatus("error");
                setErrorMessage(err instanceof Error ? err.message : "Ссылка недействительна или истекла");
            }
        };

        confirm();
    }, [userId, token]);

    const handleResend = () => {
        navigate("/register");
    };

    return (
        <Card className="auth_card">
            {status === "loading" && (
                <div style={{textAlign: "center"}}>
                    <Spinner size="large" label="Подтверждение email..."/>
                </div>
            )}

            {status === "confirmed" && (
                <div className="flex flex-col gap-[12px] items-center">
                    <CheckmarkCircle24Filled/>
                    <Subtitle1>Email успешно подтверждён!</Subtitle1>
                    <Text>
                        Теперь вы можете войти в систему.
                    </Text>
                    <Button className="w-full" appearance="primary" onClick={() => navigate("/login")}>
                        Войти
                    </Button>
                </div>
            )}

            {status === "already_confirmed" && (
                <div className="flex flex-col gap-[12px] items-center">
                    <CheckmarkCircle24Filled/>
                    <Subtitle1>Email уже был подтверждён</Subtitle1>
                    <Text>
                        Вы можете войти в систему.
                    </Text>
                    <Button className="w-full" appearance="primary" onClick={() => navigate("/login")}>
                        Войти
                    </Button>
                </div>
            )}

            {status === "error" && (
                <div className="flex flex-col gap-[12px] items-center">
                    <DismissCircle24Filled/>
                    <Subtitle1>Ошибка подтверждения</Subtitle1>
                    <Text>
                        {errorMessage}
                    </Text>
                    <div className="button-action">
                        <Button className="flex-[1]" appearance="secondary" onClick={handleResend}>
                            Повторить регистрацию
                        </Button>
                        <Button className="flex-[1]" appearance="primary" onClick={() => navigate("/login")}>
                            На страницу входа
                        </Button>
                    </div>
                </div>
            )}
        </Card>
    );
};
