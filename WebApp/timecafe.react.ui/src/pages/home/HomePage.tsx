import { useEffect, useMemo, useState } from "react";
import {
    Body2,
    LargeTitle

} from "@fluentui/react-components";
import { useNavigate } from "react-router-dom";
import { useAppSelector } from "@store/hooks";

import { calcVisitEstimate } from "@utility/visitEstimate";
import { VisitStatus } from "@app-types/visit";
import { TransactionType, type BillingTransaction } from "@app-types/billing";
import { useGetActiveVisitByUserQuery, useHasActiveVisitQuery } from "@store/api/venueApi";
import { useGetBalanceQuery, useGetDebtQuery, useGetTransactionHistoryQuery } from "@store/api/billingApi";
import { useGetProfileByUserIdQuery } from "@store/api/profileApi";
import { useComponentSize } from "@hooks/useComponentSize";
import { BalanceCard } from "./BalanceCard";
import { VisitCard } from "./VisitCard";
import { WeekSpentCard } from "./WeekSpentCard";
import { ProfileCard } from "./ProfileCard";
import { QuickActionsCard } from "./QuickActionsCard";

export const HomePage = () => {
    const navigate = useNavigate();

    const authEmail = useAppSelector((state) => state.auth.email);
    const authPhoneNumber = useAppSelector((state) => state.auth.phoneNumber);
    const userId = useAppSelector((state) => state.auth.userId);
    const emailConfirmed = useAppSelector((state) => state.auth.emailConfirmed);
    const phoneConfirmed = useAppSelector((state) => state.auth.phoneNumberConfirmed);
    const { data: profile } = useGetProfileByUserIdQuery(userId!, { skip: !userId });

    const { data: hasActive } = useHasActiveVisitQuery(userId!, { skip: !userId });
    const { data: activeVisitData } = useGetActiveVisitByUserQuery(userId!, { skip: !userId || !hasActive });
    const { data: balance, isLoading: loadingBilling } = useGetBalanceQuery(userId!, { skip: !userId });
    const { data: debtRub = 0 } = useGetDebtQuery(userId!, { skip: !userId });
    const { data: txData } = useGetTransactionHistoryQuery(
        { userId: userId!, page: 1, pageSize: 100 },
        { skip: !userId },
    );

    const balanceRub = balance?.currentBalance ?? 0;
    const transactions: BillingTransaction[] = useMemo(() => txData?.transactions ?? [], [txData?.transactions]);

    const visitStatus = activeVisitData?.status;
    const isActiveVisit = hasActive && visitStatus === VisitStatus.Active;
    const hasAnyVisit = hasActive && !!activeVisitData;
    const startedAtMs = activeVisitData ? Date.parse(activeVisitData.entryTime) : 0;

    let visitInfoText: string;
    if (isActiveVisit && activeVisitData) {
        visitInfoText = "Визит идёт";
    } else if (hasAnyVisit) {
        visitInfoText = "Есть незавершённый визит";
    } else {
        visitInfoText = "Нет активного визита";
    }

    const [now, setNow] = useState(() => Date.now());

    useEffect(() => {
        if (!isActiveVisit) return;
        const id = globalThis.setInterval(() => setNow(Date.now()), 1000);
        return () => globalThis.clearInterval(id);
    }, [isActiveVisit]);

    const activeElapsedSeconds = useMemo(() => {
        if (!isActiveVisit || !startedAtMs) return 0;
        return Math.max(0, Math.floor((now - startedAtMs) / 1000));
    }, [isActiveVisit, startedAtMs, now]);

    const activeElapsedMinutes = useMemo(
        () => Math.max(1, Math.ceil(activeElapsedSeconds / 60)),
        [activeElapsedSeconds]
    );

    const activeEstimate = useMemo(() => {
        if (!isActiveVisit || !activeVisitData) return null;
        return calcVisitEstimate(
            activeElapsedMinutes,
            activeVisitData.tariffBillingType,
            activeVisitData.tariffPricePerMinute
        );
    }, [activeElapsedMinutes, activeVisitData, isActiveVisit]);

    const displayName = useMemo(() => {
        const firstName = profile?.firstName?.trim();
        if (firstName) return firstName;
        const email = authEmail?.trim();
        if (email) return email;
        return "";
    }, [authEmail, profile?.firstName]);

    const weekSpentRub = useMemo(() => {
        const fromDate = new Date();
        fromDate.setHours(0, 0, 0, 0);
        fromDate.setDate(fromDate.getDate() - 6);

        return transactions.reduce((acc: number, transaction: BillingTransaction) => {
            const created = new Date(transaction.createdAt);
            if (Number.isNaN(created.getTime()) || created < fromDate) {
                return acc;
            }

            if (transaction.type !== TransactionType.Deposit) {
                return acc + Math.abs(Number(transaction.amount) || 0);
            }

            return acc;
        }, 0);
    }, [transactions]);

    const profileEmail = profile?.email?.trim() || authEmail?.trim() || "";
    const profilePhone = profile?.phoneNumber?.trim() || authPhoneNumber?.trim() || "";

    return (
        <div className="page-content flex flex-col gap-4">
            <div className="flex flex-col gap-4 sm:justify-between ">
                <LargeTitle truncate wrap={false}>
                    {displayName ? `Привет, ${displayName}` : "Привет"}
                </LargeTitle>

                <div className="flex flex-col gap-2 sm:flex-row sm:justify-between sm:items-end">
                    <div className="flex flex-col gap-2 ">
                        <Body2>
                            Короткий обзор: баланс, визит и быстрые действия.
                        </Body2>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                <BalanceCard
                    balanceRub={balanceRub}
                    debtRub={debtRub}
                    loading={loadingBilling}
                    onNavigate={() => navigate("/billing")}
                />

                <VisitCard
                    status={visitStatus}
                    elapsedSeconds={activeElapsedSeconds}
                    estimateTotal={activeEstimate?.total ?? null}
                    visitInfo={visitInfoText}
                    onNavigateVisit={() => navigate(hasAnyVisit ? "/visit/active" : "/visit/start")}
                    onNavigateBilling={() => navigate("/billing")}
                />

                <WeekSpentCard
                    spent={weekSpentRub}
                    onNavigate={() => navigate("/billing")}
                />
            </div>

            <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                <ProfileCard
                    profile={profile}
                    email={profileEmail}
                    phone={profilePhone}
                    emailConfirmed={emailConfirmed}
                    phoneConfirmed={phoneConfirmed}
                    onNavigate={() => navigate("/personal-data")}
                />

                <QuickActionsCard
                    onNavigateProfile={() => navigate("/personal-data")}
                    onNavigateBilling={() => navigate("/billing")}
                />
            </div>
        </div>
    );
};
