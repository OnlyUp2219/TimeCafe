import type {FC} from "react";
import {Body1, Body2, Divider, Subtitle2Stronger, Title1, Title3} from "@fluentui/react-components";
import {Info20Regular, Money20Regular, Sticker20Regular} from "@fluentui/react-icons";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import {BillingType as BillingTypeEnum, type BillingType} from "@app-types/tariff";
import {formatMoneyByN} from "@utility/formatMoney";
import {useComponentSize} from "@hooks/useComponentSize";

interface VisitDetailsCardProps {
    tariffName: string;
    billingType: BillingType;
    estimate: {
        total: number;
        breakdown: string;
        baseTotal: number;
        discountTotal: number;
        appliedDiscountPercent: number;
        isDiscounted: boolean;
    };
    personalDiscount: number;
    globalDiscount: number;
    tariffDiscount: number;
}

export const VisitDetailsCard: FC<VisitDetailsCardProps> = ({
    tariffName,
    billingType,
    estimate,
    personalDiscount,
    globalDiscount,
    tariffDiscount,
}) => {
    const { sizes } = useComponentSize();

    return (
        <HoverTiltCard className="lg:col-span-8" size={sizes.card}>
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
                                {estimate.isDiscounted && (
                                    <Title3 block className="line-through text-(--colorNeutralForeground3)">
                                        {formatMoneyByN(estimate.baseTotal)}
                                    </Title3>
                                )}
                                <Title1 block className={estimate.isDiscounted ? "text-(--colorBrandForeground1)" : ""}>
                                    {formatMoneyByN(estimate.total)}
                                </Title1>
                                <Body1 block className="text-(--colorNeutralForeground3)">{estimate.breakdown}</Body1>
                                
                                {estimate.isDiscounted && (
                                    <div className="flex flex-col gap-1 text-xs bg-(--colorNeutralBackground3) p-2 rounded border border-(--colorNeutralStroke3) mt-1 w-full text-left">
                                        {personalDiscount > 0 && (
                                            <div className="flex justify-between text-(--colorNeutralForeground2)">
                                                <span>Скидка лояльности:</span>
                                                <span className="font-semibold">-{personalDiscount}%</span>
                                            </div>
                                        )}
                                        {Math.max(globalDiscount, tariffDiscount) > 0 && (
                                            <div className="flex justify-between text-(--colorNeutralForeground2)">
                                                <span>Акционная скидка:</span>
                                                <span className="font-semibold">-{Math.max(globalDiscount, tariffDiscount)}%</span>
                                            </div>
                                        )}
                                        <Divider className="my-1" />
                                        <div className="flex justify-between text-(--colorBrandForeground1) font-semibold">
                                            <span>Итоговая скидка:</span>
                                            <span>-{estimate.appliedDiscountPercent}% (-{formatMoneyByN(estimate.discountTotal)})</span>
                                        </div>
                                    </div>
                                )}
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
};
