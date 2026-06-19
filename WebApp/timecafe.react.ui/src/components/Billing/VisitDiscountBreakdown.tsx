import type { FC } from "react";
import {
    Body1,
    Body2,
    Caption1,
    Divider,
    Body1Stronger,
    tokens,
    Card,
} from "@fluentui/react-components";
import { Calculator20Regular } from "@fluentui/react-icons";
import { formatMoneyByN } from "@utility/formatMoney";

export interface VisitDiscountBreakdownProps {
    estimate: {
        total: number;
        baseTotal: number;
        isDiscounted: boolean;
        appliedDiscountPercent: number;
        discountTotal: number;
        breakdown?: string;
    };
    personalDiscount: number;
    globalDiscount: number;
    tariffDiscount: number;
    balance?: number;
    variant?: "full" | "compact" | "simple";
}

const DiscountDetails: FC<{
    personalDiscount: number;
    activeDiscount: number;
    appliedDiscountPercent: number;
    discountTotal: number;
    className?: string;
}> = ({ personalDiscount, activeDiscount, appliedDiscountPercent, discountTotal, className }) => (
    <div className={`flex flex-col gap-1 text-xs bg-(--colorNeutralBackground3) p-2 rounded border border-(--colorNeutralStroke3) ${className || ""}`}>
        {personalDiscount > 0 && (
            <div className="flex justify-between items-center text-(--colorNeutralForeground2)">
                <Body1>Скидка лояльности:</Body1>
                <Body1Stronger>-{personalDiscount}%</Body1Stronger>
            </div>
        )}
        {activeDiscount > 0 && (
            <div className="flex justify-between items-center text-(--colorNeutralForeground2)">
                <Body1>Акционная скидка:</Body1>
                <Body1Stronger>-{activeDiscount}%</Body1Stronger>
            </div>
        )}
        <Divider className="my-1" />
        <div className="flex justify-between items-center text-(--colorBrandForeground1)">
            <Body1>Итоговая скидка:</Body1>
            <Body1Stronger>-{appliedDiscountPercent}% (-{formatMoneyByN(discountTotal)})</Body1Stronger>
        </div>
    </div>
);

export const VisitDiscountBreakdown: FC<VisitDiscountBreakdownProps> = ({
    estimate,
    personalDiscount,
    globalDiscount,
    tariffDiscount,
    balance,
    variant = "full",
}) => {
    const activeDiscount = Math.max(globalDiscount, tariffDiscount);

    if (variant === "simple") {
        return (
            <DiscountDetails
                personalDiscount={personalDiscount}
                activeDiscount={activeDiscount}
                appliedDiscountPercent={estimate.appliedDiscountPercent}
                discountTotal={estimate.discountTotal}
                className="mt-1 w-full text-left"
            />
        );
    }

    if (variant === "compact") {
        return (
            <Card size="small" style={{ backgroundColor: tokens.colorNeutralBackground3, borderColor: tokens.colorNeutralStroke3 }}>
                <div className="flex justify-between">
                    <Body1Stronger style={{ color: tokens.colorNeutralForeground2 }}>Базовая стоимость:</Body1Stronger>
                    <Body2>{formatMoneyByN(estimate.baseTotal)}</Body2>
                </div>
                <div className="flex justify-between">
                    <Body1Stronger style={{ color: tokens.colorNeutralForeground2 }}>Скидка лояльности:</Body1Stronger>
                    <Body2>-{personalDiscount}%</Body2>
                </div>
                <div className="flex justify-between">
                    <Body1Stronger style={{ color: tokens.colorNeutralForeground2 }}>Акционная скидка:</Body1Stronger>
                    <Body2>-{activeDiscount}%</Body2>
                </div>
                {estimate.isDiscounted && (
                    <>
                        <Divider />
                        <div className="flex justify-between">
                            <Body1Stronger style={{ color: tokens.colorBrandForeground1 }}>Итоговая скидка:</Body1Stronger>
                            <Body2 style={{ color: tokens.colorBrandForeground1 }}>
                                -{estimate.appliedDiscountPercent}% (-{formatMoneyByN(estimate.discountTotal)})
                            </Body2>
                        </div>
                    </>
                )}
            </Card>
        );
    }

    return (
        <div className="bg-(--colorNeutralBackground2) p-3 rounded-lg flex flex-col gap-2 border border-(--colorBrandStroke1)">
            <Body2 className="flex items-center gap-2 font-semibold">
                <Calculator20Regular className="text-(--colorBrandForeground1)" />
                Ожидаемая стоимость
            </Body2>
            <Divider className="my-1" />
            <div className="flex items-center justify-between text-sm">
                <Caption1 className="text-(--colorNeutralForeground3)">{estimate.breakdown}</Caption1>
                <div className="flex items-center gap-1.5">
                    {estimate.isDiscounted && (
                        <span className="line-through text-(--colorNeutralForeground3) text-xs">
                            {formatMoneyByN(estimate.baseTotal)}
                        </span>
                    )}
                    <Body1 className="font-semibold text-(--colorBrandForeground1)">
                        {formatMoneyByN(estimate.total)}
                    </Body1>
                </div>
            </div>
            {estimate.isDiscounted && (
                <DiscountDetails
                    personalDiscount={personalDiscount}
                    activeDiscount={activeDiscount}
                    appliedDiscountPercent={estimate.appliedDiscountPercent}
                    discountTotal={estimate.discountTotal}
                    className="mt-1"
                />
            )}
            {balance !== undefined && (
                <div className="flex items-center justify-between text-sm mt-1">
                    <Caption1 className="text-(--colorNeutralForeground3)">Баланс после списания:</Caption1>
                    <Body1 className={`font-semibold ${balance - estimate.total < 0 ? "text-(--colorPaletteRedForeground1)" : "text-(--colorPaletteGreenForeground1)"}`}>
                        {formatMoneyByN(balance - estimate.total)}
                    </Body1>
                </div>
            )}
        </div>
    );
};
