import { NO_DATA } from "@shared/const/placeholders";
import { useCallback, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
    Avatar,
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    Caption1,
    Switch,
    Title2,
    createTableColumn,
    TableCellLayout,
    Tooltip,
    MessageBar,
    MessageBarBody,
    MessageBarTitle,
} from "@fluentui/react-components";
import { DismissableError } from "@components/DismissableError/DismissableError";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { Add20Regular, Delete20Regular, Edit20Regular, ArrowClockwise20Regular, Info20Regular, Eye20Regular } from "@fluentui/react-icons";
import {
    useGetTariffsPageQuery,
    useDeleteTariffMutation,
    useActivateTariffMutation,
    useDeactivateTariffMutation,
    useGetAllPromotionsQuery,
    type Promotion,
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
import { Permissions, type Permission } from "@shared/auth/permissions";
import { RequirePermission } from "@app/components/RequirePermission/RequirePermission";
import { CURRENCY_SYMBOL } from "@shared/const/currency";
import { TariffDetailsDialog } from "@components/Tariff/TariffDetailsDialog";
import { PageLoader } from "@components/PageLoader/PageLoader";
import { usePagination } from "@hooks/usePagination";

const billingTypeLabel = (bt: number) => bt === BillingType.Hourly ? "Почасовой" : "Поминутный";




export const TariffsPage = () => {


    const navigate = useNavigate();
    const { sizes } = useComponentSize();
    const { has } = usePermissions();
    const { page: currentPage, size: pageSize, setPage: setCurrentPage, setSize: setPageSize } = usePagination("adminTariffs");
    const { data, isLoading, error, refetch } = useGetTariffsPageQuery(
        { page: currentPage, pageSize },
        { refetchOnMountOrArgChange: true }
    );
    const { data: promotions = [] } = useGetAllPromotionsQuery();

    const tariffs = data?.items ?? [];
    const totalCount = data?.metadata?.totalCount ?? 0;
    const maxCap = data?.maxTotalDiscountPercent;
    const totalPages = data?.metadata?.totalPages ?? 1;
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const [deleteTariff] = useDeleteTariffMutation();
    const [activateTariff] = useActivateTariffMutation();
    const [deactivateTariff] = useDeactivateTariffMutation();

    const [mutationError, setMutationError] = useState<string | null>(null);
    const [detailsTariff, setDetailsTariff] = useState<TariffWithTheme | null>(null);
    const [detailsOpen, setDetailsOpen] = useState(false);

    const openCreate = useCallback(() => {
        navigate("/admin/tariffs/create");
    }, [navigate]);

    const openEdit = useCallback((t: TariffWithTheme) => {
        navigate(`/admin/tariffs/${t.tariffId}/edit`);
    }, [navigate]);

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
        status: { minWidth: 150, defaultWidth: 250, idealWidth: 280 },
        actions: { minWidth: 160, defaultWidth: 180, idealWidth: 160 },
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
                            {tariff.description && <Body2 className="text-(--colorNeutralForeground3) line-clamp-2!">{tariff.description}</Body2>}
                        </div>
                    </TableCellLayout>
                ),
            }),
            createTableColumn<TariffWithTheme>({
                columnId: "price",
                compare: (a, b) => a.pricePerMinute - b.pricePerMinute,
                renderHeaderCell: () => "Цена",
                renderCell: (tariff) => {
                    const activePromos = promotions.filter((p: Promotion) => p.isActive && new Date(p.validFrom) <= new Date() && new Date(p.validTo) >= new Date());
                    const globalPromo = activePromos.filter((p: Promotion) => p.type === 1).sort((a: Promotion, b: Promotion) => (b.discountPercent ?? 0) - (a.discountPercent ?? 0))[0];
                    const tariffPromo = activePromos.filter((p: Promotion) => p.type === 2 && p.tariffId === tariff.tariffId).sort((a: Promotion, b: Promotion) => (b.discountPercent ?? 0) - (a.discountPercent ?? 0))[0];
                    const bestPromo = Math.max(globalPromo?.discountPercent ?? 0, tariffPromo?.discountPercent ?? 0);


                    const appliedDiscount = maxCap !== undefined ? Math.min(bestPromo, maxCap) : bestPromo;
                    const hasDiscount = appliedDiscount > 0;
                    const discountedPrice = hasDiscount ? tariff.pricePerMinute * (1 - appliedDiscount / 100) : tariff.pricePerMinute;

                    return (
                        <TableCellLayout truncate>
                            <div className="flex flex-col">
                                <Body1>
                                    {hasDiscount ? (
                                        <span className="flex items-center gap-2">
                                            <span className="line-through text-(--colorNeutralForeground4) text-xs">{tariff.pricePerMinute} {CURRENCY_SYMBOL}</span>
                                            <span className="text-(--colorPaletteRedForeground1) font-semibold">{discountedPrice.toFixed(2)} {CURRENCY_SYMBOL}/мин</span>
                                        </span>
                                    ) : (
                                        <span>{tariff.pricePerMinute} {CURRENCY_SYMBOL}/мин</span>
                                    )}
                                </Body1>
                                <Caption1 style={{ color: "var(--colorNeutralForeground3)" }}>
                                    {(discountedPrice * 60).toFixed(2)} {CURRENCY_SYMBOL}/час
                                    {hasDiscount && (
                                        <Tooltip content={maxCap !== undefined && bestPromo > maxCap ? `Акция ${bestPromo}% ограничена системным лимитом ${maxCap}%` : "Применена лучшая акция"} relationship="label">
                                            <span className="ml-1 text-(--colorPaletteRedForeground1) cursor-help">
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
                    <TableCellLayout truncate>{tariff.themeName || NO_DATA}</TableCellLayout>
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
                        <Tooltip content="Просмотр деталей" relationship="label">
                            <Button
                                appearance="subtle"
                                icon={<Eye20Regular />}
                                onClick={() => {
                                    setDetailsTariff(tariff);
                                    setDetailsOpen(true);
                                }}
                            />
                        </Tooltip>
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

        return allColumns.filter(col => !col.permission || has(col.permission as Permission));
    }, [handleToggleActive, openEdit, handleDelete, has, promotions, maxCap]);

    if (isLoading) {
        return <PageLoader label="Загрузка тарифов..." />;
    }

    return (
        <RequirePermission can={Permissions.VenueTariffRead}>
            <div className="flex flex-col gap-2">
                <div className="flex items-center justify-between flex-wrap gap-4">
                    <div className="flex flex-col">
                        <Title2>Тарифы</Title2>
                        <Body2>{totalCount} тарифов</Body2>
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

                <DismissableError error={queryError} />
                <DismissableError error={mutationError} />

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

                <TariffDetailsDialog
                    open={detailsOpen}
                    onOpenChange={setDetailsOpen}
                    tariffId={detailsTariff?.tariffId ?? null}
                />
            </div>
        </RequirePermission>
    );
};


