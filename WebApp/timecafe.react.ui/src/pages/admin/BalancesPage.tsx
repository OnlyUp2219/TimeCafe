import {useMemo, useState} from "react";
import {
    Avatar,
    Body1,
    Body2,
    Badge,
    Card,
    Caption1,
    MessageBar,
    MessageBarBody,
    Title2,
    Title3,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {useGetAdminBalancesQuery} from "@store/api/adminApi";
import type {AdminBalanceDto} from "@store/api/adminApi";
import {useGetProfileByUserIdReadOnlyQuery} from "@store/api/profileApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {DataTable} from "@components/DataTable/DataTable";
import {Pagination} from "@components/Pagination/Pagination";
import {CURRENCY_SYMBOL} from "@shared/const/currency";
import {NO_DATA, NO_ACCESS} from "@shared/const/placeholders";
import {useComponentSize} from "@hooks/useComponentSize";
import {usePermissions} from "@hooks/usePermissions";
import {HasPermission} from "@components/Guard/HasPermission";
import {Permissions} from "@shared/auth/permissions";

const formatMoney = (v: number) => `${v.toFixed(2)} ${CURRENCY_SYMBOL}`;
const formatDate = (iso: string) =>
    new Date(iso).toLocaleString("ru-RU", {day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit"});

const AdminUserCell = ({userId}: {userId: string}) => {
    const {data: profile} = useGetProfileByUserIdReadOnlyQuery(userId);
    const displayName = profile?.firstName || profile?.lastName
        ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim()
        : profile?.email || null;

    return (
        <TableCellLayout truncate media={<Avatar name={displayName || userId} size={28} />}>
            <div className="flex flex-col min-w-0">
                <Body1 block truncate>{displayName || userId}</Body1>
                <Caption1 block className="font-mono text-gray-400" style={{ fontSize: '10px' }}>{userId}</Caption1>
            </div>
        </TableCellLayout>
    );
};

const getBalanceType = (balance: AdminBalanceDto) => {
    if (balance.debt > 0) return "Долг";
    if (balance.currentBalance > 0) return "Положительный";
    if (balance.currentBalance < 0) return "Отрицательный";
    return "Нулевой";
};

const getBalanceTypeColor = (balance: AdminBalanceDto): "success" | "danger" | "warning" | "informative" => {
    if (balance.debt > 0) return "danger";
    if (balance.currentBalance > 0) return "success";
    if (balance.currentBalance < 0) return "warning";
    return "informative";
};

import { usePagination } from "@hooks/usePagination";

export const BalancesPage = () => {
    const {sizes} = useComponentSize();
    const {has} = usePermissions();
    const { page: currentPage, size: pageSize, setPage: setCurrentPage, setSize: setPageSize } = usePagination("adminBalances");

    const {data, isLoading, error} = useGetAdminBalancesQuery(
        {page: currentPage, pageSize},
        {refetchOnMountOrArgChange: true}
    );

    const balances = data?.balances ?? [];
    const totalCount = data?.pagination.totalCount ?? 0;
    const totalPages = data?.pagination.totalPages ?? 1;
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const totalBalance = useMemo(() => balances.reduce((s, b) => s + b.currentBalance, 0), [balances]);
    const totalDebt = useMemo(() => balances.reduce((s, b) => s + b.debt, 0), [balances]);
    const debtorsCount = useMemo(() => balances.filter(b => b.debt > 0).length, [balances]);

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        user: {minWidth: 180, defaultWidth: 260, idealWidth: 300},
        type: {minWidth: 110, defaultWidth: 150, idealWidth: 180},
        current: {minWidth: 100, defaultWidth: 130, idealWidth: 160},
        deposited: {minWidth: 100, defaultWidth: 130, idealWidth: 160},
        spent: {minWidth: 100, defaultWidth: 130, idealWidth: 160},
        debt: {minWidth: 100, defaultWidth: 120, idealWidth: 140},
        updated: {minWidth: 130, defaultWidth: 170, idealWidth: 200},
    }), []);

    const columns: TableColumnDefinition<AdminBalanceDto>[] = useMemo(() => {
        const allColumns: (TableColumnDefinition<AdminBalanceDto> & { permission?: string })[] = [
            {
                ...createTableColumn<AdminBalanceDto>({
                    columnId: "user",
                    compare: (a, b) => a.userId.localeCompare(b.userId),
                    renderHeaderCell: () => "Пользователь",
                    renderCell: (b) => <AdminUserCell userId={b.userId} />,
                }),
                permission: Permissions.UserProfileProfileRead
            },
            createTableColumn<AdminBalanceDto>({
                columnId: "type",
                compare: (a, b) => getBalanceType(a).localeCompare(getBalanceType(b)),
                renderHeaderCell: () => "Тип",
                renderCell: (b) => (
                    <TableCellLayout truncate>
                        <Badge appearance="tint" color={getBalanceTypeColor(b)}>{getBalanceType(b)}</Badge>
                    </TableCellLayout>
                ),
            }),
            {
                ...createTableColumn<AdminBalanceDto>({
                    columnId: "current",
                    compare: (a, b) => a.currentBalance - b.currentBalance,
                    renderHeaderCell: () => "Баланс",
                    renderCell: (b) => (
                        <TableCellLayout truncate>
                            <HasPermission can={Permissions.BillingBalanceRead} fallback={NO_ACCESS}>
                                <Body1 style={{ color: b.currentBalance >= 0 ? "var(--colorPaletteGreenForeground1)" : "var(--colorPaletteRedForeground1)" }}>
                                    {formatMoney(b.currentBalance)}
                                </Body1>
                            </HasPermission>
                        </TableCellLayout>
                    ),
                }),
                permission: Permissions.BillingBalanceRead
            },
            {
                ...createTableColumn<AdminBalanceDto>({
                    columnId: "deposited",
                    compare: (a, b) => a.totalDeposited - b.totalDeposited,
                    renderHeaderCell: () => "Пополнено",
                    renderCell: (b) => (
                        <TableCellLayout truncate>
                            <HasPermission can={Permissions.BillingBalanceRead} fallback={NO_ACCESS}>
                                {formatMoney(b.totalDeposited)}
                            </HasPermission>
                        </TableCellLayout>
                    ),
                }),
                permission: Permissions.BillingBalanceRead
            },
            {
                ...createTableColumn<AdminBalanceDto>({
                    columnId: "spent",
                    compare: (a, b) => a.totalSpent - b.totalSpent,
                    renderHeaderCell: () => "Потрачено",
                    renderCell: (b) => (
                        <TableCellLayout truncate>
                            <HasPermission can={Permissions.BillingBalanceRead} fallback={NO_ACCESS}>
                                {formatMoney(b.totalSpent)}
                            </HasPermission>
                        </TableCellLayout>
                    ),
                }),
                permission: Permissions.BillingBalanceRead
            },
            {
                ...createTableColumn<AdminBalanceDto>({
                    columnId: "debt",
                    compare: (a, b) => a.debt - b.debt,
                    renderHeaderCell: () => "Долг",
                    renderCell: (b) => (
                        <TableCellLayout truncate>
                            <HasPermission can={Permissions.BillingBalanceRead} fallback={NO_ACCESS}>
                                <Body1 style={{ color: b.debt > 0 ? "var(--colorPaletteRedForeground1)" : undefined }}>
                                    {b.debt > 0 ? formatMoney(b.debt) : NO_DATA}
                                </Body1>
                            </HasPermission>
                        </TableCellLayout>
                    ),
                }),
                permission: Permissions.BillingBalanceRead
            },
            createTableColumn<AdminBalanceDto>({
                columnId: "updated",
                compare: (a, b) => new Date(a.lastUpdated).getTime() - new Date(b.lastUpdated).getTime(),
                renderHeaderCell: () => "Обновлён",
                renderCell: (b) => <TableCellLayout truncate>{formatDate(b.lastUpdated)}</TableCellLayout>,
            }),
        ];

        return allColumns.filter(col => !col.permission || has(col.permission as any));
    }, [has]);

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Балансы</Title2>
                    <Body2 block>{totalCount} пользователей</Body2>
                </div>
            </div>

            <div className="grid gap-4 md:grid-cols-4 mb-4">
                <Card size={sizes.card}>
                    <Body2 block>Всего балансов</Body2>
                    <Title3>{totalCount}</Title3>
                </Card>
                <Card size={sizes.card}>
                    <Body2 block>Суммарный баланс (стр.)</Body2>
                    <HasPermission can={Permissions.BillingBalanceRead} fallback={<Title3>{NO_ACCESS}</Title3>}>
                        <Title3 style={{color: totalBalance >= 0 ? "var(--colorPaletteGreenForeground1)" : "var(--colorPaletteRedForeground1)"}}>
                            {formatMoney(totalBalance)}
                        </Title3>
                    </HasPermission>
                </Card>
                <Card size={sizes.card}>
                    <Body2 block>Суммарный долг (стр.)</Body2>
                    <HasPermission can={Permissions.BillingBalanceRead} fallback={<Title3>{NO_ACCESS}</Title3>}>
                        <Title3 style={{color: totalDebt > 0 ? "var(--colorPaletteRedForeground1)" : undefined}}>
                            {totalDebt > 0 ? formatMoney(totalDebt) : NO_DATA}
                        </Title3>
                    </HasPermission>
                </Card>
                <Card size={sizes.card}>
                    <Body2 block>Должников (стр.)</Body2>
                    <Title3>{debtorsCount}</Title3>
                </Card>
            </div>

            {queryError && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{queryError}</MessageBarBody>
                </MessageBar>
            )}

            <Card size={sizes.card}>
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
