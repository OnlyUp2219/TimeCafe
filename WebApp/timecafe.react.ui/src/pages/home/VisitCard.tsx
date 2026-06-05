import type {FC} from "react";
import {
    Button,
    Caption1,
    Divider,
    Subtitle2Stronger,
    Tag,
    Text,
    Title3,
    Tooltip,
} from "@fluentui/react-components";
import {Clock20Regular, Timer20Regular, Receipt20Regular} from "@fluentui/react-icons";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import {formatDurationSeconds} from "@utility/formatDurationSeconds";
import {formatMoneyByN} from "@utility/formatMoney";
import {CURRENCY_SYMBOL} from "@shared/const/currency";
import {VisitStatus} from "@app-types/visit";
import {useComponentSize} from "@hooks/useComponentSize";

interface VisitCardProps {
    status?: number;
    elapsedSeconds: number;
    estimateTotal: number | null;
    visitInfo: string;
    onNavigateVisit: () => void;
    onNavigateBilling: () => void;
}

const statusConfig: Record<number, {label: string; appearance: "brand" | "outline" | "warning" | "success"}> = {
    [VisitStatus.Pending]: {label: "Ожидает", appearance: "warning"},
    [VisitStatus.Approved]: {label: "Подтверждён", appearance: "success"},
    [VisitStatus.Active]: {label: "Активен", appearance: "brand"},
};

export const VisitCard: FC<VisitCardProps> = ({
    status,
    elapsedSeconds,
    estimateTotal,
    visitInfo,
    onNavigateVisit,
    onNavigateBilling,
}) => {
    const { sizes } = useComponentSize();
    const cfg = status != null ? statusConfig[status] : null;
    const isActive = status === VisitStatus.Active;
    const hasVisit = status != null;

    return (
        <HoverTiltCard className="flex flex-col justify-between min-h-[240px]" size={sizes.card}>
            <div className="flex flex-col gap-4">
                <div className="flex items-center gap-2 flex-wrap">
                    <div className="flex items-center gap-2">
                        <Clock20Regular className={isActive ? "text-(--colorBrandForeground1) animate-pulse" : ""} />
                        <Subtitle2Stronger>Визит</Subtitle2Stronger>
                    </div>
                    <Tag appearance={cfg?.appearance ?? "outline"}>
                        {cfg?.label ?? "Нет визита"}
                    </Tag>
                </div>
                <Divider className="divider grow-0"/>
            </div>

            <div className="flex flex-row justify-between gap-4 flex-wrap">
                <div className="flex flex-col gap-1">
                    <div className="flex items-center gap-1.5 text-(--colorNeutralForeground3)">
                        <Timer20Regular style={{ fontSize: "14px" }} className="shrink-0 text-(--colorNeutralForeground3)" />
                        <Caption1>Длительность</Caption1>
                    </div>
                    <Title3 className="font-semibold mt-0.5">
                        {isActive ? formatDurationSeconds(elapsedSeconds) : "—:—"}
                    </Title3>
                </div>
                <div className="flex flex-col gap-1 text-right sm:items-end">
                    <div className="flex items-center gap-1.5 text-(--colorNeutralForeground3) sm:justify-end">
                        <Receipt20Regular style={{ fontSize: "14px" }} className="shrink-0 text-(--colorNeutralForeground3)" />
                        <Caption1>Оценка</Caption1>
                    </div>
                    <Title3 className="font-semibold mt-0.5 text-(--colorBrandForeground1)">
                        {isActive && estimateTotal != null ? formatMoneyByN(estimateTotal) : `— ${CURRENCY_SYMBOL}`}
                    </Title3>
                </div>
            </div>

            {isActive ? (
                <div className="flex items-center gap-1.5 text-(--colorNeutralForeground2)">
                    <span className="flex h-2 w-2 relative">
                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full opacity-75" style={{ backgroundColor: "var(--colorStatusSuccessForeground1)" }}></span>
                        <span className="relative inline-flex rounded-full h-2 w-2" style={{ backgroundColor: "var(--colorStatusSuccessForeground1)" }}></span>
                    </span>
                    <Caption1 className="font-medium text-(--colorNeutralForeground2)">{visitInfo}</Caption1>
                </div>
            ) : (
                <div className="flex items-center gap-1.5 text-(--colorNeutralForeground3)">
                    <span className="h-2 w-2 rounded-full" style={{ backgroundColor: "var(--colorNeutralStroke1)" }}></span>
                    <Caption1 className="text-(--colorNeutralForeground3)">{visitInfo}</Caption1>
                </div>
            )}

            <div className="flex flex-col gap-2 lg:flex-row">
                <Tooltip
                    content={hasVisit ? "Перейти к визиту" : "Начать визит"}
                    relationship="label"                >
                    <Button appearance="primary" size={sizes.button} data-testid="home-visit-action" onClick={onNavigateVisit}>
                        <Text truncate wrap={false}>
                            {hasVisit ? "Открыть" : "Начать"}
                        </Text>
                    </Button>
                </Tooltip>

                <Tooltip content="Открыть биллинг" relationship="label">
                        <Button appearance="secondary" size={sizes.button} onClick={onNavigateBilling}>
                            <Text truncate wrap={false}>История операций</Text>
                        </Button>
                </Tooltip>
            </div>
        </HoverTiltCard>
    );
};
