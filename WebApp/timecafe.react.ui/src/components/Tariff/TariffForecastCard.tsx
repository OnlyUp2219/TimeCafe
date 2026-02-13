import {Body2, Caption1, Card, Divider, Title2, Title3} from "@fluentui/react-components";
import {formatMoneyByN} from "@utility/formatMoney.ts";
import type {TariffForecastCardProps} from "@components/Tariff/TariffForecastCardProps.ts";
import {BillingType as BillingTypeEnum} from "@app-types/tariff.ts";

export type CalcResult = {
    total: number;
    chargedMinutes: number;
    chargedHours: number;
    pricePerMinute: number;
    pricePerHour: number;
};


export const TariffForecastCard = ({selectedTariff, calc}: TariffForecastCardProps) => {
    return (
        <Card className="lg:col-span-5">
            <div className="flex flex-col gap-4">
                <div className="flex items-center justify-between gap-3 flex-wrap">
                    <Title3>Калькулятор</Title3>
                </div>

                <Divider/>

                {!selectedTariff || !calc ? (
                    <Body2 block>Выберите тариф и задайте параметры.</Body2>
                ) : (
                    <div className="flex flex-col gap-3">
                        <div className="rounded-2xl p-4 tc-tariff-forecast-box">
                            <div className="flex items-end justify-between gap-3 flex-wrap">
                                <div className="min-w-0">
                                    <Caption1>Ориентировочная сумма</Caption1>
                                    <Title2 block>{formatMoneyByN(calc.total)}</Title2>
                                </div>
                            </div>
                            <div className="grid grid-cols-2 gap-3">
                                <div>
                                    <Caption1>Списание</Caption1>
                                    <Title3 block>
                                        {selectedTariff.billingType === BillingTypeEnum.PerMinute
                                            ? `${calc.chargedMinutes} мин`
                                            : `${calc.chargedHours} ч (за ${calc.chargedMinutes} мин)`}
                                    </Title3>
                                </div>
                                <div>
                                    <Caption1>Ставка</Caption1>
                                    <Title3 block>
                                        {selectedTariff.billingType === BillingTypeEnum.PerMinute
                                            ? `${formatMoneyByN(calc.pricePerMinute)} / мин`
                                            : `${formatMoneyByN(calc.pricePerHour)} / час`}
                                    </Title3>
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </Card>
    );
};