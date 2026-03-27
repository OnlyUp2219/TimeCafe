import type {FC} from "react";
import {Button, Caption1, Divider, Subtitle2Stronger, Title3} from "@fluentui/react-components";
import {ArrowTrendingLines20Regular} from "@fluentui/react-icons";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import {formatRub} from "@utility/formatRub";

interface WeekSpentCardProps {
    spent: number;
    onNavigate: () => void;
}

export const WeekSpentCard: FC<WeekSpentCardProps> = ({spent, onNavigate}) => (
    <HoverTiltCard className="flex flex-col justify-between">
        <div className="flex flex-col gap-4">
            <div className="flex items-center gap-2 flex-wrap">
                <div className="flex items-center gap-2">
                    <ArrowTrendingLines20Regular/>
                    <Subtitle2Stronger>Неделя</Subtitle2Stronger>
                </div>
            </div>
            <Divider className="divider grow-0"/>
        </div>

        <div className="flex flex-col gap-1">
            <Title3>{formatRub(spent, 0)}</Title3>
            <Caption1>Расходы за последние 7 дней</Caption1>
        </div>
        <div className="mt-4">
            <Button appearance="secondary" className="w-full" onClick={onNavigate}>
                Перейти в биллинг
            </Button>
        </div>
    </HoverTiltCard>
);
