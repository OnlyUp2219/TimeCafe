import {useEffect, useMemo, useRef, useState} from "react";
import {useNavigate, useSearchParams} from "react-router-dom";
import {Spinner, Title2, Body1, Body2, Badge} from "@fluentui/react-components";
import {useAppDispatch} from "@store/hooks";
import {useConfirmEmailMutation, useConfirmEmailChangeMutation} from "@store/api/authApi";
import {setEmail, setEmailConfirmed} from "@store/authSlice";
import {authFormContainerClassName} from "@layouts/AuthLayout/authLayout.styles";
import {TooltipButton} from "@components/TooltipButton/TooltipButton";
import {AuthHero} from "@components/AuthHero/AuthHero";
import {CheckmarkCircle24Regular, ErrorCircle24Regular} from "@fluentui/react-icons";
import {useComponentSize} from "@hooks/useComponentSize";

type ViewState =
    | {status: "loading"}
    | {status: "success"; message: string}
    | {status: "error"; message: string};

export const ConfirmEmailPage = () => {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const {sizes} = useComponentSize();

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
                    setState({status: "success", message: r.message || "Почта успешно изменена и подтверждена"});
                } else {
                    const r = await confirmEmailMutation({userId, token}).unwrap();
                    dispatch(setEmailConfirmed(true));
                    setState({status: "success", message: r.message || "Почта успешно подтверждена"});
                }
            } catch (err: unknown) {
                const message = (err && typeof err === "object" && "data" in err)
                    ? String((err as { data?: { error?: string } }).data?.error ?? "Ошибка при подтверждении почты")
                    : "Ошибка при подтверждении почты";
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
                subtitle="Завершение создания вашего аккаунта в TimeCafe."
            />

            <div id="Form" className={authFormContainerClassName}>
                <div className="flex flex-col w-full max-w-md gap-6">
                    {state.status === "loading" && (
                        <div className="flex flex-col items-center text-center gap-4">
                            <Spinner size="huge" label="Входим в ритм TimeCafe... ☕" labelPosition="below" />
                            <Body2 className="text-(--colorNeutralForeground2) max-w-xs">
                                Активируем ваш доступ. Пожалуйста, не закрывайте страницу, это займет всего мгновение.
                            </Body2>
                        </div>
                    )}

                    {state.status === "success" && (
                        <div className="flex flex-col items-center text-center gap-6">
                            <Badge appearance="tint" color="success" size="extra-large" shape="rounded">
                                <CheckmarkCircle24Regular />
                            </Badge>
                            <div className="flex flex-col gap-2">
                                <Title2 className="font-semibold">Рады приветствовать вас в TimeCafe! ✨</Title2>
                                <Body1 className="text-(--colorNeutralForeground2)">
                                    Ваша почта успешно подтверждена. Теперь перед вами открыты все возможности нашего пространства! Управляйте балансом, накапливайте персональные скидки и бронируйте любимые места. Начнем знакомство?
                                </Body1>
                            </div>
                            <TooltipButton
                                appearance="primary"
                                onClick={() => navigate("/login", {replace: true})}
                                tooltip="Перейти к авторизации"
                                label="Войти и начать"
                                className="w-full"
                                size={sizes.button}
                            />
                        </div>
                    )}

                    {state.status === "error" && (
                        <div className="flex flex-col items-center text-center gap-6">
                            <Badge appearance="tint" color="danger" size="extra-large" shape="rounded">
                                <ErrorCircle24Regular />
                            </Badge>
                            <div className="flex flex-col gap-2">
                                <Title2 className="font-semibold">Упс! Что-то пошло не так 😔</Title2>
                                <Body1 className="text-(--colorNeutralForeground2)">
                                    Не удалось подтвердить почту: {state.message}. Ссылка могла устареть (она активна 24 часа) или уже была активирована.
                                </Body1>
                            </div>
                            <div className="flex flex-col sm:flex-row gap-3 w-full">
                                <TooltipButton
                                    appearance="primary"
                                    onClick={() => navigate("/email-pending", {replace: true})}
                                    tooltip="Повторно отправить письмо подтверждения"
                                    label="Отправить ссылку еще раз"
                                    className="flex-1"
                                    size={sizes.button}
                                />
                                <TooltipButton
                                    appearance="secondary"
                                    onClick={() => navigate("/login", {replace: true})}
                                    tooltip="Перейти на страницу входа"
                                    label="На страницу входа"
                                    className="flex-1"
                                    size={sizes.button}
                                />
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};
