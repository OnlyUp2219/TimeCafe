import { Body2, Caption1, Card, Divider, Title2, Title3, Badge, Body1, Body1Strong, tokens, MessageBar, MessageBarBody, MessageBarTitle } from "@fluentui/react-components";
import { formatMoneyByN } from "@utility/formatMoney";
import { formatRoundingRule } from "@utility/formatUtils";
import type { TariffForecastCardProps } from "@components/Tariff/TariffForecastCardProps";
import { BillingType as BillingTypeEnum } from "@app-types/tariff";
import { useComponentSize } from "@hooks/useComponentSize";

export type CalcResult = {
    total: number;
    baseTotal: number;
    isDiscounted: boolean;
    chargedMinutes: number;
    chargedHours: number;
    pricePerMinute: number;
    pricePerHour: number;
};


export const TariffForecastCard = ({ selectedTariff, calc }: TariffForecastCardProps) => {
    const { sizes } = useComponentSize();

    return (
        <Card className="lg:col-span-5 h-full flex flex-col" size={sizes.card}>
            <div className="flex flex-col gap-4 h-full flex-1">
                <div className="flex flex-col gap-4">
                    <div className="flex items-center justify-between gap-3 flex-wrap">
                        <Title3>Калькулятор</Title3>
                    </div>
                    <Divider />
                </div>

                {!selectedTariff || !calc ? (
                    <div className="flex-1 flex items-center justify-center min-h-[150px]">
                        <Body2>Выберите тариф и задайте параметры.</Body2>
                    </div>
                ) : (
                    <div className="flex-1 flex flex-col justify-end gap-3 mt-4">
                        <div className="flex flex-col gap-4 justify-center">
                            <div className="flex items-end justify-between gap-3 flex-wrap mb-4">
                                <div className="min-w-0 flex flex-col">
                                    <Caption1>Ориентировочная сумма</Caption1>
                                    <div className="flex items-baseline gap-2">
                                        <Title2>{formatMoneyByN(calc.total)}</Title2>
                                        {calc.isDiscounted && (
                                            <Body1 style={{ textDecoration: 'line-through', color: tokens.colorNeutralForeground3 }}>
                                                {formatMoneyByN(calc.baseTotal)}
                                            </Body1>
                                        )}
                                    </div>
                                </div>
                            </div>
                            <div className="grid grid-cols-2 gap-3 mb-4">
                                <div className="flex flex-col">
                                    <Caption1>Списание</Caption1>
                                    <Title3>
                                        {selectedTariff.billingType === BillingTypeEnum.PerMinute
                                            ? `${calc.chargedMinutes} мин`
                                            : `${calc.chargedHours} ч`}
                                    </Title3>
                                </div>
                                <div className="flex flex-col">
                                    <Caption1>Ставка</Caption1>
                                    <Title3>
                                        {selectedTariff.billingType === BillingTypeEnum.PerMinute
                                            ? `${formatMoneyByN(calc.pricePerMinute)} / мин`
                                            : `${formatMoneyByN(calc.pricePerHour)} / час`}
                                    </Title3>
                                </div>
                            </div>
                            <Divider />
                            <div style={{ display: 'flex', flexDirection: 'column', gap: tokens.spacingVerticalS, marginTop: tokens.spacingVerticalM }}>
                                {selectedTariff.minSessionMinutes && (
                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                        <Body1 style={{ color: tokens.colorNeutralForeground3 }}>Мин. время визита</Body1>
                                        <Body1Strong>{selectedTariff.minSessionMinutes} мин</Body1Strong>
                                    </div>
                                )}
                                {selectedTariff.roundingRule && selectedTariff.roundingRule !== "None" && (
                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                        <Body1 style={{ color: tokens.colorNeutralForeground3 }}>Округление</Body1>
                                        <Body1Strong>{formatRoundingRule(selectedTariff.roundingRule)}</Body1Strong>
                                    </div>
                                )}
                                {(calc.total > 0 && calc.total < (selectedTariff.billingType === BillingTypeEnum.PerMinute ? calc.pricePerMinute * calc.chargedMinutes : calc.pricePerHour * calc.chargedHours)) && (
                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                        <Body1 style={{ color: tokens.colorNeutralForeground3 }}>Скидка применена</Body1>
                                        <Badge appearance="tint" color="success">Да</Badge>
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>
                )}

                <MessageBar intent="info" style={{ marginTop: "auto" }}>
                    <MessageBarBody>
                        <MessageBarTitle>Правила тарификации</MessageBarTitle>
                        При выборе поминутного тарифа списание происходит строго посекундно. Почасовые тарифы округляются до полного часа в большую сторону.
                        Итоговая стоимость визита не превысит лимит стоп-чека, если он установлен.
                    </MessageBarBody>
                </MessageBar>
            </div>
        </Card>
    );
};
