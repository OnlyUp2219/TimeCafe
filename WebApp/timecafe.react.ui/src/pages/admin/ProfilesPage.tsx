import {useMemo, useState} from "react";
import {
    Avatar,
    Badge,
    Body1,
    Body2,
    Card,
    MessageBar,
    MessageBarBody,
    Title2,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {useGetProfilesPageQuery} from "@store/api/profileApi";
import type {Profile} from "@app-types/profile";
import {ProfileStatus} from "@app-types/profile";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {DataTable} from "@components/DataTable/DataTable";
import {Pagination} from "@components/Pagination/Pagination";
import {useComponentSize} from "@hooks/useComponentSize";

const statusLabel = (s: number) => {
    switch (s) {
        case ProfileStatus.Pending: return "Ожидает";
        case ProfileStatus.Completed: return "Заполнен";
        case ProfileStatus.Banned: return "Заблокирован";
        default: return "—";
    }
};

const statusColor = (s: number): "warning" | "success" | "danger" => {
    switch (s) {
        case ProfileStatus.Pending: return "warning";
        case ProfileStatus.Completed: return "success";
        case ProfileStatus.Banned: return "danger";
        default: return "warning";
    }
};

const genderLabel = (g: number) => {
    switch (g) {
        case 1: return "М";
        case 2: return "Ж";
        default: return "—";
    }
};

export const ProfilesPage = () => {
    const {sizes} = useComponentSize();
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(20);

    const {data, isLoading, error} = useGetProfilesPageQuery(
        {pageNumber: currentPage, pageSize},
        {refetchOnMountOrArgChange: true}
    );

    const profiles = data?.profiles ?? [];
    const totalCount = data?.totalCount ?? 0;
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        name: {minWidth: 150, defaultWidth: 220},
        email: {minWidth: 150, defaultWidth: 220},
        gender: {minWidth: 50, defaultWidth: 70},
        birthDate: {minWidth: 100, defaultWidth: 130},
        status: {minWidth: 100, defaultWidth: 140},
    }), []);

    const columns: TableColumnDefinition<Profile>[] = useMemo(() => [
        createTableColumn<Profile>({
            columnId: "name",
            compare: (a, b) => a.lastName.localeCompare(b.lastName),
            renderHeaderCell: () => "Имя",
            renderCell: (p) => (
                <TableCellLayout truncate media={<Avatar name={`${p.firstName} ${p.lastName}`} image={p.photoUrl ? {src: p.photoUrl} : undefined} />}>
                    <Body1>{p.lastName} {p.firstName} {p.middleName ?? ""}</Body1>
                </TableCellLayout>
            ),
        }),
        createTableColumn<Profile>({
            columnId: "email",
            compare: (a, b) => (a.email ?? "").localeCompare(b.email ?? ""),
            renderHeaderCell: () => "Email",
            renderCell: (p) => <TableCellLayout truncate>{p.email || "—"}</TableCellLayout>,
        }),
        createTableColumn<Profile>({
            columnId: "gender",
            compare: (a, b) => a.gender - b.gender,
            renderHeaderCell: () => "Пол",
            renderCell: (p) => <TableCellLayout truncate>{genderLabel(p.gender)}</TableCellLayout>,
        }),
        createTableColumn<Profile>({
            columnId: "birthDate",
            compare: (a, b) => (a.birthDate ?? "").localeCompare(b.birthDate ?? ""),
            renderHeaderCell: () => "Дата рождения",
            renderCell: (p) => <TableCellLayout truncate>{p.birthDate ? new Date(p.birthDate).toLocaleDateString("ru-RU") : "—"}</TableCellLayout>,
        }),
        createTableColumn<Profile>({
            columnId: "status",
            compare: (a, b) => a.profileStatus - b.profileStatus,
            renderHeaderCell: () => "Статус",
            renderCell: (p) => (
                <TableCellLayout truncate>
                    <Badge appearance="tint" color={statusColor(p.profileStatus)}>{statusLabel(p.profileStatus)}</Badge>
                </TableCellLayout>
            ),
        }),
    ], []);

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Профили</Title2>
                    <Body2 block>{totalCount} профилей</Body2>
                </div>
            </div>

            {queryError && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{queryError}</MessageBarBody>
                </MessageBar>
            )}

            <Card className="overflow-x-auto" size={sizes.card}>
                <DataTable
                    items={profiles}
                    columns={columns}
                    getRowId={(p) => p.email ?? `${p.firstName}-${p.lastName}`}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <div className="flex items-center justify-between mt-4 flex-wrap gap-2">
                <Body1>Показано {profiles.length} из {totalCount}</Body1>
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
    );
};
