import {useEffect, useMemo, useState} from "react";
import {useNavigate, useSearchParams} from "react-router-dom";
import {Button, Card, Spinner, Subtitle1} from "@fluentui/react-components";
import {useDispatch} from "react-redux";
import {authApi} from "../../shared/api/auth/authApi";
import {setEmailConfirmed} from "../../store/authSlice";

type ViewState =
    | {status: "loading"}
    | {status: "success"; message: string}
    | {status: "error"; message: string};

export const ConfirmEmailPage = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();

    const userId = useMemo(() => searchParams.get("userId") || "", [searchParams]);
    const token = useMemo(() => {
        const raw = searchParams.get("token") || "";
        return raw.replace(/ /g, "+");
    }, [searchParams]);

    const [state, setState] = useState<ViewState>({status: "loading"});

    useEffect(() => {
        const run = async () => {
            if (!userId || !token) {
                setState({status: "error", message: "Некорректная ссылка подтверждения"});
                return;
            }

            const r = await authApi.confirmEmail(userId, token);
            if (r.error) {
                setState({status: "error", message: r.error});
                return;
            }

            dispatch(setEmailConfirmed(true));
            setState({status: "success", message: r.message || "Почта подтверждена"});
        };

        void run();
    }, [dispatch, token, userId]);

    return (
        <div className="flex items-center justify-center">
            <Card className="auth_card">
                {state.status === "loading" && (
                    <div className="flex flex-col items-center gap-3">
                        <Spinner size="huge" />
                        <Subtitle1>Подтверждаем почту…</Subtitle1>
                    </div>
                )}

                {state.status === "success" && (
                    <div className="flex flex-col items-center gap-3">
                        <Subtitle1>{state.message}</Subtitle1>
                        <Button appearance="primary" onClick={() => navigate("/login", {replace: true})}>
                            Перейти ко входу
                        </Button>
                    </div>
                )}

                {state.status === "error" && (
                    <div className="flex flex-col items-center gap-3">
                        <Subtitle1>{state.message}</Subtitle1>
                        <div className="flex gap-2">
                            <Button appearance="primary" onClick={() => navigate("/email-pending", {replace: true})}>
                                Отправить письмо ещё раз
                            </Button>
                            <Button appearance="secondary" onClick={() => navigate("/login", {replace: true})}>
                                Вход
                            </Button>
                        </div>
                    </div>
                )}
            </Card>
        </div>
    );
};
