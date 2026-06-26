import type { FC } from "react";
import { Button, Caption1, Divider, Subtitle2Stronger, Title3 } from "@fluentui/react-components";
import { Money20Regular, Warning20Regular } from "@fluentui/react-icons";
import { Badge } from "@fluentui/react-components";
import { HoverTiltCard } from "@components/HoverTiltCard/HoverTiltCard";
import { formatByn } from "@utility/formatMoney";
import { useComponentSize } from "@hooks/useComponentSize";

interface BalanceCardProps {
    balanceRub: number;
    debtRub: number;
    loading: boolean;
    onNavigate: () => void;
}


export const BalanceCard: FC<BalanceCardProps> = ({ balanceRub, debtRub, loading, onNavigate }) => {
    const { sizes } = useComponentSize();

    return (
        <HoverTiltCard className="flex flex-col justify-between min-h-[240px]" size={sizes.card}>
            <div className="flex flex-col gap-4">
                <div className="flex items-center gap-2 flex-wrap">
                    <div className="flex items-center gap-2">
                        <Money20Regular className="text-(--colorStatusSuccessForeground1)" />
                        <Subtitle2Stronger>Баланс</Subtitle2Stronger>
                    </div>
                    {loading && <Badge appearance="tint" size={sizes.badge}>Загрузка</Badge>}
                </div>
                <Divider className="divider grow-0" />
            </div>

            <div className="flex items-end justify-between gap-3 flex-wrap">
                <div className="flex flex-col gap-1">
                    <Title3 className="font-semibold">{formatByn(balanceRub)}</Title3>
                    {debtRub > 0 ? (
                        <div className="flex items-center gap-1 text-(--colorStatusWarningForeground1)">
                            <Warning20Regular style={{ fontSize: "14px" }} className="shrink-0" />
                            <Caption1 className="font-medium text-(--colorStatusWarningForeground1)">
                                Есть задолженность: {formatByn(debtRub)}
                            </Caption1>
                        </div>
                    ) : (
                        <Caption1 className="text-(--colorNeutralForeground3)">
                            Доступно для оплаты визитов
                        </Caption1>
                    )}
                </div>
                <Button appearance="secondary" size={sizes.button} onClick={onNavigate}>
                    Открыть биллинг
                </Button>
            </div>
        </HoverTiltCard>
    );
};
