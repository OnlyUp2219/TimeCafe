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
    Field,
    MessageBar,
    MessageBarBody,
    Spinner,
    TableCellLayout,
    Textarea,
    Title2,
    Title3,
} from "@fluentui/react-components";
import { SecureAvatar } from "@components/SecureAvatar/SecureAvatar";
import { ArrowLeft20Regular, Delete20Regular, PeopleSettings20Regular, Eye20Regular } from "@fluentui/react-icons";
import { useGetUserByIdQuery } from "@store/api/adminApi";
import { useGetBalanceQuery, useGetTransactionHistoryQuery } from "@store/api/billingApi";
import { useGetVisitHistoryQuery, useGetUserLoyaltyQuery } from "@store/api/venueApi";
import {
    useCreateAdditionalInfoMutation,
    useDeleteAdditionalInfoMutation,
    useGetAdditionalInfosByUserIdQuery,
    useGetProfileByUserIdReadOnlyQuery
} from "@store/api/profileApi";
import { useAppSelector } from "@store/hooks";
import { Gender, ProfileStatus } from "@app-types/profile";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { EmptyState } from "@components/EmptyState/EmptyState";
import { useComponentSize } from "@hooks/useComponentSize";
import type { BillingTransaction } from "@app-types/billing";
import { TransactionSource, TransactionType } from "@app-types/billing";
import type { VisitWithTariff } from "@app-types/visitWithTariff";
import { VisitStatus } from "@app-types/visit";
import { Permissions } from "@shared/auth/permissions";
import { HasPermission } from "@components/Guard/HasPermission";
import { formatMoney } from "@shared/const/FormatMoney.ts";
import { txTypeLabel } from "@shared/const/TxTypeLabel.ts";

import { LoyaltyProgress } from "@components/Loyalty/LoyaltyProgress";

import { NO_DATA, NO_ACCESS } from "@shared/const/placeholders";

const formatDateTime = (iso: string | null) => {
    if (!iso) return NO_DATA;
    return new Date(iso).toLocaleString("ru-RU", { day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit" });
};

const txTypeColor = (t: number): "success" | "danger" | "informative" => {
    switch (t) {
        case TransactionType.Deposit: return "success";
        case TransactionType.Withdrawal: return "danger";
        default: return "informative";
    }
};

const txSourceLabel = (s: number) => {
    switch (s) {
        case TransactionSource.Visit: return "Визит";
        case TransactionSource.Manual: return "Вручную";
        case TransactionSource.Payment: return "Платёж";
        case TransactionSource.Refund: return "Возврат";
        default: return NO_DATA;
    }
};

const visitStatusLabel = (s: number) => s === VisitStatus.Active ? "Активен" : "Завершён";
const visitStatusColor = (s: number): "success" | "informative" => s === VisitStatus.Active ? "success" : "informative";

const genderLabel = (gender?: number) => {
    switch (gender) {
        case Gender.Male:
            return "Мужской";
        case Gender.Female:
            return "Женский";
        default:
            return "Не указан";
    }
};

const profileStatusLabel = (status?: number) => {
    switch (status) {
        case ProfileStatus.Pending:
            return "Ожидает заполнения";
        case ProfileStatus.Completed:
            return "Заполнен";
        case ProfileStatus.Banned:
            return "Заблокирован";
        default:
            return "Неизвестно";
    }
};

const profileStatusColor = (status?: number): "warning" | "success" | "danger" => {
    switch (status) {
        case ProfileStatus.Pending:
            return "warning";
        case ProfileStatus.Completed:
            return "success";
        case ProfileStatus.Banned:
            return "danger";
        default:
            return "warning";
    }
};

import { usePagination } from "@hooks/usePagination";

export const UserDetailPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { sizes } = useComponentSize();

    const { page: txPage, size: txPageSize, setPage: setTxPage } = usePagination("adminUserDetailTx", 1, 10);
    const { page: notesPage, size: NOTES_PAGE_SIZE, setPage: setNotesPage } = usePagination("adminUserDetailNotes", 1, 3);

    const { data: userData, isLoading: userLoading, error: userError } = useGetUserByIdQuery(id!, { skip: !id, refetchOnMountOrArgChange: true });
    const user = userData?.user;

    const { data: balance, isLoading: balanceLoading } = useGetBalanceQuery(id!, { skip: !id });
    const { data: txData, isLoading: txLoading, error: txError } = useGetTransactionHistoryQuery(
        { userId: id!, page: txPage, pageSize: txPageSize },
        { skip: !id }
    );
    const { data: visits = [], isLoading: visitsLoading } = useGetVisitHistoryQuery({ userId: id! }, { skip: !id });
    const { data: loyalty, isLoading: loyaltyLoading } = useGetUserLoyaltyQuery(id!, { skip: !id });
    const { data: profile } = useGetProfileByUserIdReadOnlyQuery(id!, { skip: !id });
    const { data: notesData, isLoading: notesLoading } = useGetAdditionalInfosByUserIdQuery(
        { userId: id!, pageNumber: notesPage, pageSize: NOTES_PAGE_SIZE },
        { skip: !id }
    );
    const adminNotes = notesData?.items ?? [];
    const totalNotes = notesData?.totalCount ?? 0;
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

    const transactions = txData?.transactions ?? [];
    const txTotalPages = txData?.pagination.totalPages ?? 1;
    const txTotalCount = txData?.pagination.totalCount ?? 0;

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

    const contactLine = [user?.email, profile?.phoneNumber].filter(Boolean).join(" · ") || NO_DATA;
    const registrationDate = profile?.birthDate ? formatDateTime(profile.birthDate) : NO_DATA;

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
                    <span className={tx.type === TransactionType.Withdrawal ? "text-red-500" : "text-green-600"}>
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
                    <Badge appearance="filled" color={visitStatusColor(v.status)}>{visitStatusLabel(v.status)}</Badge>
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
            columnId: "cost",
            compare: (a, b) => (a.calculatedCost ?? 0) - (b.calculatedCost ?? 0),
            renderHeaderCell: () => "Стоимость",
            renderCell: (v) => <TableCellLayout truncate>{v.calculatedCost != null ? formatMoney(v.calculatedCost) : NO_DATA}</TableCellLayout>,
        }),
    ], []);

    if (userLoading) {
        return <div ><Spinner /></div>;
    }

    if (errorMessage) return (
        <MessageBar intent="error">
            <MessageBarBody>{errorMessage}</MessageBarBody>
        </MessageBar>
    );

    if (!user) return (
        <div>
            <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/users")}>
                Назад
            </Button>
            <MessageBar intent="warning" className="mt-4">
                <MessageBarBody>Пользователь не найден</MessageBarBody>
            </MessageBar>
        </div>
    );

    return (
        <div className="flex flex-col gap-4">
            <div className="flex items-center gap-2 flex-wrap">
                <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/users")}>
                    Назад к списку
                </Button>
                <HasPermission anyOf={[Permissions.RbacUserRoleAssign, Permissions.RbacUserRoleRemove]}>
                    <Button appearance="outline" icon={<PeopleSettings20Regular />} onClick={() => navigate(`/admin/users/${id}/roles`)}>
                        Управление ролями
                    </Button>
                </HasPermission>
            </div>

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
                            <Body1 block>{contactLine}</Body1>
                            <Body2 block className="text-gray-500 ">{user.role} · {user.status}</Body2>
                            <div className="flex gap-2 flex-wrap">
                                {nickname && <Badge appearance="outline">{nickname}</Badge>}
                                {user && <Badge appearance="tint" color={user.emailConfirmed ? "success" : "warning"}>{user.emailConfirmed ? "Email подтверждён" : "Email не подтверждён"}</Badge>}
                                {user && <Badge appearance="tint" color={user.phoneNumberConfirmed ? "success" : "warning"}>{user.phoneNumberConfirmed ? "Телефон подтверждён" : "Телефон не подтверждён"}</Badge>}
                                {profile && <Badge appearance="tint" color={profileStatusColor(profile.profileStatus)}>{profileStatusLabel(profile.profileStatus)}</Badge>}
                            </div>
                            <Caption1 block className="text-gray-400">{user.id}</Caption1>
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
                <Card size={sizes.card} className="flex-1 min-w-[200px]">
                    <Body2 block>Баланс</Body2>
                    <HasPermission can={Permissions.BillingBalanceRead} fallback={<Title3>{NO_ACCESS}</Title3>}>
                        <Title3 className={!balanceLoading && (balance?.currentBalance ?? 0) < 0 ? "text-red-600" : "text-green-600"}>
                            {balanceLoading ? NO_DATA : formatMoney(balance?.currentBalance ?? 0)}
                        </Title3>
                    </HasPermission>
                </Card>
                <Card size={sizes.card} className="flex-1 min-w-[200px]">
                    <Body2 block>Визитов всего</Body2>
                    <HasPermission can={Permissions.VenueVisitRead} fallback={<Title3>{NO_ACCESS}</Title3>}>
                        <Title3>{visitsLoading ? NO_DATA : visits.length}</Title3>
                    </HasPermission>
                </Card>
                <Card size={sizes.card} className="flex-1 min-w-[200px]">
                    <Body2 block>Потрачено</Body2>
                    <HasPermission can={Permissions.BillingBalanceRead} fallback={<Title3>{NO_ACCESS}</Title3>}>
                        <Title3>{balanceLoading ? NO_DATA : formatMoney(balance?.totalSpent ?? 0)}</Title3>
                    </HasPermission>
                </Card>
                <Card size={sizes.card} className="flex-1 min-w-[300px]">
                    <Body1Strong block className="mb-2">Программа лояльности</Body1Strong>
                    <LoyaltyProgress
                        visitCount={profileData?.visitCount || 0}
                        currentDiscount={loyalty?.personalDiscountPercent || 0}
                    />
                </Card>
            </div>

            <div className="flex flex-wrap gap-4">
                <HasPermission
                    can={Permissions.UserProfileProfileRead}
                    fallback={
                        <Card size={sizes.card}>
                            <Title3 className="mb-3">Профиль</Title3>
                            <MessageBar intent="warning">
                                <MessageBarBody>Недостаточно прав для просмотра детального профиля</MessageBarBody>
                            </MessageBar>
                        </Card>
                    }
                >
                    <Card className="flex-[1.3] flex-grow basis-[400px]" size={sizes.card}>
                        <Title3 >Профиль</Title3>
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                            <div>
                                <Body2 block>Имя</Body2>
                                <Body1 block>{profile?.firstName || NO_DATA}</Body1>
                            </div>
                            <div>
                                <Body2 block>Фамилия</Body2>
                                <Body1 block>{profile?.lastName || NO_DATA}</Body1>
                            </div>
                            <div>
                                <Body2 block>Отчество</Body2>
                                <Body1 block>{profile?.middleName || NO_DATA}</Body1>
                            </div>
                            <div>
                                <Body2 block>Пол</Body2>
                                <Body1 block>{genderLabel(profile?.gender)}</Body1>
                            </div>
                            <div>
                                <Body2 block>Дата рождения</Body2>
                                <Body1 block>{profile?.birthDate ? new Date(profile.birthDate).toLocaleDateString("ru-RU") : NO_DATA}</Body1>
                            </div>
                            <div>
                                <Body2 block>Регистрация</Body2>
                                <Body1 block>{registrationDate}</Body1>
                            </div>
                            <div>
                                <Body2 block>Email</Body2>
                                <Body1 block>{profile?.email || user.email}</Body1>
                            </div>
                            <div>
                                <Body2 block>Телефон</Body2>
                                <Body1 block>{profile?.phoneNumber || NO_DATA}</Body1>
                            </div>
                            <div className="sm:col-span-2">
                                <Body2 block>Статус профиля</Body2>
                                <Badge appearance="tint" color={profileStatusColor(profile?.profileStatus)}>{profileStatusLabel(profile?.profileStatus)}</Badge>
                            </div>
                            {profile?.banReason && (
                                <div className="sm:col-span-2">
                                    <Body2 block>Причина блокировки</Body2>
                                    <Body1 block className="text-red-500">{profile.banReason}</Body1>
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
                                                <Field
                                                    label="Текст заметки"
                                                    validationMessage={noteError}
                                                    validationState={noteError ? "error" : "none"}
                                                    hint={{
                                                        children: (
                                                            <div className="flex justify-end">
                                                                <Caption1 style={{ color: newNoteText.length >= 2000 ? "var(--colorPaletteRedForeground1)" : "var(--colorNeutralForeground3)" }}>
                                                                    {newNoteText.length}/2000
                                                                </Caption1>
                                                            </div>
                                                        )
                                                    }}
                                                >
                                                    <Textarea
                                                        value={newNoteText}
                                                        onChange={(_e, data) => {
                                                            if (data.value.length <= 2000) {
                                                                setNewNoteText(data.value);
                                                                if (noteError) setNoteError(null);
                                                            }
                                                        }}
                                                        placeholder="Введите текст заметки..."
                                                        rows={6}
                                                        style={{ width: "100%" }}
                                                    />
                                                </Field>
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
                                        title="Нет данных"
                                        description="Добавьте первую заметку для этого пользователя"
                                    />
                                </div>
                            )}
                            <div className="flex-1">
                                {adminNotes.map((info) => (
                                    <Card appearance="filled-alternative" key={info.infoId} className="!flex !flex-row !flex-wrap mb-3 last:mb-0">
                                        <div className="flex-1">
                                            <Body1 block className="!line-clamp-4 break-words">{info.infoText}</Body1>
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
                <div className="mb-6">
                    <div className="flex items-center justify-between mb-3 flex-wrap gap-2">
                        <Title3>Транзакции</Title3>
                        <Body2>{txTotalCount} записей</Body2>
                    </div>
                    {txErrorMessage && (
                        <MessageBar intent="error" className="mb-3">
                            <MessageBarBody>{txErrorMessage}</MessageBarBody>
                        </MessageBar>
                    )}
                    <Card className="overflow-x-auto" size={sizes.card}>
                        <DataTable
                            items={transactions}
                            columns={txColumns}
                            getRowId={(tx) => tx.transactionId}
                            loading={txLoading}
                            columnSizingOptions={txColumnSizingOptions}
                        />
                    </Card>
                    <div className="flex items-center justify-between mt-3 flex-wrap gap-2">
                        <Body1>Показано {transactions.length} из {txTotalCount}</Body1>
                        <Pagination currentPage={txPage} totalPages={txTotalPages} onPageChange={setTxPage} />
                    </div>
                </div>
            </HasPermission>

            <HasPermission can={Permissions.VenueVisitRead}>
                <div>
                    <div className="flex items-center justify-between mb-3 flex-wrap gap-2">
                        <Title3>История визитов</Title3>
                        <Body2>{visits.length} визитов</Body2>
                    </div>
                    <Card className="overflow-x-auto" size={sizes.card}>
                        <DataTable
                            items={visits}
                            columns={visitColumns}
                            getRowId={(v) => v.visitId}
                            loading={visitsLoading}
                            columnSizingOptions={visitColumnSizingOptions}
                        />
                    </Card>
                </div>
            </HasPermission>
        </div>
    );
};
