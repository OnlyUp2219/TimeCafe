import { useCallback, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import {
    Avatar,
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    Field,
    Input,
    MessageBar,
    MessageBarBody,
    Spinner,
    Title2,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import { SecureAvatar } from "@components/SecureAvatar/SecureAvatar";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { Eye20Regular } from "@fluentui/react-icons";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions } from "@shared/auth/permissions";
import type { User } from "@app-types/user";
import { useGetUsersCompositeQuery } from "@store/api/adminApi";
import { formatMoneyByN } from "@utility/formatMoney";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import { NO_DATA, NO_ACCESS } from "@shared/const/placeholders";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useComponentSize } from "@hooks/useComponentSize";
import { usePermissions } from "@hooks/usePermissions";

const getUserStatusBadgeClass = (status: string): string => {
    switch (status.toLowerCase()) {
        case "active":
            return "dark-green";
        case "inactive":
            return "dark-red";
        default:
            return "beige";
    }
};

const ProfileNameCell = ({ user }: { user: User }) => {
    const profile = user.profile;
    const displayName = profile?.firstName || profile?.lastName
        ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim()
        : (user.name || NO_DATA);
    return (
        <HasPermission
            can={Permissions.UserProfileProfileRead}
            fallback={<TableCellLayout truncate media={<Avatar name={user.name || user.email} />}>{NO_ACCESS}</TableCellLayout>}
        >
            <TableCellLayout
                truncate
                media={
                    <SecureAvatar
                        name={displayName || user.email}
                        photoUrl={profile?.photoUrl}

                    />
                }
            >
                {displayName}
            </TableCellLayout>
        </HasPermission>
    );
};

const ProfilePhoneCell = ({ user }: { user: User }) => {
    return (
        <HasPermission can={Permissions.UserProfileProfileRead} fallback={<TableCellLayout truncate>{NO_ACCESS}</TableCellLayout>}>
            <TableCellLayout truncate>{user.phoneNumber || NO_DATA}</TableCellLayout>
        </HasPermission>
    );
};

const profileStatusLabel = (status?: number) => {
    switch (status) {
        case 0: return "Ожидает заполнения";
        case 1: return "Заполнен";
        case 2: return "Заблокирован";
        default: return "Неизвестно";
    }
};

const profileStatusColor = (status?: number): "warning" | "success" | "danger" => {
    switch (status) {
        case 0: return "warning";
        case 1: return "success";
        case 2: return "danger";
        default: return "warning";
    }
};

const ProfileStatusCell = ({ user }: { user: User }) => {
    const profile = user.profile;
    if (!profile) return <TableCellLayout truncate>{NO_DATA}</TableCellLayout>;
    return (
        <HasPermission can={Permissions.UserProfileProfileRead} fallback={<TableCellLayout truncate>{NO_ACCESS}</TableCellLayout>}>
            <TableCellLayout truncate>
                <Badge appearance="tint" color={profileStatusColor(profile.profileStatus)}>
                    {profileStatusLabel(profile.profileStatus)}
                </Badge>
            </TableCellLayout>
        </HasPermission>
    );
};

const BalanceCell = ({ user }: { user: User }) => {
    const balance = user.balance;
    if (balance === undefined || balance === null) return <TableCellLayout truncate>{NO_DATA}</TableCellLayout>;
    return (
        <HasPermission can={Permissions.BillingBalanceRead} fallback={<TableCellLayout truncate>{NO_ACCESS}</TableCellLayout>}>
            <TableCellLayout truncate>{formatMoneyByN(balance.currentBalance)}</TableCellLayout>
        </HasPermission>
    );
};

import { usePagination } from "@hooks/usePagination";

export const UsersListPage = () => {
    const navigate = useNavigate();
    const { sizes } = useComponentSize();
    const { has } = usePermissions();
    const { page: currentPage, size: pageSize, filters, setPage: setCurrentPage, setSize: setPageSize, setFilters } = usePagination("adminUsers");

    const search = filters.search || "";
    const statusFilter = filters.statusFilter || "";

    const setSearch = (s: string) => setFilters({ search: s });
    const setStatusFilter = (s: string) => setFilters({ statusFilter: s });

    const { data, isLoading, error } = useGetUsersCompositeQuery({
        page: currentPage,
        size: pageSize,
        search: search || undefined,
        status: statusFilter || undefined,
    });

    const users = data?.users ?? [];
    const totalPages = data?.pagination.totalPages ?? 1;
    const totalCount = data?.pagination.totalCount ?? 0;
    const errorMessage = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const handleClearFilters = useCallback(() => {
        setFilters({ search: "", statusFilter: "" });
        setCurrentPage(1);
    }, [setFilters, setCurrentPage]);

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        user: { minWidth: 150, defaultWidth: 220, idealWidth: 250 },
        email: { minWidth: 150, defaultWidth: 200, idealWidth: 250 },
        phone: { minWidth: 120, defaultWidth: 150, idealWidth: 180 },
        role: { minWidth: 80, defaultWidth: 120, idealWidth: 140 },
        authStatus: { minWidth: 80, defaultWidth: 120, idealWidth: 140 },
        profileStatus: { minWidth: 120, defaultWidth: 160, idealWidth: 200 },
        balance: { minWidth: 100, defaultWidth: 130, idealWidth: 150 },
        actions: { minWidth: 100, defaultWidth: 120, idealWidth: 150 },
    }), []);

    const columns: TableColumnDefinition<User>[] = useMemo(
        () => {
            const allColumns: (TableColumnDefinition<User> & { permission?: string })[] = [
                createTableColumn<User>({
                    columnId: "user",
                    compare: (a, b) => (a.name ?? "").localeCompare(b.name ?? ""),
                    renderHeaderCell: () => "Пользователь",
                    renderCell: (user) => <ProfileNameCell user={user} />,
                }),
                createTableColumn<User>({
                    columnId: "email",
                    compare: (a, b) => a.email.localeCompare(b.email),
                    renderHeaderCell: () => "Email",
                    renderCell: (user) => (
                        <TableCellLayout truncate>{user.email}</TableCellLayout>
                    ),
                }),
                {
                    ...createTableColumn<User>({
                        columnId: "phone",
                        compare: () => 0,
                        renderHeaderCell: () => "Телефон",
                        renderCell: (user) => <ProfilePhoneCell user={user} />,
                    }),
                    permission: Permissions.UserProfileProfileRead
                },
                createTableColumn<User>({
                    columnId: "role",
                    compare: (a, b) => a.role.localeCompare(b.role),
                    renderHeaderCell: () => "Роль",
                    renderCell: (user) => (
                        <TableCellLayout truncate>
                            <Badge appearance="outline">{user.role}</Badge>
                        </TableCellLayout>
                    ),
                }),
                createTableColumn<User>({
                    columnId: "authStatus",
                    compare: (a, b) => a.status.localeCompare(b.status),
                    renderHeaderCell: () => "Статус аккаунта",
                    renderCell: (user) => (
                        <TableCellLayout truncate>
                            <Badge
                                appearance="tint"
                                shape="rounded"

                                className={getUserStatusBadgeClass(user.status)}
                            >
                                {user.status}
                            </Badge>
                        </TableCellLayout>
                    ),
                }),
                {
                    ...createTableColumn<User>({
                        columnId: "profileStatus",
                        compare: () => 0,
                        renderHeaderCell: () => "Статус профиля",
                        renderCell: (user) => <ProfileStatusCell user={user} />,
                    }),
                    permission: Permissions.UserProfileProfileRead
                },
                {
                    ...createTableColumn<User>({
                        columnId: "balance",
                        compare: () => 0,
                        renderHeaderCell: () => "Баланс",
                        renderCell: (user) => <BalanceCell user={user} />,
                    }),
                    permission: Permissions.BillingBalanceRead
                },
                createTableColumn<User>({
                    columnId: "actions",
                    compare: () => 0,
                    renderHeaderCell: () => "Действия",
                    renderCell: (user) => (
                        <Button appearance="subtle" icon={<Eye20Regular />} onClick={() => navigate(`/admin/users/${user.id}`)}>
                            Открыть
                        </Button>
                    ),
                }),
            ];

            return allColumns.filter(col => !col.permission || has(col.permission as any));
        },
        [sizes, navigate, has]
    );

    if (isLoading) {
        return <div className="flex justify-center p-12"><Spinner label="Загрузка пользователей..." /></div>;
    }

    if (errorMessage) {
        return (
            <MessageBar intent="error" className="mb-4">
                <MessageBarBody>{errorMessage}</MessageBarBody>
            </MessageBar>
        );
    }

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Пользователи</Title2>
                    <Body2 block>{totalCount} зарегистрированных пользователей</Body2>
                </div>
            </div>

            <div className="flex gap-4 flex-wrap items-end mb-4">
                <Field label="Поиск по email" size={sizes.field}>
                    <Input size={sizes.input} value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Поиск по имени, email..." />
                </Field>
                <Field label="Статус" size={sizes.field}>
                    <Input size={sizes.input} value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} placeholder="active / inactive" />
                </Field>
                <Button appearance="secondary" size={sizes.button} onClick={handleClearFilters}>
                    Сбросить фильтры
                </Button>
            </div>

            <Card size={sizes.card}>
                <DataTable
                    items={users}
                    columns={columns}
                    getRowId={(user) => user.id}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <div className="flex items-center justify-between mt-4 flex-wrap gap-2">
                <Body1>Показано {users.length} из {totalCount}</Body1>
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
