import { useCallback, useMemo, useState } from "react";
import {
    Badge,
    Body2,
    Button,
    Card,
    Dialog,
    DialogActions,
    DialogBody,
    DialogContent,
    DialogSurface,
    DialogTitle,
    DialogTrigger,
    Field,
    Input,
    MessageBar,
    MessageBarBody,
    Spinner,
    Title2,
    createTableColumn,
    TableCellLayout,
    Body1,
} from "@fluentui/react-components";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { Add20Regular, Delete20Regular, LockClosed20Regular } from "@fluentui/react-icons";
import { useNavigate } from "react-router-dom";
import { useGetRolesQuery, useCreateRoleMutation, useDeleteRoleMutation } from "@store/api/adminApi";
import type { RoleDto } from "@store/api/adminApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { useComponentSize } from "@hooks/useComponentSize";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions } from "@shared/auth/permissions";

export const RolesPage = () => {
    const navigate = useNavigate();
    const { sizes } = useComponentSize();
    const { data, isLoading, error } = useGetRolesQuery(undefined, { refetchOnMountOrArgChange: true });
    const roles = data?.roles ?? [];
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(20);

    const totalCount = roles.length;
    const totalPages = Math.ceil(totalCount / pageSize) || 1;
    const paginatedRoles = useMemo(() => {
        const start = (currentPage - 1) * pageSize;
        return roles.slice(start, start + pageSize);
    }, [roles, currentPage, pageSize]);

    const [createRole] = useCreateRoleMutation();
    const [deleteRole] = useDeleteRoleMutation();

    const [dialogOpen, setDialogOpen] = useState(false);
    const [roleName, setRoleName] = useState("");
    const [mutationError, setMutationError] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);

    const handleCreate = useCallback(async () => {
        setSaving(true);
        setMutationError(null);
        try {
            await createRole({ roleName }).unwrap();
            setDialogOpen(false);
            setRoleName("");
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось создать роль");
        } finally {
            setSaving(false);
        }
    }, [roleName, createRole]);

    const handleDelete = useCallback(async (name: string) => {
        try {
            await deleteRole(name).unwrap();
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось удалить роль");
        }
    }, [deleteRole]);

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        roleName: { minWidth: 150, defaultWidth: 300, idealWidth: 350 },
        actions: { minWidth: 100, defaultWidth: 120, idealWidth: 150 },
    }), []);

    const columns: TableColumnDefinition<RoleDto>[] = useMemo(() => [
        createTableColumn<RoleDto>({
            columnId: "roleName",
            compare: (a, b) => a.roleName.localeCompare(b.roleName),
            renderHeaderCell: () => "Роль",
            renderCell: (r) => (
                <TableCellLayout truncate>
                    <Badge appearance="outline">{r.roleName}</Badge>
                </TableCellLayout>
            ),
        }),
        createTableColumn<RoleDto>({
            columnId: "actions",
            compare: () => 0,
            renderHeaderCell: () => "Действия",
            renderCell: (r) => (
                <div className="flex gap-1">
                    <HasPermission can={Permissions.RbacRoleClaimsUpdate}>
                        <Button
                            appearance="subtle"
                            icon={<LockClosed20Regular />}
                            onClick={() => navigate(`/admin/roles/${r.roleName}/claims`)}
                            title="Управление правами"
                        />
                    </HasPermission>
                    <HasPermission can={Permissions.RbacRoleDelete}>
                        <Button
                            appearance="subtle"
                            icon={<Delete20Regular />}
                            onClick={() => handleDelete(r.roleName)}
                            title="Удалить роль"
                        />
                    </HasPermission>
                </div>
            ),
        }),
    ], [handleDelete, navigate]);

    if (isLoading) {
        return <div className="flex justify-center p-12"><Spinner label="Загрузка ролей..." /></div>;
    }

    if (queryError) {
        return (
            <MessageBar intent="error" className="mb-4">
                <MessageBarBody>{queryError}</MessageBarBody>
            </MessageBar>
        );
    }

    return (
        <div className="flex flex-col gap-4">
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Роли</Title2>
                    <Body2 block>{totalCount} ролей</Body2>
                </div>
                <HasPermission can={Permissions.RbacRoleCreate}>
                    <Button appearance="primary" size={sizes.button} icon={<Add20Regular />} onClick={() => { setDialogOpen(true); setMutationError(null); }}>
                        Создать роль
                    </Button>
                </HasPermission>
            </div>

            {mutationError && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{mutationError}</MessageBarBody>
                </MessageBar>
            )}

            <Card size={sizes.card}>
                <DataTable
                    items={paginatedRoles}
                    columns={columns}
                    getRowId={(r) => r.roleId}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <div className="flex items-center justify-between mt-4 flex-wrap gap-2">
                <Body1>Показано {paginatedRoles.length} из {totalCount}</Body1>
                <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    onPageChange={setCurrentPage}
                    pageSize={pageSize}
                    onPageSizeChange={setPageSize}
                    totalCount={totalCount}
                />
            </div>

            <Dialog open={dialogOpen} onOpenChange={(_, d) => setDialogOpen(d.open)}>
                <DialogSurface>
                    <DialogBody>
                        <DialogTitle>Создать роль</DialogTitle>
                        <DialogContent className="flex flex-col gap-4">
                            <Field label="Название роли" required size={sizes.field}>
                                <Input
                                    value={roleName}
                                    onChange={(_, d) => setRoleName(d.value)}
                                    placeholder="Manager"
                                    size={sizes.input}
                                />
                            </Field>
                            {mutationError && (
                                <MessageBar intent="error">
                                    <MessageBarBody>{mutationError}</MessageBarBody>
                                </MessageBar>
                            )}
                        </DialogContent>
                        <DialogActions>
                            <DialogTrigger disableButtonEnhancement>
                                <Button appearance="secondary" size={sizes.button}>Отмена</Button>
                            </DialogTrigger>
                            <Button appearance="primary" size={sizes.button} onClick={handleCreate} disabled={saving || !roleName}>
                                {saving ? <Spinner size="tiny" /> : "Создать"}
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>
        </div>
    );
};
