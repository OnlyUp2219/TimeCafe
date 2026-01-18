import {
    Badge,
    Body2,
    Button,
    Card,
    Caption1,
    Divider,
    Tag,
    Text,
    Title3,
    tokens,
} from "@fluentui/react-components";
import type {FC} from "react";
import {useMemo} from "react";

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
            className="sm:w-[360px] sm:max-w-[82vw]"
            style={{
                border: `1px solid ${selected ? tokens.colorBrandStroke1 : tokens.colorNeutralStroke1}`,
                boxShadow: selected ? tokens.shadow16 : tokens.shadow8,
                backgroundImage: `radial-gradient(720px 320px at 20% 0%, ${accent} 0%, transparent 60%), linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
            }}
        >
            <div className="flex flex-col gap-3">
                <div className="flex items-start justify-between gap-3">
                    <div>
                        <div className="flex items-center gap-2 flex-wrap">
                            <Title3 truncate wrap={false} block>
                                {tariff.name}
                            </Title3>
                            {tariff.recommended && (
                                <Tag className="!hidden sm:!block" appearance="brand" size="small">
                                    Рекомендуем
                                </Tag>
                            )}
                            {!tariff.isActive && (
                                <Tag appearance="outline" size="small">
                                    Неактивен
                                </Tag>
                            )}
                        </div>
                        <Body2 className="!line-clamp-2">
                            {tariff.description}
                        </Body2>
                    </div>

                    <div>
                        {selected ? (
                            <Badge appearance="tint" size="large">
                                Выбран
                            </Badge>
                        ) : (
                            <Badge appearance="ghost" size="large">
                                Тариф
                            </Badge>
                        )}
                    </div>
                </div>

                <Divider appearance="brand" className="divider grow-0"/>

                <div className="flex flex-wrap gap-x-10">
                    <div className="flex flex-col gap-1">
                        <Caption1>Поминутно</Caption1>
                        <div className="text-base font-semibold">
                            {formatMoney(perMinute)} / мин
                        </div>
                    </div>
                    <div className="flex flex-col gap-1">
                        <Caption1>Почасово</Caption1>
                        <div className="text-base font-semibold">
                            {formatMoney(perHour)} / час
                        </div>
                    </div>
                </div>

                <div className="flex flex-wrap gap-2">
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
