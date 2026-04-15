import {useCallback, useMemo, useState} from "react";
import {
    Avatar,
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    Caption1,
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
    Switch,
    Title2,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {Add20Regular, Delete20Regular, Edit20Regular, ArrowClockwise20Regular} from "@fluentui/react-icons";
import {
    useGetTariffsPageQuery,
    useCreateTariffMutation,
    useUpdateTariffMutation,
    useDeleteTariffMutation,
    useActivateTariffMutation,
    useDeactivateTariffMutation,
} from "@store/api/venueApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import type {TariffWithTheme} from "@app-types/tariffWithTheme";
import {BillingType} from "@app-types/tariff";
import {DataTable} from "@components/DataTable/DataTable";
import {Pagination} from "@components/Pagination/Pagination";
import {useComponentSize} from "@hooks/useComponentSize";

const billingTypeLabel = (bt: number) => bt === BillingType.Hourly ? "Почасовой" : "Поминутный";

interface TariffFormState {
    name: string;
    description: string;
    pricePerMinute: string;
    billingType: 1 | 2;
    isActive: boolean;
}

const emptyForm: TariffFormState = {
    name: "",
    description: "",
    pricePerMinute: "",
    billingType: 2,
    isActive: true,
};

const calcPerHour = (perMinute: string): string => {
    const v = parseFloat(perMinute);
    if (!v || isNaN(v)) return "";
    return (v * 60).toFixed(2);
};

export const TariffsPage = () => {
    const {sizes} = useComponentSize();
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(20);
    const {data, isLoading, error, refetch} = useGetTariffsPageQuery(
        {pageNumber: currentPage, pageSize},
        {refetchOnMountOrArgChange: true}
    );
    const tariffs = data?.tariffs ?? [];
    const totalCount = data?.totalCount ?? 0;
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const [createTariff] = useCreateTariffMutation();
    const [updateTariff] = useUpdateTariffMutation();
    const [deleteTariff] = useDeleteTariffMutation();
    const [activateTariff] = useActivateTariffMutation();
    const [deactivateTariff] = useDeactivateTariffMutation();

    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingTariff, setEditingTariff] = useState<TariffWithTheme | null>(null);
    const [form, setForm] = useState<TariffFormState>(emptyForm);
    const [mutationError, setMutationError] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);

    const openCreate = useCallback(() => {
        setEditingTariff(null);
        setForm(emptyForm);
        setMutationError(null);
        setDialogOpen(true);
    }, []);

    const openEdit = useCallback((t: TariffWithTheme) => {
        setEditingTariff(t);
        setForm({
            name: t.name,
            description: t.description ?? "",
            pricePerMinute: String(t.pricePerMinute),
            billingType: t.billingType,
            isActive: t.isActive,
        });
        setMutationError(null);
        setDialogOpen(true);
    }, []);

    const handleSave = useCallback(async () => {
        setSaving(true);
        setMutationError(null);
        try {
            const body = {
                name: form.name,
                description: form.description || undefined,
                pricePerMinute: parseFloat(form.pricePerMinute) || 0,
                billingType: form.billingType,
                isActive: form.isActive,
            };
            if (editingTariff) {
                await updateTariff({tariffId: editingTariff.tariffId, ...body}).unwrap();
            } else {
                await createTariff(body).unwrap();
            }
            setDialogOpen(false);
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось сохранить тариф");
        } finally {
            setSaving(false);
        }
    }, [form, editingTariff, createTariff, updateTariff]);

    const handleDelete = useCallback(async (tariffId: string) => {
        try {
            await deleteTariff(tariffId).unwrap();
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось удалить тариф");
        }
    }, [deleteTariff]);

    const handleToggleActive = useCallback(async (t: TariffWithTheme) => {
        try {
            if (t.isActive) {
                await deactivateTariff(t.tariffId).unwrap();
            } else {
                await activateTariff(t.tariffId).unwrap();
            }
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось изменить статус");
        }
    }, [activateTariff, deactivateTariff]);

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        name: {minWidth: 150, defaultWidth: 250},
        price: {minWidth: 80, defaultWidth: 140},
        billingType: {minWidth: 100, defaultWidth: 140},
        theme: {minWidth: 80, defaultWidth: 140},
        status: {minWidth: 100, defaultWidth: 150},
        actions: {minWidth: 90, defaultWidth: 100},
    }), []);

    const columns: TableColumnDefinition<TariffWithTheme>[] = useMemo(() => [
        createTableColumn<TariffWithTheme>({
            columnId: "name",
            compare: (a, b) => a.name.localeCompare(b.name),
            renderHeaderCell: () => "Тариф",
            renderCell: (tariff) => (
                <TableCellLayout truncate media={tariff.themeEmoji ? <Avatar name={tariff.themeEmoji} /> : undefined}>
                    <div>
                        <Body1 block>{tariff.name}</Body1>
                        {tariff.description && <Body2 block className="text-gray-500">{tariff.description}</Body2>}
                    </div>
                </TableCellLayout>
            ),
        }),
        createTableColumn<TariffWithTheme>({
            columnId: "price",
            compare: (a, b) => a.pricePerMinute - b.pricePerMinute,
            renderHeaderCell: () => "Цена",
            renderCell: (tariff) => (
                <TableCellLayout truncate>
                    <div>
                        <Body1 block>{tariff.pricePerMinute} ₽/мин</Body1>
                        <Caption1 block style={{color: "var(--colorNeutralForeground3)"}}>
                            {(tariff.pricePerMinute * 60).toFixed(2)} ₽/час
                        </Caption1>
                    </div>
                </TableCellLayout>
            ),
        }),
        createTableColumn<TariffWithTheme>({
            columnId: "billingType",
            compare: (a, b) => a.billingType - b.billingType,
            renderHeaderCell: () => "Тип",
            renderCell: (tariff) => (
                <TableCellLayout truncate>
                    <Badge appearance="outline">{billingTypeLabel(tariff.billingType)}</Badge>
                </TableCellLayout>
            ),
        }),
        createTableColumn<TariffWithTheme>({
            columnId: "theme",
            compare: (a, b) => (a.themeName ?? "").localeCompare(b.themeName ?? ""),
            renderHeaderCell: () => "Тема",
            renderCell: (tariff) => (
                <TableCellLayout truncate>{tariff.themeName || "—"}</TableCellLayout>
            ),
        }),
        createTableColumn<TariffWithTheme>({
            columnId: "status",
            compare: (a, b) => Number(a.isActive) - Number(b.isActive),
            renderHeaderCell: () => "Статус",
            renderCell: (tariff) => (
                <Switch
                    checked={tariff.isActive}
                    onChange={() => handleToggleActive(tariff)}
                    label={tariff.isActive ? "Активен" : "Неактивен"}
                />
            ),
        }),
        createTableColumn<TariffWithTheme>({
            columnId: "actions",
            compare: () => 0,
            renderHeaderCell: () => "Действия",
            renderCell: (tariff) => (
                <div className="flex gap-1">
                    <Button appearance="subtle" icon={<Edit20Regular />} onClick={() => openEdit(tariff)} />
                    <Button appearance="subtle" icon={<Delete20Regular />} onClick={() => handleDelete(tariff.tariffId)} />
                </div>
            ),
        }),
    ], [sizes, handleToggleActive, openEdit, handleDelete]);

    const perHour = calcPerHour(form.pricePerMinute);

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Тарифы</Title2>
                    <Body2 block>{totalCount} тарифов</Body2>
                </div>
                <div className="flex gap-2">
                    <Button appearance="subtle" size={sizes.button} icon={<ArrowClockwise20Regular />} onClick={() => refetch()} />
                    <Button appearance="primary" size={sizes.button} icon={<Add20Regular />} onClick={openCreate}>
                        Добавить тариф
                    </Button>
                </div>
            </div>

            {(queryError || mutationError) && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{queryError || mutationError}</MessageBarBody>
                </MessageBar>
            )}

            <Card className="overflow-x-auto" size={sizes.card}>
                <DataTable
                    items={tariffs}
                    columns={columns}
                    getRowId={(t) => t.tariffId}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <div className="flex items-center justify-between mt-4 flex-wrap gap-2">
                <Body1>Показано {tariffs.length} из {totalCount}</Body1>
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
                        <DialogTitle>{editingTariff ? "Редактировать тариф" : "Новый тариф"}</DialogTitle>
                        <DialogContent className="flex flex-col gap-4">
                            <Field label="Название" required>
                                <Input value={form.name} onChange={(_, d) => setForm(f => ({...f, name: d.value}))} />
                            </Field>
                            <Field label="Описание">
                                <Input value={form.description} onChange={(_, d) => setForm(f => ({...f, description: d.value}))} />
                            </Field>
                            <Field label="Цена за минуту (₽)" required hint={perHour ? `≈ ${perHour} ₽/час` : undefined}>
                                <Input
                                    type="number"
                                    value={form.pricePerMinute}
                                    onChange={(_, d) => setForm(f => ({...f, pricePerMinute: d.value}))}
                                />
                            </Field>
                            <Field label="Тип тарификации">
                                <div className="flex gap-4">
                                    <Button
                                        appearance={form.billingType === 2 ? "primary" : "secondary"}
                                        onClick={() => setForm(f => ({...f, billingType: 2}))}
                                    >
                                        Поминутный
                                    </Button>
                                    <Button
                                        appearance={form.billingType === 1 ? "primary" : "secondary"}
                                        onClick={() => setForm(f => ({...f, billingType: 1}))}
                                    >
                                        Почасовой
                                    </Button>
                                </div>
                            </Field>
                            <Switch
                                checked={form.isActive}
                                onChange={(_, d) => setForm(f => ({...f, isActive: d.checked}))}
                                label="Активен"
                            />
                            {mutationError && (
                                <MessageBar intent="error">
                                    <MessageBarBody>{mutationError}</MessageBarBody>
                                </MessageBar>
                            )}
                        </DialogContent>
                        <DialogActions>
                            <DialogTrigger disableButtonEnhancement>
                                <Button appearance="secondary">Отмена</Button>
                            </DialogTrigger>
                            <Button appearance="primary" onClick={handleSave} disabled={saving || !form.name || !form.pricePerMinute}>
                                {saving ? <Spinner size="tiny" /> : (editingTariff ? "Сохранить" : "Создать")}
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>
        </div>
    );
};
