import {
    Body2,
    Button,
    Divider,
    MessageBar,
    MessageBarActions,
    MessageBarBody,
    MessageBarTitle,
    Title2,
} from "@fluentui/react-components";
import {DismissRegular} from "@fluentui/react-icons";

import {useCallback, useMemo, useState} from "react";
import {useAppSelector} from "@store/hooks";
import type {Tariff} from "@app-types/tariff";
import {TransactionType, type BillingActivityPoint, type BillingTransaction} from "@app-types/billing";

import "./billing.css";

import glitch from "@assets/ggglitch.svg";
import blob4 from "@assets/ssshape_blob4.svg";
import blob2 from "@assets/ssshape_blob2.svg";
import squiggl from "@assets/sssquiggl_1.svg";

import {BalanceActivityCard} from "@components/Billing/BalanceActivityCard";
import {TopUpCard} from "@components/Billing/TopUpCard";
import {TransactionsSection} from "@components/Billing/TransactionsSection";
import {RestTimeCard} from "@components/Billing/RestTimeCard";
import {DebtWarningCard} from "@components/Billing/DebtWarningCard";
import {SupportCard} from "@components/Billing/SupportCard";
import {useGetBalanceQuery, useGetDebtQuery, useGetTransactionHistoryQuery, useInitializeStripeCheckoutMutation} from "@store/api/billingApi";
import {useGetActiveTariffsQuery, useGetActiveVisitByUserQuery, useHasActiveVisitQuery} from "@store/api/venueApi";

export const BillingPage = () => {
    const userId = useAppSelector((state) => state.auth.userId);
    const selectedTariffId = useAppSelector((state) => state.visit.selectedTariffId);

    const pageSize = 20;
    const [page, setPage] = useState(1);

    const {data: balance, isLoading: loadingOverview} = useGetBalanceQuery(userId!, {skip: !userId});
    const {data: debtRub = 0} = useGetDebtQuery(userId!, {skip: !userId});
    const {data: txData, isLoading: loadingTransactions} = useGetTransactionHistoryQuery(
        {userId: userId!, page, pageSize},
        {skip: !userId},
    );
    const {data: tariffsData} = useGetActiveTariffsQuery();
    const {data: hasActive} = useHasActiveVisitQuery(userId!, {skip: !userId});
    const {data: activeVisitData} = useGetActiveVisitByUserQuery(userId!, {skip: !userId || !hasActive});
    const [initCheckout, {isLoading: initializingCheckout, error: checkoutRtkError, reset: resetCheckout}] = useInitializeStripeCheckoutMutation();

    const balanceRub = balance?.currentBalance ?? 0;
    const transactions: BillingTransaction[] = txData?.transactions ?? [];
    const pagination = txData?.pagination ?? {currentPage: 1, pageSize, totalCount: 0, totalPages: 0};
    const checkoutError = checkoutRtkError ? "Не удалось инициализировать пополнение" : null;

    const tariffs: Tariff[] = useMemo(() => {
        if (!tariffsData) return [];
        return tariffsData.map((t) => ({
            tariffId: t.tariffId,
            name: t.name,
            description: t.description ?? "",
            billingType: t.billingType,
            pricePerMinute: t.pricePerMinute,
            isActive: t.isActive,
            themeName: t.themeName,
            themeEmoji: t.themeEmoji ?? null,
        }));
    }, [tariffsData]);

    const [draftAmountText, setDraftAmountText] = useState("");

    const effectiveTariff: Tariff | null = useMemo(() => {
        if (activeVisitData) {
            return {
                tariffId: activeVisitData.tariffId,
                name: activeVisitData.tariffName,
                description: activeVisitData.tariffDescription ?? "",
                billingType: activeVisitData.tariffBillingType,
                pricePerMinute: activeVisitData.tariffPricePerMinute,
                isActive: true,
            };
        }

        const activeTariffs = tariffs.filter((t: Tariff) => t.isActive);
        if (activeTariffs.length === 0) return null;

        if (selectedTariffId) {
            const match = activeTariffs.find((t: Tariff) => t.tariffId === selectedTariffId);
            if (match) return match;
        }

        return activeTariffs[0] ?? null;
    }, [activeVisitData, tariffs, selectedTariffId]);

    const availableForVisitsRub = Math.max(0, balanceRub - Math.max(0, debtRub));

    const onPresetAdd = (deltaRub: number) => {
        const current = Number(draftAmountText);
        const currentSafe = Number.isFinite(current) ? current : 0;
        setDraftAmountText(String(Math.max(0, currentSafe + deltaRub)));
    };

    const openCheckout = useCallback(async (amountRub: number, description?: string) => {
        if (!userId) return false;
        const baseUrl = typeof window !== "undefined" ? window.location.origin : "";
        const successUrl = baseUrl ? `${baseUrl}/billing?payment=success` : undefined;
        const cancelUrl = baseUrl ? `${baseUrl}/billing?payment=cancel` : undefined;

        try {
            const result = await initCheckout({
                userId,
                amount: Math.floor(amountRub),
                successUrl,
                cancelUrl,
                description,
            }).unwrap();
            if (result.checkoutUrl) {
                window.location.assign(result.checkoutUrl);
            }
            return true;
        } catch {
            return false;
        }
    }, [initCheckout, userId]);

    const onSubmitTopUp = useCallback(async () => {
        const value = Number(draftAmountText);
        const safeValue = Number.isFinite(value) ? value : 0;
        const topUpRub = Math.round(safeValue * 100) / 100;
        if (topUpRub < 50) return;

        const isStarted = await openCheckout(topUpRub, "Пополнение баланса");
        if (isStarted) {
            setDraftAmountText("");
        }
    }, [draftAmountText, openCheckout]);

    const onPayDebt = useCallback(async () => {
        if (debtRub <= 0) return;

        const amount = Math.max(50, Math.ceil(debtRub * 100) / 100);
        setDraftAmountText(String(amount));
        await openCheckout(amount, "Погашение задолженности");
    }, [debtRub, openCheckout]);

    const onLoadMoreTransactions = useCallback(() => {
        if (!userId) return;
        if (pagination.currentPage >= pagination.totalPages) return;
        setPage(pagination.currentPage + 1);
    }, [pagination.currentPage, pagination.totalPages, userId]);

    const canLoadMoreTransactions = pagination.currentPage < pagination.totalPages;

    const weeklyActivity = useMemo<BillingActivityPoint[]>(() => {
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        const dayMap = new Map<string, BillingActivityPoint>();
        for (let offset = 6; offset >= 0; offset -= 1) {
            const date = new Date(today);
            date.setDate(today.getDate() - offset);
            const key = date.toISOString().slice(0, 10);
            dayMap.set(key, {
                date,
                depositsRub: 0,
                withdrawalsRub: 0,
            });
        }

        for (const transaction of transactions) {
            const created = new Date(transaction.createdAt);
            if (Number.isNaN(created.getTime())) continue;

            const dayDate = new Date(created);
            dayDate.setHours(0, 0, 0, 0);
            const key = dayDate.toISOString().slice(0, 10);
            const point = dayMap.get(key);
            if (!point) continue;

            const amount = Math.abs(Number(transaction.amount) || 0);
            if (transaction.type === TransactionType.Deposit) {
                point.depositsRub += amount;
            } else {
                point.withdrawalsRub += amount;
            }
        }

        return Array.from(dayMap.values());
    }, [transactions]);

    const monthDeltaPercent = useMemo(() => {
        const now = new Date();
        const currentStart = new Date(now);
        currentStart.setDate(now.getDate() - 29);
        currentStart.setHours(0, 0, 0, 0);

        const previousStart = new Date(currentStart);
        previousStart.setDate(currentStart.getDate() - 30);

        let currentPeriod = 0;
        let previousPeriod = 0;

        for (const transaction of transactions) {
            if (transaction.type === TransactionType.Deposit) continue;

            const created = new Date(transaction.createdAt);
            if (Number.isNaN(created.getTime())) continue;

            const amount = Math.abs(Number(transaction.amount) || 0);

            if (created >= currentStart) {
                currentPeriod += amount;
                continue;
            }

            if (created >= previousStart && created < currentStart) {
                previousPeriod += amount;
            }
        }

        if (previousPeriod <= 0) {
            return currentPeriod > 0 ? 100 : 0;
        }

        return Math.round(((currentPeriod - previousPeriod) / previousPeriod) * 100);
    }, [transactions]);

    const handleCallAdmin = useCallback(() => {
        window.open("https://t.me/OnlyUp_S", "_blank");
    }, []);

    return (
        <div className="relative tc-noise-overlay w-full h-full overflow-hidden">
            <div className="pointer-events-none absolute inset-0 overflow-hidden">
                <img
                    src={glitch}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[12vw] -left-[12vw] w-[52vw] max-w-[720px] select-none opacity-30"
                    draggable={false}
                />
                <img
                    src={blob4}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[18vw] -left-[10vw] w-[70vw] max-w-none select-none opacity-30"
                    draggable={false}
                />
                <img
                    src={blob2}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[10vw] -right-[12vw] w-[44vw] max-w-[640px] select-none opacity-30"
                    draggable={false}
                />
                <img
                    src={squiggl}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[8vw] -right-[10vw] w-[34vw] max-w-[520px] select-none opacity-40 rotate-[-135deg]"
                    draggable={false}
                />
            </div>

            <div className="relative mx-auto w-full max-w-6xl px-2 py-4 sm:px-3 sm:py-6">
                <div className="flex flex-col gap-4">
                    {checkoutError && (
                        <MessageBar intent="error">
                            <MessageBarBody>
                                <MessageBarTitle>Ошибка биллинга:</MessageBarTitle>
                                {checkoutError}
                            </MessageBarBody>
                            <MessageBarActions
                                containerAction={
                                    <Button
                                        appearance="transparent"
                                        aria-label="Закрыть"
                                        icon={<DismissRegular className="size-5"/>}
                                        onClick={() => resetCheckout()}
                                    />
                                }
                            />
                        </MessageBar>
                    )}

                    {initializingCheckout && (
                        <MessageBar intent="info">
                            <MessageBarBody>
                                <MessageBarTitle>Переход к оплате</MessageBarTitle>
                                Подготавливаем платёжную сессию Stripe. Это может занять несколько секунд.
                            </MessageBarBody>
                        </MessageBar>
                    )}

                    <div className="flex flex-col gap-3">
                        <Title2 block>Баланс и транзакции</Title2>
                        <Body2 block>
                            Следите за движением средств, пополняйте баланс и просматривайте историю операций.
                        </Body2>
                    </div>

                    <Divider/>

                    <div className="flex flex-col gap-4">
                        <div className="grid grid-cols-1 gap-4">
                            <BalanceActivityCard
                                balanceRub={balanceRub}
                                monthDeltaPercent={monthDeltaPercent}
                                activity={weeklyActivity}
                            />
                        </div>

                        <div className="grid grid-cols-1 items-stretch gap-4 sm:grid-cols-2">
                            <TopUpCard
                                draftAmountText={draftAmountText}
                                onDraftAmountTextChange={setDraftAmountText}
                                onPresetAdd={onPresetAdd}
                                onSubmit={onSubmitTopUp}
                                loading={initializingCheckout || loadingOverview}
                            />
                            <div className="hidden h-full sm:block">
                                <RestTimeCard
                                    availableRub={availableForVisitsRub}
                                    tariffName={effectiveTariff?.name ?? "Тариф"}
                                    pricePerMinuteRub={effectiveTariff?.pricePerMinute ?? 0}
                                />
                            </div>
                        </div>

                        <DebtWarningCard
                            debtRub={debtRub}
                            onPay={onPayDebt}
                            loading={initializingCheckout}
                        />

                        <TransactionsSection
                            transactions={transactions}
                            loading={loadingTransactions}
                            loadingMore={loadingTransactions && transactions.length > 0}
                            canLoadMore={canLoadMoreTransactions}
                            onLoadMore={onLoadMoreTransactions}
                        />

                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                            <div className="flex h-full items-center rounded-xl border border-dashed px-4 py-3">
                                <Body2 block>TODO: loyalty блок скрыт до появления backend-контракта программы лояльности.</Body2>
                            </div>
                            <SupportCard telegramUrl="https://t.me/OnlyUp_S" onCallAdmin={handleCallAdmin} />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};