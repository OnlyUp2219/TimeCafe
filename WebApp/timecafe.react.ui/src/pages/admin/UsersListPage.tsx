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
} from "@fluentui/react-components";
import {Eye20Regular} from "@fluentui/react-icons";
import {DataTable} from "@components/DataTable/DataTable";
import {Pagination} from "@components/Pagination/Pagination";
import type {User} from "@app-types/user";
import {useGetUsersQuery} from "@store/api/adminApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";

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

    const columns = useMemo(
        () => [
            {
                columnId: "user",
                renderHeaderCell: () => "Пользователь",
                compare: (a: User, b: User) => (a.name ?? "").localeCompare(b.name ?? ""),
            },
            {
                columnId: "email",
                renderHeaderCell: () => "Email",
                compare: (a: User, b: User) => a.email.localeCompare(b.email),
            },
            {
                columnId: "role",
                renderHeaderCell: () => "Роль",
                compare: (a: User, b: User) => a.role.localeCompare(b.role),
            },
            {
                columnId: "status",
                renderHeaderCell: () => "Статус",
                compare: (a: User, b: User) => a.status.localeCompare(b.status),
            },
            {
                columnId: "actions",
                renderHeaderCell: () => "Действия",
                compare: () => 0,
            },
        ],
        []
    );

    const renderCell = (user: User, columnId: string) => {
        switch (columnId) {
            case "user":
                return (
                    <div className="flex items-center gap-2">
                        <Avatar name={user.name || user.email} size={28} />
                        <span>{user.name || "—"}</span>
                    </div>
                );
            case "email":
                return user.email;
            case "role":
                return <Badge appearance="outline" size="large">{user.role}</Badge>;
            case "status":
                return (
                    <Badge
                        appearance="tint"
                        shape="rounded"
                        size="large"
                        className={getUserStatusBadgeClass(user.status)}
                    >
                        {user.status}
                    </Badge>
                );
            case "actions":
                return (
                    <Button appearance="subtle" size="small" icon={<Eye20Regular />}>
                        Открыть
                    </Button>
                );
            default:
                return "";
        }
    };

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
                <Field label="Поиск по email" size="large">
                    <Input size="large" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Поиск по имени, email..." />
                </Field>
                <Field label="Статус" size="large">
                    <Input size="large" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} placeholder="active / inactive" />
                </Field>
                <Button appearance="secondary" size="large" onClick={handleClearFilters}>
                    Сбросить фильтры
                </Button>
            </div>

            <Card className="overflow-x-auto" size="large">
                <DataTable
                    items={users}
                    columns={columns.map(col => ({
                        ...col,
                        renderCell: (item: User) => renderCell(item, col.columnId),
                    }))}
                    getRowId={(user) => user.id}
                    loading={isLoading}
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