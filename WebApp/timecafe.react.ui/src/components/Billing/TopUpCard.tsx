import {Button, Card, Input, Title3} from "@fluentui/react-components";

import {Money20Regular, Add20Regular} from "@fluentui/react-icons";

import {TooltipButton} from "@components/TooltipButton/TooltipButton";

type TopUpCardProps = {
    draftAmountText: string;
    onDraftAmountTextChange: (value: string) => void;
    onPresetAdd: (deltaRub: number) => void;
    onSubmit: () => void;
    loading?: boolean;
};

export const TopUpCard = ({
    draftAmountText,
    onDraftAmountTextChange,
    onPresetAdd,
    onSubmit,
    loading = false,
}: TopUpCardProps) => {
    return (
        <Card className="flex flex-col gap-4">
            <Title3 block>Быстрое пополнение</Title3>
            <Input
                type="number"
                placeholder="Введите сумму (₽)"
                contentBefore={<Money20Regular />}
                className="w-full"
                value={draftAmountText}
                onChange={(_, data) => onDraftAmountTextChange(data.value)}
                disabled={loading}
            />
            <div className="flex flex-wrap gap-3">
                {[500, 1000, 2500, 5000].map((val) => (
                    <Button
                        key={val}
                        appearance="outline"
                        icon={<Add20Regular />}
                        onClick={() => onPresetAdd(val)}
                        disabled={loading}
                    >
                        +{val}
                    </Button>
                ))}
            </div>

            <TooltipButton
                appearance="primary"
                icon={<Add20Regular />}
                tooltip="Перейти к оплате Stripe"
                label={loading ? "Переход к Stripe..." : "Перейти к оплате Stripe"}
                onClick={onSubmit}
                disabled={loading}
            />
        </Card>
    );
};
