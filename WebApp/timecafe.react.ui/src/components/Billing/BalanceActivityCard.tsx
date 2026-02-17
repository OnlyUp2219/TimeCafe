import {
    Badge,
    Body1,
    Caption1,
    Card,
    Display,
    Divider,
    Subtitle2Stronger,
    Title3,
    tokens,
} from "@fluentui/react-components";

import {ResponsiveContainer, VerticalBarChart, type VerticalBarChartDataPoint} from "@fluentui/react-charts";

import type {BillingActivityPoint} from "@app-types/billing";
import {formatRub} from "@utility/formatRub";

type BalanceActivityCardProps = {
    balanceRub: number;
    monthDeltaPercent?: number;
    activity: BillingActivityPoint[];
};

export const BalanceActivityCard = ({balanceRub, monthDeltaPercent, activity}: BalanceActivityCardProps) => {
    const tickValues: Date[] = activity.map((p) => p.date);
    const formatTick = (date: Date) =>
        date.toLocaleDateString("ru-RU", {
            day: "2-digit",
            month: "2-digit",
        });

    const chartPoints: VerticalBarChartDataPoint[] = activity.flatMap((p) => {
        const deposits: VerticalBarChartDataPoint = {
            x: p.date,
            y: p.depositsRub,
            color: tokens.colorBrandBackground,
            legend: "Пополнения",
        };

        const withdrawals: VerticalBarChartDataPoint = {
            x: p.date,
            y: -Math.abs(p.withdrawalsRub),
            color: tokens.colorStatusDangerBackground3,
            legend: "Списание",
        };

        return [deposits, withdrawals];
    });

    const weekTotalRub = activity.reduce((acc, p) => acc + p.depositsRub - p.withdrawalsRub, 0);

    const deltaBadge =
        typeof monthDeltaPercent === "number" ? (
            <Badge appearance="tint" color={monthDeltaPercent >= 0 ? "success" : "danger"} size="large">
                {monthDeltaPercent >= 0 ? "↑" : "↓"} {Math.abs(monthDeltaPercent)}% за месяц
            </Badge>
        ) : null;

    return (
        <Card className="flex flex-col gap-6 sm:flex-row sm:items-stretch">
            <div className="flex flex-col gap-4 sm:min-w-[16rem]">
                <Body1 block className="uppercase">
                    Доступно для оплаты
                </Body1>
                <div className="flex flex-wrap items-baseline gap-3">
                    <Display truncate wrap={false}>
                        {formatRub(balanceRub, 0)}
                    </Display>
                    {deltaBadge}
                </div>
            </div>

            <div className="flex flex-col gap-4 w-full">
                <Subtitle2Stronger>Активность</Subtitle2Stronger>

                <ResponsiveContainer height={200}>
                    <VerticalBarChart
                        culture={typeof window !== "undefined" ? window.navigator.language : "ru-RU"}
                        data={chartPoints}
                        maxBarWidth={250}
                        hideLegend={true}
                        barWidth={"auto"}
                        roundCorners={true}
                        hideLabels={true}
                        roundedTicks={true}
                        enableGradient={true}
                        tickValues={tickValues}
                        useUTC={false}
                        rotateXAxisLables={true}
                        customDateTimeFormatter={formatTick}
                        onRenderCalloutPerDataPoint={(props) => {
                            if (!props) return null;

                            const isWithdrawal = props.legend === "Списание";
                            const date = props.x instanceof Date ? props.x : new Date(String(props.x));
                            const dateLabel = date.toLocaleDateString("ru-RU", {day: "2-digit", month: "2-digit"});

                            return (
                                <div className="flex items-center gap-3">
                                    <Badge
                                        appearance="filled"
                                        size="extra-small"
                                        shape="circular"
                                        color={isWithdrawal ? "danger" : "brand"}
                                    />

                                    <div className="flex flex-col justify-center gap-1">
                                        <Caption1 block>
                                            {props.legend}: {dateLabel}
                                        </Caption1>
                                        <Title3>{formatRub(Number(props.y), 0)}</Title3>
                                    </div>
                                </div>
                            );
                        }}
                    />
                </ResponsiveContainer>

                <Divider />

                <div className="flex flex-wrap justify-between items-center">
                    <Caption1>Всего за неделю</Caption1>
                    <Caption1>{formatRub(weekTotalRub, 0)}</Caption1>
                </div>
            </div>
        </Card>
    );
};
