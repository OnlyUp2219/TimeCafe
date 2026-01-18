import {useMemo} from "react";
import {
    Badge,
    Body2,
    Caption1,
    LargeTitle,
    Subtitle2Stronger,
    Title3,
    Button,
    Divider,
    Tag,
    tokens,
    Text,
} from "@fluentui/react-components";
import {
    ArrowTrendingLines20Regular,
    Clock20Regular,
    Money20Regular,
    Person20Regular,
    Sparkle20Regular,
} from "@fluentui/react-icons";
import {useNavigate} from "react-router-dom";
import {useSelector} from "react-redux";
import type {RootState} from "../../store";
import {HoverTiltCard} from "../../components/HoverTiltCard/HoverTiltCard";
import vortex from "../../assets/vvvortex.svg";
import repeat from "../../assets/rrrepeat (2).svg";
import surf from "../../assets/sssurf.svg";

export const HomePage = () => {
    const navigate = useNavigate();

    const authEmail = useSelector((state: RootState) => state.auth.email);
    const emailConfirmed = useSelector((state: RootState) => state.auth.emailConfirmed);
    const client = useSelector((state: RootState) => state.client.data);

    const displayName = useMemo(() => {
        const firstName = client?.firstName?.trim();
        if (firstName) return firstName;
        const email = authEmail?.trim();
        if (email) return email;
        return "";
    }, [authEmail, client?.firstName]);

    const demo = useMemo(
        () => ({
            balance: "3 500 ₽",
            balanceHint: "Доступно для оплаты визитов",
            visitDuration: "—:—",
            visitEstimate: "— ₽",
            visitHint: "Нет активного визита",
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
                    style={{
                        backgroundImage: `radial-gradient(900px 480px at 20% 10%, ${tokens.colorBrandBackground2} 0%, transparent 60%), radial-gradient(720px 420px at 90% 0%, ${tokens.colorPaletteLightGreenBackground2} 0%, transparent 55%), linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
                        border: `1px solid ${tokens.colorNeutralStroke1}`,
                        boxShadow: tokens.shadow16,
                    }}
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
                                    <Tag appearance={client?.phoneNumberConfirmed ? "brand" : "outline"}>
                                        {client?.phoneNumberConfirmed ? "Телефон подтверждён" : "Телефон не подтверждён"}
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
                                    <Tag size="small" appearance="outline">Нет визита</Tag>
                                </div>
                                <Divider className="divider grow-0"/>
                            </div>

                            <div className="flex flex-row justify-between gap-4 flex-wrap">
                                <div className="flex flex-col gap-1">
                                    <Caption1>Длительность</Caption1>
                                    <Title3>{demo.visitDuration}</Title3>
                                </div>
                                <div className="flex flex-col gap-1 text-right">
                                    <Caption1>Оценка</Caption1>
                                    <Title3>{demo.visitEstimate}</Title3>
                                </div>
                            </div>

                            <Caption1>
                                {demo.visitHint}
                            </Caption1>

                            <div className="flex flex-col gap-2 lg:flex-row">
                                <Button appearance="primary" disabled>
                                    Начать (скоро)
                                </Button>
                                <Button appearance="secondary" disabled>
                                    <Text truncate wrap={false}>
                                        История визитов (скоро)
                                    </Text>
                                </Button>
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
                                        <Person20Regular/>
                                        <Subtitle2Stronger>Профиль</Subtitle2Stronger>
                                    </div>
                                    <Tag appearance={client ? "brand" : "outline"}>
                                        {client ? "Заполнен" : "Не заполнен"}
                                    </Tag>
                                </div>
                                <Divider className="divider grow-0"/>
                            </div>

                            {client ? (
                                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                                    <div className="flex flex-col gap-1 min-w-0">
                                        <Caption1>ФИО</Caption1>
                                        <Title3>{`${client.lastName} ${client.firstName}${client.middleName ? ` ${client.middleName}` : ""}`}</Title3>
                                    </div>
                                    <div className="flex flex-col gap-1 sm:text-right min-w-0">
                                        <Caption1>Контакты</Caption1>
                                        <Body2>{client.email}</Body2>
                                        <Caption1>
                                            {client.phoneNumber ?? "Телефон не указан"}
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
