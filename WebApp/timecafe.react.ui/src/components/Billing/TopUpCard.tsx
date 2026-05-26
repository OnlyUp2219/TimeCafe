import {Button, Card, Input, Title3} from "@fluentui/react-components";
import {Money20Regular, Add20Regular} from "@fluentui/react-icons";
import {TooltipButton} from "@components/TooltipButton/TooltipButton";
import {CURRENCY_SYMBOL} from "@shared/const/currency";
import {useComponentSize} from "@hooks/useComponentSize";

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
    const { sizes } = useComponentSize();

    return (
        <Card className="flex h-full flex-col gap-4" size={sizes.card}>
            <Title3 block>Быстрое пополнение</Title3>
            <Input
                type="number"
                placeholder={`Введите сумму (${CURRENCY_SYMBOL})`}
                contentBefore={<Money20Regular />}
                className="w-full"
                value={draftAmountText}
                onChange={(_, data) => onDraftAmountTextChange(data.value)}
                disabled={loading}
                size={sizes.input}
            />
            <div className="flex flex-wrap gap-3">
                {[100, 200, 500, 1000].map((val) => (
                    <Button
                        key={val}
                        appearance="outline"
                        icon={<Add20Regular />}
                        onClick={() => onPresetAdd(val)}
                        disabled={loading}
                        size={sizes.button}
                    >
                        +{val}
                    </Button>
                ))}
            </div>

            <TooltipButton
                appearance="primary"
                icon={<Add20Regular />}
                tooltip="Перейти к оплате Stripe"
                label={loading ? "Подготавливаем Stripe Checkout..." : "Перейти к оплате Stripe"}
                onClick={onSubmit}
                disabled={loading}
                size={sizes.button}
            />
        </Card>
    );
};
