import {
    Badge,
    Button,
    Caption1,
    Card,
    Divider,
    Field,
    Input,
    Radio,
    RadioGroup,
    Tag,
    Text,
    Title2,
    Title3,
    tokens,
} from "@fluentui/react-components";
import {
    Carousel,
    CarouselCard,
    CarouselNav,
    CarouselNavButton,
    CarouselNavContainer,
    CarouselSlider,
    CarouselViewport,
} from "@fluentui/react-components";
import {Clock20Regular, Money20Regular, Sparkle20Regular} from "@fluentui/react-icons";
import {useCallback, useMemo, useState} from "react";
import {TariffCard, type TariffBillingType, type UiTariff} from "../../components/TariffCard/TariffCard";

const clamp = (value: number, min: number, max: number) => Math.max(min, Math.min(max, value));

const formatMoney = (value: number) => {
    try {
        return new Intl.NumberFormat("ru-RU", {
            style: "currency",
            currency: "BYN",
            maximumFractionDigits: 2,
        }).format(value);
    } catch {
        return `${value.toFixed(2)} BYN`;
    }
};

const formatDuration = (totalMinutes: number) => {
    const minutes = Math.max(0, Math.floor(totalMinutes));
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    if (h <= 0) return `${m} мин`;
    if (m <= 0) return `${h} ч`;
    return `${h} ч ${m} мин`;
};

type CalcResult = {
    total: number;
    chargedMinutes: number;
    chargedHours: number;
    pricePerMinute: number;
    pricePerHour: number;
};

const calculate = (minutes: number, pricePerMinute: number, billingType: TariffBillingType): CalcResult => {
    const safeMinutes = clamp(Math.floor(minutes), 1, 12 * 60);
    const priceMinute = Math.max(0, pricePerMinute);
    const priceHour = priceMinute * 60;

    if (billingType === "PerMinute") {
        return {
            total: safeMinutes * priceMinute,
            chargedMinutes: safeMinutes,
            chargedHours: 0,
            pricePerMinute: priceMinute,
            pricePerHour: priceHour,
        };
    }

    const hours = Math.max(1, Math.ceil(safeMinutes / 60));

    return {
        total: hours * priceHour,
        chargedMinutes: safeMinutes,
        chargedHours: hours,
        pricePerMinute: priceMinute,
        pricePerHour: priceHour,
    };
};

export const TariffSelectionPage = () => {
    const tariffs = useMemo<UiTariff[]>(
        () => [
            {
                tariffId: "standard",
                name: "Стандартный",
                description: "Идеально для большинства визитов — без ограничений и с прозрачным расчётом.",
                pricePerMinute: 0.12,
                isActive: true,
                accent: "brand",
                recommended: true,
            },
            {
                tariffId: "quiet",
                name: "Тихая зона",
                description: "Спокойная атмосфера и минимальный шум — для работы и учёбы.",
                pricePerMinute: 0.10,
                isActive: true,
                accent: "green",
            },
            {
                tariffId: "night",
                name: "Ночной",
                description: "Для поздних визитов: мягкий свет, меньше людей и приятный темп.",
                pricePerMinute: 0.09,
                isActive: true,
                accent: "purple",
            },
            {
                tariffId: "promo",
                name: "Промо",
                description: "Тариф для акций и промокодов. В этом UI — как пример неактивного тарифа.",
                pricePerMinute: 0.07,
                isActive: false,
                accent: "pink",
            },
        ],
        []
    );

    const [selectedTariffId, setSelectedTariffId] = useState<string | null>("standard");
    const selectedTariff = useMemo(
        () => tariffs.find((t) => t.tariffId === selectedTariffId) ?? null,
        [tariffs, selectedTariffId]
    );

    const initialActiveIndex = useMemo(() => {
        const idx = tariffs.findIndex((t) => t.tariffId === selectedTariffId);
        return idx >= 0 ? idx : 0;
    }, [tariffs, selectedTariffId]);

    const [activeIndex, setActiveIndex] = useState<number>(initialActiveIndex);

    const [billingType, setBillingType] = useState<TariffBillingType>("PerMinute");
    const [durationMinutes, setDurationMinutes] = useState<number>(90);

    const calc = useMemo(() => {
        if (!selectedTariff) return null;
        return calculate(durationMinutes, selectedTariff.pricePerMinute, billingType);
    }, [billingType, durationMinutes, selectedTariff]);

    const setActiveTariff = useCallback(
        (index: number) => {
            const safeIndex = clamp(index, 0, Math.max(0, tariffs.length - 1));
            setActiveIndex(safeIndex);
            const next = tariffs[safeIndex];
            if (next) setSelectedTariffId(next.tariffId);
        },
        [tariffs]
    );

    const onSelectTariff = useCallback(
        (tariffId: string) => {
            const idx = tariffs.findIndex((t) => t.tariffId === tariffId);
            if (idx >= 0) setActiveTariff(idx);
            else setSelectedTariffId(tariffId);
        },
        [setActiveTariff, tariffs]
    );

    const subtleTextStyle = useMemo(() => ({color: tokens.colorNeutralForeground2}), []);

    const sliderPadding = useMemo(
        () => ({
            gap: tokens.spacingHorizontalL,
            padding: `0 ${tokens.spacingHorizontalXXL}`,
        }),
        []
    );

    const presets = useMemo(() => [30, 60, 90, 120, 180, 240], []);

    return (
        <div className="tc-noise-overlay relative overflow-hidden min-h-screen">
            <div className="mx-auto w-full max-w-6xl px-4 py-6 relative z-10">
                <div
                    className="rounded-3xl p-5 sm:p-8"
                    style={{
                        backgroundImage: `radial-gradient(900px 480px at 20% 10%, ${tokens.colorBrandBackground2} 0%, transparent 60%), radial-gradient(720px 420px at 90% 0%, ${tokens.colorPaletteLightGreenBackground2} 0%, transparent 55%), linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
                        border: `1px solid ${tokens.colorNeutralStroke1}`,
                        boxShadow: tokens.shadow16,
                    }}
                >
                    <div className="flex flex-col gap-4">
                        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                            <div className="min-w-0">
                                <div className="flex items-center gap-2 flex-wrap">
                                    <Title2>Выбор тарифа</Title2>
                                    <Badge appearance="tint" size="large">UI</Badge>
                                    <Tag appearance="brand" icon={<Sparkle20Regular />}>Визит</Tag>
                                </div>
                                <Text block style={subtleTextStyle} className="mt-2">
                                    Выберите тариф в карусели, затем задайте примерное время и способ расчёта.
                                </Text>
                            </div>

                            <div className="flex flex-wrap gap-2">
                                <Tag appearance="outline" icon={<Clock20Regular />}>Без бэка</Tag>
                                <Tag appearance="outline" icon={<Money20Regular />}>Калькулятор</Tag>
                            </div>
                        </div>

                        <Divider />

                        <div className="flex flex-col gap-3">
                            <div className="flex items-center justify-between gap-3 flex-wrap">
                                <Title3>Тарифы</Title3>
                                <Text style={subtleTextStyle}>
                                    Потяните мышкой/тачем или используйте стрелки.
                                </Text>
                            </div>

                            <div
                                className="rounded-2xl"
                                style={{
                                    backgroundImage: `linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
                                    border: `1px solid ${tokens.colorNeutralStroke1}`,
                                    boxShadow: tokens.shadow8,
                                }}
                            >
                                <Carousel
                                    activeIndex={activeIndex}
                                    groupSize={1}
                                    onActiveIndexChange={(_, data) => setActiveTariff(data.index)}
                                    circular
                                    draggable
                                    align="center"
                                    whitespace
                                >
                                    <div className="flex items-center justify-between gap-4 flex-wrap px-3 pt-3">
                                        <Text as="h3" className="text-base font-semibold">
                                            Доступные варианты
                                        </Text>
                                        <CarouselNavContainer
                                            next={{"aria-label": "следующий тариф"}}
                                            prev={{"aria-label": "предыдущий тариф"}}
                                        >
                                            <CarouselNav>
                                                {(index) => (
                                                    <CarouselNavButton aria-label={`Тариф ${index + 1}`} />
                                                )}
                                            </CarouselNav>
                                        </CarouselNavContainer>
                                    </div>

                                    <CarouselViewport>
                                        <CarouselSlider cardFocus style={sliderPadding} className="py-6">
                                            {tariffs.map((tariff, index) => (
                                                <CarouselCard key={tariff.tariffId} autoSize aria-label={`${index + 1} из ${tariffs.length}`}>
                                                    <div
                                                        className="transition-[transform,opacity] duration-200 ease-out"
                                                        style={{
                                                            transform: index === activeIndex ? "scale(1)" : "scale(0.92)",
                                                            opacity: index === activeIndex ? 1 : 0.55,
                                                        }}
                                                    >
                                                        <TariffCard
                                                            tariff={tariff}
                                                            selected={index === activeIndex}
                                                            onSelect={onSelectTariff}
                                                        />
                                                    </div>
                                                </CarouselCard>
                                            ))}
                                        </CarouselSlider>
                                    </CarouselViewport>
                                </Carousel>
                            </div>
                        </div>

                        <Divider />

                        <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                            <Card className="lg:col-span-7">
                                <div className="flex flex-col gap-4">
                                    <div className="flex items-center justify-between gap-3 flex-wrap">
                                        <Title3>Параметры визита</Title3>
                                        <Badge appearance="outline">Demo</Badge>
                                    </div>
                                    <Divider />

                                    {!selectedTariff ? (
                                        <Text style={subtleTextStyle}>Сначала выберите тариф в карусели.</Text>
                                    ) : (
                                        <div className="flex flex-col gap-4">
                                            <Field
                                                label="Примерное время пребывания"
                                                hint="Можно выбрать пресет или ввести вручную (в минутах)"
                                            >
                                                <Input
                                                    type="number"
                                                    value={String(durationMinutes)}
                                                    onChange={(_, data) => {
                                                        const next = Number(data.value);
                                                        if (!Number.isFinite(next)) return;
                                                        setDurationMinutes(clamp(next, 1, 12 * 60));
                                                    }}
                                                    min={1}
                                                    max={12 * 60}
                                                />
                                            </Field>

                                            <div className="flex flex-wrap gap-2">
                                                {presets.map((m) => (
                                                    <Button
                                                        key={m}
                                                        appearance={durationMinutes === m ? "primary" : "secondary"}
                                                        onClick={() => setDurationMinutes(m)}
                                                    >
                                                        {formatDuration(m)}
                                                    </Button>
                                                ))}
                                            </div>

                                            <Field label="Тип тарифа" hint="Для UI можно переключать, чтобы увидеть расчёт">
                                                <RadioGroup
                                                    value={billingType}
                                                    onChange={(_, data) => setBillingType(data.value as TariffBillingType)}
                                                    layout="horizontal"
                                                >
                                                    <Radio value="PerMinute" label="Поминутно" />
                                                    <Radio value="Hourly" label="Почасово" />
                                                </RadioGroup>
                                            </Field>

                                            <div className="flex flex-wrap items-center gap-2">
                                                <Tag appearance="brand">{selectedTariff.name}</Tag>
                                                <Tag appearance="outline">{billingType === "PerMinute" ? "Оплата за минуты" : "Округление до часа"}</Tag>
                                                <Tag appearance="outline">{formatDuration(durationMinutes)}</Tag>
                                            </div>
                                        </div>
                                    )}
                                </div>
                            </Card>

                            <Card className="lg:col-span-5">
                                <div className="flex flex-col gap-4">
                                    <div className="flex items-center justify-between gap-3 flex-wrap">
                                        <Title3>Калькулятор</Title3>
                                        <Tag appearance="outline" icon={<Money20Regular />}>Прогноз</Tag>
                                    </div>

                                    <Divider />

                                    {!selectedTariff || !calc ? (
                                        <Text style={subtleTextStyle}>Выберите тариф и задайте параметры.</Text>
                                    ) : (
                                        <div className="flex flex-col gap-3">
                                            <div
                                                className="rounded-2xl p-4"
                                                style={{
                                                    backgroundImage: `radial-gradient(640px 280px at 20% 0%, ${tokens.colorBrandBackground2} 0%, transparent 65%), linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
                                                    border: `1px solid ${tokens.colorNeutralStroke1}`,
                                                }}
                                            >
                                                <div className="flex items-end justify-between gap-3 flex-wrap">
                                                    <div className="min-w-0">
                                                        <Caption1 style={subtleTextStyle}>Ориентировочная сумма</Caption1>
                                                        <div className="text-3xl font-semibold tracking-tight">
                                                            {formatMoney(calc.total)}
                                                        </div>
                                                    </div>
                                                    <Badge appearance="tint" size="large">UI</Badge>
                                                </div>

                                                <div className="mt-3 grid grid-cols-2 gap-3">
                                                    <div>
                                                        <Caption1 style={subtleTextStyle}>Списание</Caption1>
                                                        <div className="font-semibold">
                                                            {billingType === "PerMinute"
                                                                ? `${calc.chargedMinutes} мин`
                                                                : `${calc.chargedHours} ч (за ${calc.chargedMinutes} мин)`}
                                                        </div>
                                                    </div>
                                                    <div className="text-right">
                                                        <Caption1 style={subtleTextStyle}>Ставка</Caption1>
                                                        <div className="font-semibold">
                                                            {billingType === "PerMinute"
                                                                ? `${formatMoney(calc.pricePerMinute)} / мин`
                                                                : `${formatMoney(calc.pricePerHour)} / час`}
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>

                                            <Button appearance="primary" disabled className="w-full">
                                                <Text truncate wrap={false}>Начать визит (скоро)</Text>
                                            </Button>
                                            <Button appearance="secondary" disabled className="w-full">
                                                <Text truncate wrap={false}>Перейти к визиту (скоро)</Text>
                                            </Button>

                                            <Text style={subtleTextStyle} size={200}>
                                                Это демо-страница: расчёт выполняется только на клиенте.
                                            </Text>
                                        </div>
                                    )}
                                </div>
                            </Card>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};
