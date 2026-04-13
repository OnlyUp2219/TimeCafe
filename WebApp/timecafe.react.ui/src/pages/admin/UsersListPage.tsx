import {useCallback, useMemo, useState} from "react";
import {Badge, Body2, Button, Card, Field, Input, Title2} from "@fluentui/react-components";
import {DataTable} from "@components/DataTable/DataTable";
import {Pagination} from "@components/Pagination/Pagination";
import type {User} from "@app-types/user";
import {useGetUsersQuery} from "@store/api/adminApi";

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

    const {data, isLoading} = useGetUsersQuery({
        page: currentPage,
        size: pageSize,
        search: search || undefined,
        status: statusFilter || undefined,
    });

    const users = data?.users ?? [];
    const totalPages = data?.pagination.totalPages ?? 1;

    const handleClearFilters = useCallback(() => {
        setSearch("");
        setStatusFilter("");
        setCurrentPage(1);
    }, []);

    const columns = useMemo(
        () => [
            {
                columnId: "email",
                renderHeaderCell: () => "Email",
                compare: (a: User, b: User) => a.email.localeCompare(b.email),
            },
            {
                columnId: "name",
                renderHeaderCell: () => "Имя",
                compare: (a: User, b: User) => (a.name ?? "").localeCompare(b.name ?? ""),
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
            case "email":
                return user.email;
            case "name":
                return user.name || "-";
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
                return <Button appearance="secondary" size="large">Открыть</Button>;
            default:
                return "";
        }
    };

    return (
        <div className="relative mx-auto w-full max-w-6xl px-2 py-4 sm:px-3 sm:py-6">
            <div className="mb-4">
                <Title2>Управление пользователями</Title2>
                <Body2 block>Просматривайте учетные записи, фильтруйте по статусу и переходите к деталям.</Body2>
            </div>

            <div>
                <div className="flex gap-4 flex-wrap items-end">
                    <Field label="Поиск по email" size="large">
                        <Input size="large" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Введите email" />
                    </Field>
                    <Field label="Статус" size="large">
                        <Input size="large" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} placeholder="active / inactive" />
                    </Field>
                    <Button appearance="secondary" size="large" onClick={handleClearFilters}>
                        Сбросить фильтры
                    </Button>
                </div>
                <div className="mt-4 flex flex-wrap items-center justify-end gap-2">

                </div>
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
            <Pagination className="flex items-center justify-end gap-2 p-4" currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />
        </div>
    );
};