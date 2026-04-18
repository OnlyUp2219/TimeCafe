import { useCallback, useMemo, useState } from "react";
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
    Title2,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { Eye20Regular } from "@fluentui/react-icons";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import type { User } from "@app-types/user";
import { useGetUsersQuery } from "@store/api/adminApi";
import { useGetProfileByUserIdReadOnlyQuery } from "@store/api/profileApi";
import { useGetBalanceQuery } from "@store/api/billingApi";
import { formatMoneyByN } from "@utility/formatMoney";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useComponentSize } from "@hooks/useComponentSize";

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
    const { data: profile } = useGetProfileByUserIdReadOnlyQuery(user.id);
    const displayName = profile?.firstName || profile?.lastName
        ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim()
        : (user.name || "—");
    return (
        <TableCellLayout truncate media={<Avatar name={displayName || user.email} />}>
            {displayName}
        </TableCellLayout>
    );
};

const ProfilePhoneCell = ({ user }: { user: User }) => {
    const { data: profile } = useGetProfileByUserIdReadOnlyQuery(user.id);
    return <TableCellLayout truncate>{profile?.phoneNumber || "—"}</TableCellLayout>;
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
    const { data: profile } = useGetProfileByUserIdReadOnlyQuery(user.id);
    if (!profile) return <TableCellLayout truncate>—</TableCellLayout>;
    return (
        <TableCellLayout truncate>
            <Badge appearance="tint" color={profileStatusColor(profile.profileStatus)}>
                {profileStatusLabel(profile.profileStatus)}
            </Badge>
        </TableCellLayout>
    );
};

const BalanceCell = ({ user }: { user: User }) => {
    const { data: balance, isLoading } = useGetBalanceQuery(user.id);
    if (isLoading) return <TableCellLayout truncate>—</TableCellLayout>;
    if (balance === undefined) return <TableCellLayout truncate>—</TableCellLayout>;
    return <TableCellLayout truncate>{formatMoneyByN(balance.currentBalance)}</TableCellLayout>;
};

export const UsersListPage = () => {
    const navigate = useNavigate();
    const { sizes } = useComponentSize();
    const [search, setSearch] = useState("");
    const [statusFilter, setStatusFilter] = useState("");
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(20);

    const { data, isLoading, error } = useGetUsersQuery({
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
        setSearch("");
        setStatusFilter("");
        setCurrentPage(1);
    }, []);

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
        () => [
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
            createTableColumn<User>({
                columnId: "phone",
                compare: () => 0,
                renderHeaderCell: () => "Телефон",
                renderCell: (user) => <ProfilePhoneCell user={user} />,
            }),
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
            createTableColumn<User>({
                columnId: "profileStatus",
                compare: () => 0,
                renderHeaderCell: () => "Статус профиля",
                renderCell: (user) => <ProfileStatusCell user={user} />,
            }),
            createTableColumn<User>({
                columnId: "balance",
                compare: () => 0,
                renderHeaderCell: () => "Баланс",
                renderCell: (user) => <BalanceCell user={user} />,
            }),
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
        ],
        [sizes, navigate]
    );

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Пользователи</Title2>
                    <Body2 block>{totalCount} зарегистрированных пользователей</Body2>
                </div>
            </div>

            {errorMessage && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{errorMessage}</MessageBarBody>
                </MessageBar>
            )}

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

