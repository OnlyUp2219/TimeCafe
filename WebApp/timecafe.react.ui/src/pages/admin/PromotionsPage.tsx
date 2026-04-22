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
    Switch,
    Title2,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {Add20Regular, Delete20Regular, Edit20Regular} from "@fluentui/react-icons";
import {
    useGetAllPromotionsQuery,
    useCreatePromotionMutation,
    useUpdatePromotionMutation,
    useDeletePromotionMutation,
    useActivatePromotionMutation,
    useDeactivatePromotionMutation,
} from "@store/api/venueApi";
import type {Promotion} from "@store/api/venueApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {DataTable} from "@components/DataTable/DataTable";
import {useComponentSize} from "@hooks/useComponentSize";
import {HasPermission} from "@components/Guard/HasPermission";
import {Permissions} from "@shared/auth/permissions";

const formatDate = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleDateString("ru-RU", {day: "2-digit", month: "2-digit", year: "numeric"});
};

const toInputDate = (iso: string) => iso ? iso.substring(0, 10) : "";

interface PromotionFormState {
    name: string;
    description: string;
    discountPercent: string;
    validFrom: string;
    validTo: string;
    isActive: boolean;
}

const emptyForm: PromotionFormState = {
    name: "",
    description: "",
    discountPercent: "",
    validFrom: "",
    validTo: "",
    isActive: true,
};

export const PromotionsPage = () => {
    const {sizes} = useComponentSize();
    const {data: promotions = [], isLoading, error} = useGetAllPromotionsQuery();
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const [createPromotion] = useCreatePromotionMutation();
    const [updatePromotion] = useUpdatePromotionMutation();
    const [deletePromotion] = useDeletePromotionMutation();
    const [activatePromotion] = useActivatePromotionMutation();
    const [deactivatePromotion] = useDeactivatePromotionMutation();

    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingPromotion, setEditingPromotion] = useState<Promotion | null>(null);
    const [form, setForm] = useState<PromotionFormState>(emptyForm);
    const [mutationError, setMutationError] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);

    const openCreate = useCallback(() => {
        setEditingPromotion(null);
        setForm(emptyForm);
        setMutationError(null);
        setDialogOpen(true);
    }, []);

    const openEdit = useCallback((p: Promotion) => {
        setEditingPromotion(p);
        setForm({
            name: p.name,
            description: p.description ?? "",
            discountPercent: p.discountPercent != null ? String(p.discountPercent) : "",
            validFrom: toInputDate(p.validFrom),
            validTo: toInputDate(p.validTo),
            isActive: p.isActive,
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
                discountPercent: form.discountPercent ? parseFloat(form.discountPercent) : undefined,
                validFrom: form.validFrom,
                validTo: form.validTo,
                isActive: form.isActive,
            };
            if (editingPromotion) {
                await updatePromotion({promotionId: editingPromotion.promotionId, ...body}).unwrap();
            } else {
                await createPromotion(body).unwrap();
            }
            setDialogOpen(false);
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось сохранить акцию");
        } finally {
            setSaving(false);
        }
    }, [form, editingPromotion, createPromotion, updatePromotion]);

    const handleDelete = useCallback(async (promotionId: string) => {
        try {
            await deletePromotion(promotionId).unwrap();
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось удалить акцию");
        }
    }, [deletePromotion]);

    const handleToggleActive = useCallback(async (p: Promotion) => {
        try {
            if (p.isActive) {
                await deactivatePromotion(p.promotionId).unwrap();
            } else {
                await activatePromotion(p.promotionId).unwrap();
            }
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось изменить статус");
        }
    }, [activatePromotion, deactivatePromotion]);

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        name: {minWidth: 150, defaultWidth: 250},
        discount: {minWidth: 80, defaultWidth: 120},
        period: {minWidth: 150, defaultWidth: 220},
        status: {minWidth: 100, defaultWidth: 150},
        actions: {minWidth: 90, defaultWidth: 100},
    }), []);

    const columns: TableColumnDefinition<Promotion>[] = useMemo(() => [
        createTableColumn<Promotion>({
            columnId: "name",
            compare: (a, b) => a.name.localeCompare(b.name),
            renderHeaderCell: () => "Акция",
            renderCell: (promo) => (
                <TableCellLayout truncate>
                    <div>
                        <Body1 block>{promo.name}</Body1>
                        {promo.description && <Body2 block className="text-gray-500">{promo.description}</Body2>}
                    </div>
                </TableCellLayout>
            ),
        }),
        createTableColumn<Promotion>({
            columnId: "discount",
            compare: (a, b) => (a.discountPercent ?? 0) - (b.discountPercent ?? 0),
            renderHeaderCell: () => "Скидка",
            renderCell: (promo) => (
                <TableCellLayout truncate>
                    {promo.discountPercent != null
                        ? <Badge appearance="outline">{promo.discountPercent}%</Badge>
                        : "—"}
                </TableCellLayout>
            ),
        }),
        createTableColumn<Promotion>({
            columnId: "period",
            compare: (a, b) => a.validFrom.localeCompare(b.validFrom),
            renderHeaderCell: () => "Период",
            renderCell: (promo) => (
                <TableCellLayout truncate>
                    {formatDate(promo.validFrom)} — {formatDate(promo.validTo)}
                </TableCellLayout>
            ),
        }),
        createTableColumn<Promotion>({
            columnId: "status",
            compare: (a, b) => Number(a.isActive) - Number(b.isActive),
            renderHeaderCell: () => "Статус",
            renderCell: (promo) => (
                <HasPermission anyOf={[Permissions.VenuePromotionActivate, Permissions.VenuePromotionDeactivate]} fallback={
                    <Badge appearance="tint" color={promo.isActive ? "success" : "warning"}>
                        {promo.isActive ? "Активна" : "Неактивна"}
                    </Badge>
                }>
                    <Switch
                        checked={promo.isActive}
                        onChange={() => handleToggleActive(promo)}
                        label={promo.isActive ? "Активна" : "Неактивна"}
                    />
                </HasPermission>
            ),
        }),
        createTableColumn<Promotion>({
            columnId: "actions",
            compare: () => 0,
            renderHeaderCell: () => "Действия",
            renderCell: (promo) => (
                <div className="flex gap-1">
                    <HasPermission can={Permissions.VenuePromotionUpdate}>
                        <Button appearance="subtle" icon={<Edit20Regular />} onClick={() => openEdit(promo)} />
                    </HasPermission>
                    <HasPermission can={Permissions.VenuePromotionDelete}>
                        <Button appearance="subtle" icon={<Delete20Regular />} onClick={() => handleDelete(promo.promotionId)} />
                    </HasPermission>
                </div>
            ),
        }),
    ], [sizes, handleToggleActive, openEdit, handleDelete]);

    if (isLoading) {
        return <div className="flex justify-center p-12"><Spinner label="Загрузка акций..." /></div>;
    }

    if (queryError) {
        return (
            <MessageBar intent="error" className="mb-4">
                <MessageBarBody>{queryError}</MessageBarBody>
            </MessageBar>
        );
    }

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Акции</Title2>
                    <Body2 block>{promotions.length} акций</Body2>
                </div>
                <HasPermission can={Permissions.VenuePromotionCreate}>
                    <Button appearance="primary" size={sizes.button} icon={<Add20Regular />} onClick={openCreate}>
                        Добавить акцию
                    </Button>
                </HasPermission>
            </div>

            {mutationError && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{mutationError}</MessageBarBody>
                </MessageBar>
            )}

            <Card className="overflow-x-auto" size={sizes.card}>
                <DataTable
                    items={promotions}
                    columns={columns}
                    getRowId={(p) => p.promotionId}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <Dialog open={dialogOpen} onOpenChange={(_, d) => setDialogOpen(d.open)}>
                <DialogSurface>
                    <DialogBody>
                        <DialogTitle>{editingPromotion ? "Редактировать акцию" : "Новая акция"}</DialogTitle>
                        <DialogContent className="flex flex-col gap-4">
                            <Field label="Название" required>
                                <Input value={form.name} onChange={(_, d) => setForm(f => ({...f, name: d.value}))} />
                            </Field>
                            <Field label="Описание">
                                <Input value={form.description} onChange={(_, d) => setForm(f => ({...f, description: d.value}))} />
                            </Field>
                            <Field label="Скидка (%)">
                                <Input type="number" value={form.discountPercent} onChange={(_, d) => setForm(f => ({...f, discountPercent: d.value}))} />
                            </Field>
                            <Field label="Действует с" required>
                                <Input type="date" value={form.validFrom} onChange={(_, d) => setForm(f => ({...f, validFrom: d.value}))} />
                            </Field>
                            <Field label="Действует до" required>
                                <Input type="date" value={form.validTo} onChange={(_, d) => setForm(f => ({...f, validTo: d.value}))} />
                            </Field>
                            <Switch
                                checked={form.isActive}
                                onChange={(_, d) => setForm(f => ({...f, isActive: d.checked}))}
                                label="Активна"
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
                            <Button appearance="primary" onClick={handleSave} disabled={saving || !form.name || !form.validFrom || !form.validTo}>
                                {saving ? <Spinner size="tiny" /> : (editingPromotion ? "Сохранить" : "Создать")}
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>
        </div>
    );
};
