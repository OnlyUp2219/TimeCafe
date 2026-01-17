import {
    Badge,
    Button,
    Card,
    Caption1,
    Divider,
    Tag,
    Text,
    tokens,
} from "@fluentui/react-components";
import type {FC} from "react";
import {useMemo} from "react";
import {TruncatedText} from "../TruncatedText/TruncatedText";

export type TariffBillingType = "Hourly" | "PerMinute";

export type UiTariff = {
    tariffId: string;
    name: string;
    description: string;
    pricePerMinute: number;
    isActive: boolean;
    accent?: "brand" | "green" | "pink" | "purple";
    recommended?: boolean;
};

type Props = {
    tariff: UiTariff;
    selected?: boolean;
    onSelect?: (tariffId: string) => void;
};

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

export const TariffCard: FC<Props> = ({tariff, selected = false, onSelect}) => {
    const subtleTextStyle = useMemo(() => ({color: tokens.colorNeutralForeground2}), []);

    const accent = useMemo(() => {
        if (tariff.accent === "green") return tokens.colorPaletteLightGreenBackground2;
        if (tariff.accent === "pink") return tokens.colorPaletteMagentaBackground2;
        if (tariff.accent === "purple") return tokens.colorPalettePurpleBackground2;
        return tokens.colorBrandBackground2;
    }, [tariff.accent]);

    const perMinute = tariff.pricePerMinute;
    const perHour = tariff.pricePerMinute * 60;

    return (
        <Card
            className="w-[360px] max-w-[82vw]"
            style={{
                border: `1px solid ${selected ? tokens.colorBrandStroke1 : tokens.colorNeutralStroke1}`,
                boxShadow: selected ? tokens.shadow16 : tokens.shadow8,
                backgroundImage: `radial-gradient(720px 320px at 20% 0%, ${accent} 0%, transparent 60%), linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
            }}
        >
            <div className="flex flex-col gap-3">
                <div className="flex items-start justify-between gap-3">
                    <div className="min-w-0">
                        <div className="flex items-center gap-2 flex-wrap">
                            <div className="min-w-0 flex-1">
                                <TruncatedText as="title3" lines={2}>
                                    {tariff.name}
                                </TruncatedText>
                            </div>
                            {tariff.recommended && (
                                <Tag appearance="brand" size="small">
                                    Рекомендуем
                                </Tag>
                            )}
                            {!tariff.isActive && (
                                <Tag appearance="outline" size="small">
                                    Неактивен
                                </Tag>
                            )}
                        </div>
                        <div className="mt-1">
                            <TruncatedText as="body2" lines={2} textStyle={subtleTextStyle}>
                                {tariff.description}
                            </TruncatedText>
                        </div>
                    </div>

                    {selected ? (
                        <Badge appearance="tint" size="large">
                            Выбран
                        </Badge>
                    ) : (
                        <Badge appearance="outline" size="large">
                            Тариф
                        </Badge>
                    )}
                </div>

                <Divider />

                <div className="grid grid-cols-2 gap-3">
                    <div className="flex flex-col gap-1">
                        <Caption1 style={subtleTextStyle}>Поминутно</Caption1>
                        <div className="text-base font-semibold">
                            {formatMoney(perMinute)} / мин
                        </div>
                    </div>
                    <div className="flex flex-col gap-1 text-right">
                        <Caption1 style={subtleTextStyle}>Почасово</Caption1>
                        <div className="text-base font-semibold">
                            {formatMoney(perHour)} / час
                        </div>
                    </div>
                </div>

                <div className="flex gap-2 pt-1">
                    <Button
                        appearance={selected ? "primary" : "secondary"}
                        onClick={() => onSelect?.(tariff.tariffId)}
                        disabled={!tariff.isActive}
                    >
                        {selected ? "Выбран" : "Выбрать"}
                    </Button>
                    <Button appearance="secondary" disabled>
                        <Text truncate wrap={false}>Детали (скоро)</Text>
                    </Button>
                </div>
            </div>
        </Card>
    );
};
