import type {FC} from "react";
import {
    Body2,
    Button,
    Caption1,
    Divider,
    Subtitle2Stronger,
    Tag,
    Text,
    Title3,
    Tooltip,
} from "@fluentui/react-components";
import {Clock20Regular} from "@fluentui/react-icons";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import {formatDurationSeconds} from "@utility/formatDurationSeconds";
import {formatMoneyByN} from "@utility/formatMoney";

interface VisitCardProps {
    isActive: boolean;
    elapsedSeconds: number;
    estimateTotal: number | null;
    visitInfo: string;
    onNavigateVisit: () => void;
    onNavigateBilling: () => void;
}

export const VisitCard: FC<VisitCardProps> = ({
    isActive,
    elapsedSeconds,
    estimateTotal,
    visitInfo,
    onNavigateVisit,
    onNavigateBilling,
}) => (
    <HoverTiltCard className="flex flex-col justify-between">
        <div className="flex flex-col gap-4">
            <div className="flex items-center gap-2 flex-wrap">
                <div className="flex items-center gap-2">
                    <Clock20Regular/>
                    <Subtitle2Stronger>Визит</Subtitle2Stronger>
                </div>
                <Tag size="small" appearance={isActive ? "brand" : "outline"}>
                    {isActive ? "Активен" : "Нет визита"}
                </Tag>
            </div>
            <Divider className="divider grow-0"/>
        </div>

        <div className="flex flex-row justify-between gap-4 flex-wrap">
            <div className="flex flex-col gap-1">
                <Caption1>Длительность</Caption1>
                <Title3>
                    {isActive ? formatDurationSeconds(elapsedSeconds) : "—:—"}
                </Title3>
            </div>
            <div className="flex flex-col gap-1 text-right">
                <Caption1>Оценка</Caption1>
                <Title3>
                    {isActive && estimateTotal != null ? formatMoneyByN(estimateTotal) : "— ₽"}
                </Title3>
            </div>
        </div>

        <Caption1>{visitInfo}</Caption1>

        <div className="flex flex-col gap-2 lg:flex-row">
            <Tooltip
                content={isActive ? "Перейти к активному визиту" : "Начать визит"}
                relationship="label"
            >
                <Button appearance="primary" data-testid="home-visit-action" onClick={onNavigateVisit}>
                    <Text truncate wrap={false}>
                        {isActive ? "Открыть" : "Начать"}
                    </Text>
                </Button>
            </Tooltip>

            <Tooltip content="Открыть биллинг" relationship="label">
                <span>
                    <Button appearance="secondary" onClick={onNavigateBilling}>
                        <Text truncate wrap={false}>История операций</Text>
                    </Button>
                </span>
            </Tooltip>
        </div>
    </HoverTiltCard>
);
