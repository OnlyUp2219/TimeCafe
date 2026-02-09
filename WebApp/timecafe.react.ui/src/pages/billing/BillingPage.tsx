import {LargeTitle} from "@fluentui/react-components";

import {useMemo, useState} from "react";
import {useDispatch, useSelector} from "react-redux";

import type {RootState} from "@store";
import type {Tariff} from "@app-types/tariff";
import {setBalanceRub, setDebtRub} from "@store/billingSlice";

import {mockWeeklyActivity} from "./billing.mock";

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
import {LoyaltyCard} from "@components/Billing/LoyaltyCard";
import {SupportCard} from "@components/Billing/SupportCard";

export const BillingPage = () => {
    const dispatch = useDispatch();

    const balanceRub = useSelector((state: RootState) => state.billing.balanceRub);
    const debtRub = useSelector((state: RootState) => state.billing.debtRub);
    const tariffs = useSelector((state: RootState) => state.billing.tariffs);
    const lastVisitTariffId = useSelector((state: RootState) => state.billing.lastVisitTariffId);
    const selectedTariffId = useSelector((state: RootState) => state.visit.selectedTariffId);

    const [draftAmountText, setDraftAmountText] = useState("");

    const effectiveTariff: Tariff | null = useMemo(() => {
        const activeTariffs = tariffs.filter((t) => t.isActive);
        if (activeTariffs.length === 0) return null;

        const preferredId = lastVisitTariffId ?? selectedTariffId;
        if (preferredId) {
            const match = activeTariffs.find((t) => t.tariffId === preferredId);
            if (match) return match;
        }

        return activeTariffs[0] ?? null;
    }, [tariffs, lastVisitTariffId, selectedTariffId]);

    const availableForVisitsRub = Math.max(0, balanceRub - Math.max(0, debtRub));

    const onPresetAdd = (deltaRub: number) => {
        const current = Number(draftAmountText);
        const currentSafe = Number.isFinite(current) ? current : 0;
        setDraftAmountText(String(Math.max(0, currentSafe + deltaRub)));
    };

    const onSubmitTopUp = () => {
        const value = Number(draftAmountText);
        const safeValue = Number.isFinite(value) ? value : 0;
        const topUpRub = Math.floor(safeValue);
        if (topUpRub <= 0) return;

        dispatch(setBalanceRub(balanceRub + topUpRub));
        setDraftAmountText("");
    };

    const onPayDebt = () => {
        if (debtRub <= 0) return;

        const canPay = Math.max(0, balanceRub);
        if (canPay >= debtRub) {
            dispatch(setBalanceRub(balanceRub - debtRub));
            dispatch(setDebtRub(0));
            return;
        }

        dispatch(setBalanceRub(0));
        dispatch(setDebtRub(debtRub - canPay));
    };

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
                <div
                    className="flex flex-col gap-6 overflow-hidden rounded-3xl p-5 sm:p-8 tc-billing-panel"
                >
                    <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
                        <LargeTitle>Баланс и транзакции</LargeTitle>
                    </div>

                    <div className="flex flex-col gap-4">
                        <div className="grid grid-cols-1 gap-4">
                            <BalanceActivityCard
                                balanceRub={balanceRub}
                                monthDeltaPercent={12}
                                activity={mockWeeklyActivity}
                            />
                        </div>

                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                            <TopUpCard
                                draftAmountText={draftAmountText}
                                onDraftAmountTextChange={setDraftAmountText}
                                onPresetAdd={onPresetAdd}
                                onSubmit={onSubmitTopUp}
                            />
                            <RestTimeCard
                                availableRub={availableForVisitsRub}
                                tariffName={effectiveTariff?.name ?? "Тариф"}
                                pricePerMinuteRub={effectiveTariff?.pricePerMinute ?? 0}
                            />
                        </div>

                        <DebtWarningCard debtRub={debtRub} onPay={onPayDebt} />

                        <TransactionsSection />

                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                            <LoyaltyCard
                                statusLabel="Серебро"
                                progress={0.75}
                                spentRubText="750 ₽ потрачено"
                                leftRubText="250 ₽ до золота"
                                perkText="Ваша скидка 5% уже применяется ко всем визитам."
                            />
                            <SupportCard telegramUrl="https://t.me/your_admin" onCallAdmin={() => {}} />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};