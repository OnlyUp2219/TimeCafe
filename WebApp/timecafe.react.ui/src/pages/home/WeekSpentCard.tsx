import type {FC} from "react";
import {Button, Caption1, Divider, Subtitle2Stronger, Title3} from "@fluentui/react-components";
import {ArrowTrendingLines20Regular, Calendar20Regular} from "@fluentui/react-icons";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import {formatByn} from "@utility/formatMoney";
import {useComponentSize} from "@hooks/useComponentSize";

interface WeekSpentCardProps {
    spent: number;
    onNavigate: () => void;
}

export const WeekSpentCard: FC<WeekSpentCardProps> = ({spent, onNavigate}) => {
    const { sizes } = useComponentSize();

    return (
        <HoverTiltCard className="flex flex-col justify-between min-h-[240px]" size={sizes.card}>
            <div className="flex flex-col gap-4">
                <div className="flex items-center gap-2 flex-wrap">
                    <div className="flex items-center gap-2">
                        <ArrowTrendingLines20Regular className="text-(--colorBrandForeground1)" />
                        <Subtitle2Stronger>Неделя</Subtitle2Stronger>
                    </div>
                </div>
                <Divider className="divider grow-0"/>
            </div>

            <div className="flex flex-col gap-1.5">
                <Title3 className="font-semibold">{formatByn(spent)}</Title3>
                <div className="flex items-center gap-1.5 text-(--colorNeutralForeground3)">
                    <Calendar20Regular style={{ fontSize: "14px" }} className="shrink-0 text-(--colorNeutralForeground3)" />
                    <Caption1 className="text-(--colorNeutralForeground3)">Расходы за последние 7 дней</Caption1>
                </div>
            </div>
            <div className="mt-4">
                <Button appearance="primary" size={sizes.button} className="w-full" onClick={onNavigate}>
                    Перейти в биллинг
                </Button>
            </div>
        </HoverTiltCard>
    );
};
