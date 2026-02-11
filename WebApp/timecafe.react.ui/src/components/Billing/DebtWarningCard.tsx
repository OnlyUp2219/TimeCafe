import {Badge, Body1, Card, Title3} from "@fluentui/react-components";

import {Warning20Filled} from "@fluentui/react-icons";

import {TooltipButton} from "@components/TooltipButton/TooltipButton";
import {formatRub} from "@pages/billing/billing.mock";

import "@pages/billing/billing.css";


type DebtWarningCardProps = {
    debtRub: number;
    onPay: () => void;
};

export const DebtWarningCard = ({debtRub, onPay}: DebtWarningCardProps) => {
    if (debtRub <= 0) return null;

    return (
        <Card
            appearance="subtle"
            className="border-2 border-dashed flex flex-col sm:flex-row items-center justify-between gap-4 tc-billing-debt-card"
        >
            <div className="flex items-center gap-4">
                <Badge
                    appearance="tint"
                    color="danger"
                    size="large"
                    shape="circular"
                    className="tc-billing-debt-badge"
                    icon={<Warning20Filled aria-label="Внимание" />}
                />
                <div className="flex flex-col gap-1">
                    <Title3 block>Внимание: задолженность</Title3>
                    <Body1 block>
                        Ваш баланс ниже нуля. Пожалуйста, погасите долг {formatRub(debtRub, 0)}
                    </Body1>
                </div>
            </div>

            <TooltipButton
                appearance="primary"
                tooltip="Погасить задолженность"
                label="Погасить сейчас"
                onClick={onPay}
            />
        </Card>
    );
};
