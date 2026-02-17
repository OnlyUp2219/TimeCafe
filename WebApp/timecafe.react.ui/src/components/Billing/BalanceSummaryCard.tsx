import {Body1, Caption1, Card, Display} from "@fluentui/react-components";

import {formatRub} from "@utility/formatRub";

type BalanceSummaryCardProps = {
    balanceRub: number;
    monthDeltaPercent?: number;
};

export const BalanceSummaryCard = ({balanceRub, monthDeltaPercent}: BalanceSummaryCardProps) => {
    const deltaText =
        typeof monthDeltaPercent === "number"
            ? `${monthDeltaPercent >= 0 ? "↑" : "↓"} ${Math.abs(monthDeltaPercent)}% за месяц`
            : null;

    return (
        <Card className="flex flex-col gap-3">
            <Body1 block className="uppercase">
                Доступно для оплаты
            </Body1>

            <div className="flex flex-wrap items-baseline gap-3">
                <Display truncate wrap={false}>
                    {formatRub(balanceRub, 0)}
                </Display>
                {deltaText ? <Caption1 block>{deltaText}</Caption1> : null}
            </div>
        </Card>
    );
};
