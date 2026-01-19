import {
    Badge,
    Button,
    Card,
    Divider,
    Tag,
    Title2,
    Title3,
    LargeTitle,
    Title1,
    Subtitle2Stronger,
    Body1,
    Body2,
    Caption1,
    Dialog,
    DialogBody,
    DialogSurface,
    DialogTitle,
    DialogActions,
    DialogContent,
    ProgressBar,
    tokens,
} from "@fluentui/react-components";
import {
    Clock20Regular,
    DoorArrowRight20Regular,
    Money20Regular,
    Shield20Regular,
    Info20Regular,
    Sticker20Regular,
    WeatherMoon20Regular,
} from "@fluentui/react-icons";
import {useEffect, useMemo, useState} from "react";

import vortex from "../../assets/vvvortex.svg";
import repeat from "../../assets/rrrepeat (2).svg";
import surf from "../../assets/sssurf.svg";
import { HoverTiltCard } from "../../components/HoverTiltCard/HoverTiltCard";

type BillingType = "Hourly" | "PerMinute";

const clampMin = (value: number, min: number) => Math.max(min, value);

const pad2 = (value: number) => value.toString().padStart(2, "0");

const formatTimeHHmm = (hhmm: string, fallback: Date) => {
    if (/^\d{2}:\d{2}$/.test(hhmm)) return hhmm;
    return `${pad2(fallback.getHours())}:${pad2(fallback.getMinutes())}`;
};

const formatMoneyRub = (value: number) => {
    try {
        return new Intl.NumberFormat("ru-RU", {
            style: "currency",
            currency: "RUB",
            maximumFractionDigits: 0,
        }).format(value);
    } catch {
        return `${Math.round(value)} ₽`;
    }
};

const formatDuration = (totalSeconds: number) => {
    const safeSeconds = clampMin(Math.floor(totalSeconds), 0);
    const hours = Math.floor(safeSeconds / 3600);
    const minutes = Math.floor((safeSeconds % 3600) / 60);
    const seconds = safeSeconds % 60;

    if (hours <= 0) return `${minutes}м ${pad2(seconds)}с`;
    return `${hours}ч ${pad2(minutes)}м ${pad2(seconds)}с`;
};

type Estimate = {
    totalRub: number;
    breakdown: string;
    chargedHours?: number;
    chargedMinutes?: number;
};

const calcEstimate = (elapsedMinutes: number, billingType: BillingType): Estimate => {
    const minutes = clampMin(Math.floor(elapsedMinutes), 1);

    if (billingType === "PerMinute") {
        const pricePerMinute = 7;
        return {
            totalRub: minutes * pricePerMinute,
            chargedMinutes: minutes,
            breakdown: `${pricePerMinute} ₽/мин × ${minutes} мин`,
        };
    }

    const pricePerHour = 250;
    const hours = clampMin(Math.floor(minutes / 60), 1);

    return {
        totalRub: hours * pricePerHour,
        chargedHours: hours,
        breakdown: `${pricePerHour} ₽/ч × ${hours} ч`,
    };
};

export const ActiveVisitPage = () => {
    const [now, setNow] = useState(() => Date.now());
    const [confirmOpen, setConfirmOpen] = useState(false);
    const [exitComplete, setExitComplete] = useState(false);

    const demo = useMemo(
        () => ({
            entryTimeLabel: "14:30",
            tariffName: "Стандартный",
            billingType: "Hourly" as BillingType,
            startedAtMs: Date.now() - 135 * 60 * 1000,
        }),
        []
    );

    useEffect(() => {
        const id = window.setInterval(() => setNow(Date.now()), 1000);
        return () => window.clearInterval(id);
    }, []);

    const elapsedSeconds = useMemo(() => {
        const delta = now - demo.startedAtMs;
        return Math.max(0, Math.floor(delta / 1000));
    }, [demo.startedAtMs, now]);

    const elapsedMinutes = useMemo(() => Math.max(1, Math.ceil(elapsedSeconds / 60)), [elapsedSeconds]);

    const estimate = useMemo(() => calcEstimate(elapsedMinutes, demo.billingType), [demo.billingType, elapsedMinutes]);

    const progressToNextHour = useMemo(() => {
        if (demo.billingType !== "Hourly") return null;
        const minutesIntoHour = elapsedMinutes % 60;
        return minutesIntoHour / 60;
    }, [demo.billingType, elapsedMinutes]);

    return (
        <div className="tc-noise-overlay relative overflow-hidden min-h-full">
            <div className="pointer-events-none absolute inset-0 overflow-hidden">
                <img
                    src={vortex}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[10vw] -left-[10vw] w-[55vw] max-w-[680px] select-none"
                    style={{opacity: 0.22}}
                    draggable={false}
                />
                <img
                    src={repeat}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[6vw] -right-[8vw] w-[45vw] max-w-[560px] select-none"
                    style={{opacity: 0.22}}
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[16vw] left-0 w-[100vw] max-w-none select-none"
                    style={{opacity: 0.22}}
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[16vw] right-0 -scale-x-100 w-[100vw] max-w-none select-none"
                    style={{opacity: 0.22}}
                    draggable={false}
                />
            </div>

            <div className="mx-auto w-full max-w-6xl px-2 py-4 sm:px-3 sm:py-6 relative z-10">
                <div
                    className="rounded-3xl p-5 sm:p-8"
                    style={{
                        backgroundImage: `radial-gradient(900px 480px at 20% 10%, ${tokens.colorBrandBackground2} 0%, transparent 60%), radial-gradient(720px 420px at 90% 0%, ${tokens.colorPaletteLightGreenBackground2} 0%, transparent 55%), linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
                        border: `1px solid ${tokens.colorNeutralStroke1}`,
                        boxShadow: tokens.shadow16,
                    }}
                >
                    <div className="flex flex-col gap-4">
                        <div className="flex flex-col gap-3">
                            <div className="flex items-center gap-2 flex-wrap">
                                <Tag appearance="brand" icon={<Clock20Regular />}>Визит</Tag>
                                <Badge appearance="tint" size="large">UI</Badge>
                                <Tag appearance={demo.billingType === "Hourly" ? "outline" : "brand"}>
                                    {demo.billingType === "Hourly" ? "Почасовая" : "Поминутная"}
                                </Tag>
                            </div>

                            <Title2 block>Вы в заведении</Title2>
                            <Body2 block>
                                Таймер и ориентировочная стоимость обновляются в реальном времени.
                            </Body2>
                            <Caption1 block>
                                Сейчас это демо-интерфейс без реального списания.
                            </Caption1>
                        </div>

                        <Divider />

                        <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                            <HoverTiltCard className="order-2 lg:order-1 lg:col-span-8">
                                <div className="flex flex-col gap-4">
                                    <div className="flex items-center justify-between gap-3 flex-wrap">
                                        <div className="flex items-center gap-2">
                                            <Clock20Regular />
                                            <Subtitle2Stronger>Активный визит</Subtitle2Stronger>
                                        </div>
                                        <div className="flex items-center gap-2 flex-wrap">
                                            <Tag appearance="outline" icon={<Sticker20Regular />}>{demo.tariffName}</Tag>
                                            <Tag appearance="outline" icon={<Money20Regular />}>{formatMoneyRub(estimate.totalRub)}</Tag>
                                        </div>
                                    </div>

                                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                                        <div className="flex flex-col gap-2">
                                            <Caption1 block>Прошедшее время</Caption1>
                                            <LargeTitle block>{formatDuration(elapsedSeconds)}</LargeTitle>
                                            <Caption1 block>Обновление: каждую секунду</Caption1>
                                        </div>

                                        <div className="flex flex-col gap-2 sm:items-end sm:text-right">
                                            <Caption1 block>Ориентировочная стоимость</Caption1>
                                            <Title1 block>{formatMoneyRub(estimate.totalRub)}</Title1>
                                            <Caption1 block>{estimate.breakdown}</Caption1>
                                        </div>
                                    </div>
                                </div>
                            </HoverTiltCard>

                            <HoverTiltCard className="order-1 lg:order-2 lg:col-span-4">
                                <div className="flex flex-col gap-4">
                                    <div className="flex items-center gap-2">
                                        <DoorArrowRight20Regular />
                                        <Subtitle2Stronger>Действия</Subtitle2Stronger>
                                    </div>

                                    <Button
                                        appearance="primary"
                                        icon={<DoorArrowRight20Regular />}
                                        onClick={() => setConfirmOpen(true)}
                                        disabled={exitComplete}
                                    >
                                        Выход из заведения
                                    </Button>

                                    <div className="flex flex-col gap-2">
                                        <div className="flex items-center justify-between gap-3">
                                            <Caption1>Время входа</Caption1>
                                            <Title3>
                                                {formatTimeHHmm(demo.entryTimeLabel, new Date(demo.startedAtMs))}
                                            </Title3>
                                        </div>
                                        <div className="flex items-center justify-between gap-3">
                                            <Caption1>Тип</Caption1>
                                            <Body1>{demo.billingType === "Hourly" ? "Почасовая" : "Поминутная"}</Body1>
                                        </div>
                                    </div>

                                    <Divider />

                                    <Caption1 block>
                                        {exitComplete ? "Визит завершён (demo)" : "Списание и финальный расчёт будут подключены позже"}
                                    </Caption1>
                                </div>
                            </HoverTiltCard>
                        </div>

                        <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                            <HoverTiltCard className="lg:col-span-8">
                                <div className="flex flex-col gap-4">
                                    <div className="flex items-center gap-2">
                                        <Info20Regular />
                                        <Subtitle2Stronger>Детали расчёта</Subtitle2Stronger>
                                    </div>

                                    <div>
                                        <div className="flex flex-col gap-3">
                                            <div className="flex items-center justify-between gap-4 flex-wrap">
                                                <div className="flex items-center gap-2">
                                                    <Sticker20Regular />
                                                    <Caption1 block>Тариф</Caption1>
                                                </div>
                                                <Title3 block>{demo.tariffName}</Title3>
                                            </div>

                                            <Divider />

                                            <div className="flex items-center justify-between gap-4 flex-wrap">
                                                <div className="flex items-center gap-2">
                                                    <Money20Regular />
                                                    <Caption1 block>Тип</Caption1>
                                                </div>
                                                <Title3 block>
                                                    {demo.billingType === "Hourly" ? "Почасовая" : "Поминутная"}
                                                </Title3>
                                            </div>

                                            <Divider />

                                            <div className="flex items-start justify-between gap-4">
                                                <div className="flex items-center gap-2">
                                                    <Info20Regular />
                                                    <Caption1 block>Расчёт</Caption1>
                                                </div>

                                                <div className="flex flex-col items-end gap-2 text-right">
                                                    <Title1 block>{formatMoneyRub(estimate.totalRub)}</Title1>
                                                    <Caption1 block>{estimate.breakdown}</Caption1>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <Body2 block>
                                        {demo.billingType === "Hourly"
                                            ? "Почасовая: учитываются только полные часы (demo)."
                                            : "Поминутная: учитываются минуты (demo)."}
                                    </Body2>
                                </div>
                            </HoverTiltCard>

                            <HoverTiltCard className="!hidden lg:!block lg:col-span-4">
                                <div className="flex flex-col gap-4">
                                    <div className="flex items-center gap-2">
                                        <WeatherMoon20Regular />
                                        <Subtitle2Stronger>Атмосфера</Subtitle2Stronger>
                                    </div>

                                    <Body2 block>
                                        Декоративная панель для десктопа: потом сюда можно добавить подсказки, правила или мини-статус заведения.
                                    </Body2>

                                    <Divider />

                                    {demo.billingType === "Hourly" && progressToNextHour !== null ? (
                                        <div className="flex flex-col gap-2">
                                            <Caption1 block>Прогресс до следующего часа</Caption1>
                                            <ProgressBar value={progressToNextHour} />
                                            <Caption1 block>
                                                {elapsedMinutes % 60} мин из 60
                                            </Caption1>
                                        </div>
                                    ) : (
                                        <div className="flex flex-col gap-2">
                                            <Caption1 block>Статус визита</Caption1>
                                            <ProgressBar />
                                            <Caption1 block>Идёт в реальном времени</Caption1>
                                        </div>
                                    )}
                                </div>
                            </HoverTiltCard>
                        </div>

                        <Dialog open={confirmOpen} onOpenChange={(_, data) => setConfirmOpen(data.open)}>
                            <DialogSurface className="w-[calc(100vw-32px)] max-w-[520px]">
                                <DialogBody>
                                    <DialogTitle>
                                        <div className="flex items-center gap-2">
                                            <Shield20Regular />
                                            Завершить визит
                                        </div>
                                    </DialogTitle>
                                    <DialogContent>
                                        <div className="flex flex-col gap-4">
                                            <Body1 block>
                                                Подтвердите выход — будет рассчитана финальная стоимость и списан баланс.
                                            </Body1>
                                            <Caption1 block>
                                                Сейчас это демо: показываем сумму, но ничего не списываем.
                                            </Caption1>

                                            <Card>
                                                <div className="flex flex-col gap-3">
                                                    <div className="flex items-center justify-between gap-3">
                                                        <Caption1>Тариф</Caption1>
                                                        <Body1>{demo.tariffName}</Body1>
                                                    </div>
                                                    <div className="flex items-center justify-between gap-3">
                                                        <Caption1>Длительность</Caption1>
                                                        <Body1>{formatDuration(elapsedSeconds)}</Body1>
                                                    </div>

                                                    <Divider />

                                                    <div className="flex items-center justify-between gap-3">
                                                        <Caption1 block>
                                                            Итого к списанию
                                                        </Caption1>
                                                        <Title3>{formatMoneyRub(estimate.totalRub)}</Title3>
                                                    </div>
                                                    <Caption1>{estimate.breakdown}</Caption1>
                                                </div>
                                            </Card>
                                        </div>
                                    </DialogContent>

                                    <DialogActions>
                                        <Button appearance="secondary" onClick={() => setConfirmOpen(false)}>
                                            Отмена
                                        </Button>
                                        <Button
                                            appearance="primary"
                                            onClick={() => {
                                                setExitComplete(true);
                                                setConfirmOpen(false);
                                            }}
                                        >
                                            Завершить визит
                                        </Button>
                                    </DialogActions>
                                </DialogBody>
                            </DialogSurface>
                        </Dialog>
                    </div>
                </div>
            </div>
        </div>
    );
};
