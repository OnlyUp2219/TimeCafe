import { useParams, useNavigate } from "react-router-dom";
import {
    Avatar,
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    Caption1,
    Spinner,
    Title2,
    Title3,
    Subtitle2Stronger,
} from "@fluentui/react-components";
import { ArrowLeft20Regular, Checkmark20Regular, CheckmarkCircle20Regular, Money20Regular, Clock20Regular, Warning20Regular } from "@fluentui/react-icons";
import { useGetVisitByIdQuery, useFixateVisitTimeMutation, useApproveVisitMutation, useRejectVisitMutation, useForceEndVisitMutation } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useComponentSize } from "@hooks/useComponentSize";
import { VisitStatus } from "@app-types/visit";
import { useState, useEffect } from "react";
import { HasPermission } from "@components/Guard/HasPermission";
import { DismissableError } from "@components/DismissableError/DismissableError";
import { Permissions } from "@shared/auth/permissions";
import { VisitStatusBadge } from "@components/VisitStatusBadge";
import { ApproveVisitDialog } from "@components/Admin/ApproveVisitDialog/ApproveVisitDialog";
import { useGetProfileByUserIdQuery } from "@store/api/profileApi";
import { useGetBalanceQuery, useGetInvoiceByVisitIdQuery, usePayInvoiceMutation } from "@store/api/billingApi";
import { NO_DATA } from "@shared/const/placeholders";
import { PageLoader } from "@components/PageLoader/PageLoader";
import { RequirePermission } from "@app/components/RequirePermission/RequirePermission";
import { getUserFullName } from "@utility/userUtils";
import { formatDateTime } from "@utility/dateUtils";
import { formatMoney } from "@utility/formatUtils";

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
    const [forceEndVisit, { isLoading: forcingEnd }] = useForceEndVisitMutation();

    const visit = visitData;
    const { data: invoice, refetch: refetchInvoice } = useGetInvoiceByVisitIdQuery(
        id ?? "",
        { skip: !visit || (visit.status !== VisitStatus.WaitingForPayment && visit.status !== VisitStatus.Completed) }
    );

    const { data: profile } = useGetProfileByUserIdQuery(visit?.userId ?? "", { skip: !visit?.userId });
    const { data: balanceObj } = useGetBalanceQuery(visit?.userId ?? "", { skip: !visit?.userId });

    const userFullName = getUserFullName(profile, visit?.userId);

    const balance = balanceObj?.currentBalance ?? 0;
    const errorMessage = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const warnings: string[] = [];
    if (visit?.resourceMaxGuests != null && (visit?.guestsCount || 1) > visit.resourceMaxGuests) {
        warnings.push(`Превышена вместимость комнаты (макс: ${visit.resourceMaxGuests} чел., гостей: ${visit.guestsCount || 1})`);
    }
    if (visit?.userId && balanceObj) {
        if (balance < 0 || (visit.status === VisitStatus.Active && balance < (visit.calculatedCost ?? 0))) {
            warnings.push(`У пользователя недостаточно средств (Текущий баланс: ${balance.toLocaleString("ru-RU")} ₽)`);
        }
    }

    const [elapsedMinutes, setElapsedMinutes] = useState(0);

    useEffect(() => {
        if (!visit?.entryTime) return;
        if (visit.status !== VisitStatus.Active && visit.status !== VisitStatus.WaitingForPayment && visit.status !== VisitStatus.Completed) {
            setElapsedMinutes(0);
            return;
        }

        const calculateElapsed = () => {
            const start = new Date(visit.entryTime).getTime();
            const end = visit.exitTime ? new Date(visit.exitTime).getTime() : new Date().getTime();
            const mins = Math.floor((end - start) / 60000);
            setElapsedMinutes(mins > 0 ? mins : 0);
        };

        calculateElapsed();
        const interval = setInterval(calculateElapsed, 60000);
        return () => clearInterval(interval);
    }, [visit?.entryTime, visit?.exitTime, visit?.status]);

    useEffect(() => {
        if (visit?.status === VisitStatus.WaitingForPayment) {
            const interval = setInterval(() => {
                void refetch();
            }, 2000);
            return () => clearInterval(interval);
        }
    }, [visit?.status, refetch]);

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

    const handleForceEnd = async () => {
        if (!id) return;
        setEndError(null);
        try {
            await forceEndVisit(id).unwrap();
            void refetch();
        } catch (err) {
            setEndError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось принудительно завершить визит");
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
            // Polling will automatically pick up the status change.
            void refetchInvoice();
            void refetch();
        } catch (err) {
            setEndError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось отметить оплату");
        }
    };

    if (isLoading) return <PageLoader />;

    const notFoundError = !visit && !isLoading && !errorMessage ? "Визит не найден" : null;

    return (
        <RequirePermission can={Permissions.VenueVisitRead}>
            <div className="flex flex-col gap-2">
                <Button size={sizes.button} appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/visits")} className="w-fit">
                    Назад к визитам
                </Button>

                <div className="flex items-center justify-between flex-wrap gap-4">
                    <Title2>Визит</Title2>
                    <div className="flex gap-2 flex-wrap">
                        <HasPermission can={Permissions.VenueVisitApprove}>
                            {visit?.status === VisitStatus.Pending && (
                                <Button
                                    size={sizes.button}
                                    appearance="primary"
                                    icon={approving ? <Spinner size="tiny" /> : <CheckmarkCircle20Regular />}
                                    onClick={() => setDialogOpen(true)}
                                    disabled={approving}
                                >
                                    Подтвердить
                                </Button>
                            )}
                        </HasPermission>
                        <HasPermission can={Permissions.VenueVisitUpdate}>
                            {visit?.status === VisitStatus.Active && (
                                <Button
                                    size={sizes.button}
                                    appearance="primary"
                                    icon={ending ? <Spinner size="tiny" /> : <Checkmark20Regular />}
                                    onClick={handleEnd}
                                    disabled={ending}
                                >
                                    Завершить визит
                                </Button>
                            )}
                        </HasPermission>
                        <HasPermission can={Permissions.VenueVisitForceEnd}>
                            {visit?.status === VisitStatus.Active && !visit?.isFinishRequested && (
                                <Button
                                    size={sizes.button}
                                    appearance="secondary"
                                    icon={forcingEnd ? <Spinner size="tiny" /> : <Clock20Regular />}
                                    onClick={handleForceEnd}
                                    disabled={forcingEnd}
                                >
                                    Принудительный выход
                                </Button>
                            )}
                        </HasPermission>
                    </div>
                </div>

                {visit && (
                    <ApproveVisitDialog
                        open={dialogOpen}
                        visit={visit}
                        onOpenChange={setDialogOpen}
                        onApprove={handleApprove}
                        onReject={handleReject}
                        approving={approving}
                        rejecting={rejecting}
                    />
                )}

                <DismissableError error={errorMessage} />
                <DismissableError error={notFoundError} />
                <DismissableError error={endError} />

                <div className="flex flex-col gap-4">
                    <Card size={sizes.card}>
                        <div className="flex items-center gap-4 flex-wrap">
                            <Avatar
                                name={visit?.userId ? userFullName : "Анонимный гость"}
                                image={profile?.photoUrl ? { src: profile.photoUrl } : undefined}
                                size={56}
                            />
                            <div className="min-w-0 flex-1 flex flex-col gap-1">
                                <Body2 className="font-semibold text-lg">{visit?.userId ? userFullName : "Анонимный гость (Walk-in)"}</Body2>
                                <Caption1 className="font-mono text-(--colorNeutralForeground3)">ID: {visit?.userId || "Не зарегистрирован"}</Caption1>
                                <Badge color="informative" appearance="tint" className="w-fit mt-1">Количество гостей: {visit?.guestsCount || 1}</Badge>
                            </div>
                            {balanceObj && (
                                <div className="flex flex-col items-end">
                                    <Caption1 className="text-(--colorNeutralForeground3)">Баланс гостя</Caption1>
                                    <Badge
                                        color={balance < 0 ? "danger" : balance === 0 ? "warning" : "success"}
                                        appearance="filled"
                                    >
                                        {balance.toLocaleString("ru-RU")} ₽
                                    </Badge>
                                </div>
                            )}
                            {visit && <VisitStatusBadge status={visit.status} size="large" />}
                        </div>
                    </Card>

                    {warnings.length > 0 && (
                        <Card size={sizes.card} className="border-(--colorStatusWarningBorderActive) bg-(--colorStatusWarningBackground1)">
                            <div className="flex flex-col gap-2">
                                <Body2 className="font-semibold flex items-center gap-2">
                                    <Warning20Regular className="text-(--colorStatusWarningForeground1)" />
                                    Предупреждения
                                </Body2>
                                <div className="flex flex-col gap-1">
                                    {warnings.map((w, i) => (
                                        <Body1 key={i} className="text-(--colorStatusWarningForeground1)">• {w}</Body1>
                                    ))}
                                </div>
                            </div>
                        </Card>
                    )}

                    {(visit?.status === VisitStatus.WaitingForPayment || visit?.status === VisitStatus.Completed) && invoice && (
                        <Card size={sizes.card} className="border-(--colorBrandStroke1) border-2 bg-(--colorNeutralBackground2)">
                            <div className="flex flex-col gap-4">
                                <div className="flex items-center justify-between flex-wrap gap-4">
                                    <div className="flex flex-col gap-2">
                                        <Subtitle2Stronger>Счёт к оплате (Инвойс)</Subtitle2Stronger>
                                        <Caption1 className="text-(--colorNeutralForeground3)">ID счёта: {invoice.invoiceId}</Caption1>
                                        <Badge color={invoice.status === 2 ? "success" : "warning"} appearance="filled" className="w-fit">
                                            {invoice.status === 2 ? "Оплачен" : "Ожидает оплаты"}
                                        </Badge>
                                    </div>
                                    <div className="flex flex-col items-end gap-2">
                                        <Caption1 className="text-(--colorNeutralForeground3)">{invoice.status === 2 ? "Оплачено" : "К оплате"}</Caption1>
                                        <Title3 style={{ color: invoice.status === 2 ? "var(--colorPaletteGreenForeground1)" : "var(--colorBrandForeground1)" }}>{formatMoney(invoice.totalAmount)}</Title3>
                                    </div>
                                </div>

                                {visit.status === VisitStatus.WaitingForPayment && invoice.status !== 2 && (
                                    <HasPermission can={Permissions.BillingInvoicePay}>
                                        <div className="flex flex-col gap-2">
                                            <Body2>Провести оплату вручную:</Body2>
                                            <div className="flex gap-2 flex-wrap">
                                                <Button size={sizes.button} appearance="primary" onClick={() => handleReceivePayment(2)}>
                                                    Наличные
                                                </Button>
                                                <Button size={sizes.button} appearance="secondary" onClick={() => handleReceivePayment(1)}>
                                                    Терминал
                                                </Button>
                                                <Button size={sizes.button} appearance="secondary" onClick={() => handleReceivePayment(3)}>
                                                    Перевод (Онлайн)
                                                </Button>
                                            </div>
                                        </div>
                                    </HasPermission>
                                )}
                            </div>
                        </Card>
                    )}

                    <div className="flex flex-row flex-wrap gap-4 w-full">
                        <Card size={sizes.card} className="flex-1 basis-[400px] min-w-[300px] bg-(--colorNeutralBackground1)">
                            <div className="flex flex-col gap-2">
                                <Subtitle2Stronger>Детали тарифа</Subtitle2Stronger>
                                <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-2">
                                    <div className="flex flex-col">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Название</Caption1>
                                        <Body1 className="font-semibold">{visit?.tariffName || NO_DATA}</Body1>
                                    </div>
                                    <div className="flex flex-col">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Тип биллинга</Caption1>
                                        <Body1>{visit?.tariffBillingType === 1 ? "Почасовой" : "Поминутный"}</Body1>
                                    </div>
                                    <div className="flex flex-col">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Ставка (за мин)</Caption1>
                                        <Body1>{formatMoney(visit?.tariffPricePerMinute ?? 0)}</Body1>
                                    </div>
                                    <div className="flex flex-col">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Мин. время (мин)</Caption1>
                                        <Body1>{visit?.tariffMinSessionMinutes ?? "Нет"}</Body1>
                                    </div>
                                    <div className="flex flex-col">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Округление</Caption1>
                                        <Body1>{visit?.tariffRoundingRule ?? "Нет"}</Body1>
                                    </div>
                                </div>
                                {visit?.tariffDescription && (
                                    <div className="flex flex-col mt-3 border-t border-(--colorNeutralStroke1) pt-3">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Описание тарифа</Caption1>
                                        <Body2 className="text-(--colorNeutralForeground2)">{visit.tariffDescription}</Body2>
                                    </div>
                                )}
                            </div>
                        </Card>

                        <Card size={sizes.card} className="flex-1 basis-[400px] min-w-[300px] border border-(--colorBrandStroke1) bg-(--colorNeutralBackground2)">
                            <div className="flex flex-col gap-2">
                                <Subtitle2Stronger>Детализация визита и стоимости</Subtitle2Stronger>
                                <div className="flex flex-row flex-wrap gap-6 mt-2">
                                    <div className="flex flex-col min-w-[120px]">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Вход</Caption1>
                                        <Body1>{formatDateTime(visit?.entryTime ?? null)}</Body1>
                                    </div>
                                    <div className="flex flex-col min-w-[120px]">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Выход</Caption1>
                                        <Body1>
                                            {visit?.exitTime
                                                ? formatDateTime(visit.exitTime)
                                                : visit?.status === VisitStatus.Active
                                                    ? "Активен"
                                                    : NO_DATA}
                                        </Body1>
                                    </div>
                                    <div className="flex flex-col min-w-[120px]">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Длительность</Caption1>
                                        <Body1>{elapsedMinutes} мин.</Body1>
                                    </div>
                                    <div className="flex flex-col min-w-[120px]">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Запланировано</Caption1>
                                        <Body1>{visit?.plannedMinutes ? `${visit.plannedMinutes} мин.` : "Без ограничений"}</Body1>
                                    </div>
                                    <div className="flex flex-col min-w-[120px]">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Текущая стоимость</Caption1>
                                        {visit?.status === VisitStatus.Active || visit?.status === VisitStatus.WaitingForPayment || visit?.status === VisitStatus.Completed
                                            ? <Title3 style={{ color: "var(--colorBrandForeground1)" }}>
                                                {formatMoney(visit?.calculatedCost ?? (visit?.tariffPricePerMinute ?? 0) * elapsedMinutes * (visit?.guestsCount || 1))}
                                            </Title3>
                                            : <Body1 className="text-(--colorNeutralForeground3)">—</Body1>}
                                    </div>
                                </div>
                            </div>
                        </Card>
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                        <Card size={sizes.card} className="col-span-1">
                            <div className="flex flex-col">
                                <Body2>ID визита</Body2>
                                <Caption1 className="font-mono ">{visit?.visitId || NO_DATA}</Caption1>
                            </div>
                        </Card>

                        {visit?.approvedByUserId && (
                            <Card size={sizes.card} className="col-span-1">
                                <div className="flex flex-col">
                                    <Body2>Подтверждён пользователем</Body2>
                                    <Caption1 className="font-mono ">{visit.approvedByUserId}</Caption1>
                                    <Caption1 className="text-(--colorNeutralForeground3)">{visit.approvedAt ? formatDateTime(visit.approvedAt) : NO_DATA}</Caption1>
                                </div>
                            </Card>
                        )}
                        {visit?.rejectionReason && (
                            <Card size={sizes.card} className="col-span-1 sm:col-span-2 lg:col-span-3 border-(--colorPaletteRedBorderActive) bg-(--colorPaletteRedBackground1)">
                                <div className="flex flex-col gap-2">
                                    <Body2 className="font-semibold text-(--colorPaletteRedForeground1) flex items-center gap-2">
                                        Причина отклонения / отмены
                                    </Body2>
                                    <Body1 className="text-(--colorPaletteRedForeground1)">{visit.rejectionReason}</Body1>
                                </div>
                            </Card>
                        )}
                    </div>
                </div>
            </div>
        </RequirePermission>
    );
};
