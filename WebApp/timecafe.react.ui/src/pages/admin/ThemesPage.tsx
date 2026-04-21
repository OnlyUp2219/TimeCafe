import {useCallback, useMemo, useState} from "react";
import {
    Body1,
    Body2,
    Caption1,
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
    Subtitle2,
    Title2,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {Add20Regular, Delete20Regular, Edit20Regular} from "@fluentui/react-icons";
import {
    useGetAllThemesQuery,
    useCreateThemeMutation,
    useUpdateThemeMutation,
    useDeleteThemeMutation,
} from "@store/api/venueApi";
import type {Theme} from "@store/api/venueApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {DataTable} from "@components/DataTable/DataTable";
import {useComponentSize} from "@hooks/useComponentSize";
import {HasPermission} from "@components/Guard/HasPermission";
import {Permissions} from "@shared/auth/permissions";

interface ThemeFormState {
    name: string;
    emoji: string;
    colors: string;
}

const emptyForm: ThemeFormState = {name: "", emoji: "", colors: ""};

export const ThemesPage = () => {
    const {sizes} = useComponentSize();
    const {data: themes = [], isLoading, error} = useGetAllThemesQuery();
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const [createTheme] = useCreateThemeMutation();
    const [updateTheme] = useUpdateThemeMutation();
    const [deleteTheme] = useDeleteThemeMutation();

    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingTheme, setEditingTheme] = useState<Theme | null>(null);
    const [form, setForm] = useState<ThemeFormState>(emptyForm);
    const [mutationError, setMutationError] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);

    const openCreate = useCallback(() => {
        setEditingTheme(null);
        setForm(emptyForm);
        setMutationError(null);
        setDialogOpen(true);
    }, []);

    const openEdit = useCallback((t: Theme) => {
        setEditingTheme(t);
        setForm({name: t.name, emoji: t.emoji ?? "", colors: t.colors ?? ""});
        setMutationError(null);
        setDialogOpen(true);
    }, []);

    const handleSave = useCallback(async () => {
        setSaving(true);
        setMutationError(null);
        try {
            const body = {
                name: form.name,
                emoji: form.emoji || undefined,
                colors: form.colors || undefined,
            };
            if (editingTheme) {
                await updateTheme({themeId: editingTheme.themeId, ...body}).unwrap();
            } else {
                await createTheme(body).unwrap();
            }
            setDialogOpen(false);
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось сохранить тему");
        } finally {
            setSaving(false);
        }
    }, [form, editingTheme, createTheme, updateTheme]);

    const handleDelete = useCallback(async (themeId: string) => {
        try {
            await deleteTheme(themeId).unwrap();
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось удалить тему");
        }
    }, [deleteTheme]);

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        emoji: {minWidth: 60, defaultWidth: 80},
        name: {minWidth: 150, defaultWidth: 250},
        colors: {minWidth: 150, defaultWidth: 300},
        actions: {minWidth: 90, defaultWidth: 100},
    }), []);

    const columns: TableColumnDefinition<Theme>[] = useMemo(() => [
        createTableColumn<Theme>({
            columnId: "emoji",
            compare: (a, b) => (a.emoji ?? "").localeCompare(b.emoji ?? ""),
            renderHeaderCell: () => "Эмодзи",
            renderCell: (theme) => (
                <TableCellLayout truncate>
                    <span className="text-xl">{theme.emoji || "—"}</span>
                </TableCellLayout>
            ),
        }),
        createTableColumn<Theme>({
            columnId: "name",
            compare: (a, b) => a.name.localeCompare(b.name),
            renderHeaderCell: () => "Название",
            renderCell: (theme) => (
                <TableCellLayout truncate>
                    <Body1>{theme.name}</Body1>
                </TableCellLayout>
            ),
        }),
        createTableColumn<Theme>({
            columnId: "colors",
            compare: (a, b) => (a.colors ?? "").localeCompare(b.colors ?? ""),
            renderHeaderCell: () => "Цветовая палитра",
            renderCell: (theme) => (
                <TableCellLayout truncate>
                    {theme.colors ? (
                        <div className="flex items-center gap-1">
                            {theme.colors.split(",").map((c) => (
                                <span
                                    key={c}
                                    className="inline-block w-5 h-5 rounded-sm border border-gray-300"
                                    style={{backgroundColor: c.trim()}}
                                />
                            ))}
                            <Body2 className="ml-1 text-gray-500">{theme.colors}</Body2>
                        </div>
                    ) : "—"}
                </TableCellLayout>
            ),
        }),
        createTableColumn<Theme>({
            columnId: "actions",
            compare: () => 0,
            renderHeaderCell: () => "Действия",
            renderCell: (theme) => (
                <div className="flex gap-1">
                    <HasPermission can={Permissions.VenueThemeUpdate}>
                        <Button appearance="subtle" icon={<Edit20Regular />} onClick={() => openEdit(theme)} />
                    </HasPermission>
                    <HasPermission can={Permissions.VenueThemeDelete}>
                        <Button appearance="subtle" icon={<Delete20Regular />} onClick={() => handleDelete(theme.themeId)} />
                    </HasPermission>
                </div>
            ),
        }),
    ], [sizes, openEdit, handleDelete]);

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Темы оформления</Title2>
                    <Body2 block>{themes.length} тем</Body2>
                </div>
                <HasPermission can={Permissions.VenueThemeCreate}>
                    <Button appearance="primary" size={sizes.button} icon={<Add20Regular />} onClick={openCreate}>
                        Добавить тему
                    </Button>
                </HasPermission>
            </div>

            {(queryError || mutationError) && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{queryError || mutationError}</MessageBarBody>
                </MessageBar>
            )}

            <Card className="overflow-x-auto" size={sizes.card}>
                <DataTable
                    items={themes}
                    columns={columns}
                    getRowId={(t) => t.themeId}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <Dialog open={dialogOpen} onOpenChange={(_, d) => setDialogOpen(d.open)}>
                <DialogSurface>
                    <DialogBody>
                        <DialogTitle>{editingTheme ? "Редактировать тему" : "Новая тема"}</DialogTitle>
                        <DialogContent className="flex flex-col gap-4">
                            <Field label="Название" required size={sizes.field}>
                                <Input value={form.name} onChange={(_, d) => setForm(f => ({...f, name: d.value}))} size={sizes.input} />
                            </Field>
                            <Field label="Эмодзи" size={sizes.field}>
                                <Input value={form.emoji} placeholder="🚀" onChange={(_, d) => setForm(f => ({...f, emoji: d.value}))} size={sizes.input} />
                            </Field>
                            <Field label="Цвета (через запятую)" size={sizes.field}>
                                <Input value={form.colors} placeholder="#1a1a2e,#16213e,#0f3460" onChange={(_, d) => setForm(f => ({...f, colors: d.value}))} size={sizes.input} />
                            </Field>
                            {(form.colors || form.emoji) && (
                                <div>
                                    <Caption1 block style={{marginBottom: 6, color: "var(--colorNeutralForeground3)"}}>Превью темы</Caption1>
                                    <div
                                        className="rounded-lg flex flex-col items-center justify-center gap-2"
                                        style={{
                                            minHeight: 100,
                                            background: form.colors
                                                ? `linear-gradient(135deg, ${form.colors.split(",").map(c => c.trim()).join(", ")})`
                                                : "var(--colorNeutralBackground3)",
                                            padding: "16px 24px",
                                        }}
                                    >
                                        {form.emoji && <span style={{fontSize: 32}}>{form.emoji}</span>}
                                        {form.name && (
                                            <Subtitle2 style={{color: "#fff", textShadow: "0 1px 3px rgba(0,0,0,0.5)"}}>
                                                {form.name}
                                            </Subtitle2>
                                        )}
                                        {form.colors && (
                                            <div className="flex gap-1 mt-1">
                                                {form.colors.split(",").map((c) => (
                                                    <span
                                                        key={c}
                                                        title={c.trim()}
                                                        className="inline-block w-5 h-5 rounded-full border-2 border-white/50"
                                                        style={{backgroundColor: c.trim()}}
                                                    />
                                                ))}
                                            </div>
                                        )}
                                    </div>
                                </div>
                            )}
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
                            <Button appearance="primary" size={sizes.button} onClick={handleSave} disabled={saving || !form.name}>
                                {saving ? "Сохранение..." : "Сохранить"}
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>
        </div>
    );
};
