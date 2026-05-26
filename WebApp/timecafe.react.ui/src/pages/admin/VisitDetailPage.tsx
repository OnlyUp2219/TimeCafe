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
} from "@fluentui/react-components";
import { ArrowLeft20Regular, Checkmark20Regular, CheckmarkCircle20Regular, DismissCircle20Regular } from "@fluentui/react-icons";
import { useGetVisitByIdQuery, useEndVisitMutation, useApproveVisitMutation, useRejectVisitMutation } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useComponentSize } from "@hooks/useComponentSize";
import { VisitStatus } from "@app-types/visit";
import { useState } from "react";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions } from "@shared/auth/permissions";
import { VisitStatusBadge } from "@components/VisitStatusBadge";
import { ApproveVisitDialog } from "@components/Admin/ApproveVisitDialog/ApproveVisitDialog";

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

    const { data: visitData, isLoading, error } = useGetVisitByIdQuery(id!, { skip: !id });
    const [endVisit, { isLoading: ending }] = useEndVisitMutation();
    const [approveVisit, { isLoading: approving }] = useApproveVisitMutation();
    const [rejectVisit, { isLoading: rejecting }] = useRejectVisitMutation();

    const visit = visitData;
    const errorMessage = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const handleEnd = async () => {
        if (!id) return;
        setEndError(null);
        try {
            await endVisit(id).unwrap();
        } catch (err) {
            setEndError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось завершить визит");
        }
    };

    const handleApprove = async (visitId: string) => {
        setEndError(null);
        try {
            await approveVisit(visitId).unwrap();
            setDialogOpen(false);
        } catch (err) {
            setEndError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось подтвердить визит");
        }
    };

    const handleReject = async (visitId: string, reason: string) => {
        setEndError(null);
        try {
            await rejectVisit({ visitId, reason }).unwrap();
            setDialogOpen(false);
        } catch (err) {
            setEndError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось отклонить визит");
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
                                Завершить визит
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
                        <Avatar name={visit.userId} size={48} />
                        <div className="min-w-0 flex-1">
                            <Body2 block>Пользователь</Body2>
                            <Caption1 block className="font-mono">{visit.userId}</Caption1>
                        </div>
                        <VisitStatusBadge status={visit.status} size="large" />
                    </div>
                </Card>

                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    <Card size={sizes.card}>
                        <Body2 block>Тариф</Body2>
                        <Title3>{visit.tariffName || NO_DATA}</Title3>
                    </Card>
                    <Card size={sizes.card}>
                        <Body2 block>Вход</Body2>
                        <Body1 block>{formatDateTime(visit.entryTime)}</Body1>
                    </Card>
                    <Card size={sizes.card}>
                        <Body2 block>Выход</Body2>
                        <Body1 block>{formatDateTime(visit.exitTime)}</Body1>
                    </Card>
                    <Card size={sizes.card}>
                        <Body2 block>Стоимость</Body2>
                        <Title3>{formatMoney(visit.calculatedCost)}</Title3>
                    </Card>
                    <Card size={sizes.card}>
                        <Body2 block>ID визита</Body2>
                        <Caption1 block className="font-mono">{visit.visitId}</Caption1>
                    </Card>
                    {visit.approvedByUserId && (
                        <Card size={sizes.card}>
                            <Body2 block>Подтверждён пользователем</Body2>
                            <Caption1 block className="font-mono">{visit.approvedByUserId}</Caption1>
                            <Caption1 block>{visit.approvedAt ? formatDateTime(visit.approvedAt) : NO_DATA}</Caption1>
                        </Card>
                    )}
                    {visit.rejectionReason && (
                        <Card size={sizes.card}>
                            <Body2 block>Причина отклонения</Body2>
                            <Body1 block>{visit.rejectionReason}</Body1>
                        </Card>
                    )}
                </div>
            </div>
        </div>
    );
};
