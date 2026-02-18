import {Body2, Divider, Title2} from "@fluentui/react-components";

import {useCallback, useEffect, useMemo, useState} from "react";
import {useDispatch, useSelector} from "react-redux";

import type {AppDispatch, RootState} from "@store";
import type {Tariff} from "@app-types/tariff";
import {TransactionType, type BillingActivityPoint} from "@app-types/billing";
import {
    clearBillingError,
    clearCheckoutUrl,
    initializeBillingCheckout,
    loadBillingOverview,
    loadBillingTransactions,
} from "@store/billingSlice";
import {loadActiveTariffs} from "@store/visitSlice";
import {
    Button,
    MessageBar,
    MessageBarActions,
    MessageBarBody,
    MessageBarTitle,
} from "@fluentui/react-components";
import {DismissRegular} from "@fluentui/react-icons";

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

export const BillingPage = () => {
    const dispatch = useDispatch<AppDispatch>();

    const userId = useSelector((state: RootState) => state.auth.userId);
    const balanceRub = useSelector((state: RootState) => state.billing.balanceRub);
    const debtRub = useSelector((state: RootState) => state.billing.debtRub);
    const billingError = useSelector((state: RootState) => state.billing.error);
    const checkoutError = useSelector((state: RootState) => state.billing.checkoutError);
    const checkoutUrl = useSelector((state: RootState) => state.billing.checkoutUrl);
    const loadingOverview = useSelector((state: RootState) => state.billing.loading);
    const loadingTransactions = useSelector((state: RootState) => state.billing.loadingTransactions);
    const initializingCheckout = useSelector((state: RootState) => state.billing.initializingCheckout);
    const transactions = useSelector((state: RootState) => state.billing.transactions);
    const pagination = useSelector((state: RootState) => state.billing.pagination);
    const tariffs = useSelector((state: RootState) => state.visit.tariffs);
    const activeVisit = useSelector((state: RootState) => state.visit.activeVisit);
    const selectedTariffId = useSelector((state: RootState) => state.visit.selectedTariffId);

    const [draftAmountText, setDraftAmountText] = useState("");

    const effectiveTariff: Tariff | null = useMemo(() => {
        if (activeVisit?.tariff?.isActive) {
            return activeVisit.tariff;
        }

        const activeTariffs = tariffs.filter((t) => t.isActive);
        if (activeTariffs.length === 0) return null;

        if (selectedTariffId) {
            const match = activeTariffs.find((t) => t.tariffId === selectedTariffId);
            if (match) return match;
        }

        return activeTariffs[0] ?? null;
    }, [activeVisit, tariffs, selectedTariffId]);

    useEffect(() => {
        if (!userId) return;

        void dispatch(loadBillingOverview({userId}));
        void dispatch(loadBillingTransactions({userId, page: 1, pageSize: pagination.pageSize, append: false}));
    }, [dispatch, pagination.pageSize, userId]);

    useEffect(() => {
        if (tariffs.length > 0) return;
        void dispatch(loadActiveTariffs());
    }, [dispatch, tariffs.length]);

    useEffect(() => {
        if (!checkoutUrl) return;
        window.location.assign(checkoutUrl);
        dispatch(clearCheckoutUrl());
    }, [checkoutUrl, dispatch]);

    const availableForVisitsRub = Math.max(0, balanceRub - Math.max(0, debtRub));

    const onPresetAdd = (deltaRub: number) => {
        const current = Number(draftAmountText);
        const currentSafe = Number.isFinite(current) ? current : 0;
        setDraftAmountText(String(Math.max(0, currentSafe + deltaRub)));
    };

    const openCheckout = useCallback(async (amountRub: number, description?: string) => {
        const action = await dispatch(initializeBillingCheckout({amountRub, userId, description}));
        return initializeBillingCheckout.fulfilled.match(action);
    }, [dispatch, userId]);

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

        const nextPage = pagination.currentPage + 1;
        void dispatch(loadBillingTransactions({
            userId,
            page: nextPage,
            pageSize: pagination.pageSize,
            append: true,
        }));
    }, [dispatch, pagination.currentPage, pagination.pageSize, pagination.totalPages, userId]);

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
                    {(billingError || checkoutError) && (
                        <MessageBar intent="error">
                            <MessageBarBody>
                                <MessageBarTitle>Ошибка биллинга:</MessageBarTitle>
                                {checkoutError ?? billingError}
                            </MessageBarBody>
                            <MessageBarActions
                                containerAction={
                                    <Button
                                        appearance="transparent"
                                        aria-label="Закрыть"
                                        icon={<DismissRegular className="size-5"/>}
                                        onClick={() => dispatch(clearBillingError())}
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