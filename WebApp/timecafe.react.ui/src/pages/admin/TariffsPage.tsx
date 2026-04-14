import {useCallback, useMemo, useState} from "react";
import {
    Avatar,
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
    Switch,
    Title2,
} from "@fluentui/react-components";
import {Add20Regular, Delete20Regular, Edit20Regular} from "@fluentui/react-icons";
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

export const TariffsPage = () => {
    const [currentPage, setCurrentPage] = useState(1);
    const pageSize = 20;
    const {data, isLoading, error} = useGetTariffsPageQuery({pageNumber: currentPage, pageSize});
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

    const columns = useMemo(() => [
        {
            columnId: "name",
            renderHeaderCell: () => "Тариф",
            compare: (a: TariffWithTheme, b: TariffWithTheme) => a.name.localeCompare(b.name),
        },
        {
            columnId: "price",
            renderHeaderCell: () => "Цена/мин",
            compare: (a: TariffWithTheme, b: TariffWithTheme) => a.pricePerMinute - b.pricePerMinute,
        },
        {
            columnId: "billingType",
            renderHeaderCell: () => "Тип",
            compare: (a: TariffWithTheme, b: TariffWithTheme) => a.billingType - b.billingType,
        },
        {
            columnId: "theme",
            renderHeaderCell: () => "Тема",
            compare: (a: TariffWithTheme, b: TariffWithTheme) => (a.themeName ?? "").localeCompare(b.themeName ?? ""),
        },
        {
            columnId: "status",
            renderHeaderCell: () => "Статус",
            compare: (a: TariffWithTheme, b: TariffWithTheme) => Number(a.isActive) - Number(b.isActive),
        },
        {
            columnId: "actions",
            renderHeaderCell: () => "Действия",
            compare: () => 0,
        },
    ], []);

    const renderCell = (tariff: TariffWithTheme, columnId: string) => {
        switch (columnId) {
            case "name":
                return (
                    <div className="flex items-center gap-2">
                        {tariff.themeEmoji && <Avatar name={tariff.themeEmoji} size={28} />}
                        <div>
                            <Body1 block>{tariff.name}</Body1>
                            {tariff.description && <Body2 block className="text-gray-500">{tariff.description}</Body2>}
                        </div>
                    </div>
                );
            case "price":
                return `${tariff.pricePerMinute} ₽`;
            case "billingType":
                return <Badge appearance="outline">{billingTypeLabel(tariff.billingType)}</Badge>;
            case "theme":
                return tariff.themeName || "—";
            case "status":
                return (
                    <Switch
                        checked={tariff.isActive}
                        onChange={() => handleToggleActive(tariff)}
                        label={tariff.isActive ? "Активен" : "Неактивен"}
                    />
                );
            case "actions":
                return (
                    <div className="flex gap-1">
                        <Button appearance="subtle" size="small" icon={<Edit20Regular />} onClick={() => openEdit(tariff)} />
                        <Button appearance="subtle" size="small" icon={<Delete20Regular />} onClick={() => handleDelete(tariff.tariffId)} />
                    </div>
                );
            default:
                return "";
        }
    };

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Тарифы</Title2>
                    <Body2 block>{totalCount} тарифов</Body2>
                </div>
                <Button appearance="primary" size="large" icon={<Add20Regular />} onClick={openCreate}>
                    Добавить тариф
                </Button>
            </div>

            {(queryError || mutationError) && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{queryError || mutationError}</MessageBarBody>
                </MessageBar>
            )}

            <Card className="overflow-x-auto" size="large">
                <DataTable
                    items={tariffs}
                    columns={columns.map(col => ({
                        ...col,
                        renderCell: (item: TariffWithTheme) => renderCell(item, col.columnId),
                    }))}
                    getRowId={(t) => t.tariffId}
                    loading={isLoading}
                />
            </Card>

            <div className="flex items-center justify-between mt-4 flex-wrap gap-2">
                <Body1>Показано {tariffs.length} из {totalCount}</Body1>
                <Pagination className="" currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />
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
                            <Field label="Цена за минуту (₽)" required>
                                <Input type="number" value={form.pricePerMinute} onChange={(_, d) => setForm(f => ({...f, pricePerMinute: d.value}))} />
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
