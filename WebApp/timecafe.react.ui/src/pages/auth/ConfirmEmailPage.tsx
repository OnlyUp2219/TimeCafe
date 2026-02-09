import {useEffect, useMemo, useRef, useState} from "react";
import {useNavigate, useSearchParams} from "react-router-dom";
import {Spinner, Subtitle1} from "@fluentui/react-components";
import {useDispatch} from "react-redux";
import {authApi} from "@api/auth/authApi";
import {setEmail, setEmailConfirmed} from "@store/authSlice";
import {authFormContainerClassName} from "@layouts/authLayout";
import {TooltipButton} from "@components/TooltipButton/TooltipButton";

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
    const newEmail = useMemo(() => searchParams.get("newEmail") || "", [searchParams]);

    const [state, setState] = useState<ViewState>({status: "loading"});
    const didConfirmRef = useRef(false);

    useEffect(() => {
        if (didConfirmRef.current) return;
        didConfirmRef.current = true;

        const run = async () => {
            if (!userId || !token) {
                setState({status: "error", message: "Некорректная ссылка подтверждения"});
                return;
            }

            if (newEmail) {
                const r = await authApi.confirmEmailChange(userId, newEmail, token);
                if (r.error) {
                    setState({status: "error", message: r.error});
                    return;
                }

                dispatch(setEmail(newEmail));
                dispatch(setEmailConfirmed(true));
                setState({status: "success", message: r.message || "Почта изменена и подтверждена"});
            } else {
                const r = await authApi.confirmEmail(userId, token);
                if (r.error) {
                    setState({status: "error", message: r.error});
                    return;
                }

                dispatch(setEmailConfirmed(true));
                setState({status: "success", message: r.message || "Почта подтверждена"});
            }
        };

        void run();
    }, [dispatch, newEmail, token, userId]);

    return (
        <div
            className="!grid grid-cols-1 items-center justify-center
             sm:grid-cols-2 sm:justify-stretch sm:items-stretch">
            <div id="Left Side" className="relative hidden sm:block bg-[url(/src/assets/abstract_bg.svg)] bg-left bg-cover bg-no-repeat">
                <div className="absolute inset-0 bg-black/40 pointer-events-none" />
            </div>

            <div id="Form" className={authFormContainerClassName}>
                <div className="flex flex-col w-full max-w-md gap-[12px]">
                    {state.status === "loading" && (
                        <div className="flex flex-col items-center gap-[12px]">
                            <Spinner size="huge" />
                            <Subtitle1>Подтверждаем почту…</Subtitle1>
                        </div>
                    )}

                    {state.status === "success" && (
                        <div className="flex flex-col items-center gap-[12px]">
                            <Subtitle1>{state.message}</Subtitle1>
                            <TooltipButton
                                appearance="primary"
                                onClick={() => navigate("/login", {replace: true})}
                                tooltip="Перейти на страницу входа"
                                label="Перейти ко входу"
                                className="w-full sm:w-auto"
                            />
                        </div>
                    )}

                    {state.status === "error" && (
                        <div className="flex flex-col items-center gap-[12px]">
                            <Subtitle1>{state.message}</Subtitle1>
                            <div className="grid grid-cols-1 gap-[12px] w-full sm:grid-cols-2">
                                <TooltipButton
                                    appearance="primary"
                                    onClick={() => navigate("/email-pending", {replace: true})}
                                    tooltip="Повторно отправить письмо подтверждения"
                                    label="Отправить ещё раз"
                                    className="w-full"
                                />
                                <TooltipButton
                                    appearance="secondary"
                                    onClick={() => navigate("/login", {replace: true})}
                                    tooltip="Перейти на страницу входа"
                                    label="Вход"
                                    className="w-full"
                                />
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};
