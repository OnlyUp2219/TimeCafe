import { useCallback, useMemo, useState } from "react";
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
    Dropdown,
    Option,
    Tooltip,
    MessageBarTitle,
} from "@fluentui/react-components";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { Add20Regular, Delete20Regular, Edit20Regular, ArrowClockwise20Regular, Info20Regular } from "@fluentui/react-icons";
import {
    useGetTariffsPageQuery,
    useCreateTariffMutation,
    useUpdateTariffMutation,
    useDeleteTariffMutation,
    useActivateTariffMutation,
    useDeactivateTariffMutation,
    useGetAllThemesQuery,
    useGetAllPromotionsQuery,
} from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import type { TariffWithTheme } from "@app-types/tariffWithTheme";
import { BillingType } from "@app-types/tariff";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { useComponentSize } from "@hooks/useComponentSize";
import { usePermissions } from "@hooks/usePermissions";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions } from "@shared/auth/permissions";
import { CURRENCY_SYMBOL } from "@shared/const/currency";

const billingTypeLabel = (bt: number) => bt === BillingType.Hourly ? "Почасовой" : "Поминутный";

interface TariffFormState {
    name: string;
    description: string;
    pricePerMinute: string;
    billingType: 1 | 2;
    themeId: string;
    isActive: boolean;
}

const emptyForm: TariffFormState = {
    name: "",
    description: "",
    pricePerMinute: "",
    billingType: 2,
    themeId: "",
    isActive: true,
};

const calcPerHour = (perMinute: string): string => {
    const v = parseFloat(perMinute);
    if (!v || isNaN(v)) return "";
    return (v * 60).toFixed(2);
};

import { usePagination } from "@hooks/usePagination";

export const TariffsPage = () => {
    const { sizes } = useComponentSize();
    const { has } = usePermissions();
    const { page: currentPage, size: pageSize, setPage: setCurrentPage, setSize: setPageSize } = usePagination("adminTariffs");
    const { data, isLoading, error, refetch } = useGetTariffsPageQuery(
        { pageNumber: currentPage, pageSize },
        { refetchOnMountOrArgChange: true }
    );
    const { data: themes = [], isLoading: themesLoading } = useGetAllThemesQuery();
    const { data: promotions = [] } = useGetAllPromotionsQuery();

    const tariffs = data?.tariffs ?? [];
    const totalCount = data?.totalCount ?? 0;
    const maxCap = data?.maxTotalDiscountPercent;
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
            themeId: t.themeId ?? "",
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
                themeId: form.themeId || undefined,
                isActive: form.isActive,
            };
            if (editingTariff) {
                await updateTariff({ tariffId: editingTariff.tariffId, ...body }).unwrap();
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
        name: { minWidth: 150, defaultWidth: 250, idealWidth: 300 },
        price: { minWidth: 80, defaultWidth: 140, idealWidth: 160 },
        billingType: { minWidth: 100, defaultWidth: 140, idealWidth: 160 },
        theme: { minWidth: 80, defaultWidth: 140, idealWidth: 160 },
        status: { minWidth: 100, defaultWidth: 150, idealWidth: 180 },
        actions: { minWidth: 140, defaultWidth: 180, idealWidth: 160 },
    }), []);

    const columns: TableColumnDefinition<TariffWithTheme>[] = useMemo(() => {
        const allColumns: (TableColumnDefinition<TariffWithTheme> & { permission?: string })[] = [
            createTableColumn<TariffWithTheme>({
                columnId: "name",
                compare: (a, b) => a.name.localeCompare(b.name),
                renderHeaderCell: () => "Тариф",
                renderCell: (tariff) => (
                    <TableCellLayout
                        truncate
                        media={
                            <Avatar
                                name={tariff.name}
                                initials={tariff.themeEmoji || tariff.name.substring(0, 1).toUpperCase()}
                            />
                        }
                    >
                        <div className="flex flex-col">
                            <Body1 >{tariff.name}</Body1>
                            {tariff.description && <Body2 className="text-gray-500 !line-clamp-2">{tariff.description}</Body2>}
                        </div>
                    </TableCellLayout>
                ),
            }),
            createTableColumn<TariffWithTheme>({
                columnId: "price",
                compare: (a, b) => a.pricePerMinute - b.pricePerMinute,
                renderHeaderCell: () => "Цена",
                renderCell: (tariff) => {
                    const activePromos = promotions.filter(p => p.isActive && new Date(p.validFrom) <= new Date() && new Date(p.validTo) >= new Date());
                    const globalPromo = activePromos.filter(p => p.type === 1).sort((a, b) => (b.discountPercent ?? 0) - (a.discountPercent ?? 0))[0];
                    const tariffPromo = activePromos.filter(p => p.type === 2 && p.tariffId === tariff.tariffId).sort((a, b) => (b.discountPercent ?? 0) - (a.discountPercent ?? 0))[0];
                    const bestPromo = Math.max(globalPromo?.discountPercent ?? 0, tariffPromo?.discountPercent ?? 0);


                    const appliedDiscount = maxCap !== undefined ? Math.min(bestPromo, maxCap) : bestPromo;
                    const hasDiscount = appliedDiscount > 0;
                    const discountedPrice = hasDiscount ? tariff.pricePerMinute * (1 - appliedDiscount / 100) : tariff.pricePerMinute;

                    return (
                        <TableCellLayout truncate>
                            <div>
                                <Body1 block>
                                    {hasDiscount ? (
                                        <span className="flex items-center gap-2">
                                            <span className="line-through text-gray-400 text-xs">{tariff.pricePerMinute} {CURRENCY_SYMBOL}</span>
                                            <span className="text-red-500 font-semibold">{discountedPrice.toFixed(2)} {CURRENCY_SYMBOL}/мин</span>
                                        </span>
                                    ) : (
                                        <span>{tariff.pricePerMinute} {CURRENCY_SYMBOL}/мин</span>
                                    )}
                                </Body1>
                                <Caption1 block style={{ color: "var(--colorNeutralForeground3)" }}>
                                    {(discountedPrice * 60).toFixed(2)} {CURRENCY_SYMBOL}/час
                                    {hasDiscount && (
                                        <Tooltip content={maxCap !== undefined && bestPromo > maxCap ? `Акция ${bestPromo}% ограничена системным лимитом ${maxCap}%` : "Применена лучшая акция"} relationship="label">
                                            <span className="ml-1 text-red-500 cursor-help">
                                                (-{appliedDiscount}%){maxCap !== undefined && bestPromo > maxCap && " *"}
                                            </span>
                                        </Tooltip>
                                    )}
                                </Caption1>
                            </div>
                        </TableCellLayout>
                    );
                },
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
                    <HasPermission anyOf={[Permissions.VenueTariffActivate, Permissions.VenueTariffDeactivate]} fallback={
                        <Badge appearance="tint" color={tariff.isActive ? "success" : "warning"}>
                            {tariff.isActive ? "Активен" : "Неактивен"}
                        </Badge>
                    }>
                        <Switch
                            checked={tariff.isActive}
                            onChange={() => handleToggleActive(tariff)}
                            label={tariff.isActive ? "Активен" : "Неактивен"}
                        />
                    </HasPermission>
                ),
            }),
            createTableColumn<TariffWithTheme>({
                columnId: "actions",
                compare: () => 0,
                renderHeaderCell: () => "Действия",
                renderCell: (tariff) => (
                    <div className="flex gap-1">
                        <HasPermission can={Permissions.VenueTariffUpdate}>
                            <Button appearance="subtle" icon={<Edit20Regular />} onClick={() => openEdit(tariff)} />
                        </HasPermission>
                        <HasPermission can={Permissions.VenueTariffDelete}>
                            <Button appearance="subtle" icon={<Delete20Regular />} onClick={() => handleDelete(tariff.tariffId)} />
                        </HasPermission>
                    </div>
                ),
            }),
        ];

        return allColumns.filter(col => !col.permission || has(col.permission as any));
    }, [sizes, handleToggleActive, openEdit, handleDelete, has, promotions, maxCap]);

    const perHour = calcPerHour(form.pricePerMinute);

    if (isLoading) {
        return <div className="flex justify-center p-12"><Spinner label="Загрузка тарифов..." /></div>;
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
            <div className="flex items-center justify-between flex-wrap gap-4">
                <div>
                    <Title2>Тарифы</Title2>
                    <Body2 block>{totalCount} тарифов</Body2>
                </div>
                <div className="flex gap-2">
                    <Button appearance="subtle" size={sizes.button} icon={<ArrowClockwise20Regular />} onClick={() => refetch()} />
                    <HasPermission can={Permissions.VenueTariffCreate}>
                        <Button appearance="primary" size={sizes.button} icon={<Add20Regular />} onClick={openCreate}>
                            Добавить тариф
                        </Button>
                    </HasPermission>
                </div>
            </div>

            <MessageBar intent="info" shape="square" icon={<Info20Regular />}>
                <MessageBarBody>
                    <MessageBarTitle>Информация о тарификации</MessageBarTitle>
                    <div className="flex flex-col gap-1">
                        <Body1>
                            Система выбирает лучшую из активных акций (Глобальная или Тарифная) и применяет её к базовой стоимости.
                        </Body1>
                        <Caption1 italic>
                            {maxCap !== undefined ? (
                                <>* Итоговая скидка ограничена системным лимитом <b>{maxCap}%</b>. При расчете визита к ней также плюсуется персональная скидка гостя.</>
                            ) : (
                                <>* Системный лимит скидки не определен (данные не загружены).</>
                            )}
                        </Caption1>
                    </div>
                </MessageBarBody>
            </MessageBar>

            {mutationError && (
                <MessageBar intent="error">
                    <MessageBarBody>{mutationError}</MessageBarBody>
                </MessageBar>
            )}

            <Card size={sizes.card}>
                <DataTable
                    items={tariffs}
                    columns={columns}
                    getRowId={(t) => t.tariffId}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <div className="flex items-center justify-between flex-wrap gap-2">
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
                                <Input value={form.name} onChange={(_, d) => setForm(f => ({ ...f, name: d.value }))} />
                            </Field>
                            <Field label="Описание">
                                <Input value={form.description} onChange={(_, d) => setForm(f => ({ ...f, description: d.value }))} />
                            </Field>
                            <Field label={`Цена за минуту (${CURRENCY_SYMBOL})`} required hint={perHour ? `≈ ${perHour} ${CURRENCY_SYMBOL}/час` : undefined}>
                                <Input
                                    type="number"
                                    value={form.pricePerMinute}
                                    onChange={(_, d) => setForm(f => ({ ...f, pricePerMinute: d.value }))}
                                />
                            </Field>
                            <Field label="Тип тарификации">
                                <div className="flex gap-4">
                                    <Button
                                        appearance={form.billingType === 2 ? "primary" : "secondary"}
                                        onClick={() => setForm(f => ({ ...f, billingType: 2 }))}
                                    >
                                        Поминутный
                                    </Button>
                                    <Button
                                        appearance={form.billingType === 1 ? "primary" : "secondary"}
                                        onClick={() => setForm(f => ({ ...f, billingType: 1 }))}
                                    >
                                        Почасовой
                                    </Button>
                                </div>
                            </Field>
                            <Field label="Тема оформления">
                                <Dropdown
                                    placeholder="Выберите тему"
                                    value={themes.find(t => t.themeId === form.themeId)?.name ?? (form.themeId === "" ? "Без темы" : "")}
                                    selectedOptions={form.themeId ? [form.themeId] : []}
                                    onOptionSelect={(_, data) => setForm(f => ({ ...f, themeId: data.optionValue ?? "" }))}
                                    disabled={themesLoading}
                                >
                                    <Option key="none" value="" text="Без темы">
                                        Без темы
                                    </Option>
                                    {themes.map((theme) => (
                                        <Option key={theme.themeId} value={theme.themeId} text={theme.name}>
                                            {theme.emoji} {theme.name}
                                        </Option>
                                    ))}
                                </Dropdown>
                            </Field>
                            <Switch
                                checked={form.isActive}
                                onChange={(_, d) => setForm(f => ({ ...f, isActive: d.checked }))}
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
