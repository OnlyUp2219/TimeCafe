import { useNavigate, useParams } from "react-router-dom";
import { useCallback, useMemo, useState } from "react";
import {
    type TableColumnDefinition,
    type TableColumnSizingOptions,
    Tooltip,
    Badge,
    Body1,
    Body1Strong,
    Body2,
    Button,
    Caption1,
    Card,
    createTableColumn,
    Dialog,
    DialogActions,
    DialogBody,
    DialogContent,
    DialogSurface,
    DialogTitle,
    DialogTrigger,
    MessageBar,
    MessageBarBody,
    Spinner,
    TableCellLayout,
    Title2,
    Title3,
} from "@fluentui/react-components";
import { SecureAvatar } from "@components/SecureAvatar/SecureAvatar";
import { PageLoader } from "@components/PageLoader/PageLoader";
import { ArrowLeft20Regular, Delete20Regular, PeopleSettings20Regular, Eye20Regular } from "@fluentui/react-icons";
import { TextareaWithCounter } from "@components/FormFields";
import { AuditLogDetails } from "@components/Admin/AuditLogDetails";
import { useGetUserByIdQuery, useGetAuditLogsQuery, type AuditLogDto } from "@store/api/adminApi";
import { useGetBalanceQuery, useGetTransactionHistoryQuery } from "@store/api/billingApi";
import { useGetVisitHistoryQuery, useGetUserLoyaltyQuery } from "@store/api/venueApi";
import {
    useCreateAdditionalInfoMutation,
    useDeleteAdditionalInfoMutation,
    useGetAdditionalInfosByUserIdQuery,
    useGetProfileByUserIdReadOnlyQuery
} from "@store/api/profileApi";
import { useAppSelector } from "@store/hooks";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { EmptyState } from "@components/EmptyState/EmptyState";
import { useComponentSize } from "@hooks/useComponentSize";
import type { BillingTransaction } from "@app-types/billing";
import { TransactionType } from "@app-types/billing";
import type { VisitWithTariff } from "@app-types/visitWithTariff";
import { VisitStatus } from "@app-types/visit";
import { Permissions } from "@shared/auth/permissions";
import { HasPermission } from "@components/Guard/HasPermission";
import { DismissableError } from "@components/DismissableError/DismissableError";
import { LoyaltyProgress } from "@components/Loyalty/LoyaltyProgress";
import { NO_DATA } from "@shared/const/placeholders";
import { formatDateTime, formatDate } from "@utility/dateUtils";
import { formatMoney, formatDurationMs } from "@utility/formatUtils";
import { txTypeColor, txSourceLabel, txTypeLabel } from "@utility/billingUtils";
import { VisitStatusBadge } from "@components/VisitStatusBadge";
import { genderLabel, profileStatusLabel, profileStatusColor } from "@utility/userUtils";

import { usePagination } from "@hooks/usePagination";

export const UserDetailPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { sizes } = useComponentSize();

    const { page: txPage, size: txPageSize, setPage: setTxPage } = usePagination("adminUserDetailTx", 1, 10);
    const { page: notesPage, size: NOTES_PAGE_SIZE, setPage: setNotesPage } = usePagination("adminUserDetailNotes", 1, 3);
    const { page: auditPage, size: auditPageSize, setPage: setAuditPage } = usePagination("adminUserDetailAudit", 1, 5);

    const { data: userData, isLoading: userLoading, error: userError } = useGetUserByIdQuery(id!, { skip: !id, refetchOnMountOrArgChange: true });
    const user = userData?.user;

    const { data: balance, isLoading: balanceLoading } = useGetBalanceQuery(id!, { skip: !id });
    const { data: txData, isLoading: txLoading, error: txError } = useGetTransactionHistoryQuery(
        { userId: id!, page: txPage, pageSize: txPageSize },
        { skip: !id }
    );
    const [visitPage, setVisitPage] = useState(1);
    const visitPageSize = 10;
    const { data: visits = [], isLoading: visitsLoading } = useGetVisitHistoryQuery(
        { userId: id!, pageSize: 1000 },
        { skip: !id }
    );
    const paginatedVisits = useMemo(() => {
        const startIndex = (visitPage - 1) * visitPageSize;
        return visits.slice(startIndex, startIndex + visitPageSize);
    }, [visits, visitPage, visitPageSize]);
    const visitTotalPages = useMemo(() => {
        return Math.max(1, Math.ceil(visits.length / visitPageSize));
    }, [visits, visitPageSize]);

    const { data: loyalty, } = useGetUserLoyaltyQuery(id!, { skip: !id });
    const { data: profile } = useGetProfileByUserIdReadOnlyQuery(id!, { skip: !id });
    const { data: notesData, isLoading: notesLoading } = useGetAdditionalInfosByUserIdQuery(
        { userId: id!, page: notesPage, pageSize: NOTES_PAGE_SIZE },
        { skip: !id }
    );
    const adminNotes = notesData?.items ?? [];
    const totalNotes = notesData?.metadata?.totalCount ?? 0;
    const [createNote, { isLoading: creatingNote }] = useCreateAdditionalInfoMutation();
    const [deleteNote] = useDeleteAdditionalInfoMutation();
    const adminEmail = useAppSelector((state) => state.auth.email);

    const [addNoteOpen, setAddNoteOpen] = useState(false);
    const [newNoteText, setNewNoteText] = useState("");
    const [noteError, setNoteError] = useState<string | null>(null);

    const handleAddNote = useCallback(async () => {
        if (!id || !newNoteText.trim()) return;
        setNoteError(null);
        try {
            await createNote({ userId: id, infoText: newNoteText.trim(), createdBy: adminEmail || "admin" }).unwrap();
            setNewNoteText("");
            setAddNoteOpen(false);
        } catch (err) {
            setNoteError(getRtkErrorMessage(err as FetchBaseQueryError) || "Ошибка при сохранении заметки");
        }
    }, [id, newNoteText, createNote, adminEmail]);

    const handleDeleteNote = useCallback(async (infoId: string) => {
        if (!id) return;
        try {
            await deleteNote({ infoId, userId: id }).unwrap();
        } catch { /* error handled by RTK */ }
    }, [id, deleteNote]);

    const { data: auditData, isLoading: auditLoading, error: auditError } = useGetAuditLogsQuery(
        { userId: id!, page: auditPage, pageSize: auditPageSize },
        { skip: !id }
    );
    const auditLogs = auditData?.items ?? [];
    const auditTotalPages = auditData?.metadata?.totalPages ?? 1;
    const auditTotalCount = auditData?.metadata?.totalCount ?? 0;
    const auditErrorMessage = auditError ? getRtkErrorMessage(auditError as FetchBaseQueryError) : null;

    const [selectedAuditLog, setSelectedAuditLog] = useState<AuditLogDto | null>(null);
    const [auditDetailsOpen, setAuditDetailsOpen] = useState(false);

    const handleOpenDetails = useCallback((log: AuditLogDto) => {
        setSelectedAuditLog(log);
        setAuditDetailsOpen(true);
    }, []);

    const transactions = txData?.items ?? [];
    const txTotalPages = txData?.metadata.totalPages ?? 1;
    const txTotalCount = txData?.metadata.totalCount ?? 0;

    const errorMessage = userError ? getRtkErrorMessage(userError as FetchBaseQueryError) : null;
    const txErrorMessage = txError ? getRtkErrorMessage(txError as FetchBaseQueryError) : null;

    const displayName = useMemo(() => {
        const parts = [profile?.lastName, profile?.firstName, profile?.middleName].filter(Boolean).map(String);
        return parts.join(" ").trim() || user?.name || user?.email || "Пользователь";
    }, [profile?.firstName, profile?.lastName, profile?.middleName, user?.email, user?.name]);

    const nickname = useMemo(() => {
        if (user?.name === user?.email) return "";
        if (user?.name) return `@${user.name}`;
        const emailLogin = user?.email?.split("@")[0]?.trim();
        if (emailLogin) return `@${emailLogin}`;
        return "";
    }, [user?.email, user?.name]);

    const contactLine = [user?.email, user?.phoneNumber].filter(Boolean).join(" · ") || NO_DATA;

    const txColumnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        date: { minWidth: 130, defaultWidth: 160 },
        type: { minWidth: 100, defaultWidth: 140 },
        source: { minWidth: 100, defaultWidth: 130 },
        amount: { minWidth: 80, defaultWidth: 120 },
        balance: { minWidth: 80, defaultWidth: 120 },
        comment: { minWidth: 100, defaultWidth: 200 },
    }), []);

    const txColumns: TableColumnDefinition<BillingTransaction>[] = useMemo(() => [
        createTableColumn<BillingTransaction>({
            columnId: "date",
            compare: (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
            renderHeaderCell: () => "Дата",
            renderCell: (tx) => <TableCellLayout truncate>{formatDateTime(tx.createdAt)}</TableCellLayout>,
        }),
        createTableColumn<BillingTransaction>({
            columnId: "type",
            compare: (a, b) => a.type - b.type,
            renderHeaderCell: () => "Тип",
            renderCell: (tx) => (
                <TableCellLayout truncate>
                    <Badge appearance="tint" color={txTypeColor(tx.type)}>{txTypeLabel(tx.type)}</Badge>
                </TableCellLayout>
            ),
        }),
        createTableColumn<BillingTransaction>({
            columnId: "source",
            compare: (a, b) => a.source - b.source,
            renderHeaderCell: () => "Источник",
            renderCell: (tx) => <TableCellLayout truncate>{txSourceLabel(tx.source)}</TableCellLayout>,
        }),
        createTableColumn<BillingTransaction>({
            columnId: "amount",
            compare: (a, b) => a.amount - b.amount,
            renderHeaderCell: () => "Сумма",
            renderCell: (tx) => (
                <TableCellLayout truncate>
                    <span className={tx.type === TransactionType.Withdrawal ? "text-(--colorPaletteRedForeground1)" : "text-(--colorPaletteGreenForeground1)"}>
                        {tx.type === TransactionType.Withdrawal ? "−" : "+"}{formatMoney(Math.abs(tx.amount))}
                    </span>
                </TableCellLayout>
            ),
        }),
        createTableColumn<BillingTransaction>({
            columnId: "balance",
            compare: (a, b) => a.balanceAfter - b.balanceAfter,
            renderHeaderCell: () => "Баланс после",
            renderCell: (tx) => <TableCellLayout truncate>{formatMoney(tx.balanceAfter)}</TableCellLayout>,
        }),
        createTableColumn<BillingTransaction>({
            columnId: "comment",
            compare: (a, b) => (a.comment ?? "").localeCompare(b.comment ?? ""),
            renderHeaderCell: () => "Комментарий",
            renderCell: (tx) => <TableCellLayout truncate>{tx.comment || NO_DATA}</TableCellLayout>,
        }),
    ], []);

    const visitColumnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        tariff: { minWidth: 120, defaultWidth: 180 },
        status: { minWidth: 80, defaultWidth: 120 },
        entry: { minWidth: 130, defaultWidth: 160 },
        exit: { minWidth: 130, defaultWidth: 160 },
        duration: { minWidth: 120, defaultWidth: 145 },
        cost: { minWidth: 80, defaultWidth: 110 },
    }), []);

    const visitColumns: TableColumnDefinition<VisitWithTariff>[] = useMemo(() => [
        createTableColumn<VisitWithTariff>({
            columnId: "tariff",
            compare: (a, b) => a.tariffName.localeCompare(b.tariffName),
            renderHeaderCell: () => "Тариф",
            renderCell: (v) => <TableCellLayout truncate><Body1>{v.tariffName || NO_DATA}</Body1></TableCellLayout>,
        }),
        createTableColumn<VisitWithTariff>({
            columnId: "status",
            compare: (a, b) => a.status - b.status,
            renderHeaderCell: () => "Статус",
            renderCell: (v) => (
                <TableCellLayout truncate>
                    <VisitStatusBadge status={v.status} />
                </TableCellLayout>
            ),
        }),
        createTableColumn<VisitWithTariff>({
            columnId: "entry",
            compare: (a, b) => new Date(a.entryTime).getTime() - new Date(b.entryTime).getTime(),
            renderHeaderCell: () => "Вход",
            renderCell: (v) => <TableCellLayout truncate>{formatDateTime(v.entryTime)}</TableCellLayout>,
        }),
        createTableColumn<VisitWithTariff>({
            columnId: "exit",
            compare: (a, b) => new Date(a.exitTime ?? "").getTime() - new Date(b.exitTime ?? "").getTime(),
            renderHeaderCell: () => "Выход",
            renderCell: (v) => <TableCellLayout truncate>{formatDateTime(v.exitTime)}</TableCellLayout>,
        }),
        createTableColumn<VisitWithTariff>({
            columnId: "duration",
            compare: (a, b) => {
                const durA = a.exitTime ? new Date(a.exitTime).getTime() - new Date(a.entryTime).getTime() : 0;
                const durB = b.exitTime ? new Date(b.exitTime).getTime() - new Date(b.entryTime).getTime() : 0;
                return durA - durB;
            },
            renderHeaderCell: () => "Время проведения",
            renderCell: (v) => (
                <TableCellLayout truncate>
                    {v.status === VisitStatus.Pending || v.status === VisitStatus.Approved || v.status === VisitStatus.Rejected || v.status === VisitStatus.Cancelled
                        ? "0 мин."
                        : v.exitTime
                            ? formatDurationMs(new Date(v.exitTime).getTime() - new Date(v.entryTime).getTime())
                            : "В процессе"}
                </TableCellLayout>
            ),
        }),
        createTableColumn<VisitWithTariff>({
            columnId: "cost",
            compare: (a, b) => (a.calculatedCost ?? 0) - (b.calculatedCost ?? 0),
            renderHeaderCell: () => "Стоимость",
            renderCell: (v) => <TableCellLayout truncate>{v.calculatedCost != null ? formatMoney(v.calculatedCost) : NO_DATA}</TableCellLayout>,
        }),
    ], []);

    if (userLoading) {
        return <PageLoader label="Загрузка пользователя..." />;
    }

    return (
        <div className="flex flex-col gap-4">
            <div className="flex items-center gap-2 flex-wrap">
                <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/users")}>
                    Назад к списку
                </Button>
                {user && (
                    <HasPermission anyOf={[Permissions.RbacUserRoleAssign, Permissions.RbacUserRoleRemove]}>
                        <Button appearance="outline" icon={<PeopleSettings20Regular />} onClick={() => navigate(`/admin/users/${id}/roles`)}>
                            Управление ролями
                        </Button>
                    </HasPermission>
                )}
            </div>

            <DismissableError error={errorMessage} />

            {!user && !userLoading && (
                <MessageBar intent="warning" >
                    <MessageBarBody>Пользователь не найден</MessageBarBody>
                </MessageBar>
            )}

            {user && (
                <>
                    <Card size={sizes.card}>
                        <div className="flex items-start gap-4 flex-wrap ">
                            <div className="flex items-start gap-4 flex-1">
                                <SecureAvatar
                                    name={displayName}
                                    size={72}
                                    photoUrl={profile?.photoUrl}
                                />
                                <div className="flex gap-2 flex-wrap  flex-col">
                                    <Title2>{displayName}</Title2>
                                    <Body1>{contactLine}</Body1>
                                    <Body2 block className="text-(--colorNeutralForeground3) ">{user.role} · {user.status}</Body2>
                                    <div className="flex gap-2 flex-wrap">
                                        {nickname && <Badge appearance="outline">{nickname}</Badge>}
                                        {user && <Badge appearance="tint" color={user.emailConfirmed ? "success" : "warning"}>{user.emailConfirmed ? "Email подтверждён" : "Email не подтверждён"}</Badge>}
                                        {user && <Badge appearance="tint" color={user.phoneNumberConfirmed ? "success" : "warning"}>{user.phoneNumberConfirmed ? "Телефон подтверждён" : "Телефон не подтверждён"}</Badge>}
                                        {profile && <Badge appearance="tint" color={profileStatusColor(profile.profileStatus)}>{profileStatusLabel(profile.profileStatus)}</Badge>}
                                    </div>
                                    <Caption1 block className="text-(--colorNeutralForeground4)">{user.id}</Caption1>
                                </div>
                            </div>
                            <div className="flex gap-2">
                                <HasPermission anyOf={[Permissions.RbacUserRoleAssign, Permissions.RbacUserRoleRemove]}>
                                    <Button appearance="outline" onClick={() => navigate(`/admin/users/${id}/roles`)}>
                                        Роли
                                    </Button>
                                </HasPermission>
                            </div>
                        </div>
                    </Card>

                    <div className="flex flex-wrap gap-4 justify-between">
                        <HasPermission can={Permissions.BillingBalanceRead}>
                            <Card size={sizes.card} className="flex-1 min-w-[200px]">
                                <Body2>Баланс</Body2>
                                <Title3 className={!balanceLoading && (balance?.currentBalance ?? 0) < 0 ? "text-(--colorPaletteRedForeground1)" : "text-(--colorPaletteGreenForeground1)"}>
                                    {balanceLoading ? NO_DATA : formatMoney(balance?.currentBalance ?? 0)}
                                </Title3>
                            </Card>
                        </HasPermission>
                        <HasPermission can={Permissions.VenueVisitRead}>
                            <Card size={sizes.card} className="flex-1 min-w-[200px]">
                                <Body2>Визитов всего</Body2>
                                <Title3>{visitsLoading ? NO_DATA : visits.length}</Title3>
                            </Card>
                        </HasPermission>
                        <HasPermission can={Permissions.BillingBalanceRead}>
                            <Card size={sizes.card} className="flex-1 min-w-[200px]">
                                <Body2>Потрачено</Body2>
                                <Title3>
                                    {balanceLoading ? NO_DATA : formatMoney(balance?.totalSpent ?? 0)}
                                </Title3>
                            </Card>
                        </HasPermission>
                        <HasPermission can={Permissions.VenueLoyaltyRead}>
                            <Card size={sizes.card} className="flex-1 min-w-[300px]">
                                <Body1Strong block className="mb-2">Программа лояльности</Body1Strong>
                                <LoyaltyProgress
                                    visitCount={profile?.visitCount || 0}
                                    currentDiscount={loyalty?.personalDiscountPercent || 0}
                                />
                            </Card>
                        </HasPermission>
                    </div>

                    <div className="flex flex-wrap gap-4">
                        <HasPermission can={Permissions.UserProfileProfileRead}>
                            <Card className="flex-[1.3] flex-grow basis-[400px]" size={sizes.card}>
                                <Title3 >Профиль</Title3>
                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                    <div className="flex flex-col gap-0.5">
                                        <Body1 className="text-(--colorNeutralForeground3)">Имя</Body1>
                                        <Body1Strong>{profile?.firstName || NO_DATA}</Body1Strong>
                                    </div>
                                    <div className="flex flex-col gap-0.5">
                                        <Body1 className="text-(--colorNeutralForeground3)">Фамилия</Body1>
                                        <Body1Strong>{profile?.lastName || NO_DATA}</Body1Strong>
                                    </div>
                                    <div className="flex flex-col gap-0.5">
                                        <Body1 className="text-(--colorNeutralForeground3)">Отчество</Body1>
                                        <Body1Strong>{profile?.middleName || NO_DATA}</Body1Strong>
                                    </div>
                                    <div className="flex flex-col gap-0.5">
                                        <Body1 className="text-(--colorNeutralForeground3)">Пол</Body1>
                                        <Body1Strong>{genderLabel(profile?.gender)}</Body1Strong>
                                    </div>
                                    <div className="flex flex-col gap-0.5">
                                        <Body1 className="text-(--colorNeutralForeground3)">Дата рождения</Body1>
                                        <Body1Strong>{formatDate(profile?.birthDate)}</Body1Strong>
                                    </div>
                                    <div className="flex flex-col gap-0.5">
                                        <Body1 className="text-(--colorNeutralForeground3)">Email</Body1>
                                        <Body1Strong>{user.email || NO_DATA}</Body1Strong>
                                    </div>
                                    <div className="flex flex-col gap-0.5">
                                        <Body1 className="text-(--colorNeutralForeground3)">Телефон</Body1>
                                        <Body1Strong>{user.phoneNumber || NO_DATA}</Body1Strong>
                                    </div>
                                    <div className="flex flex-col gap-0.5">
                                        <Body1 className="text-(--colorNeutralForeground3)">Статус профиля</Body1>
                                        <div>
                                            <Badge appearance="tint" color={profileStatusColor(profile?.profileStatus)}>{profileStatusLabel(profile?.profileStatus)}</Badge>
                                        </div>
                                    </div>
                                    {profile?.banReason && (
                                        <div className="flex flex-col gap-0.5 sm:col-span-2">
                                            <Body1 className="text-(--colorNeutralForeground3)">Причина блокировки</Body1>
                                            <Body1Strong className="text-(--colorPaletteRedForeground1)">{profile.banReason}</Body1Strong>
                                        </div>
                                    )}
                                </div>
                            </Card>
                        </HasPermission>

                        <HasPermission can={Permissions.UserProfileAdditionalInfoRead}>
                            <Card className="flex-1 flex-grow basis-[300px] min-h-[360px] flex flex-col" size={sizes.card}>
                                <div className="flex items-center justify-between gap-3">
                                    <Title3>Заметки</Title3>
                                    <HasPermission can={Permissions.UserProfileAdditionalInfoCreate}>
                                        <Dialog open={addNoteOpen} onOpenChange={(_e, data) => { setAddNoteOpen(data.open); if (!data.open) setNoteError(null); }}>
                                            <DialogTrigger disableButtonEnhancement>
                                                <Button appearance="outline" size="small">+ Добавить</Button>
                                            </DialogTrigger>
                                            <DialogSurface>
                                                <DialogBody>
                                                    <DialogTitle>Новая заметка</DialogTitle>
                                                    <DialogContent>
                                                        <TextareaWithCounter
                                                            label="Текст заметки"
                                                            value={newNoteText}
                                                            onChange={(val) => {
                                                                setNewNoteText(val);
                                                                if (noteError) setNoteError(null);
                                                            }}
                                                            maxLength={2000}
                                                            placeholder="Введите текст заметки..."
                                                            rows={6}
                                                            validationMessage={noteError}
                                                            validationState={noteError ? "error" : "none"}
                                                        />
                                                    </DialogContent>
                                                    <DialogActions>
                                                        <DialogTrigger disableButtonEnhancement>
                                                            <Button appearance="secondary">Отмена</Button>
                                                        </DialogTrigger>
                                                        <Button appearance="primary" onClick={handleAddNote} disabled={!newNoteText.trim() || creatingNote}>
                                                            {creatingNote ? "Сохранение..." : "Сохранить"}
                                                        </Button>
                                                    </DialogActions>
                                                </DialogBody>
                                            </DialogSurface>
                                        </Dialog>
                                    </HasPermission>
                                </div>

                                <div className="flex flex-col gap-3 flex-1">
                                    {notesLoading && <Spinner size="small" />}
                                    {!notesLoading && adminNotes.length === 0 && (
                                        <div className="flex-1 flex items-center justify-center">
                                            <EmptyState
                                                title={NO_DATA}
                                                description="Добавьте первую заметку для этого пользователя"
                                            />
                                        </div>
                                    )}
                                    <div className="flex-1">
                                        {adminNotes.map((info) => (
                                            <Card appearance="filled-alternative" key={info.infoId} className="flex! flex-row! flex-wrap! mb-3 last:mb-0">
                                                <div className="flex-1">
                                                    <Body1 block className="line-clamp-4! wrap-break-word">{info.infoText}</Body1>
                                                    <Caption1>Добавлено: {info.createdBy || NO_DATA} · {formatDateTime(info.createdAt)}</Caption1>
                                                </div>
                                                <div className="flex gap-1 shrink-0">
                                                    <Dialog>
                                                        <DialogTrigger disableButtonEnhancement>
                                                            <Tooltip content="Просмотреть заметку" relationship="label">
                                                                <Button
                                                                    appearance="subtle"
                                                                    size="small"
                                                                    icon={<Eye20Regular />}
                                                                    aria-label="Просмотреть заметку"
                                                                />
                                                            </Tooltip>
                                                        </DialogTrigger>
                                                        <DialogSurface>
                                                            <DialogBody>
                                                                <DialogTitle>Просмотр заметки</DialogTitle>
                                                                <DialogContent>
                                                                    <div className="whitespace-pre-wrap break-words py-2">
                                                                        <Body1>{info.infoText}</Body1>
                                                                    </div>
                                                                    <div className="mt-4 pt-2 border-t border-neutral-200">
                                                                        <Caption1 block style={{ color: "var(--colorNeutralForeground3)" }}>
                                                                            Автор: {info.createdBy || NO_DATA}
                                                                        </Caption1>
                                                                        <Caption1 block style={{ color: "var(--colorNeutralForeground3)" }}>
                                                                            Дата: {formatDateTime(info.createdAt)}
                                                                        </Caption1>
                                                                    </div>
                                                                </DialogContent>
                                                                <DialogActions>
                                                                    <DialogTrigger disableButtonEnhancement>
                                                                        <Button appearance="secondary">Закрыть</Button>
                                                                    </DialogTrigger>
                                                                </DialogActions>
                                                            </DialogBody>
                                                        </DialogSurface>
                                                    </Dialog>

                                                    <HasPermission can={Permissions.UserProfileAdditionalInfoDelete}>
                                                        <Tooltip content="Удалить заметку" relationship="label">
                                                            <Button
                                                                appearance="subtle"
                                                                size="small"
                                                                icon={<Delete20Regular />}
                                                                onClick={() => handleDeleteNote(info.infoId)}
                                                                aria-label="Удалить заметку"
                                                            />
                                                        </Tooltip>
                                                    </HasPermission>
                                                </div>
                                            </Card>
                                        ))}
                                    </div>

                                    {totalNotes > 0 && (
                                        <div className="mt-auto pt-2 flex justify-end border-t border-neutral-100">
                                            <Pagination
                                                currentPage={notesPage}
                                                totalPages={Math.ceil(totalNotes / NOTES_PAGE_SIZE)}
                                                onPageChange={setNotesPage}
                                                size="small"
                                            />
                                        </div>
                                    )}
                                </div>
                            </Card>
                        </HasPermission>
                    </div>

                    <HasPermission can={Permissions.BillingTransactionRead}>
                        <div className="flex flex-col gap-4">
                            <div className="flex items-center justify-between flex-wrap gap-2">
                                <Title3>Транзакции</Title3>
                                <Body2>{txTotalCount} записей</Body2>
                            </div>
                            <DismissableError error={txErrorMessage} />
                            <Card className="overflow-x-auto min-h-[320px]" size={sizes.card}>
                                <DataTable
                                    items={transactions}
                                    columns={txColumns}
                                    getRowId={(tx) => tx.transactionId}
                                    loading={txLoading}
                                    columnSizingOptions={txColumnSizingOptions}
                                />
                            </Card>
                            <div className="flex items-center justify-between flex-wrap gap-2">
                                <Body1>Показано {transactions.length} из {txTotalCount}</Body1>
                                <Pagination currentPage={txPage} totalPages={txTotalPages} onPageChange={setTxPage} />
                            </div>
                        </div>
                    </HasPermission>

                    <HasPermission can={Permissions.VenueVisitRead}>
                        <div className="flex flex-col gap-4">
                            <div className="flex items-center justify-between flex-wrap gap-2">
                                <Title3>История визитов</Title3>
                                <Body2>{visits.length} визитов</Body2>
                            </div>
                            <Card className="overflow-x-auto min-h-[320px]" size={sizes.card}>
                                <DataTable
                                    items={paginatedVisits}
                                    columns={visitColumns}
                                    getRowId={(v) => v.visitId}
                                    loading={visitsLoading}
                                    columnSizingOptions={visitColumnSizingOptions}
                                />
                            </Card>
                            {visits.length > 0 && (
                                <div className="flex items-center justify-between flex-wrap gap-2">
                                    <Body1>Показано {paginatedVisits.length} из {visits.length}</Body1>
                                    <Pagination currentPage={visitPage} totalPages={visitTotalPages} onPageChange={setVisitPage} />
                                </div>
                            )}
                        </div>
                    </HasPermission>

                    <HasPermission can={Permissions.AuditLogAdminRead}>
                        <div className="flex flex-col gap-4">
                            <div className="flex items-center justify-between flex-wrap gap-2">
                                <Title3>История действий (Аудит)</Title3>
                                <Body2>{auditTotalCount} записей</Body2>
                            </div>
                            <DismissableError error={auditErrorMessage} />
                            <Card size={sizes.card} className="relative min-h-[320px]">
                                {auditLoading && <Spinner size={sizes.spinner} />}
                                {!auditLoading && auditLogs.length === 0 && (
                                    <EmptyState
                                        title={NO_DATA}
                                        description="Нет записей аудита для этого пользователя"
                                    />
                                )}
                                {!auditLoading && auditLogs.length > 0 && (
                                    <div className="flex flex-col gap-6">
                                        {auditLogs.map((log, index) => (
                                            <div key={log.id} className="flex gap-4">
                                                <div className="flex flex-col items-center shrink-0">
                                                    <div className="w-3 h-3 rounded-full bg-(--colorCompoundBrandStroke) border-2 border-(--colorNeutralBackground1)" />
                                                    {index < auditLogs.length - 1 && (
                                                        <div className="w-0.5 grow bg-(--colorNeutralStroke1)" />
                                                    )}
                                                </div>

                                                <div className="flex flex-col gap-2 grow">
                                                    <div className="flex justify-between items-start gap-4 flex-wrap">
                                                        <div className="flex flex-col">
                                                            <div className="flex items-center gap-2 flex-wrap">
                                                                <Body1Strong>{log.action}</Body1Strong>
                                                                <Badge size={sizes.badge} appearance="tint" color={log.eventType === "Exception" ? "danger" : "brand"}>
                                                                    {log.eventType}
                                                                </Badge>
                                                            </div>
                                                            <Caption1 className="text-(--colorNeutralForeground3)">
                                                                {formatDateTime(log.createdAt)} {log.duration > 0 && `· ${formatDurationMs(log.duration)}`}
                                                            </Caption1>
                                                        </div>
                                                        <Button
                                                            appearance="subtle"
                                                            size={sizes.button}
                                                            icon={<Eye20Regular />}
                                                            onClick={() => handleOpenDetails(log)}
                                                        >
                                                            Детали
                                                        </Button>
                                                    </div>

                                                    <div className="flex gap-4 text-xs text-(--colorNeutralForeground4) flex-wrap">
                                                        {log.machineName && <span>Хост: {log.machineName}</span>}
                                                        {log.domainName && <span>Домен: {log.domainName}</span>}
                                                        {log.correlationId && <span>Correlation: {log.correlationId.slice(0, 8)}...</span>}
                                                    </div>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </Card>
                            {auditTotalCount > 0 && (
                                <div className="flex items-center justify-between flex-wrap gap-2">
                                    <Body1>Показано {auditLogs.length} из {auditTotalCount}</Body1>
                                    <Pagination currentPage={auditPage} totalPages={auditTotalPages} onPageChange={setAuditPage} />
                                </div>
                            )}
                        </div>
                    </HasPermission>
                </>
            )}

            <Dialog open={auditDetailsOpen} onOpenChange={(_, data) => setAuditDetailsOpen(data.open)}>
                <DialogSurface className="max-w-[800px] w-full">
                    <DialogBody>
                        <DialogTitle>Детали записи аудита</DialogTitle>
                        <DialogContent>
                            {selectedAuditLog && (
                                <AuditLogDetails log={selectedAuditLog} />
                            )}
                        </DialogContent>
                        <DialogActions>
                            <Button size={sizes.button} appearance="secondary" onClick={() => setAuditDetailsOpen(false)}>
                                Закрыть
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>
        </div>
    );
};
