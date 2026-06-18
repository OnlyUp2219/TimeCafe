import type { FC } from "react";
import {
    Button,
    Card,
    Title2,
    Body1,
    Body2,
    Divider,
    LargeTitle,
    Subtitle2Stronger,
    MessageBar,
    MessageBarBody,
    Tag,
    tokens,
} from "@fluentui/react-components";
import { Clock20Regular, Clock24Regular, Dismiss24Regular, Money20Regular, Money24Regular, Receipt20Regular, DoorArrowRight20Regular } from "@fluentui/react-icons";
import { HoverTiltCard } from "@components/HoverTiltCard/HoverTiltCard";
import { formatMoneyByN } from "@utility/formatMoney";

const Body2Stronger = (props: React.ComponentProps<typeof Body2>) => (
    <Body2 {...props} style={{ fontWeight: tokens.fontWeightSemibold, ...props.style }} />
);

interface WaitingForPaymentStateProps {
    invoice: any;
    loadingInvoice: boolean;
    userBalance: any;
    sizes: any;
    userId: string | null;
    payingInvoice: boolean;
    initializingStripe: boolean;
    personalDiscount: number;
    globalDiscount: number;
    tariffDiscount: number;
    estimate: any;

    onRetryInvoice: () => void;
    onPayFromBalance: () => void;
    onPayViaStripe: () => void;
    onGoToStart: () => void;
    onShowReceipt: () => void;
}

export const WaitingForPaymentState: FC<WaitingForPaymentStateProps> = ({
    invoice,
    loadingInvoice,
    userBalance,
    sizes,
    userId,
    payingInvoice,
    initializingStripe,
    personalDiscount,
    globalDiscount,
    tariffDiscount,
    estimate,
    onRetryInvoice,
    onPayFromBalance,
    onPayViaStripe,
    onGoToStart,
    onShowReceipt,
}) => {
    if (loadingInvoice) {
        return (
            <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center">
                <Clock24Regular className="opacity-50 animate-spin" />
                <Title2>Загрузка счёта...</Title2>
                <Body2>Пожалуйста, подождите, мы получаем информацию об инвойсе.</Body2>
            </Card>
        );
    }

    if (!invoice) {
        return (
            <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center">
                <Dismiss24Regular style={{ color: tokens.colorPaletteRedForeground1, opacity: 0.5 }} />
                <Title2>Счёт не найден</Title2>
                <Body2>Не удалось загрузить информацию о счёте для оплаты визита.</Body2>
                <Button onClick={onRetryInvoice}>Попробовать снова</Button>
            </Card>
        );
    }

    const isPaid = invoice.status === 2;
    if (isPaid) {
        let receiptButton = null;
        if (invoice.fiscalReceiptNumber) {
            receiptButton = (
                <Button size={sizes.button} appearance="outline" icon={<Receipt20Regular />} onClick={onShowReceipt}>
                    Показать чек
                </Button>
            );
        }

        return (
            <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center">
                <Clock24Regular className="text-(--colorPaletteGreenForeground1)" />
                <Title2>Визит успешно оплачен!</Title2>
                <Body2>Благодарим вас за визит! Ваш столик освобожден. Будем рады видеть вас снова!</Body2>
                <div className="flex gap-2 flex-wrap justify-center">
                    <Button appearance="primary" size={sizes.button} onClick={onGoToStart}>
                        К выбору тарифа
                    </Button>
                    {receiptButton}
                </div>
            </Card>
        );
    }

    const balanceAmount = userBalance?.currentBalance ?? 0;
    const canPayWithBalance = balanceAmount >= invoice.totalAmount;

    let gridColsClass = "grid grid-cols-1 gap-4";
    if (userId) {
        gridColsClass = "grid grid-cols-1 md:grid-cols-2 gap-4";
    }

    let balanceCard = null;
    if (userId) {
        balanceCard = (
            <HoverTiltCard size={sizes.card} className="flex flex-col gap-3 justify-between">
                <div className="flex flex-col gap-1">
                    <Subtitle2Stronger>С внутреннего баланса</Subtitle2Stronger>
                    <Body2>Быстрая оплата в одно нажатие.</Body2>
                    <div className="bg-(--colorNeutralBackground2) p-2 rounded flex justify-between items-center mt-2">
                        <Body2>Ваш баланс:</Body2>
                        <Body2Stronger style={{ color: canPayWithBalance ? tokens.colorPaletteGreenForeground1 : tokens.colorPaletteRedForeground1 }}>
                            {formatMoneyByN(balanceAmount)}
                        </Body2Stronger>
                    </div>
                </div>
                <Button
                    appearance="primary"
                    onClick={onPayFromBalance}
                    disabled={!canPayWithBalance || payingInvoice}
                    icon={<Money20Regular />}
                >
                    Оплатить с баланса
                </Button>
            </HoverTiltCard>
        );
    }

    return (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
            <Card className="lg:col-span-8" size={sizes.card}>
                <div className="flex flex-col gap-4">
                    <div className="flex items-center gap-2">
                        <Money24Regular style={{ color: tokens.colorBrandForeground1 }} />
                        <Subtitle2Stronger>Счёт на оплату визита</Subtitle2Stronger>
                    </div>
                    <Divider />

                    <div className="flex flex-col gap-3">
                        <div className="flex justify-between items-baseline bg-(--colorNeutralBackground1) p-2 flex-wrap gap-2">
                            <Body1>Итого к оплате:</Body1>
                            <div className="flex items-baseline gap-2 flex-wrap">
                                {estimate.isDiscounted ? (
                                    <>
                                        <LargeTitle style={{ color: tokens.colorBrandForeground1 }}>
                                            {formatMoneyByN(estimate.total)}
                                        </LargeTitle>
                                        <Body1 className="line-through" style={{ color: tokens.colorNeutralForeground3 }}>
                                            {formatMoneyByN(estimate.baseTotal)}
                                        </Body1>
                                        <Tag appearance="brand" color="success">
                                            -{estimate.appliedDiscountPercent}%
                                        </Tag>
                                    </>
                                ) : (
                                    <LargeTitle style={{ color: tokens.colorBrandForeground1 }}>
                                        {formatMoneyByN(estimate.total)}
                                    </LargeTitle>
                                )}
                            </div>
                        </div>

                        <Card size="small" style={{ backgroundColor: tokens.colorNeutralBackground2, borderColor: tokens.colorNeutralStroke2 }}>
                            <div className="flex justify-between items-center">
                                <Body2 style={{ color: tokens.colorNeutralForeground3 }}>ID счёта:</Body2>
                                <Body2 style={{ fontFamily: "monospace" }}>{invoice.invoiceId}</Body2>
                            </div>
                            <div className="flex justify-between items-center">
                                <Body2 style={{ color: tokens.colorNeutralForeground3 }}>Время создания:</Body2>
                                <Body2>{new Date(invoice.createdAt).toLocaleString()}</Body2>
                            </div>
                        </Card>

                        <Card size="small" style={{ backgroundColor: tokens.colorNeutralBackground3, borderColor: tokens.colorNeutralStroke3 }}>
                            <div className="flex justify-between">
                                <Body2Stronger style={{ color: tokens.colorNeutralForeground2 }}>Базовая стоимость:</Body2Stronger>
                                <Body2>{formatMoneyByN(estimate.baseTotal)}</Body2>
                            </div>
                            <div className="flex justify-between">
                                <Body2Stronger style={{ color: tokens.colorNeutralForeground2 }}>Скидка лояльности:</Body2Stronger>
                                <Body2>-{personalDiscount}%</Body2>
                            </div>
                            <div className="flex justify-between">
                                <Body2Stronger style={{ color: tokens.colorNeutralForeground2 }}>Акционная скидка:</Body2Stronger>
                                <Body2>-{Math.max(globalDiscount, tariffDiscount)}%</Body2>
                            </div>
                            {estimate.isDiscounted && (
                                <>
                                    <Divider />
                                    <div className="flex justify-between">
                                        <Body2Stronger style={{ color: tokens.colorBrandForeground1 }}>Итоговая скидка:</Body2Stronger>
                                        <Body2 style={{ color: tokens.colorBrandForeground1 }}>
                                            -{estimate.appliedDiscountPercent}% (-{formatMoneyByN(estimate.discountTotal)})
                                        </Body2>
                                    </div>
                                </>
                            )}
                        </Card>
                    </div>

                    <Divider />
                    <div className="flex flex-col gap-3">
                        <Subtitle2Stronger>Способы оплаты</Subtitle2Stronger>

                        <div className={gridColsClass}>
                            {balanceCard}

                            <HoverTiltCard size={sizes.card} className="flex flex-col gap-3 justify-between">
                                <div className="flex flex-col gap-1">
                                    <Subtitle2Stronger>Банковской картой (Stripe)</Subtitle2Stronger>
                                    <Body2>Безопасная онлайн-оплата через Stripe Checkout.</Body2>
                                </div>
                                <Button
                                    appearance="primary"
                                    onClick={onPayViaStripe}
                                    disabled={initializingStripe}
                                    icon={<DoorArrowRight20Regular />}
                                >
                                    Оплатить картой онлайн
                                </Button>
                            </HoverTiltCard>
                        </div>
                    </div>
                </div>
            </Card>

            <HoverTiltCard className="lg:col-span-4" size={sizes.card}>
                <div className="flex flex-col gap-4">
                    <div className="flex items-center gap-2">
                        <Clock20Regular />
                        <Subtitle2Stronger>Оплата на кассе</Subtitle2Stronger>
                    </div>
                    <Divider />
                    <Body2>
                        Вы также можете подойти к стойке администратора и произвести оплату наличными средствами или через банковский терминал.
                    </Body2>
                    <MessageBar intent="warning">
                        <MessageBarBody>
                            После фиксации администратором оплаты на кассе, ваш визит автоматически перейдет в статус завершенного, а столик освободится.
                        </MessageBarBody>
                    </MessageBar>
                </div>
            </HoverTiltCard>
        </div>
    );
};
