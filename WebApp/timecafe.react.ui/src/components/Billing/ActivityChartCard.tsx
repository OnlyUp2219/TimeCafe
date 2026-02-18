import {
    Card,
    Caption1,
    Caption1Strong,
    Divider,
    Subtitle2Stronger,
    Title3,
    tokens,
} from "@fluentui/react-components";

import {ResponsiveContainer, VerticalBarChart, type VerticalBarChartDataPoint} from "@fluentui/react-charts";

import type {BillingActivityPoint} from "@app-types/billing";
import {formatRub} from "@utility/formatRub";

type ActivityChartCardProps = {
    points: BillingActivityPoint[];
};

export const ActivityChartCard = ({points}: ActivityChartCardProps) => {
    const tickValues: Date[] = points.map((p) => p.date);
    const getPointDate = (date: Date, hourOffset: number) => {
        const normalized = new Date(date);
        normalized.setHours(hourOffset, 0, 0, 0);
        return normalized;
    };
    const formatTick = (date: Date) =>
        date.toLocaleDateString("ru-RU", {
            day: "2-digit",
            month: "2-digit",
        });

    const chartPoints: VerticalBarChartDataPoint[] = points.flatMap((p) => {
        const deposits: VerticalBarChartDataPoint = {
            x: getPointDate(p.date, 8),
            y: p.depositsRub,
            color: tokens.colorBrandBackground,
            legend: "Пополнения",
        };

        const withdrawals: VerticalBarChartDataPoint = {
            x: getPointDate(p.date, 20),
            y: -Math.abs(p.withdrawalsRub),
            color: tokens.colorStatusDangerBackground3,
            legend: "Списание",
        };

        return [deposits, withdrawals];
    });

    const weekTotalRub = points.reduce((acc, p) => acc + p.depositsRub - p.withdrawalsRub, 0);

    return (
        <Card className="flex flex-col gap-4">
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

                        const date = props.x instanceof Date ? props.x : new Date(String(props.x));
                        const dateLabel = date.toLocaleDateString("ru-RU", {day: "2-digit", month: "2-digit"});

                        return (
                            <div className="flex flex-col gap-1">
                                <Caption1Strong block>
                                    {props.legend}: {dateLabel}
                                </Caption1Strong>
                                <Title3>{formatRub(Number(props.y), 0)}</Title3>
                            </div>
                        );
                    }}
                />
            </ResponsiveContainer>

            <Divider />

            <div className="flex flex-wrap justify-between items-center">
                <Caption1>Всего за неделю</Caption1>
                <Caption1Strong>{formatRub(weekTotalRub, 0)}</Caption1Strong>
            </div>
        </Card>
    );
};
