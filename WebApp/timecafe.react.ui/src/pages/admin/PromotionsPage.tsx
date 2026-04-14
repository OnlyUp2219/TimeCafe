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
} from "@fluentui/react-components";
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

    const columns = useMemo(() => [
        {
            columnId: "name",
            renderHeaderCell: () => "Акция",
            compare: (a: Promotion, b: Promotion) => a.name.localeCompare(b.name),
        },
        {
            columnId: "discount",
            renderHeaderCell: () => "Скидка",
            compare: (a: Promotion, b: Promotion) => (a.discountPercent ?? 0) - (b.discountPercent ?? 0),
        },
        {
            columnId: "period",
            renderHeaderCell: () => "Период",
            compare: (a: Promotion, b: Promotion) => a.validFrom.localeCompare(b.validFrom),
        },
        {
            columnId: "status",
            renderHeaderCell: () => "Статус",
            compare: (a: Promotion, b: Promotion) => Number(a.isActive) - Number(b.isActive),
        },
        {
            columnId: "actions",
            renderHeaderCell: () => "Действия",
            compare: () => 0,
        },
    ], []);

    const renderCell = (promo: Promotion, columnId: string) => {
        switch (columnId) {
            case "name":
                return (
                    <div>
                        <Body1 block>{promo.name}</Body1>
                        {promo.description && <Body2 block className="text-gray-500">{promo.description}</Body2>}
                    </div>
                );
            case "discount":
                return promo.discountPercent != null
                    ? <Badge appearance="outline">{promo.discountPercent}%</Badge>
                    : "—";
            case "period":
                return `${formatDate(promo.validFrom)} — ${formatDate(promo.validTo)}`;
            case "status":
                return (
                    <Switch
                        checked={promo.isActive}
                        onChange={() => handleToggleActive(promo)}
                        label={promo.isActive ? "Активна" : "Неактивна"}
                    />
                );
            case "actions":
                return (
                    <div className="flex gap-1">
                        <Button appearance="subtle" size="small" icon={<Edit20Regular />} onClick={() => openEdit(promo)} />
                        <Button appearance="subtle" size="small" icon={<Delete20Regular />} onClick={() => handleDelete(promo.promotionId)} />
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
                    <Title2>Акции</Title2>
                    <Body2 block>{promotions.length} акций</Body2>
                </div>
                <Button appearance="primary" size="large" icon={<Add20Regular />} onClick={openCreate}>
                    Добавить акцию
                </Button>
            </div>

            {(queryError || mutationError) && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{queryError || mutationError}</MessageBarBody>
                </MessageBar>
            )}

            <Card className="overflow-x-auto" size="large">
                <DataTable
                    items={promotions}
                    columns={columns.map(col => ({
                        ...col,
                        renderCell: (item: Promotion) => renderCell(item, col.columnId),
                    }))}
                    getRowId={(p) => p.promotionId}
                    loading={isLoading}
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
