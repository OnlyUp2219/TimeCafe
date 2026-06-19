import type { FC } from "react";
import {
    Button,
    Divider,
    Tag,
    Subtitle1,
    Body2,
    Body1,
    Body1Stronger,
    Card,
    MessageBar,
    MessageBarBody,
    MessageBarTitle,
    tokens,
} from "@fluentui/react-components";
import {
    Clock20Regular,
    Clock24Regular,
    DoorArrowRight20Regular,
    Money20Regular,
    Sticker24Regular,
    Payment20Regular,
    Info20Regular,
    Person20Regular,
    Wallet20Regular,
} from "@fluentui/react-icons";
import { HoverTiltCard } from "@components/HoverTiltCard/HoverTiltCard";
import { formatMoneyByN } from "@utility/formatMoney";
import { BillingType as BillingTypeEnum } from "@app-types/tariff";
import { formatTimeHHmm } from "@utility/dateUtils";
import { formatDurationSeconds } from "@utility/formatDurationSeconds";
import { LoyaltyProgress } from "@components/Loyalty/LoyaltyProgress";
import { VisitDiscountBreakdown } from "@components/Billing/VisitDiscountBreakdown";

const Body2Stronger = (props: React.ComponentProps<typeof Body2>) => (
    <Body2 {...props} style={{ fontWeight: tokens.fontWeightSemibold, ...props.style }} />
);

const Subtitle1Stronger = (props: React.ComponentProps<typeof Subtitle1>) => (
    <Subtitle1 {...props} style={{ fontWeight: tokens.fontWeightSemibold, ...props.style }} />
);

interface ActiveVisitStateProps {
    activeVisit: any;
    visitCount: number;
    sizes: any;
    estimate: any;
    elapsedSeconds: number;
    now: number;
    userBalance: any;
    personalDiscount: number;
    globalDiscount: number;
    tariffDiscount: number;
    gracePeriodEnd: number | null;
    graceSecondsLeft: number;
    graceAcknowledged: boolean;
    exitComplete: boolean;
    endingVisit: boolean;

    onExitClick: () => void;
    onGraceStripeCheckout: () => void;
    onAcknowledgeGrace: () => void;
    onChangePaymentMethod: () => void;
}

export const ActiveVisitState: FC<ActiveVisitStateProps> = ({
    activeVisit,
    visitCount,
    sizes,
    estimate,
    elapsedSeconds,
    now,
    userBalance,
    personalDiscount,
    globalDiscount,
    tariffDiscount,
    gracePeriodEnd,
    graceSecondsLeft,
    graceAcknowledged,
    exitComplete,
    endingVisit,
    onExitClick,
    onGraceStripeCheckout,
    onAcknowledgeGrace,
    onChangePaymentMethod,
}) => {
    const isFinishRequested = activeVisit?.isFinishRequested;
    const startedAtMs = activeVisit ? Date.parse(activeVisit.entryTime) : 0;
    const tariffName = activeVisit?.tariffName ?? "Тариф";
    const billingType = activeVisit?.tariffBillingType ?? BillingTypeEnum.PerMinute;
    const pricePerUnit = activeVisit?.tariffPricePerMinute ?? 0;
    const minSessionMinutes = activeVisit?.tariffMinSessionMinutes ?? null;
    const roundingRule = activeVisit?.tariffRoundingRule ?? null;
    const guestsCount = activeVisit?.guestsCount ?? 1;

    const elapsedMinutes = Math.max(0, Math.floor(elapsedSeconds / 60));
    const isUnderMinTime = minSessionMinutes !== null && elapsedMinutes < minSessionMinutes;

    const balanceAmount = userBalance?.currentBalance ?? 0;
    const isBalanceInsufficient = balanceAmount < estimate.total;

    let finishMessage = null;
    if (isFinishRequested) {
        finishMessage = (
            <MessageBar intent="warning">
                <MessageBarBody>
                    <MessageBarTitle>Запрос на завершение визита отправлен</MessageBarTitle>
                    <Body2>
                        Мы зафиксируем точное время окончания сессии, как только вы подойдете к стойке ресепшена. В целях безопасности ваш таймер активен до подтверждения администратором на кассе.
                    </Body2>
                </MessageBarBody>
            </MessageBar>
        );
    }

    let minTimeNotice = null;
    if (isUnderMinTime) {
        minTimeNotice = (
            <MessageBar intent="info">
                <MessageBarBody>
                    <Body2>
                        Действует минимальное оплачиваемое время тарифа: <strong>{minSessionMinutes} мин.</strong> Стоимость рассчитана по минимальной планке.
                    </Body2>
                </MessageBarBody>
            </MessageBar>
        );
    }

    const priceDisplay = (
        <div className="flex flex-col gap-2">
            <div className="flex items-baseline gap-2 flex-wrap">
                {estimate.isDiscounted ? (
                    <>
                        <Subtitle1 style={{ color: tokens.colorBrandForeground1 }}>
                            {formatMoneyByN(estimate.total)}
                        </Subtitle1>
                        <Body1 className="line-through" style={{ color: tokens.colorNeutralForeground3 }}>
                            {formatMoneyByN(estimate.baseTotal)}
                        </Body1>
                        <Tag appearance="brand" color="success">
                            -{estimate.appliedDiscountPercent}%
                        </Tag>
                    </>
                ) : (
                    <Subtitle1 style={{ color: tokens.colorNeutralForeground1 }}>
                        {formatMoneyByN(estimate.total)}
                    </Subtitle1>
                )}
            </div>

            <VisitDiscountBreakdown
                estimate={estimate}
                personalDiscount={personalDiscount}
                globalDiscount={globalDiscount}
                tariffDiscount={tariffDiscount}
                variant="compact"
            />
        </div>
    );

    let actionsContent = null;
    if (isFinishRequested) {
        if (gracePeriodEnd !== null && !graceAcknowledged) {
            let balanceButtonText = "Оплатить с баланса";
            if (isBalanceInsufficient) {
                balanceButtonText = "Недостаточно средств";
            }

            actionsContent = (
                <Card size={sizes.card} style={{ backgroundColor: tokens.colorStatusWarningBackground1, borderColor: tokens.colorStatusWarningBorder1 }}>
                    <div className="flex flex-col gap-3">
                        <div className="flex items-center justify-between">
                            <div className="flex items-center gap-2" style={{ color: tokens.colorStatusWarningForeground1 }}>
                                <Clock20Regular />
                                <Subtitle1Stronger>Ожидаем оплату</Subtitle1Stronger>
                            </div>
                            <Subtitle1 className="text-lg" style={{ color: tokens.colorStatusWarningForeground1 }}>
                                {Math.floor(graceSecondsLeft / 60)}:{(graceSecondsLeft % 60).toString().padStart(2, '0')}
                            </Subtitle1>
                        </div>
                        <Body2>
                            Для завершения визита пополните баланс картой онлайн или выберите списание с баланса. При истечении льготного времени таймер возобновит работу.
                        </Body2>
                        <div className="flex flex-col gap-2">
                            <Button
                                appearance="primary"
                                onClick={onGraceStripeCheckout}
                                icon={<Payment20Regular />}
                            >
                                Пополнить баланс картой
                            </Button>
                            <Button
                                onClick={onAcknowledgeGrace}
                                icon={<Money20Regular />}
                                disabled={isBalanceInsufficient}
                            >
                                {balanceButtonText}
                            </Button>
                        </div>
                    </div>
                </Card>
            );
        } else {
            actionsContent = (
                <div className="flex flex-col gap-3">
                    <MessageBar intent="success">
                        <MessageBarBody>
                            <MessageBarTitle>Выбрана оплата с баланса</MessageBarTitle>
                            <Body2>Ожидайте подтверждения выхода администратором. Списание произойдёт автоматически.</Body2>
                        </MessageBarBody>
                    </MessageBar>
                    <Button
                        appearance="outline"
                        onClick={onChangePaymentMethod}
                        icon={<Payment20Regular />}
                        size={sizes.button}
                    >
                        Изменить способ оплаты
                    </Button>
                </div>
            );
        }
    } else {
        actionsContent = (
            <Button
                appearance="primary"
                icon={<DoorArrowRight20Regular />}
                data-testid="visit-active-exit"
                onClick={onExitClick}
                disabled={exitComplete || endingVisit}
                size={sizes.button}
                className="w-full"
            >
                Выход из заведения
            </Button>
        );
    }

    let balanceWidget = (
        <div className="flex flex-col gap-2 bg-(--colorNeutralBackground2) p-3 rounded border border-(--colorNeutralStroke2)">
            <div className="flex justify-between items-center">
                <div className="flex items-center gap-2">
                    <Wallet20Regular className="text-(--colorBrandForeground1)" />
                    <Body2Stronger className="text-(--colorNeutralForeground3)">Внутренний баланс</Body2Stronger>
                </div>
                <Subtitle1 className={isBalanceInsufficient ? "text-(--colorPaletteRedForeground1)" : "text-(--colorPaletteGreenForeground1)"}>
                    {formatMoneyByN(balanceAmount)}
                </Subtitle1>
            </div>
            {isBalanceInsufficient && !isFinishRequested && (
                <div className="flex flex-col gap-2 mt-1">
                    <MessageBar intent="warning">
                        <MessageBarBody>
                            <Body2>Баланса не хватает для автосписания. Вы можете пополнить его прямо сейчас.</Body2>
                        </MessageBarBody>
                    </MessageBar>
                    <Button
                        appearance="outline"
                        onClick={onGraceStripeCheckout}
                        icon={<Payment20Regular />}
                        size="small"
                        className="w-full"
                    >
                        Пополнить через Stripe
                    </Button>
                </div>
            )}
        </div>
    );

    const rateText = billingType === BillingTypeEnum.Hourly
        ? `${formatMoneyByN(pricePerUnit * 60)} / час`
        : `${formatMoneyByN(pricePerUnit)} / мин`;

    const roundingLabel = roundingRule === "FiveMinutes"
        ? "округление до 5 мин"
        : roundingRule === "FifteenMinutes"
            ? "округление до 15 мин"
            : roundingRule === "SixtyMinutes"
                ? "округление до часа"
                : "без округления";

    return (
        <div className="flex flex-col gap-4">
            {finishMessage}

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Карточка 1: Тариф (на мобилках идет второй) */}
                <HoverTiltCard className="flex flex-col gap-4 order-2 md:order-1" style={{ backgroundColor: tokens.colorBrandBackground2 }} size={sizes.card}>
                    <div className="flex flex-col gap-4">
                        <div className="flex items-center gap-2">
                            <Sticker24Regular className="text-(--colorBrandForeground1)" />
                            <Subtitle1Stronger>Тариф</Subtitle1Stronger>
                        </div>
                        <Subtitle1Stronger className="text-xl">{tariffName}</Subtitle1Stronger>
                        <Divider />
                        <div className="flex flex-col gap-3">
                            <div className="flex justify-between items-center">
                                <div className="flex items-center gap-2">
                                    <Clock20Regular className="text-(--colorBrandForegroundLink)" />
                                    <Body2Stronger>Тип биллинга:</Body2Stronger>
                                </div>
                                <Body2>
                                    {billingType === BillingTypeEnum.Hourly ? "Почасовой" : "Поминутный"}
                                </Body2>
                            </div>
                            <div className="flex justify-between items-center">
                                <div className="flex items-center gap-2">
                                    <Money20Regular className="text-(--colorPaletteGreenForeground1)" />
                                    <Body2Stronger>Ставка тарифа:</Body2Stronger>
                                </div>
                                <Body2>{rateText}</Body2>
                            </div>
                            <div className="flex justify-between items-center">
                                <div className="flex items-center gap-2">
                                    <Person20Regular className="text-(--colorPaletteMulberryForeground1)" />
                                    <Body2Stronger>Гости:</Body2Stronger>
                                </div>
                                <Body2>
                                    {guestsCount === 1 ? "1 чел" : `${guestsCount} чел`}
                                </Body2>
                            </div>
                        </div>

                        {(minSessionMinutes || roundingRule) && (
                            <>
                                <Divider />
                                <div className="flex flex-col gap-3">
                                    {minSessionMinutes && (
                                        <div className="flex justify-between items-center">
                                            <div className="flex items-center gap-2">
                                                <Info20Regular className="text-(--colorPaletteDarkOrangeForeground1)" />
                                                <Body2Stronger>Минимум визита:</Body2Stronger>
                                            </div>
                                            <Body2>{minSessionMinutes} мин</Body2>
                                        </div>
                                    )}
                                    <div className="flex justify-between items-center">
                                        <div className="flex items-center gap-2">
                                            <Info20Regular className="text-(--colorBrandForegroundLink)" />
                                            <Body2Stronger>Округление:</Body2Stronger>
                                        </div>
                                        <Body2>{roundingLabel}</Body2>
                                    </div>
                                </div>
                            </>
                        )}

                        {/* Индикатор лояльности и прогресс */}
                        {visitCount !== undefined && (
                            <>
                                <Divider />
                                <div className="pt-1">
                                    <LoyaltyProgress visitCount={visitCount} currentDiscount={personalDiscount} />
                                </div>
                            </>
                        )}
                    </div>
                </HoverTiltCard>

                {/* Карточка 2: Время, расчёт, баланс и управление визитом (на мобилках идет первой) */}
                <HoverTiltCard className="flex flex-col gap-4 order-1 md:order-2" size={sizes.card}>
                    <div className="flex flex-col gap-4">
                        <div className="flex items-center gap-2">
                            <Clock24Regular className="text-(--colorBrandForeground1)" />
                            <Subtitle1Stronger>Активный визит</Subtitle1Stronger>
                        </div>
                        <Divider />

                        <div className="flex flex-col gap-4">
                            <div className="flex flex-col gap-1">
                                <Body1Stronger className="text-(--colorNeutralForeground3)">Прошедшее время</Body1Stronger>
                                <Subtitle1 className="text-xl">
                                    {formatDurationSeconds(elapsedSeconds)}
                                </Subtitle1>
                                <Body1 className="text-(--colorNeutralForeground4)">
                                    Вход: {formatTimeHHmm("", new Date(startedAtMs ?? now))}
                                </Body1>
                            </div>

                            <div className="flex flex-col gap-1">
                                <Body1Stronger className="text-(--colorNeutralForeground3)">Текущая стоимость</Body1Stronger>
                                {priceDisplay}
                                <Body1 className="text-(--colorNeutralForeground4)">
                                    Расчет: {estimate.breakdown}
                                </Body1>
                            </div>

                            <Divider />

                            {balanceWidget}

                            <Divider />

                            {actionsContent}
                        </div>

                        {minTimeNotice}
                    </div>
                </HoverTiltCard>
            </div>
        </div>
    );
};
