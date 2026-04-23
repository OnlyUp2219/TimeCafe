import {
    Badge,
    Body2,
    Button,
    Card,
    CardHeader,
    CardFooter,
    Caption1,
    Divider,
    Tag,
    Text,
    Title3,
    Tooltip,
    tokens,
} from "@fluentui/react-components";
import type { FC } from "react";
import { useMemo } from "react";
import { BillingType as BillingTypeEnum, type Tariff } from "@app-types/tariff";
import { formatMoneyByN } from "@utility/formatMoney";
import { parseThemeConfig, getThemeStyles, getPatternLayerStyles } from "@utility/themeStyles";

type Props = {
    tariff: Tariff;
    selected?: boolean;
    onSelect?: (tariffId: string) => void;
};

export const TariffCard: FC<Props> = ({ tariff, selected = false, onSelect }) => {

    const config = useMemo(() => parseThemeConfig(tariff.themeColors || tariff.colors), [tariff]);
    const themeStyles = useMemo(() => getThemeStyles(config), [config]);

    const rateLabel = tariff.billingType === BillingTypeEnum.Hourly ? "Почасовой" : "Поминутный";
    const unitLabel = tariff.billingType === BillingTypeEnum.Hourly ? "/ час" : "/ мин";
    const rateValue = tariff.billingType === BillingTypeEnum.Hourly ? tariff.pricePerMinute * 60 : tariff.pricePerMinute;

    return (
        <Card
            className="sm:w-[360px] sm:max-w-[82vw] transition-all duration-300"
            style={{
                ...themeStyles,
                border: `1px solid ${selected ? tokens.colorBrandStroke1 : "transparent"}`,
                boxShadow: selected ? tokens.shadow16 : tokens.shadow8,
                position: "relative",
                overflow: "hidden"
            }}
        >
            {(config.patterns || []).map((layer, idx) => (
                <div key={idx} style={getPatternLayerStyles(layer)} />
            ))}
            <CardHeader
                className="relative z-10"
                image={tariff.themeEmoji && <span className="text-3xl">{tariff.themeEmoji}</span>}
                header={
                    <div className="flex items-center gap-2 flex-wrap">
                        <Title3 truncate wrap={false} block style={{ color: themeStyles.color }}>
                            {tariff.name}
                        </Title3>
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
                }
                description={
                    <Body2 className="!line-clamp-2 min-h-[2.75rem]" style={{ color: themeStyles.color, opacity: 0.8 }}>
                        {tariff.description}
                    </Body2>
                }
                action={
                    selected ? (
                        <Badge appearance="tint" size="large">
                            Выбран
                        </Badge>
                    ) : (
                        <Badge appearance="ghost" size="large" style={{ color: themeStyles.color }}>
                            Тариф
                        </Badge>
                    )
                }
            />

            <Divider style={{ opacity: 0.3 }} className="relative z-10" />

            <div className="flex flex-col gap-1 px-3 relative z-10">
                <Caption1 style={{ color: themeStyles.color, opacity: 0.7 }}>{rateLabel}</Caption1>
                <Title3 block style={{ color: themeStyles.color }}>
                    {formatMoneyByN(rateValue)} {unitLabel}
                </Title3>
            </div>

            <CardFooter className="mt-2 relative z-10">
                <Button
                    appearance={selected ? "primary" : "secondary"}
                    style={!selected ? { backgroundColor: "rgba(255,255,255,0.1)", color: themeStyles.color, backdropFilter: "blur(4px)", border: "1px solid rgba(255,255,255,0.2)" } : {}}
                    onClick={() => onSelect?.(tariff.tariffId)}
                    disabled={!tariff.isActive}
                >
                    {selected ? "Выбран" : "Выбрать"}
                </Button>
                <Tooltip content="Детали (скоро)" relationship="label">
                    <span>
                        <Button appearance="secondary" disabled style={{ backgroundColor: "rgba(255,255,255,0.05)", color: themeStyles.color, border: "none" }}>
                            <Text truncate wrap={false}>Детали</Text>
                        </Button>
                    </span>
                </Tooltip>
            </CardFooter>
        </Card>
    );
};
