import {
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    Divider,
    LargeTitle,
    Subtitle2Stronger,
    Tag,
    Text,
    Title2,
    tokens,
} from "@fluentui/react-components";
import {
    Money20Regular,
    Wallet20Regular,
    Warning20Regular,
} from "@fluentui/react-icons";
import { DataTable } from "../../components/DataTable/DataTable";
import { HoverTiltCard } from "../../components/HoverTiltCard/HoverTiltCard";
import vortex from "../../assets/vvvortex.svg";
import repeat from "../../assets/rrrepeat (2).svg";
import surf from "../../assets/sssurf.svg";
import { useMemo } from "react";

// Тип для строки транзакции (можно вынести в types/billing.ts позже)
type Transaction = {
    date: string;
    type: string;
    amount: number;
    balanceAfter: number;
    comment: string;
};

const columns = [
    {
        columnId: "date",
        renderHeaderCell: () => "Дата",
        renderCell: (item: Transaction) => item.date,
    },
    {
        columnId: "type",
        renderHeaderCell: () => "Тип",
        renderCell: (item: Transaction) => (
            <Tag appearance={item.amount > 0 ? "brand" : "outline"} size="small">
                {item.type}
            </Tag>
        ),
    },
    {
        columnId: "amount",
        renderHeaderCell: () => "Сумма",
        renderCell: (item: Transaction) => (
            <Text
                weight="semibold"
                style={{
                    color: item.amount > 0 ? tokens.colorPaletteGreenForeground1 : undefined,
                }}
            >
                {item.amount > 0 ? "+" : ""}
                {item.amount.toLocaleString("ru-RU")} ₽
            </Text>
        ),
    },
    {
        columnId: "balanceAfter",
        renderHeaderCell: () => "Баланс после",
        renderCell: (item: Transaction) => `${item.balanceAfter.toLocaleString("ru-RU")} ₽`,
    },
    {
        columnId: "comment",
        renderHeaderCell: () => "Комментарий",
        renderCell: (item: Transaction) => (
            <Body2 style={{ color: tokens.colorNeutralForeground2 }}>{item.comment}</Body2>
        ),
    },
];




export const BalancePage = () => {
    // Демо-данные (в реальности — из Redux / API)
    const balance = 3500;
    const debt = 280;

    const transactions: Transaction[] = useMemo(
        () => [
            {
                date: "14.01.2026",
                type: "Визит",
                amount: -500,
                balanceAfter: 3500,
                comment: "Визит #3841, Стандартный, 2 ч 40 мин",
            },
            {
                date: "13.01.2026",
                type: "Пополнение",
                amount: 3000,
                balanceAfter: 4000,
                comment: "Stripe · Visa ****4242",
            },
            {
                date: "10.01.2026",
                type: "Визит",
                amount: -1200,
                balanceAfter: 1000,
                comment: "Ночной тариф · 4 ч 15 мин",
            },
            {
                date: "08.01.2026",
                type: "Бонус",
                amount: 200,
                balanceAfter: 2200,
                comment: "Пригласил друга",
            },
            {
                date: "05.01.2026",
                type: "Пополнение",
                amount: 1500,
                balanceAfter: 2000,
                comment: "СБП · Тинькофф",
            },
        ],
        []
    );

    return (
        <div className="tc-noise-overlay relative overflow-hidden min-h-full">
            <div className="pointer-events-none absolute inset-0 overflow-hidden">
                <img
                    src={vortex}
                    alt=""
                    className="absolute -top-[10vw] -left-[10vw] w-[50vw] max-w-[640px] opacity-25 select-none"
                    draggable={false}
                />
                <img
                    src={repeat}
                    alt=""
                    className="absolute -top-[6vw] -right-[8vw] w-[40vw] max-w-[520px] opacity-25 select-none"
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    className="absolute -bottom-[16vw] left-0 w-[100vw] opacity-25 select-none"
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    className="absolute -bottom-[16vw] right-0 -scale-x-100 w-[100vw] opacity-25 select-none"
                    draggable={false}
                />
            </div>

            <div className="relative mx-auto w-full max-w-6xl px-2 py-4 sm:px-3 sm:py-6">
                <div
                    className="rounded-3xl p-5 sm:p-8"
                    style={{
                        backgroundImage: `radial-gradient(900px 480px at 20% 10%, ${tokens.colorBrandBackground2} 0%, transparent 60%), radial-gradient(720px 420px at 90% 0%, ${tokens.colorPaletteLightGreenBackground2} 0%, transparent 55%), linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
                        border: `1px solid ${tokens.colorNeutralStroke1}`,
                        boxShadow: tokens.shadow16,
                    }}
                >
                    <div className="flex flex-col gap-5">
                        {/* Заголовок + действие */}
                        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                            <div className="flex items-center gap-3 flex-wrap">
                                <Tag appearance="brand" size="medium" icon={<Wallet20Regular />} >
                                    Баланс
                                </Tag>
                                <LargeTitle>Мой баланс</LargeTitle>
                            </div>
                            <Button appearance="primary" >
                                Пополнить
                            </Button>
                        </div>

                        <Divider />

                        {/* Баланс + долг */}
                        <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
                            <HoverTiltCard className="lg:col-span-2">
                                <div className="flex flex-col gap-6">
                                    <div>
                                        <Body2>Доступно сейчас</Body2>
                                        <LargeTitle style={{ fontSize: "3.8rem", lineHeight: "0.9" }}>
                                            {balance.toLocaleString("ru-RU")} ₽
                                        </LargeTitle>
                                    </div>

                                    {debt > 0 && (
                                        <Card
                                            style={{
                                                background: tokens.colorPaletteRedBackground2,
                                                borderColor: tokens.colorPaletteRedBorderActive,
                                            }}
                                        >
                                            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                                                <div className="flex items-center gap-3">
                                                    <Warning20Regular
                                                        fontSize={28}
                                                        color={tokens.colorPaletteRedForeground1}
                                                    />
                                                    <div>
                                                        <Title2>Задолженность</Title2>
                                                        <LargeTitle
                                                            style={{
                                                                color: tokens.colorPaletteRedForeground1,
                                                            }}
                                                        >
                                                            {debt.toLocaleString("ru-RU")} ₽
                                                        </LargeTitle>
                                                    </div>
                                                </div>
                                                <Button
                                                    appearance="primary"
                                                    style={{
                                                        backgroundColor: tokens.colorPaletteRedForeground1,
                                                        color: "white",
                                                    }}
                                                >
                                                    Погасить полностью
                                                </Button>
                                            </div>
                                        </Card>
                                    )}
                                </div>
                            </HoverTiltCard>

                            {/* Быстрые действия (правая колонка) */}
                            <HoverTiltCard>
                                <div className="flex flex-col gap-4">
                                    <Subtitle2Stronger>Действия</Subtitle2Stronger>
                                    <Divider />
                                    <div className="flex flex-col gap-3">
                                        <Button appearance="primary" icon={<Money20Regular />}>
                                            Пополнить баланс
                                        </Button>
                                        <Button appearance="secondary">История операций</Button>
                                        <Button appearance="secondary" disabled>
                                            Вывод средств (скоро)
                                        </Button>
                                    </div>
                                </div>
                            </HoverTiltCard>
                        </div>

                        {/* Таблица последних операций */}
                        <HoverTiltCard>
                            <div className="flex flex-col gap-4">
                                <div className="flex items-center justify-between flex-wrap gap-3">
                                    <Title2>Последние операции</Title2>
                                    <Button appearance="secondary" size="small">
                                        Все операции →
                                    </Button>
                                </div>

                                <Divider />
                  
                            </div>
                        </HoverTiltCard>
                    </div>
                </div>
            </div>
        </div>
    );
};