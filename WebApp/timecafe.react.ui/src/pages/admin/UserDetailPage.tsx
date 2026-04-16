import {useParams, useNavigate} from "react-router-dom";
import {useState, useMemo} from "react";
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
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {ArrowLeft20Regular} from "@fluentui/react-icons";
import {useGetUserByIdQuery} from "@store/api/adminApi";
import {useGetTransactionHistoryQuery, useGetBalanceQuery} from "@store/api/billingApi";
import {useGetVisitHistoryQuery} from "@store/api/venueApi";
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

    const transactions = txData?.transactions ?? [];
    const txTotalPages = txData?.pagination.totalPages ?? 1;
    const txTotalCount = txData?.pagination.totalCount ?? 0;

    const errorMessage = userError ? getRtkErrorMessage(userError as FetchBaseQueryError) : null;
    const txErrorMessage = txError ? getRtkErrorMessage(txError as FetchBaseQueryError) : null;

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
            <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/users")} className="mb-4">
                Назад к списку
            </Button>

            <Card className="mb-6" size={sizes.card}>
                <div className="flex items-center gap-4 flex-wrap">
                    <Avatar name={user.name || user.email} size={64} />
                    <div className="min-w-0 flex-1">
                        <Title2>{user.name || "—"}</Title2>
                        <Body1 block>{user.email}</Body1>
                        <div className="flex gap-2 mt-1 flex-wrap">
                            <Badge appearance="outline">{user.role}</Badge>
                            <Badge appearance="tint" color={user.status.toLowerCase() === "active" ? "success" : "danger"}>
                                {user.status}
                            </Badge>
                        </div>
                        <Caption1 block className="mt-1 font-mono text-gray-400">{user.id}</Caption1>
                    </div>
                    {!balanceLoading && balance && (
                        <div className="flex gap-4 flex-wrap">
                            <div className="text-center">
                                <Body2 block>Баланс</Body2>
                                <Title3>{formatMoney(balance.currentBalance)}</Title3>
                            </div>
                            <div className="text-center">
                                <Body2 block>Пополнено</Body2>
                                <Body1 block className="text-green-600">{formatMoney(balance.totalDeposited)}</Body1>
                            </div>
                            <div className="text-center">
                                <Body2 block>Потрачено</Body2>
                                <Body1 block className="text-red-500">{formatMoney(balance.totalSpent)}</Body1>
                            </div>
                            {balance.debt > 0 && (
                                <div className="text-center">
                                    <Body2 block>Долг</Body2>
                                    <Body1 block className="text-red-600 font-bold">{formatMoney(balance.debt)}</Body1>
                                </div>
                            )}
                        </div>
                    )}
                </div>
            </Card>

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
