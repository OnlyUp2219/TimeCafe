import {useMemo, useState} from "react";
import {
    Body1,
    Body2,
    Caption1,
    Card,
    MessageBar,
    MessageBarBody,
    Title2,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {useGetAdminBalancesQuery} from "@store/api/adminApi";
import type {AdminBalanceDto} from "@store/api/adminApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {DataTable} from "@components/DataTable/DataTable";
import {Pagination} from "@components/Pagination/Pagination";
import {useComponentSize} from "@hooks/useComponentSize";

const formatMoney = (v: number) => `${v.toFixed(2)} ₽`;
const formatDate = (iso: string) =>
    new Date(iso).toLocaleString("ru-RU", {day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit"});

export const BalancesPage = () => {
    const {sizes} = useComponentSize();
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(20);

    const {data, isLoading, error} = useGetAdminBalancesQuery(
        {page: currentPage, pageSize},
        {refetchOnMountOrArgChange: true}
    );

    const balances = data?.balances ?? [];
    const totalCount = data?.pagination.totalCount ?? 0;
    const totalPages = data?.pagination.totalPages ?? 1;
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        userId: {minWidth: 120, defaultWidth: 200},
        current: {minWidth: 80, defaultWidth: 130},
        deposited: {minWidth: 80, defaultWidth: 130},
        spent: {minWidth: 80, defaultWidth: 130},
        debt: {minWidth: 80, defaultWidth: 110},
        updated: {minWidth: 130, defaultWidth: 170},
    }), []);

    const columns: TableColumnDefinition<AdminBalanceDto>[] = useMemo(() => [
        createTableColumn<AdminBalanceDto>({
            columnId: "userId",
            compare: (a, b) => a.userId.localeCompare(b.userId),
            renderHeaderCell: () => "Пользователь",
            renderCell: (b) => (
                <TableCellLayout truncate>
                    <Caption1 className="font-mono">{b.userId.slice(0, 8)}…</Caption1>
                </TableCellLayout>
            ),
        }),
        createTableColumn<AdminBalanceDto>({
            columnId: "current",
            compare: (a, b) => a.currentBalance - b.currentBalance,
            renderHeaderCell: () => "Баланс",
            renderCell: (b) => (
                <TableCellLayout truncate>
                    <Body1 style={{color: b.currentBalance >= 0 ? "var(--colorPaletteGreenForeground1)" : "var(--colorPaletteRedForeground1)"}}>
                        {formatMoney(b.currentBalance)}
                    </Body1>
                </TableCellLayout>
            ),
        }),
        createTableColumn<AdminBalanceDto>({
            columnId: "deposited",
            compare: (a, b) => a.totalDeposited - b.totalDeposited,
            renderHeaderCell: () => "Пополнено",
            renderCell: (b) => <TableCellLayout truncate>{formatMoney(b.totalDeposited)}</TableCellLayout>,
        }),
        createTableColumn<AdminBalanceDto>({
            columnId: "spent",
            compare: (a, b) => a.totalSpent - b.totalSpent,
            renderHeaderCell: () => "Потрачено",
            renderCell: (b) => <TableCellLayout truncate>{formatMoney(b.totalSpent)}</TableCellLayout>,
        }),
        createTableColumn<AdminBalanceDto>({
            columnId: "debt",
            compare: (a, b) => a.debt - b.debt,
            renderHeaderCell: () => "Долг",
            renderCell: (b) => (
                <TableCellLayout truncate>
                    <Body1 style={{color: b.debt > 0 ? "var(--colorPaletteRedForeground1)" : undefined}}>
                        {b.debt > 0 ? formatMoney(b.debt) : "—"}
                    </Body1>
                </TableCellLayout>
            ),
        }),
        createTableColumn<AdminBalanceDto>({
            columnId: "updated",
            compare: (a, b) => new Date(a.lastUpdated).getTime() - new Date(b.lastUpdated).getTime(),
            renderHeaderCell: () => "Обновлён",
            renderCell: (b) => <TableCellLayout truncate>{formatDate(b.lastUpdated)}</TableCellLayout>,
        }),
    ], []);

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Балансы</Title2>
                    <Body2 block>{totalCount} балансов</Body2>
                </div>
            </div>

            {queryError && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{queryError}</MessageBarBody>
                </MessageBar>
            )}

            <Card className="overflow-x-auto" size={sizes.card}>
                <DataTable
                    items={balances}
                    columns={columns}
                    getRowId={(b) => b.userId}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <div className="flex items-center justify-between mt-4 flex-wrap gap-2">
                <Body1>Показано {balances.length} из {totalCount}</Body1>
                <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    onPageChange={setCurrentPage}
                    pageSize={pageSize}
                    onPageSizeChange={setPageSize}
                    totalCount={totalCount}
                />
            </div>
        </div>
    );
};
