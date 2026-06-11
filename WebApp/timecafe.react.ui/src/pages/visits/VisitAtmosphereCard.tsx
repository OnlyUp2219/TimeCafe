import { NO_DATA } from "@shared/const/placeholders";
import type {FC} from "react";import {Body1, Divider, Subtitle2Stronger, tokens} from "@fluentui/react-components";import {Info20Regular} from "@fluentui/react-icons";import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";import {BillingType as BillingTypeEnum, type BillingType} from "@app-types/tariff";import {useComponentSize} from "@hooks/useComponentSize";import {formatMoneyByN} from "@utility/formatMoney";

interface VisitAtmosphereCardProps {
    billingType: BillingType;
    tariffPricePerMinute: number;
    minSessionMinutes: number | null;
    roundingRule: string | null;
    guestsCount: number;
}

export const VisitAtmosphereCard: FC<VisitAtmosphereCardProps> = ({
    billingType,
    tariffPricePerMinute,
    minSessionMinutes,
    roundingRule,
    guestsCount,
}) => {
    const { sizes } = useComponentSize();

    const priceLabel = billingType === BillingTypeEnum.Hourly
        ? `${formatMoneyByN(tariffPricePerMinute * 60)} / час`
        : `${formatMoneyByN(tariffPricePerMinute)} / мин`;

    const roundingLabel = roundingRule === "FiveMinutes"
        ? "5 минут"
        : roundingRule === "FifteenMinutes"
        ? "15 минут"
        : roundingRule === "SixtyMinutes"
        ? "60 минут"
        : "Без округления";

    return (
        <HoverTiltCard className="lg:col-span-4 h-full flex flex-col" size={sizes.card}>
            <div className="flex flex-col gap-4 h-full flex-1 justify-between">
                <div className="flex flex-col gap-4 flex-1">
                    <div className="flex items-center gap-2">
                        <Info20Regular/>
                        <Subtitle2Stronger>Параметры тарифа</Subtitle2Stronger>
                    </div>
     
                    <div className="flex flex-col gap-3">
                        <div className="flex justify-between items-center">
                            <Body1>Стоимость:</Body1>
                            <Body1 className="font-semibold">{priceLabel}</Body1>
                        </div>
                        <Divider/>
                        <div className="flex justify-between items-center">
                            <Body1>Минимум:</Body1>
                            <Body1 className="font-semibold">{minSessionMinutes ? `${minSessionMinutes} мин.` : NO_DATA}</Body1>
                        </div>
                        <Divider/>
                        <div className="flex justify-between items-center">
                            <Body1>Округление:</Body1>
                            <Body1 className="font-semibold">{roundingLabel}</Body1>
                        </div>
                        <Divider/>
                        <div className="flex justify-between items-center">
                            <Body1>Количество гостей:</Body1>
                            <Body1 className="font-semibold">{guestsCount} чел.</Body1>
                        </div>
                    </div>
                </div>
            </div>
        </HoverTiltCard>
    );
};
