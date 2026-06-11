import {
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    CardHeader,
    CardFooter,
    Caption1,
    Divider,
    Tag,
    Title3,
    Tooltip,
    tokens,
} from "@fluentui/react-components";
import type { FC } from "react";
import { useMemo } from "react";
import { BillingType as BillingTypeEnum, type Tariff } from "@app-types/tariff";
import { formatMoneyByN } from "@utility/formatMoney";
import { formatRoundingRule } from "@utility/formatUtils";
import { parseThemeConfig, getThemeStyles, getPatternLayerStyles } from "@utility/themeStyles";
import { useComponentSize } from "@hooks/useComponentSize";

type Props = {
    tariff: Tariff;
    selected?: boolean;
    onSelect?: (tariffId: string) => void;
    onOpenDetails?: (tariff: Tariff) => void;
};

export const TariffCard: FC<Props> = ({ tariff, selected = false, onSelect, onOpenDetails }) => {
    const { sizes } = useComponentSize();
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
                border: selected ? `1px solid ${tokens.colorBrandStroke1}` : "none",
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
                        <Title3 truncate wrap={false} style={{ color: themeStyles.color }}>
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
                    <Body2 className="line-clamp-2! min-h-[2.75rem]" style={{ color: themeStyles.color, opacity: 0.8 }}>
                        {tariff.description}
                    </Body2>
                }
                action={
                    selected ? (
                        <Badge appearance="tint" size={sizes.badge}>
                            Выбран
                        </Badge>
                    ) : (
                        <Badge appearance="ghost" size={sizes.badge} style={{ color: themeStyles.color }}>
                            Тариф
                        </Badge>
                    )
                }
            />

            <Divider style={{ opacity: 0.3 }} className="relative z-10" />

            <div
                className="flex flex-col gap-1 relative z-10"
                style={{
                    paddingLeft: tokens.spacingHorizontalM,
                    paddingRight: tokens.spacingHorizontalM,
                }}
            >
                <Caption1 style={{ color: themeStyles.color, opacity: 0.7 }}>{rateLabel}</Caption1>
                <Title3 style={{ color: themeStyles.color }}>
                    {formatMoneyByN(rateValue)} {unitLabel}
                </Title3>
            </div>

            <div
                className="flex flex-col gap-1 relative z-10"
                style={{
                    color: themeStyles.color,
                    opacity: 0.85,
                    paddingLeft: tokens.spacingHorizontalM,
                    paddingRight: tokens.spacingHorizontalM,
                    marginTop: tokens.spacingVerticalS,
                }}
            >
                {tariff.minSessionMinutes && (
                    <div className="flex justify-between items-center gap-2">
                        <Caption1>Мин. время визита:</Caption1>
                        <Caption1 style={{ fontWeight: tokens.fontWeightSemibold }}>
                            {tariff.minSessionMinutes} мин
                        </Caption1>
                    </div>
                )}
                {tariff.roundingRule && tariff.roundingRule !== "None" && (
                    <div className="flex justify-between items-center gap-2">
                        <Caption1>Округление:</Caption1>
                        <Caption1 style={{ fontWeight: tokens.fontWeightSemibold }}>
                            {formatRoundingRule(tariff.roundingRule)}
                        </Caption1>
                    </div>
                )}
                {tariff.maxGuests && (
                    <div className="flex justify-between items-center gap-2">
                        <Caption1>Макс. гостей:</Caption1>
                        <Caption1 style={{ fontWeight: tokens.fontWeightSemibold }}>
                            {tariff.maxGuests}
                        </Caption1>
                    </div>
                )}
                {tariff.audienceTags && tariff.audienceTags.length > 0 && (
                    <div className="flex flex-wrap gap-1" style={{ marginTop: tokens.spacingVerticalXS }}>
                        {tariff.audienceTags.slice(0, 3).map((tag, idx) => (
                            <Tag
                                key={idx}
                                size="small"
                                appearance="outline"
                                style={{
                                    color: themeStyles.color,
                                    borderColor: "rgba(255, 255, 255, 0.2)",
                                    backgroundColor: "rgba(255, 255, 255, 0.1)",
                                }}
                            >
                                {tag}
                            </Tag>
                        ))}
                    </div>
                )}
            </div>

            <CardFooter className="relative z-10">
                <Button
                    appearance={selected ? "primary" : "secondary"}
                    style={!selected ? { backgroundColor: "rgba(255,255,255,0.1)", color: themeStyles.color, backdropFilter: "blur(4px)", border: "1px solid rgba(255,255,255,0.2)" } : {}}
                    onClick={() => onSelect?.(tariff.tariffId)}
                    disabled={!tariff.isActive}
                    size={sizes.button}
                >
                    {selected ? "Выбран" : "Выбрать"}
                </Button>
                <Tooltip content="Подробная информация о тарифе" relationship="label">
                    <span>
                        <Button
                            appearance="secondary"
                            style={{ backgroundColor: "rgba(255,255,255,0.05)", color: themeStyles.color, border: "none" }}
                            onClick={() => onOpenDetails?.(tariff)}
                            size={sizes.button}
                        >
                            <Body1 truncate wrap={false}>Детали</Body1>
                        </Button>
                    </span>
                </Tooltip>
            </CardFooter>
        </Card>
    );
};
