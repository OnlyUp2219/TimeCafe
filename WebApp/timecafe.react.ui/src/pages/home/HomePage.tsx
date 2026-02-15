import {useEffect, useMemo, useState} from "react";
import {
    Badge,
    Body2,
    Button,
    Caption1,
    Divider,
    LargeTitle,
    Subtitle2Stronger,
    Tag,
    Text,
    Tooltip,
    Title3,
} from "@fluentui/react-components";
import {
    ArrowTrendingLines20Regular,
    Clock20Regular,
    Money20Regular,
    PersonRegular,
    Sparkle20Regular,
} from "@fluentui/react-icons";
import {useNavigate} from "react-router-dom";
import {useDispatch, useSelector} from "react-redux";
import type {AppDispatch, RootState} from "@store";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import vortex from "@assets/vvvortex.svg";
import repeat from "@assets/rrrepeat (2).svg";
import surf from "@assets/sssurf.svg";
import {formatMoneyByN} from "@utility/formatMoney";
import {formatDurationSeconds} from "@utility/formatDurationSeconds";
import {calcVisitEstimate} from "@utility/visitEstimate";
import {loadActiveVisitByUser, VisitUiStatus} from "@store/visitSlice";

export const HomePage = () => {
    const navigate = useNavigate();
    const dispatch = useDispatch<AppDispatch>();

    const authEmail = useSelector((state: RootState) => state.auth.email);
    const userId = useSelector((state: RootState) => state.auth.userId);
    const emailConfirmed = useSelector((state: RootState) => state.auth.emailConfirmed);
    const phoneConfirmed = useSelector((state: RootState) => state.auth.phoneNumberConfirmed);
    const profile = useSelector((state: RootState) => state.profile.data);
    const visitStatus = useSelector((state: RootState) => state.visit.status);
    const activeVisit = useSelector((state: RootState) => state.visit.activeVisit);

    const [now, setNow] = useState(() => Date.now());

    useEffect(() => {
        if (!userId) return;
        void dispatch(loadActiveVisitByUser({userId}));
    }, [dispatch, userId]);

    useEffect(() => {
        if (visitStatus !== VisitUiStatus.Active || !activeVisit) return;
        const id = window.setInterval(() => setNow(Date.now()), 1000);
        return () => window.clearInterval(id);
    }, [activeVisit, visitStatus]);

    const activeElapsedSeconds = useMemo(() => {
        if (visitStatus !== VisitUiStatus.Active || !activeVisit) return 0;
        return Math.max(0, Math.floor((now - activeVisit.startedAtMs) / 1000));
    }, [activeVisit, now, visitStatus]);

    const activeElapsedMinutes = useMemo(
        () => Math.max(1, Math.ceil(activeElapsedSeconds / 60)),
        [activeElapsedSeconds]
    );

    const activeEstimate = useMemo(() => {
        if (visitStatus !== VisitUiStatus.Active || !activeVisit) return null;
        return calcVisitEstimate(
            activeElapsedMinutes,
            activeVisit.tariff.billingType,
            activeVisit.tariff.pricePerMinute
        );
    }, [activeElapsedMinutes, activeVisit, visitStatus]);

    const displayName = useMemo(() => {
        const firstName = profile?.firstName?.trim();
        if (firstName) return firstName;
        const email = authEmail?.trim();
        if (email) return email;
        return "";
    }, [authEmail, profile?.firstName]);

    const demo = useMemo(
        () => ({
            balance: "3 500 ₽",
            balanceHint: "Доступно для оплаты визитов",
            visitDuration: "—:—",
            visitEstimate: "— Br",
            weekSpent: "1 240 ₽",
            weekSpentHint: "Расходы за 7 дней (demo)",
        }),
        []
    );

    return (
        <div className="relative tc-noise-overlay w-full h-full overflow-hidden">
            <div className="pointer-events-none absolute inset-0 overflow-hidden">
                <img
                    src={vortex}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[10vw] -left-[10vw] w-[50vw] max-w-[640px] select-none"
                    style={{opacity: 0.30}}
                    draggable={false}
                />
                <img
                    src={repeat}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[6vw] -right-[8vw] w-[40vw] max-w-[520px] select-none"
                    style={{opacity: 0.30}}
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[16vw] left-0 w-[100vw] max-w-none select-none"
                    style={{opacity: 0.30}}
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[16vw] right-0 -scale-x-100 w-[100vw] max-w-none select-none"
                    style={{opacity: 0.30}}
                    draggable={false}
                />
            </div>

            <div className="relative mx-auto w-full max-w-6xl px-2 py-4 sm:px-3 sm:py-6">
                <div
                    className="flex flex-col gap-4 overflow-hidden rounded-3xl p-5 sm:p-8"
                >
                    <div className="flex flex-col gap-4 sm:justify-between ">
                        <div className="flex flex-col gap-2 min-w-0">
                            <div className="flex flex-wrap items-center gap-2">
                                <Tag appearance="brand" icon={<Sparkle20Regular/>}>Дашборд</Tag>
                                <Badge appearance="tint" size="large">Home2</Badge>
                            </div>

                            <LargeTitle
                                truncate wrap={false} block
                            >
                                {displayName ? `Привет, ${displayName}` : "Привет"}
                            </LargeTitle>
                        </div>

                        <div className="flex flex-col gap-2 sm:flex-row sm:justify-between sm:items-end">

                            <div className="flex flex-col gap-2 ">
                                <Body2>
                                    Короткий обзор: баланс, визит и быстрые действия.
                                </Body2>

                                <div className="flex flex-wrap gap-2 ">
                                    <Tag appearance={emailConfirmed ? "brand" : "outline"}>
                                        {emailConfirmed ? "Email подтверждён" : "Email не подтверждён"}
                                    </Tag>
                                    <Tag appearance={phoneConfirmed ? "brand" : "outline"}>
                                        {phoneConfirmed ? "Телефон подтверждён" : "Телефон не подтверждён"}
                                    </Tag>
                                </div>
                            </div>

                            <div className="flex flex-row gap-2 items-end shrink-0">
                                <Button appearance="primary" onClick={() => navigate("/personal-data")}>
                                    <Text truncate wrap={false}>
                                        Персональные данные
                                    </Text>
                                </Button>
                                <Button appearance="secondary" onClick={() => navigate("/personal-data")}>
                                    Смена пароля
                                </Button>
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                        <HoverTiltCard className="flex flex-col justify-between">
                            <div className="flex flex-col gap-4">
                                <div className="flex items-center gap-2 flex-wrap">
                                    <div className="flex items-center gap-2">
                                        <Money20Regular/>
                                        <Subtitle2Stronger>Баланс</Subtitle2Stronger>
                                    </div>
                                    <Badge appearance="tint">Demo</Badge>
                                </div>
                                <Divider className="divider grow-0"/>
                            </div>

                            <div className="flex items-end justify-between gap-3 flex-wrap">
                                <div className="flex flex-col gap-1">
                                    <Title3>{demo.balance}</Title3>
                                    <Caption1>{demo.balanceHint}</Caption1>
                                </div>
                                <Button appearance="secondary" disabled>
                                    Операции (скоро)
                                </Button>
                            </div>
                        </HoverTiltCard>

                        <HoverTiltCard className="flex flex-col justify-between">
                            <div className="flex flex-col gap-4">
                                <div className="flex items-center gap-2 flex-wrap">
                                    <div className="flex items-center gap-2">
                                        <Clock20Regular/>
                                        <Subtitle2Stronger>Визит</Subtitle2Stronger>
                                    </div>
                                    <Tag size="small" appearance={visitStatus === VisitUiStatus.Active ? "brand" : "outline"}>
                                        {visitStatus === VisitUiStatus.Active ? "Активен" : "Нет визита"}
                                    </Tag>
                                </div>
                                <Divider className="divider grow-0"/>
                            </div>

                            <div className="flex flex-row justify-between gap-4 flex-wrap">
                                <div className="flex flex-col gap-1">
                                    <Caption1>Длительность</Caption1>
                                    <Title3>
                                        {visitStatus === VisitUiStatus.Active && activeVisit
                                            ? formatDurationSeconds(activeElapsedSeconds)
                                            : demo.visitDuration}
                                    </Title3>
                                </div>
                                <div className="flex flex-col gap-1 text-right">
                                    <Caption1>Оценка</Caption1>
                                    <Title3>
                                        {visitStatus === VisitUiStatus.Active && activeEstimate
                                            ? formatMoneyByN(activeEstimate.total)
                                            : demo.visitEstimate}
                                    </Title3>
                                </div>
                            </div>

                            <Caption1>
                                {visitStatus === VisitUiStatus.Active && activeVisit
                                    ? "Визит идёт"
                                    : "Нет активного визита"}
                            </Caption1>

                            <div className="flex flex-col gap-2 lg:flex-row">
                                <Tooltip
                                    content={visitStatus === VisitUiStatus.Active ? "Перейти к активному визиту" : "Начать визит"}
                                    relationship="label"
                                >
                                    <Button
                                        appearance="primary"
                                        data-testid="home-visit-action"
                                        onClick={() =>
                                            navigate(visitStatus === VisitUiStatus.Active ? "/visit/active" : "/visit/start")
                                        }
                                    >
                                        <Text truncate wrap={false}>
                                            {visitStatus === VisitUiStatus.Active ? "Открыть" : "Начать"}
                                        </Text>
                                    </Button>
                                </Tooltip>

                                <Tooltip content="История визитов (скоро)" relationship="label">
                                    <span>
                                        <Button appearance="secondary" disabled>
                                            <Text truncate wrap={false}>История визитов (скоро)</Text>
                                        </Button>
                                    </span>
                                </Tooltip>
                            </div>
                        </HoverTiltCard>

                        <HoverTiltCard className="flex flex-col justify-between">
                            <div className="flex flex-col gap-4">
                                <div className="flex items-center gap-2 flex-wrap">
                                    <div className="flex items-center gap-2">
                                        <ArrowTrendingLines20Regular/>
                                        <Subtitle2Stronger>Неделя</Subtitle2Stronger>
                                    </div>
                                    <Badge appearance="outline">Demo</Badge>
                                </div>
                                <Divider className="divider grow-0"/>
                            </div>

                            <div className="flex flex-col gap-1">
                                <Title3>{demo.weekSpent}</Title3>
                                <Caption1>{demo.weekSpentHint}</Caption1>
                            </div>
                            <div className="mt-4">
                                <Button appearance="secondary" className="w-full" disabled>
                                    Посмотреть (скоро)
                                </Button>
                            </div>
                        </HoverTiltCard>
                    </div>

                    <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                        <HoverTiltCard className="flex flex-col justify-between lg:col-span-7">
                            <div className="flex flex-col gap-4">
                                <div className="flex items-center gap-2 flex-wrap justify-between">
                                    <div className="flex items-center gap-2">
                                        <PersonRegular/>
                                        <Subtitle2Stronger>Профиль</Subtitle2Stronger>
                                    </div>
                                    <Tag appearance={profile ? "brand" : "outline"}>
                                        {profile ? "Заполнен" : "Не заполнен"}
                                    </Tag>
                                </div>
                                <Divider className="divider grow-0"/>
                            </div>

                            {profile ? (
                                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                                    <div className="flex flex-col gap-1 min-w-0">
                                        <Caption1>ФИО</Caption1>
                                        <Title3>{`${profile.lastName} ${profile.firstName}${profile.middleName ? ` ${profile.middleName}` : ""}`}</Title3>
                                    </div>
                                    <div className="flex flex-col gap-1 sm:text-right min-w-0">
                                        <Caption1>Контакты</Caption1>
                                        <Body2>{profile.email ?? "—"}</Body2>
                                        <Caption1>
                                            {profile.phoneNumber ?? "Телефон не указан"}
                                        </Caption1>
                                    </div>
                                </div>
                            ) : (
                                <div className="flex flex-col gap-2">
                                    <Body2>
                                        Заполните профиль, чтобы продолжить пользовательский сценарий.
                                    </Body2>
                                    <div className="flex flex-col gap-2 sm:flex-row sm:flex-nowrap sm:min-w-0">
                                        <Button appearance="primary" onClick={() => navigate("/personal-data")}>
                                            <Text truncate wrap={false}>
                                                Заполнить профиль
                                            </Text>
                                        </Button>
                                        <Button appearance="secondary" disabled>
                                            Настройки (скоро)
                                        </Button>
                                    </div>
                                </div>
                            )}
                        </HoverTiltCard>

                        <HoverTiltCard className="flex flex-col justify-between lg:col-span-5">
                            <div className="flex flex-col gap-4">
                                <div className="flex items-center gap-2 flex-wrap justify-between">
                                    <Subtitle2Stronger>Быстрые действия</Subtitle2Stronger>
                                    <Tag appearance="outline">Shortcuts</Tag>
                                </div>
                                <Divider className="divider grow-0"/>
                            </div>

                            <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
                                <Button appearance="primary" onClick={() => navigate("/personal-data")}>
                                    <Text truncate wrap={false}>
                                        Профиль
                                    </Text>
                                </Button>
                                <Button appearance="secondary" onClick={() => navigate("/personal-data")}>
                                    Безопасность
                                </Button>
                                <Button appearance="secondary" disabled>
                                    <Text truncate wrap={false}>
                                        Баланс (скоро)
                                    </Text>
                                </Button>
                                <Button appearance="secondary" disabled>
                                    Операции (скоро)
                                </Button>
                            </div>
                        </HoverTiltCard>
                    </div>
                </div>
            </div>
        </div>
    );
};
