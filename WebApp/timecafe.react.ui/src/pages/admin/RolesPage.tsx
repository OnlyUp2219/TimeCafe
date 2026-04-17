import {useCallback, useMemo, useState} from "react";
import {
    Badge,
    Body1,
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
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {Add20Regular, Delete20Regular, LockClosed20Regular} from "@fluentui/react-icons";
import {useNavigate} from "react-router-dom";
import {useGetRolesQuery, useCreateRoleMutation, useDeleteRoleMutation} from "@store/api/adminApi";
import type {RoleDto} from "@store/api/adminApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {DataTable} from "@components/DataTable/DataTable";
import {useComponentSize} from "@hooks/useComponentSize";

export const RolesPage = () => {
    const navigate = useNavigate();
    const {sizes} = useComponentSize();
    const {data, isLoading, error} = useGetRolesQuery();
    const roles = data?.roles ?? [];
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

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
            await createRole({roleName}).unwrap();
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
        name: {minWidth: 150, defaultWidth: 250},
        normalized: {minWidth: 150, defaultWidth: 250},
        actions: {minWidth: 90, defaultWidth: 100},
    }), []);

    const columns: TableColumnDefinition<RoleDto>[] = useMemo(() => [
        createTableColumn<RoleDto>({
            columnId: "name",
            compare: (a, b) => a.name.localeCompare(b.name),
            renderHeaderCell: () => "Роль",
            renderCell: (r) => (
                <TableCellLayout truncate>
                    <Badge appearance="outline">{r.name}</Badge>
                </TableCellLayout>
            ),
        }),
        createTableColumn<RoleDto>({
            columnId: "normalized",
            compare: (a, b) => a.normalizedName.localeCompare(b.normalizedName),
            renderHeaderCell: () => "Нормализованное имя",
            renderCell: (r) => <TableCellLayout truncate>{r.normalizedName}</TableCellLayout>,
        }),
        createTableColumn<RoleDto>({
            columnId: "actions",
            compare: () => 0,
            renderHeaderCell: () => "Действия",
            renderCell: (r) => (
                <div className="flex gap-1">
                    <Button
                        appearance="subtle"
                        icon={<LockClosed20Regular />}
                        onClick={() => navigate(`/admin/roles/${r.name}/claims`)}
                    />
                    <Button
                        appearance="subtle"
                        icon={<Delete20Regular />}
                        onClick={() => handleDelete(r.name)}
                    />
                </div>
            ),
        }),
    ], [handleDelete, navigate]);

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Роли</Title2>
                    <Body2 block>{roles.length} ролей</Body2>
                </div>
                <Button appearance="primary" size={sizes.button} icon={<Add20Regular />} onClick={() => { setDialogOpen(true); setMutationError(null); }}>
                    Создать роль
                </Button>
            </div>

            {(queryError || mutationError) && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{queryError || mutationError}</MessageBarBody>
                </MessageBar>
            )}

            <Card className="overflow-x-auto" size={sizes.card}>
                <DataTable
                    items={roles}
                    columns={columns}
                    getRowId={(r) => r.name}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

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
