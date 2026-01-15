import {
    DataGrid,
    DataGridBody,
    DataGridCell,
    DataGridHeader,
    DataGridHeaderCell,
    DataGridRow,
    Spinner,
    Text,
    tokens,
} from "@fluentui/react-components";
import type { TableColumnDefinition, TableRowId } from "@fluentui/react-components";
import { useMemo } from "react";
import type { ReactNode } from "react";

export type DataTableProps<TItem> = {
    items: TItem[];
    columns: TableColumnDefinition<TItem>[];
    getRowId?: (item: TItem) => TableRowId;
    loading?: boolean;
    emptyMessage?: string;
    ariaLabel?: string;
    className?: string;
};

export function DataTable<TItem>({
    items,
    columns,
    getRowId,
    loading = false,
    emptyMessage = "Нет данных",
    ariaLabel = "Data table",
    className,
}: DataTableProps<TItem>) {
    const isEmpty = !loading && items.length === 0;

    const emptyStyle = useMemo(
        () => ({
            color: tokens.colorNeutralForeground2,
        }),
        []
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
                <Text style={emptyStyle}>{emptyMessage}</Text>
            </div>
        );
    }

    return (
        <DataGrid
            className={className}
            items={items}
            columns={columns}
            getRowId={getRowId}
            aria-label={ariaLabel}
        >
            <DataGridHeader>
                <DataGridRow>
                    {({ renderHeaderCell }: { renderHeaderCell: () => ReactNode }) => (
                        <DataGridHeaderCell>{renderHeaderCell()}</DataGridHeaderCell>
                    )}
                </DataGridRow>
            </DataGridHeader>

            <DataGridBody>
                {({ item, rowId }) => (
                    <DataGridRow key={rowId}>
                        {({ renderCell }) => <DataGridCell>{renderCell(item)}</DataGridCell>}
                    </DataGridRow>
                )}
            </DataGridBody>
        </DataGrid>
    );
}
