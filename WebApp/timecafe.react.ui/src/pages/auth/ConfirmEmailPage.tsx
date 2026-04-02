import {useEffect, useMemo, useRef, useState} from "react";
import {useNavigate, useSearchParams} from "react-router-dom";
import {Spinner, Subtitle1} from "@fluentui/react-components";
import {useAppDispatch} from "@store/hooks";
import {useConfirmEmailMutation, useConfirmEmailChangeMutation} from "@store/api/authApi";
import {setEmail, setEmailConfirmed} from "@store/authSlice";
import {authFormContainerClassName} from "@layouts/AuthLayout/authLayout.styles";
import {TooltipButton} from "@components/TooltipButton/TooltipButton";
import {AuthHero} from "@components/AuthHero/AuthHero";

type ViewState =
    | {status: "loading"}
    | {status: "success"; message: string}
    | {status: "error"; message: string};

export const ConfirmEmailPage = () => {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();

    const userId = useMemo(() => searchParams.get("userId") || "", [searchParams]);
    const token = useMemo(() => {
        const raw = searchParams.get("token") || "";
        return raw.replace(/ /g, "+");
    }, [searchParams]);
    const newEmail = useMemo(() => searchParams.get("newEmail") || "", [searchParams]);

    const [confirmEmailMutation] = useConfirmEmailMutation();
    const [confirmEmailChangeMutation] = useConfirmEmailChangeMutation();

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

            try {
                if (newEmail) {
                    const r = await confirmEmailChangeMutation({userId, newEmail, token}).unwrap();
                    dispatch(setEmail(newEmail));
                    dispatch(setEmailConfirmed(true));
                    setState({status: "success", message: r.message || "Почта изменена и подтверждена"});
                } else {
                    const r = await confirmEmailMutation({userId, token}).unwrap();
                    dispatch(setEmailConfirmed(true));
                    setState({status: "success", message: r.message || "Почта подтверждена"});
                }
            } catch (err: unknown) {
                const message = (err && typeof err === "object" && "data" in err)
                    ? String((err as { data?: { error?: string } }).data?.error ?? "Ошибка подтверждения")
                    : "Ошибка подтверждения";
                setState({status: "error", message});
            }
        };

        void run();
    }, [confirmEmailChangeMutation, confirmEmailMutation, dispatch, newEmail, token, userId]);

    return (
        <div
            className="!grid grid-cols-1 items-center justify-center
             sm:grid-cols-2 sm:justify-stretch sm:items-stretch">
            <AuthHero
                title="Подтверждение почты"
                subtitle="Проверяем вашу ссылку подтверждения."
            />

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
