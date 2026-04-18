import {
    DataGrid,
    DataGridBody,
    DataGridCell,
    DataGridHeader,
    DataGridHeaderCell,
    DataGridRow,
    Menu,
    MenuList,
    MenuPopover,
    MenuTrigger,
    MenuItem,
    Spinner,
    TableCellLayout,
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableRowId, DataGridProps, TableColumnSizingOptions} from "@fluentui/react-components";
import {useRef, useMemo, useCallback} from "react";
import type {ReactNode} from "react";
import {EmptyState} from "@components/EmptyState/EmptyState";

export type DataTableProps<TItem> = {
    items: TItem[];
    columns: TableColumnDefinition<TItem>[];
    getRowId?: (item: TItem) => TableRowId;
    loading?: boolean;
    emptyMessage?: string;
    ariaLabel?: string;
    className?: string;
    sortable?: boolean;
    selectionMode?: "multiselect" | "single";
    resizableColumns?: boolean;
    columnSizingOptions?: TableColumnSizingOptions;
    onSelectionChange?: DataGridProps["onSelectionChange"];
    selectedItems?: Set<TableRowId>;
    size?: "small" | "medium" | "extra-small";
};

export function DataTable<TItem>({
    items,
    columns,
    getRowId,
    loading = false,
    emptyMessage = "Нет данных",
    ariaLabel = "Data table",
    className,
    sortable = true,
    selectionMode = "multiselect",
    resizableColumns = true,
    columnSizingOptions,
    onSelectionChange,
    selectedItems,
    size = "medium",
}: DataTableProps<TItem>) {
    const isEmpty = !loading && items.length === 0;
    const refMap = useRef<Record<string, HTMLElement | null>>({});

    const selectionProps = useMemo(() => {
        const props: Partial<DataGridProps> = {};
        if (selectionMode) {
            props.selectionMode = selectionMode;
            props.subtleSelection = true;
        }
        if (selectedItems) props.selectedItems = selectedItems;
        if (onSelectionChange) props.onSelectionChange = onSelectionChange;
        return props;
    }, [selectionMode, selectedItems, onSelectionChange]);

    const renderHeaderRow = useCallback(
        ({renderHeaderCell, columnId}: {renderHeaderCell: () => ReactNode; columnId: string}, dataGrid: any) => {
            if (resizableColumns && dataGrid.resizableColumns) {
                return (
                    <Menu openOnContext>
                        <MenuTrigger>
                            <DataGridHeaderCell
                                ref={(el: HTMLElement | null) => {
                                    refMap.current[columnId] = el;
                                }}
                            >
                                {renderHeaderCell()}
                            </DataGridHeaderCell>
                        </MenuTrigger>
                        <MenuPopover>
                            <MenuList>
                                <MenuItem
                                    onClick={dataGrid.columnSizing_unstable.enableKeyboardMode(columnId)}
                                >
                                    Изменить ширину колонки
                                </MenuItem>
                            </MenuList>
                        </MenuPopover>
                    </Menu>
                );
            }
            return <DataGridHeaderCell>{renderHeaderCell()}</DataGridHeaderCell>;
        },
        [resizableColumns]
    );

    if (loading) {
        return (
            <div className={className}>
                <Spinner label="Загрузка..." />
            </div>
        );
    }

    if (isEmpty) {
        return (
            <div className={className}>
                <EmptyState title={emptyMessage} />
            </div>
        );
    }

    return (
        <div style={{ overflowX: "auto" }}>
            <DataGrid
                className={className}
                items={items}
                columns={columns}
                getRowId={getRowId}
                aria-label={ariaLabel}
                sortable={sortable}
                resizableColumns={resizableColumns}
                columnSizingOptions={columnSizingOptions}
                focusMode="composite"
                size={size}
                {...selectionProps}
                resizableColumnsOptions={{
                    autoFitColumns: true,
                }}
            >
                <DataGridHeader>
                    <DataGridRow
                        selectionCell={selectionMode ? {checkboxIndicator: {"aria-label": "Выбрать все"}} : undefined}
                    >
                        {renderHeaderRow as any}
                    </DataGridRow>
                </DataGridHeader>

                <DataGridBody<TItem>>
                    {({item, rowId}) => (
                        <DataGridRow<TItem>
                            key={rowId}
                            selectionCell={selectionMode ? {checkboxIndicator: {"aria-label": "Выбрать строку"}} : undefined}
                        >
                            {({renderCell}) => <DataGridCell>{renderCell(item)}</DataGridCell>}
                        </DataGridRow>
                    )}
                </DataGridBody>
            </DataGrid>
        </div>
    );
}

export {TableCellLayout};
