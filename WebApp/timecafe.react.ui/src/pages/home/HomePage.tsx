import {useEffect, useMemo, useState} from "react";
import {
    Badge,
    Body2,
    Button,
    LargeTitle,
    Tag,
    Text,
} from "@fluentui/react-components";
import {Sparkle20Regular} from "@fluentui/react-icons";
import {useNavigate} from "react-router-dom";
import {useAppSelector} from "@store/hooks";
import vortex from "@assets/vvvortex.svg";
import repeat from "@assets/rrrepeat (2).svg";
import surf from "@assets/sssurf.svg";
import {calcVisitEstimate} from "@utility/visitEstimate";
import {VisitStatus} from "@app-types/visit";
import {TransactionType, type BillingTransaction} from "@app-types/billing";
import {useGetActiveVisitByUserQuery, useHasActiveVisitQuery} from "@store/api/venueApi";
import {useGetBalanceQuery, useGetDebtQuery, useGetTransactionHistoryQuery} from "@store/api/billingApi";
import {useGetProfileByUserIdQuery} from "@store/api/profileApi";
import {BalanceCard} from "./BalanceCard";
import {VisitCard} from "./VisitCard";
import {WeekSpentCard} from "./WeekSpentCard";
import {ProfileCard} from "./ProfileCard";
import {QuickActionsCard} from "./QuickActionsCard";

export const HomePage = () => {
    const navigate = useNavigate();

    const authEmail = useAppSelector((state) => state.auth.email);
    const authPhoneNumber = useAppSelector((state) => state.auth.phoneNumber);
    const userId = useAppSelector((state) => state.auth.userId);
    const emailConfirmed = useAppSelector((state) => state.auth.emailConfirmed);
    const phoneConfirmed = useAppSelector((state) => state.auth.phoneNumberConfirmed);
    const {data: profile} = useGetProfileByUserIdQuery(userId!, {skip: !userId});

    const {data: hasActive} = useHasActiveVisitQuery(userId!, {skip: !userId});
    const {data: activeVisitData} = useGetActiveVisitByUserQuery(userId!, {skip: !userId || !hasActive});
    const {data: balance, isLoading: loadingBilling} = useGetBalanceQuery(userId!, {skip: !userId});
    const {data: debtRub = 0} = useGetDebtQuery(userId!, {skip: !userId});
    const {data: txData} = useGetTransactionHistoryQuery(
        {userId: userId!, page: 1, pageSize: 100},
        {skip: !userId},
    );

    const balanceRub = balance?.currentBalance ?? 0;
    const transactions: BillingTransaction[] = useMemo(() => txData?.transactions ?? [], [txData?.transactions]);

    const isActiveVisit = hasActive && activeVisitData?.status === VisitStatus.Active;
    const startedAtMs = activeVisitData ? Date.parse(activeVisitData.entryTime) : 0;

    const [now, setNow] = useState(() => Date.now());

    useEffect(() => {
        if (!isActiveVisit) return;
        const id = window.setInterval(() => setNow(Date.now()), 1000);
        return () => window.clearInterval(id);
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

    const profileEmail = profile?.email?.trim() || authEmail?.trim() || "—";
    const profilePhone = profile?.phoneNumber?.trim() || authPhoneNumber?.trim() || "Телефон не указан";

    return (
        <div className="relative tc-noise-overlay w-full h-full overflow-hidden">
            <div className="pointer-events-none absolute inset-0 overflow-hidden">
                <img
                    src={vortex}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[10vw] -left-[10vw] w-[50vw] max-w-[640px] select-none"
                    style={{opacity: 0.30}}
                    draggable={false}
                />
                <img
                    src={repeat}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[6vw] -right-[8vw] w-[40vw] max-w-[520px] select-none"
                    style={{opacity: 0.30}}
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[16vw] left-0 w-[100vw] max-w-none select-none"
                    style={{opacity: 0.30}}
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[16vw] right-0 -scale-x-100 w-[100vw] max-w-none select-none"
                    style={{opacity: 0.3}}
                    draggable={false}
                />
            </div>

            <div className="page-content relative">
                <div
                    className="flex flex-col gap-4 overflow-hidden rounded-3xl p-5 sm:p-8"
                >
                    <div className="flex flex-col gap-4 sm:justify-between ">
                        <div className="flex flex-col gap-2 min-w-0">
                            <div className="flex flex-wrap items-center gap-2">
                                <Tag appearance="brand" icon={<Sparkle20Regular/>}>Дашборд</Tag>
                                <Badge appearance="tint" size="large">Home2</Badge>
                            </div>

                            <LargeTitle
                                truncate wrap={false} block
                            >
                                {displayName ? `Привет, ${displayName}` : "Привет"}
                            </LargeTitle>
                        </div>

                        <div className="flex flex-col gap-2 sm:flex-row sm:justify-between sm:items-end">

                            <div className="flex flex-col gap-2 ">
                                <Body2>
                                    Короткий обзор: баланс, визит и быстрые действия.
                                </Body2>

                                <div className="flex flex-wrap gap-2 ">
                                    <Tag appearance={emailConfirmed ? "brand" : "outline"}>
                                        {emailConfirmed ? "Email подтверждён" : "Email не подтверждён"}
                                    </Tag>
                                    <Tag appearance={phoneConfirmed ? "brand" : "outline"}>
                                        {phoneConfirmed ? "Телефон подтверждён" : "Телефон не подтверждён"}
                                    </Tag>
                                </div>
                            </div>

                            <div className="flex flex-row gap-2 items-end shrink-0">
                                <Button appearance="primary" onClick={() => navigate("/personal-data")}>
                                    <Text truncate wrap={false}>
                                        Персональные данные
                                    </Text>
                                </Button>
                                <Button appearance="secondary" onClick={() => navigate("/personal-data")}>
                                    Смена пароля
                                </Button>
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
                            isActive={!!isActiveVisit}
                            elapsedSeconds={activeElapsedSeconds}
                            estimateTotal={activeEstimate?.total ?? null}
                            visitInfo={isActiveVisit && activeVisitData ? "Визит идёт" : "Нет активного визита"}
                            onNavigateVisit={() => navigate(isActiveVisit ? "/visit/active" : "/visit/start")}
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
                            onNavigate={() => navigate("/personal-data")}
                        />

                        <QuickActionsCard
                            onNavigateProfile={() => navigate("/personal-data")}
                            onNavigateBilling={() => navigate("/billing")}
                        />
                    </div>
                </div>
            </div>
        </div>
    );
};
