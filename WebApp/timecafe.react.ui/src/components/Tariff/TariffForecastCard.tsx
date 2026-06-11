import {Body2, Caption1, Card, Divider, Title2, Title3} from "@fluentui/react-components";
import {formatMoneyByN} from "@utility/formatMoney";
import type {TariffForecastCardProps} from "@components/Tariff/TariffForecastCardProps";
import {BillingType as BillingTypeEnum} from "@app-types/tariff";
import {useComponentSize} from "@hooks/useComponentSize";

export type CalcResult = {
    total: number;
    chargedMinutes: number;
    chargedHours: number;
    pricePerMinute: number;
    pricePerHour: number;
};


export const TariffForecastCard = ({selectedTariff, calc}: TariffForecastCardProps) => {
    const { sizes } = useComponentSize();

    return (
        <Card className="lg:col-span-5 h-full flex flex-col" size={sizes.card}>
            <div className="flex flex-col gap-4 h-full flex-1">
                <div className="flex flex-col gap-4">
                    <div className="flex items-center justify-between gap-3 flex-wrap">
                        <Title3>Калькулятор</Title3>
                    </div>
                    <Divider/>
                </div>

                {!selectedTariff || !calc ? (
                    <div className="flex-1 flex items-center justify-center min-h-[150px]">
                        <Body2 block>Выберите тариф и задайте параметры.</Body2>
                    </div>
                ) : (
                    <div className="flex-1 flex flex-col justify-end gap-3 mt-4">
                        <div className="rounded-2xl p-4 bg-(--colorNeutralBackground2) border border-(--colorNeutralStroke1) flex flex-col justify-center">
                            <div className="flex items-end justify-between gap-3 flex-wrap mb-4">
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
                                            : `${calc.chargedHours} ч`}
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
