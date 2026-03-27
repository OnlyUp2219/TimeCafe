import type {FC} from "react";
import {Button, Caption1, Divider, Subtitle2Stronger, Title3} from "@fluentui/react-components";
import {Money20Regular} from "@fluentui/react-icons";
import {Badge} from "@fluentui/react-components";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import {formatRub} from "@utility/formatRub";

interface BalanceCardProps {
    balanceRub: number;
    debtRub: number;
    loading: boolean;
    onNavigate: () => void;
}

export const BalanceCard: FC<BalanceCardProps> = ({balanceRub, debtRub, loading, onNavigate}) => (
    <HoverTiltCard className="flex flex-col justify-between">
        <div className="flex flex-col gap-4">
            <div className="flex items-center gap-2 flex-wrap">
                <div className="flex items-center gap-2">
                    <Money20Regular/>
                    <Subtitle2Stronger>Баланс</Subtitle2Stronger>
                </div>
                {loading && <Badge appearance="tint">Загрузка</Badge>}
            </div>
            <Divider className="divider grow-0"/>
        </div>

        <div className="flex items-end justify-between gap-3 flex-wrap">
            <div className="flex flex-col gap-1">
                <Title3>{formatRub(balanceRub, 0)}</Title3>
                <Caption1>
                    {debtRub > 0
                        ? `Есть задолженность: ${formatRub(debtRub, 0)}`
                        : "Доступно для оплаты визитов"}
                </Caption1>
            </div>
            <Button appearance="secondary" onClick={onNavigate}>
                Открыть биллинг
            </Button>
        </div>
    </HoverTiltCard>
);
