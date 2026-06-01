import { useParams, useNavigate } from "react-router-dom";
import {
    Avatar,
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    Caption1,
    MessageBar,
    MessageBarBody,
    Spinner,
    Title2,
    Title3,
    Subtitle2Stronger,
} from "@fluentui/react-components";
import { ArrowLeft20Regular, Checkmark20Regular, CheckmarkCircle20Regular, Money20Regular } from "@fluentui/react-icons";
import { useGetVisitByIdQuery, useFixateVisitTimeMutation, useApproveVisitMutation, useRejectVisitMutation } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useComponentSize } from "@hooks/useComponentSize";
import { VisitStatus } from "@app-types/visit";
import { useState } from "react";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions } from "@shared/auth/permissions";
import { VisitStatusBadge } from "@components/VisitStatusBadge";
import { ApproveVisitDialog } from "@components/Admin/ApproveVisitDialog/ApproveVisitDialog";
import { useGetProfileByUserIdQuery } from "@store/api/profileApi";
import { useGetBalanceQuery, useGetInvoiceByVisitIdQuery, usePayInvoiceMutation } from "@store/api/billingApi";

import { CURRENCY_SYMBOL } from "@shared/const/currency";
import { NO_DATA } from "@shared/const/placeholders";
import { PageLoader } from "@components/PageLoader/PageLoader";

const formatDateTime = (iso: string | null) => {
    if (!iso) return NO_DATA;
    return new Date(iso).toLocaleString("ru-RU", {
        day: "2-digit", month: "2-digit", year: "numeric",
        hour: "2-digit", minute: "2-digit", second: "2-digit"
    });
};

const formatMoney = (v: number | null) => v != null ? `${v.toFixed(2)} ${CURRENCY_SYMBOL}` : NO_DATA;



export const VisitDetailPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { sizes } = useComponentSize();
    const [endError, setEndError] = useState<string | null>(null);
    const [dialogOpen, setDialogOpen] = useState(false);

    const { data: visitData, isLoading, error, refetch } = useGetVisitByIdQuery(id!, { skip: !id });
    const [fixateVisitTime, { isLoading: ending }] = useFixateVisitTimeMutation();
    const [approveVisit, { isLoading: approving }] = useApproveVisitMutation();
    const [rejectVisit, { isLoading: rejecting }] = useRejectVisitMutation();
    const [payInvoice, { isLoading: paying }] = usePayInvoiceMutation();

    const visit = visitData;
    const { data: invoice, refetch: refetchInvoice } = useGetInvoiceByVisitIdQuery(
        id ?? "",
        { skip: !visit || visit.status !== VisitStatus.WaitingForPayment }
    );

    const { data: profile } = useGetProfileByUserIdQuery(visit?.userId ?? "", { skip: !visit?.userId });
    const { data: balanceObj } = useGetBalanceQuery(visit?.userId ?? "", { skip: !visit?.userId });

    const userFullName = profile && (profile.firstName || profile.lastName)
        ? [profile.firstName, profile.middleName, profile.lastName].filter(Boolean).join(" ")
        : visit?.userId ? "Зарегистрированный пользователь" : "Анонимный гость (Walk-in)";

    const balance = balanceObj?.currentBalance ?? 0;
    const errorMessage = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const handleEnd = async () => {
        if (!id) return;
        setEndError(null);
        try {
            await fixateVisitTime(id).unwrap();
            void refetch();
        } catch (err) {
            setEndError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось зафиксировать время визита");
        }
    };

    const handleApprove = async (visitId: string) => {
        setEndError(null);
        try {
            await approveVisit(visitId).unwrap();
            setDialogOpen(false);
            void refetch();
        } catch (err) {
            setEndError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось подтвердить визит");
        }
    };

    const handleReject = async (visitId: string, reason: string) => {
        setEndError(null);
        try {
            await rejectVisit({ visitId, reason }).unwrap();
            setDialogOpen(false);
            void refetch();
        } catch (err) {
            setEndError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось отклонить визит");
        }
    };

    const handleReceivePayment = async (method: number) => {
        if (!invoice) return;
        setEndError(null);
        try {
            await payInvoice({ invoiceId: invoice.invoiceId, method }).unwrap();
            void refetchInvoice();
            void refetch();
        } catch (err) {
            setEndError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось отметить оплату");
        }
    };

    if (isLoading) return <PageLoader />;

    if (errorMessage) return (
        <div>
            <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/visits")} className="mb-4">
                Назад к визитам
            </Button>
            <MessageBar intent="error">
                <MessageBarBody>{errorMessage}</MessageBarBody>
            </MessageBar>
        </div>
    );

    if (!visit) return (
        <div>
            <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/visits")}>
                Назад
            </Button>
            <MessageBar intent="warning" className="mt-4">
                <MessageBarBody>Визит не найден</MessageBarBody>
            </MessageBar>
        </div>
    );

    return (
        <div>
            <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/visits")} className="mb-4">
                Назад к визитам
            </Button>

            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <Title2>Визит</Title2>
                <div className="flex gap-2 flex-wrap">
                    <HasPermission can={Permissions.VenueVisitApprove}>
                        {visit.status === VisitStatus.Pending && (
                            <Button
                                appearance="primary"
                                icon={approving ? <Spinner size="tiny" /> : <CheckmarkCircle20Regular />}
                                onClick={() => setDialogOpen(true)}
                                disabled={approving}
                            >
                                Подтвердить / Отклонить
                            </Button>
                        )}
                    </HasPermission>
                    <HasPermission can={Permissions.VenueVisitEnd}>
                        {visit.status === VisitStatus.Active && (
                            <Button
                                appearance="primary"
                                icon={ending ? <Spinner size="tiny" /> : <Checkmark20Regular />}
                                onClick={handleEnd}
                                disabled={ending}
                            >
                                {visit.isFinishRequested ? "Зафиксировать время (Запрошен выход)" : "Зафиксировать время (Касса)"}
                            </Button>
                        )}
                    </HasPermission>
                </div>
            </div>

            <ApproveVisitDialog
                open={dialogOpen}
                visit={visit}
                onOpenChange={setDialogOpen}
                onApprove={handleApprove}
                onReject={handleReject}
                approving={approving}
                rejecting={rejecting}
            />

            {endError && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{endError}</MessageBarBody>
                </MessageBar>
            )}

            <div className="flex flex-col gap-4">
                <Card size={sizes.card}>
                    <div className="flex items-center gap-4 flex-wrap">
                        <Avatar
                            name={userFullName}
                            image={profile?.photoUrl ? { src: profile.photoUrl } : undefined}
                            size={48}
                        />
                        <div className="min-w-0 flex-1 flex flex-col">
                            <Body2 className="font-semibold text-base">{userFullName}</Body2>
                            <Caption1 className="font-mono text-[var(--colorNeutralForeground3)]">ID: {visit.userId}</Caption1>
                        </div>
                        {balanceObj && (
                            <div className="flex flex-col items-end">
                                <Caption1 className="text-[var(--colorNeutralForeground3)]">Баланс гостя</Caption1>
                                <Badge
                                    color={balance < 0 ? "danger" : balance === 0 ? "warning" : "success"}
                                    appearance="filled"
                                >
                                    {balance.toLocaleString("ru-RU")} ₽
                                </Badge>
                            </div>
                        )}
                        <VisitStatusBadge status={visit.status} size="large" />
                    </div>
                </Card>

                {visit.status === VisitStatus.WaitingForPayment && invoice && (
                    <Card className="border-[var(--colorBrandStroke1)] border-2 bg-[var(--colorNeutralBackground2)] p-6">
                        <div className="flex flex-col gap-4">
                            <div className="flex items-center justify-between border-b pb-4 flex-wrap gap-4">
                                <div className="flex flex-col">
                                    <Subtitle2Stronger>Счёт к оплате (Инвойс)</Subtitle2Stronger>
                                    <Caption1 className="font-mono mt-1 text-[var(--colorNeutralForeground3)]">ID счёта: {invoice.invoiceId}</Caption1>
                                </div>
                                <div className="flex flex-col items-end">
                                    <Caption1 className="text-[var(--colorNeutralForeground3)]">К оплате</Caption1>
                                    <Title3 style={{ color: "var(--colorPaletteRedBorderActive)" }}>{formatMoney(invoice.totalAmount)}</Title3>
                                </div>
                            </div>

                            <div className="flex flex-col gap-3">
                                <Body2 className="font-semibold">Принять оплату на кассе:</Body2>
                                <div className="flex gap-3 flex-wrap">
                                    <Button
                                        appearance="primary"
                                        icon={paying ? <Spinner size="tiny" /> : <CheckmarkCircle20Regular />}
                                        onClick={() => handleReceivePayment(2)}
                                        disabled={paying}
                                    >
                                        Наличными (Cash)
                                    </Button>
                                    <Button
                                        appearance="outline"
                                        icon={paying ? <Spinner size="tiny" /> : <Money20Regular />}
                                        onClick={() => handleReceivePayment(1)}
                                        disabled={paying}
                                    >
                                        Картой через POS-терминал (Card)
                                    </Button>
                                    
                                    {visit.userId && (
                                        <Button
                                            appearance="subtle"
                                            icon={paying ? <Spinner size="tiny" /> : <Checkmark20Regular />}
                                            onClick={() => handleReceivePayment(3)}
                                            disabled={paying || balance < invoice.totalAmount}
                                        >
                                            Списать с баланса гостя (доступно {formatMoney(balance)})
                                        </Button>
                                    )}
                                </div>
                                {visit.userId && balance < invoice.totalAmount && (
                                    <Caption1 style={{ color: "var(--colorPaletteRedBorderActive)" }}>
                                        Недостаточно средств на балансе гостя для списания.
                                    </Caption1>
                                )}
                            </div>
                        </div>
                    </Card>
                )}

                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    <Card size={sizes.card}>
                        <div className="flex flex-col">
                            <Body2>Тариф</Body2>
                            <Title3>{visit.tariffName || NO_DATA}</Title3>
                        </div>
                    </Card>
                    <Card size={sizes.card}>
                        <div className="flex flex-col">
                            <Body2>Вход</Body2>
                            <Body1>{formatDateTime(visit.entryTime)}</Body1>
                        </div>
                    </Card>
                    <Card size={sizes.card}>
                        <div className="flex flex-col">
                            <Body2>Выход</Body2>
                            <Body1>{formatDateTime(visit.exitTime)}</Body1>
                        </div>
                    </Card>
                    <Card size={sizes.card}>
                        <div className="flex flex-col">
                            <Body2>Стоимость</Body2>
                            <Title3>{formatMoney(visit.calculatedCost)}</Title3>
                        </div>
                    </Card>
                    <Card size={sizes.card}>
                        <div className="flex flex-col">
                            <Body2>ID визита</Body2>
                            <Caption1 className="font-mono">{visit.visitId}</Caption1>
                        </div>
                    </Card>
                    {visit.approvedByUserId && (
                        <Card size={sizes.card}>
                            <div className="flex flex-col">
                                <Body2>Подтверждён пользователем</Body2>
                                <Caption1 className="font-mono">{visit.approvedByUserId}</Caption1>
                                <Caption1>{visit.approvedAt ? formatDateTime(visit.approvedAt) : NO_DATA}</Caption1>
                            </div>
                        </Card>
                    )}
                    {visit.rejectionReason && (
                        <Card size={sizes.card}>
                            <div className="flex flex-col">
                                <Body2>Причина отклонения</Body2>
                                <Body1>{visit.rejectionReason}</Body1>
                            </div>
                        </Card>
                    )}
                </div>
            </div>
        </div>
    );
};
