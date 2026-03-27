import type {FC} from "react";
import {Body1, Body2, Divider, Subtitle2Stronger, Title1, Title3} from "@fluentui/react-components";
import {Info20Regular, Money20Regular, Sticker20Regular} from "@fluentui/react-icons";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import {BillingType as BillingTypeEnum, type BillingType} from "@app-types/tariff";
import {formatMoneyByN} from "@utility/formatMoney";

interface VisitDetailsCardProps {
    tariffName: string;
    billingType: BillingType;
    estimateTotal: number;
    estimateBreakdown: string;
}

export const VisitDetailsCard: FC<VisitDetailsCardProps> = ({
    tariffName,
    billingType,
    estimateTotal,
    estimateBreakdown,
}) => (
    <HoverTiltCard className="lg:col-span-8">
        <div className="flex flex-col gap-4">
            <div className="flex items-center gap-2">
                <Info20Regular/>
                <Subtitle2Stronger>Детали расчёта</Subtitle2Stronger>
            </div>
            <Divider/>
            <div>
                <div className="flex flex-col gap-3">
                    <div className="flex items-center justify-between gap-4 flex-wrap">
                        <div className="flex items-center gap-2">
                            <Sticker20Regular/>
                            <Body1 block>Тариф</Body1>
                        </div>
                        <Title3 block>{tariffName}</Title3>
                    </div>

                    <Divider/>

                    <div className="flex items-center justify-between gap-4 flex-wrap">
                        <div className="flex items-center gap-2">
                            <Money20Regular/>
                            <Body1 block>Тип</Body1>
                        </div>
                        <Title3 block>
                            {billingType === BillingTypeEnum.Hourly ? "Почасовой" : "Поминутный"}
                        </Title3>
                    </div>

                    <Divider/>

                    <div className="flex items-start justify-between gap-4">
                        <div className="flex items-center gap-2">
                            <Info20Regular/>
                            <Body1 block>Расчёт</Body1>
                        </div>

                        <div className="flex flex-col items-end gap-2 text-right">
                            <Title1 block>{formatMoneyByN(estimateTotal)}</Title1>
                            <Body1 block>{estimateBreakdown}</Body1>
                        </div>
                    </div>
                </div>
            </div>

            <Body2 block>
                {billingType === BillingTypeEnum.Hourly
                    ? "Почасовой: округление вверх до часа."
                    : "Поминутный: расчёт по минутам."}
            </Body2>
        </div>
    </HoverTiltCard>
);
