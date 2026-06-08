import { NO_DATA } from "@shared/const/placeholders";
import { useCallback, useMemo, useState } from "react";
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
    Spinner,
    Switch,
    Title2,
    createTableColumn,
    TableCellLayout,
    Dropdown,
    Option,
    RadioGroup,
    Radio,
    Caption1,
} from "@fluentui/react-components";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { Add20Regular, Delete20Regular, Edit20Regular, ArrowClockwise20Regular } from "@fluentui/react-icons";
import { DismissableError } from "@components/DismissableError/DismissableError";
import {
    useGetAllPromotionsQuery,
    useGetPromotionsPageQuery,
    useCreatePromotionMutation,
    useUpdatePromotionMutation,
    useDeletePromotionMutation,
    useActivatePromotionMutation,
    useDeactivatePromotionMutation,
    useGetAllTariffsQuery,
} from "@store/api/venueApi";
import type { Promotion } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { useComponentSize } from "@hooks/useComponentSize";
import { usePermissions } from "@hooks/usePermissions";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions, type Permission } from "@shared/auth/permissions";
import { RequirePermission } from "@app/components/RequirePermission/RequirePermission";
import { formatDateTime } from "@utility/formatDate";
import { usePagination } from "@app/hooks/usePagination";
import { PageLoader } from "@components/PageLoader/PageLoader";
import { CURRENCY_SYMBOL } from "@shared/const/currency";

const formatDate = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleDateString("ru-RU", { day: "2-digit", month: "2-digit", year: "numeric" });
};

const toInputDate = (iso: string) => iso ? iso.substring(0, 10) : "";

interface PromotionFormState {
    name: string;
    description: string;
    discountPercent: string;
    validFrom: string;
    validTo: string;
    isActive: boolean;
    type: number;
    tariffId: string;
}

const emptyForm: PromotionFormState = {
    name: "",
    description: "",
    discountPercent: "",
    validFrom: "",
    validTo: "",
    isActive: true,
    type: 1,
    tariffId: "",
};

export const PromotionsPage = () => {
    const { sizes } = useComponentSize();
    const { has } = usePermissions();
    const { page: currentPage, size: pageSize, setPage: setCurrentPage, setSize: setPageSize } = usePagination("adminPromotions");

    const { data, isLoading, error, refetch } = useGetPromotionsPageQuery(
        { page: currentPage, pageSize },
        { refetchOnMountOrArgChange: true }
    );
    const { data: allPromotions = [] } = useGetAllPromotionsQuery();
    const { data: tariffs = [] } = useGetAllTariffsQuery();

    const promotions = data?.items ?? [];
    const totalCount = data?.metadata?.totalCount ?? 0;
    const totalPages = data?.metadata?.totalPages ?? 1;
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

    const hasActiveGlobal = useMemo(() => {
        return allPromotions.some(p => p.type === 1 && p.isActive && p.promotionId !== editingPromotion?.promotionId);
    }, [allPromotions, editingPromotion]);

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
            type: p.type || 1,
            tariffId: p.tariffId ?? "",
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
                type: form.type,
                tariffId: form.type === 2 && form.tariffId ? form.tariffId : undefined,
            };
            if (editingPromotion) {
                await updatePromotion({ promotionId: editingPromotion.promotionId, ...body }).unwrap();
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
        name: { minWidth: 150, defaultWidth: 250 },
        discount: { minWidth: 80, defaultWidth: 120 },
        type: { minWidth: 100, defaultWidth: 150 },
        period: { minWidth: 150, defaultWidth: 220 },
        condition: { minWidth: 120, defaultWidth: 160 },
        status: { minWidth: 100, defaultWidth: 150 },
        actions: { minWidth: 90, defaultWidth: 100 },
    }), []);

    const columns: TableColumnDefinition<Promotion>[] = useMemo(() => {
        const allColumns: (TableColumnDefinition<Promotion> & { permission?: string })[] = [
            createTableColumn<Promotion>({
                columnId: "name",
                compare: (a, b) => a.name.localeCompare(b.name),
                renderHeaderCell: () => "Акция",
                renderCell: (promo) => (
                    <TableCellLayout truncate>
                        <div className="flex flex-col">
                            <Body1>{promo.name}</Body1>
                            {promo.description && <Body2 className="text-(--colorNeutralForeground3)">{promo.description}</Body2>}
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
                            : NO_DATA}
                    </TableCellLayout>
                ),
            }),
            createTableColumn<Promotion>({
                columnId: "type",
                compare: (a, b) => a.type - b.type,
                renderHeaderCell: () => "Тип",
                renderCell: (promo) => (
                    <TableCellLayout truncate>
                        {promo.type === 1 ? (
                            <Badge appearance="filled" color="brand">Глобальная</Badge>
                        ) : promo.type === 2 ? (
                            <div className="flex flex-col">
                                <Badge appearance="outline" color="informative">Для тарифа</Badge>
                                <Caption1 className="text-(--colorNeutralForeground3) truncate mt-1">
                                    {tariffs.find(t => t.tariffId.toLowerCase() === promo.tariffId?.toLowerCase())?.name || (tariffs.length === 0 ? "Загрузка..." : "Неизвестный тариф")}
                                </Caption1>
                            </div>
                        ) : (
                            <Badge appearance="outline" color="danger">Черновик</Badge>
                        )}
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
                columnId: "condition",
                compare: (a, b) => {
                    const aExp = new Date(a.validTo) < new Date();
                    const bExp = new Date(b.validTo) < new Date();
                    return Number(aExp) - Number(bExp);
                },
                renderHeaderCell: () => "Состояние",
                renderCell: (promo) => {
                    const isExpired = new Date(promo.validTo) < new Date();
                    const isNotStarted = new Date(promo.validFrom) > new Date();

                    if (isExpired) return <Badge color="danger" appearance="tint">Истекла</Badge>;
                    if (isNotStarted) return <Badge color="warning" appearance="tint">Ожидает</Badge>;
                    return <Badge color="success" appearance="tint">Актуальна</Badge>;
                },
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
                            <Button appearance="subtle" size={sizes.button} icon={<Edit20Regular />} onClick={() => openEdit(promo)} />
                        </HasPermission>
                        <HasPermission can={Permissions.VenuePromotionDelete}>
                            <Button appearance="subtle" size={sizes.button} icon={<Delete20Regular />} onClick={() => handleDelete(promo.promotionId)} />
                        </HasPermission>
                    </div>
                ),
            }),
        ];

        return allColumns.filter(col => !col.permission || has(col.permission as Permission));
    }, [handleToggleActive, openEdit, handleDelete, has, tariffs, sizes.button]);

    if (isLoading) {
        return <PageLoader label="Загрузка акций..." />;
    }

    return (
        <RequirePermission can={Permissions.VenuePromotionRead}>
            <div className="flex flex-col gap-2">
                <div className="flex items-center justify-between flex-wrap gap-4">
                    <div className="flex flex-col">
                        <Title2>Акции</Title2>
                        <Body2>{totalCount} созданных акций</Body2>
                    </div>

                    <div className="flex gap-2">
                        <Button appearance="subtle" size={sizes.button} icon={<ArrowClockwise20Regular />} onClick={() => refetch()} />
                        <HasPermission can={Permissions.VenuePromotionCreate}>
                            <Button appearance="primary" size={sizes.button} icon={<Add20Regular />} onClick={openCreate}>
                                Добавить акцию
                            </Button>
                        </HasPermission>
                    </div>
                </div>

                <DismissableError error={queryError} />
                <DismissableError error={mutationError} />

                <Card className="overflow-x-auto" size={sizes.card}>
                    <DataTable
                        items={promotions}
                        columns={columns}
                        getRowId={(p) => p.promotionId}
                        loading={isLoading}
                        columnSizingOptions={columnSizingOptions}
                    />
                </Card>

                <div className="flex items-center justify-between flex-wrap gap-2">
                    <Body1>Показано {promotions.length} из {totalCount}</Body1>
                    <Pagination
                        currentPage={currentPage}
                        totalPages={totalPages}
                        onPageChange={setCurrentPage}
                        pageSize={pageSize}
                        onPageSizeChange={setPageSize}
                        totalCount={totalCount}
                    />
                </div>
            </div>

            <Dialog open={dialogOpen} onOpenChange={(_, d) => setDialogOpen(d.open)}>
                <DialogSurface>
                    <DialogBody>
                        <DialogTitle>{editingPromotion ? "Редактировать акцию" : "Новая акция"}</DialogTitle>
                        <DialogContent className="flex flex-col gap-4">
                            <Field label="Название" required>
                                <Input value={form.name} onChange={(_, d) => setForm(f => ({ ...f, name: d.value }))} size={sizes.input} />
                            </Field>
                            <Field label="Описание">
                                <Input value={form.description} onChange={(_, d) => setForm(f => ({ ...f, description: d.value }))} size={sizes.input} />
                            </Field>
                            <Field label="Скидка (%)">
                                <Input type="number" value={form.discountPercent} onChange={(_, d) => setForm(f => ({ ...f, discountPercent: d.value }))} size={sizes.input} />
                            </Field>
                            <Field label="Тип акции">
                                <RadioGroup
                                    layout="horizontal"
                                    value={form.type.toString()}
                                    onChange={(_, data) => setForm(f => ({ ...f, type: parseInt(data.value) }))}
                                >
                                    <Radio value="1" label="Глобальная" />
                                    <Radio value="2" label="Для тарифа" />
                                </RadioGroup>
                                {form.type === 1 && form.isActive && hasActiveGlobal && (
                                    <Caption1 className="text-(--colorPaletteRedForeground1) mt-1 flex">
                                        Внимание: уже существует другая активная глобальная акция. Сначала деактивируйте её, либо сохраните эту как неактивную.
                                    </Caption1>
                                )}
                            </Field>
                            {form.type === 2 && (
                                <Field label="Тариф" required>
                                    <Dropdown
                                        placeholder="Выберите тариф"
                                        value={tariffs.find(t => t.tariffId.toLowerCase() === form.tariffId.toLowerCase())?.name ?? ""}
                                        selectedOptions={form.tariffId ? [form.tariffId] : []}
                                        onOptionSelect={(_, data) => setForm(f => ({ ...f, tariffId: data.optionValue ?? "" }))}
                                        size={sizes.dropdown}
                                    >
                                        {tariffs.map(t => (
                                            <Option key={t.tariffId} value={t.tariffId}>
                                                {t.name} ({t.pricePerMinute} {CURRENCY_SYMBOL}/мин)
                                            </Option>
                                        ))}
                                    </Dropdown>
                                </Field>
                            )}
                            <Field label="Действует с" required>
                                <Input type="date" value={form.validFrom} onChange={(_, d) => setForm(f => ({ ...f, validFrom: d.value }))} size={sizes.input} />
                            </Field>
                            <Field label="Действует до" required>
                                <Input type="date" value={form.validTo} onChange={(_, d) => setForm(f => ({ ...f, validTo: d.value }))} size={sizes.input} />
                            </Field>
                            <Switch
                                checked={form.isActive}
                                onChange={(_, d) => setForm(f => ({ ...f, isActive: d.checked }))}
                                label="Активна"
                            />
                            <DismissableError error={mutationError} />
                        </DialogContent>
                        <DialogActions>
                            <DialogTrigger disableButtonEnhancement>
                                <Button appearance="secondary" size={sizes.button}>Отмена</Button>
                            </DialogTrigger>
                            <Button appearance="primary" onClick={handleSave} disabled={saving || !form.name || !form.validFrom || !form.validTo || (form.type === 1 && form.isActive && hasActiveGlobal)} size={sizes.button}>
                                {saving ? <Spinner size="tiny" /> : (editingPromotion ? "Сохранить" : "Создать")}
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>
        </RequirePermission>
    );
};
