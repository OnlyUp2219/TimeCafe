import {useParams, useNavigate} from "react-router-dom";
import {useCallback, useMemo, useState} from "react";
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
    createTableColumn,
    TableCellLayout,
    Textarea,
    Dialog,
    DialogTrigger,
    DialogSurface,
    DialogBody,
    DialogTitle,
    DialogContent,
    DialogActions,
    Field,
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {ArrowLeft20Regular, PeopleSettings20Regular, Delete20Regular} from "@fluentui/react-icons";
import {useGetUserByIdQuery} from "@store/api/adminApi";
import {useGetTransactionHistoryQuery, useGetBalanceQuery} from "@store/api/billingApi";
import {useGetVisitHistoryQuery} from "@store/api/venueApi";
import {useGetProfileByUserIdQuery, useGetAdditionalInfosByUserIdQuery, useCreateAdditionalInfoMutation, useDeleteAdditionalInfoMutation} from "@store/api/profileApi";
import {useAppSelector} from "@store/hooks";
import {ProfileStatus, Gender} from "@app-types/profile";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {DataTable} from "@components/DataTable/DataTable";
import {Pagination} from "@components/Pagination/Pagination";
import {useComponentSize} from "@hooks/useComponentSize";
import type {BillingTransaction} from "@app-types/billing";
import {TransactionType, TransactionSource} from "@app-types/billing";
import type {VisitWithTariff} from "@app-types/visitWithTariff";
import {VisitStatus} from "@app-types/visit";

const formatDateTime = (iso: string | null) => {
    if (!iso) return "—";
    return new Date(iso).toLocaleString("ru-RU", {day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit"});
};

const formatMoney = (v: number) => `${v.toFixed(2)} ₽`;

const txTypeLabel = (t: number) => {
    switch (t) {
        case TransactionType.Deposit: return "Пополнение";
        case TransactionType.Withdrawal: return "Списание";
        case TransactionType.Adjustment: return "Корректировка";
        default: return "—";
    }
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
        default: return "—";
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

export const UserDetailPage = () => {
    const {id} = useParams<{id: string}>();
    const navigate = useNavigate();
    const {sizes} = useComponentSize();

    const [txPage, setTxPage] = useState(1);
    const txPageSize = 10;

    const {data: userData, isLoading: userLoading, error: userError} = useGetUserByIdQuery(id!, {skip: !id});
    const user = userData?.user;

    const {data: balance, isLoading: balanceLoading} = useGetBalanceQuery(id!, {skip: !id});
    const {data: txData, isLoading: txLoading, error: txError} = useGetTransactionHistoryQuery(
        {userId: id!, page: txPage, pageSize: txPageSize},
        {skip: !id}
    );
    const {data: visits = [], isLoading: visitsLoading} = useGetVisitHistoryQuery(id!, {skip: !id});
    const {data: profile} = useGetProfileByUserIdQuery(id!, {skip: !id});
    const {data: adminNotes = [], isLoading: notesLoading} = useGetAdditionalInfosByUserIdQuery(id!, {skip: !id});
    const [createNote, {isLoading: creatingNote}] = useCreateAdditionalInfoMutation();
    const [deleteNote] = useDeleteAdditionalInfoMutation();
    const adminEmail = useAppSelector((state) => state.auth.email);

    const [addNoteOpen, setAddNoteOpen] = useState(false);
    const [newNoteText, setNewNoteText] = useState("");

    const handleAddNote = useCallback(async () => {
        if (!id || !newNoteText.trim()) return;
        try {
            await createNote({userId: id, infoText: newNoteText.trim(), createdBy: adminEmail || "admin"}).unwrap();
            setNewNoteText("");
            setAddNoteOpen(false);
        } catch { /* error handled by RTK */ }
    }, [id, newNoteText, createNote, adminEmail]);

    const handleDeleteNote = useCallback(async (infoId: string) => {
        if (!id) return;
        try {
            await deleteNote({infoId, userId: id}).unwrap();
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
        const emailLogin = user?.email?.split("@")[0]?.trim();
        if (emailLogin) return `@${emailLogin}`;
        return user?.name?.trim() ? `@${user.name.trim().toLowerCase().replace(/\s+/g, "_")}` : "—";
    }, [user?.email, user?.name]);

    const contactLine = [user?.email, profile?.phoneNumber, nickname].filter(Boolean).join(" · ") || "—";
    const registrationDate = profile?.birthDate ? formatDateTime(profile.birthDate) : "—";

    const txColumnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        date: {minWidth: 130, defaultWidth: 160},
        type: {minWidth: 100, defaultWidth: 140},
        source: {minWidth: 100, defaultWidth: 130},
        amount: {minWidth: 80, defaultWidth: 120},
        balance: {minWidth: 80, defaultWidth: 120},
        comment: {minWidth: 100, defaultWidth: 200},
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
            renderCell: (tx) => <TableCellLayout truncate>{tx.comment || "—"}</TableCellLayout>,
        }),
    ], []);

    const visitColumnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        tariff: {minWidth: 120, defaultWidth: 180},
        status: {minWidth: 80, defaultWidth: 120},
        entry: {minWidth: 130, defaultWidth: 160},
        exit: {minWidth: 130, defaultWidth: 160},
        cost: {minWidth: 80, defaultWidth: 110},
    }), []);

    const visitColumns: TableColumnDefinition<VisitWithTariff>[] = useMemo(() => [
        createTableColumn<VisitWithTariff>({
            columnId: "tariff",
            compare: (a, b) => a.tariffName.localeCompare(b.tariffName),
            renderHeaderCell: () => "Тариф",
            renderCell: (v) => <TableCellLayout truncate><Body1>{v.tariffName || "—"}</Body1></TableCellLayout>,
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
            renderCell: (v) => <TableCellLayout truncate>{v.calculatedCost != null ? formatMoney(v.calculatedCost) : "—"}</TableCellLayout>,
        }),
    ], []);

    if (userLoading) return <div className="flex justify-center p-8"><Spinner /></div>;

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
        <div>
            <div className="flex items-center gap-2 mb-4 flex-wrap">
                <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/users")}>
                    Назад к списку
                </Button>
                <Button appearance="outline" size="small" icon={<PeopleSettings20Regular />} onClick={() => navigate(`/admin/users/${id}/roles`)}>
                    Управление ролями
                </Button>
            </div>

            <Card className="mb-6" size={sizes.card}>
                <div className="flex items-start gap-4 flex-wrap justify-between">
                    <div className="flex items-start gap-4 flex-wrap min-w-0 flex-1">
                        <Avatar name={displayName} size={72} />
                        <div className="min-w-0 flex-1">
                            <Title2>{displayName}</Title2>
                            <Body1 block>{contactLine}</Body1>
                            <Body2 block className="mt-1 text-gray-500">{user.role} · {user.status}</Body2>
                            <div className="flex gap-2 mt-2 flex-wrap">
                                <Badge appearance="outline">{nickname}</Badge>
                                {profile && <Badge appearance="tint" color={profile.emailConfirmed ? "success" : "warning"}>{profile.emailConfirmed ? "Email подтверждён" : "Email не подтверждён"}</Badge>}
                                {profile?.phoneNumber && <Badge appearance="tint" color={profile.phoneNumberConfirmed ? "success" : "warning"}>{profile.phoneNumberConfirmed ? "Телефон подтверждён" : "Телефон не подтверждён"}</Badge>}
                                {profile && <Badge appearance="tint" color={profileStatusColor(profile.profileStatus)}>{profileStatusLabel(profile.profileStatus)}</Badge>}
                            </div>
                            <Caption1 block className="mt-1 font-mono text-gray-400">{user.id}</Caption1>
                        </div>
                    </div>
                    <div className="flex gap-2 flex-wrap">
                        <Button appearance="outline" onClick={() => navigate(`/admin/users/${id}/roles`)}>
                            Роли
                        </Button>
                        <Button appearance="outline" onClick={() => navigate(`/admin/users/${id}`)}>
                            Редактировать
                        </Button>
                    </div>
                </div>
            </Card>

            <div className="grid gap-4 md:grid-cols-3 mb-6">
                <Card size={sizes.card}>
                    <Body2 block>Баланс</Body2>
                    <Title3>{balanceLoading ? "—" : formatMoney(balance?.currentBalance ?? 0)}</Title3>
                </Card>
                <Card size={sizes.card}>
                    <Body2 block>Визитов всего</Body2>
                    <Title3>{visitsLoading ? "—" : visits.length}</Title3>
                </Card>
                <Card size={sizes.card}>
                    <Body2 block>Потрачено</Body2>
                    <Title3>{balanceLoading ? "—" : formatMoney(balance?.totalSpent ?? 0)}</Title3>
                </Card>
            </div>

            <div className="grid gap-4 lg:grid-cols-[1.3fr_0.9fr] mb-6">
                <Card size={sizes.card}>
                    <Title3 className="mb-3">Профиль</Title3>
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                        <div>
                            <Body2 block>Имя</Body2>
                            <Body1 block>{profile?.firstName || "—"}</Body1>
                        </div>
                        <div>
                            <Body2 block>Фамилия</Body2>
                            <Body1 block>{profile?.lastName || "—"}</Body1>
                        </div>
                        <div>
                            <Body2 block>Отчество</Body2>
                            <Body1 block>{profile?.middleName || "—"}</Body1>
                        </div>
                        <div>
                            <Body2 block>Пол</Body2>
                            <Body1 block>{genderLabel(profile?.gender)}</Body1>
                        </div>
                        <div>
                            <Body2 block>Дата рождения</Body2>
                            <Body1 block>{profile?.birthDate ? new Date(profile.birthDate).toLocaleDateString("ru-RU") : "—"}</Body1>
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
                            <Body1 block>{profile?.phoneNumber || "—"}</Body1>
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

                <Card size={sizes.card}>
                    <div className="flex items-center justify-between gap-3 mb-3">
                        <Title3>Заметки администратора</Title3>
                        <Dialog open={addNoteOpen} onOpenChange={(_e, data) => setAddNoteOpen(data.open)}>
                            <DialogTrigger disableButtonEnhancement>
                                <Button appearance="outline" size="small">+ Добавить</Button>
                            </DialogTrigger>
                            <DialogSurface>
                                <DialogBody>
                                    <DialogTitle>Новая заметка</DialogTitle>
                                    <DialogContent>
                                        <Field label="Текст заметки">
                                            <Textarea
                                                value={newNoteText}
                                                onChange={(_e, data) => setNewNoteText(data.value)}
                                                placeholder="Введите заметку..."
                                                rows={4}
                                                style={{width: "100%"}}
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
                    </div>
                    <div className="flex flex-col gap-3">
                        {notesLoading && <Spinner size="small" />}
                        {!notesLoading && adminNotes.length === 0 && (
                            <Body2 style={{color: "var(--colorNeutralForeground3)"}}>Нет заметок</Body2>
                        )}
                        {adminNotes.map((info) => (
                            <div key={info.infoId} className="rounded-md bg-neutral-100 p-3 flex justify-between items-start gap-2">
                                <div className="min-w-0 flex-1">
                                    <Body1 block className="mb-1">{info.infoText}</Body1>
                                    <Caption1>Добавлено: {info.createdBy || "—"} · {formatDateTime(info.createdAt)}</Caption1>
                                </div>
                                <Button
                                    appearance="subtle"
                                    size="small"
                                    icon={<Delete20Regular />}
                                    onClick={() => handleDeleteNote(info.infoId)}
                                    aria-label="Удалить заметку"
                                />
                            </div>
                        ))}
                    </div>
                </Card>
            </div>

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
        </div>
    );
};
