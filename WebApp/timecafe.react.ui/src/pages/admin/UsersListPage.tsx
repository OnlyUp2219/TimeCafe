import {useCallback, useMemo, useState} from "react";
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
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {Eye20Regular} from "@fluentui/react-icons";
import {DataTable} from "@components/DataTable/DataTable";
import {Pagination} from "@components/Pagination/Pagination";
import type {User} from "@app-types/user";
import {useGetUsersQuery} from "@store/api/adminApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {useComponentSize} from "@hooks/useComponentSize";

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

export const UsersListPage = () => {
    const {sizes} = useComponentSize();
    const [search, setSearch] = useState("");
    const [statusFilter, setStatusFilter] = useState("");
    const [currentPage, setCurrentPage] = useState(1);
    const pageSize = 20;

    const {data, isLoading, error} = useGetUsersQuery({
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
        user: {minWidth: 150, defaultWidth: 220},
        email: {minWidth: 150, defaultWidth: 250},
        role: {minWidth: 80, defaultWidth: 120},
        status: {minWidth: 80, defaultWidth: 120},
        actions: {minWidth: 90, defaultWidth: 120},
    }), []);

    const columns: TableColumnDefinition<User>[] = useMemo(
        () => [
            createTableColumn<User>({
                columnId: "user",
                compare: (a, b) => (a.name ?? "").localeCompare(b.name ?? ""),
                renderHeaderCell: () => "Пользователь",
                renderCell: (user) => (
                    <TableCellLayout truncate media={<Avatar name={user.name || user.email} />}>
                        {user.name || "—"}
                    </TableCellLayout>
                ),
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
                columnId: "status",
                compare: (a, b) => a.status.localeCompare(b.status),
                renderHeaderCell: () => "Статус",
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
                columnId: "actions",
                compare: () => 0,
                renderHeaderCell: () => "Действия",
                renderCell: () => (
                    <Button appearance="subtle" icon={<Eye20Regular />}>
                        Открыть
                    </Button>
                ),
            }),
        ],
        [sizes]
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

            <Card className="overflow-x-auto" size={sizes.card}>
                <DataTable
                    items={users}
                    columns={columns}
                    getRowId={(user) => user.id}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <div className="flex items-center justify-between mt-4 flex-wrap gap-2">
                <Body1>
                    Показано {users.length} из {totalCount}
                </Body1>
                <Pagination className="" currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />
            </div>
        </div>
    );
};