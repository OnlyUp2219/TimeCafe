import {
    Body1,
    Body2,
    Card,
    MessageBar,
    MessageBarBody,
    Title2,
    Title3,
} from "@fluentui/react-components";
import {Payment20Regular, ArrowTrending20Regular} from "@fluentui/react-icons";
import {useComponentSize} from "@hooks/useComponentSize";

export const PaymentsPage = () => {
    const {sizes} = useComponentSize();

    return (
        <div>
            <div className="mb-6">
                <Title2>Платежи</Title2>
                <Body2 block className="mt-1">Управление платёжными операциями</Body2>
            </div>

            <MessageBar intent="info" className="mb-6">
                <MessageBarBody>
                    Страница платежей находится в разработке. Здесь будет отображаться история Stripe-платежей.
                </MessageBarBody>
            </MessageBar>

            <div className="flex gap-4 flex-wrap">
                <Card className="flex-1 min-w-[240px]" size={sizes.card}>
                    <div className="flex items-center gap-3">
                        <div className="text-2xl opacity-50"><Payment20Regular /></div>
                        <div>
                            <Body2 block>Stripe Checkout</Body2>
                            <Title3>—</Title3>
                        </div>
                    </div>
                    <Body1 block className="mt-2 text-gray-500">
                        Платежи через Stripe Checkout будут отображаться здесь после интеграции с Billing API.
                    </Body1>
                </Card>

                <Card className="flex-1 min-w-[240px]" size={sizes.card}>
                    <div className="flex items-center gap-3">
                        <div className="text-2xl opacity-50"><ArrowTrending20Regular /></div>
                        <div>
                            <Body2 block>Транзакции</Body2>
                            <Title3>—</Title3>
                        </div>
                    </div>
                    <Body1 block className="mt-2 text-gray-500">
                        Для просмотра транзакций перейдите в раздел «Транзакции».
                    </Body1>
                </Card>
            </div>
        </div>
    );
};
