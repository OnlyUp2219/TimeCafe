import { useNavigate } from "react-router-dom";
import {
    Body1,
    Body2,
    Caption1,
    Button,
    Card,
    CardFooter,
    CardHeader,
    Subtitle2,
    Divider,
    Subtitle1,
    Title1,
    Title2
} from "@fluentui/react-components";
import { DismissableError } from "@components/DismissableError/DismissableError";

import { PageLoader } from "@components/PageLoader/PageLoader";
import { Add20Regular, Delete20Regular, Edit20Regular, ArrowClockwise20Regular } from "@fluentui/react-icons";
import {
    useGetThemesPageQuery,
    useDeleteThemeMutation,
} from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { HasPermission } from "@components/Guard/HasPermission";
import { RequirePermission } from "@app/components/RequirePermission/RequirePermission";
import { Permissions } from "@shared/auth/permissions";
import { parseThemeConfig, getThemeStyles, getPatternLayerStyles } from "@utility/themeStyles";
import { Pagination } from "@components/Pagination/Pagination";

import { usePagination } from "@hooks/usePagination";

export const ThemesPage = () => {
    const navigate = useNavigate();
    const { page: currentPage, size: pageSize, setPage: setCurrentPage, setSize: setPageSize } = usePagination("adminThemes", 1, 12);

    const { data, isLoading, error: queryError, refetch } = useGetThemesPageQuery({
        page: currentPage,
        pageSize
    });
    const themes = data?.items ?? [];
    const totalCount = data?.metadata?.totalCount ?? 0;
    const totalPages = data?.metadata?.totalPages ?? 1;
    const [deleteTheme, { error: deleteError }] = useDeleteThemeMutation();

    const mutationError = deleteError ? getRtkErrorMessage(deleteError as FetchBaseQueryError) : null;
    const finalError = queryError ? getRtkErrorMessage(queryError as FetchBaseQueryError) : mutationError;

    const handleDelete = async (id: string) => {
        if (confirm("Удалить эту тему?")) {
            await deleteTheme(id);
        }
    };

    if (isLoading) {
        return <PageLoader label="Загружаем палитры..." />;
    }

    return (
        <RequirePermission can={Permissions.VenueThemeRead}>
            <div className="flex flex-col gap-2">
                <div className="flex justify-between items-center flex-wrap gap-4">
                    <div className="flex flex-col">
                        <Title2>Визуальные темы</Title2>
                        <Body2>{totalCount} визуальных тем</Body2>
                    </div>
                    <div className="flex gap-2">
                        <Button appearance="subtle" size="large" icon={<ArrowClockwise20Regular />} onClick={() => refetch()} />
                        <HasPermission can={Permissions.VenueThemeCreate}>
                            <Button
                                appearance="primary"
                                size="large"
                                icon={<Add20Regular />}
                                onClick={() => navigate("/admin/themes/create")}
                            >
                                Создать тему
                            </Button>
                        </HasPermission>
                    </div>
                </div>

                <DismissableError error={finalError} />

                {themes.length === 0 && (
                    <Card appearance="filled-alternative" size="large" className="items-center justify-center !min-h-[250px] ">
                        <Subtitle1 className="text-(--colorNeutralForeground4) ">Тем пока нет. Создайте первую!</Subtitle1>
                        <CardFooter className="flex ">
                            <Button size="large" appearance="outline" onClick={() => navigate("/admin/themes/create")}>Добавить оформление</Button>
                        </CardFooter>
                    </Card>
                )}

                {themes.length > 0 && (
                    <>
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                            {themes.map((theme) => {
                                const config = parseThemeConfig(theme.colors);
                                const styles = getThemeStyles(config);
                                return (
                                    <Card
                                        key={theme.themeId}
                                        className="min-h-[240px]"
                                        style={{ ...styles }}
                                        appearance="filled"
                                    >
                                        {(config.patterns || []).map((layer, idx) => (
                                            <div key={idx} style={getPatternLayerStyles(layer)} />
                                        ))}
                                        <CardHeader
                                            className="relative z-10"
                                            image={<span className="text-3xl">{theme.emoji}</span>}
                                            header={<Subtitle2 style={{ color: config.textColor, fontWeight: "bold" }}>{theme.name}</Subtitle2>}
                                            description={
                                                <Body2 style={{ color: config.textColor, opacity: 0.8 }} className="!line-clamp-3">
                                                    Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sempiternum iter est ad astra.
                                                </Body2>
                                            }
                                        />

                                        <div className="flex-grow flex flex-col justify-end relative z-10">
                                            <Divider style={{ color: config.textColor, opacity: 0.2 }} />
                                        </div>

                                        <CardFooter className="relative z-10">
                                            <Caption1 style={{ color: config.textColor, opacity: 0.5 }} className="grow">
                                                {config.type.toUpperCase()}
                                            </Caption1>
                                            <div className="flex gap-1">
                                                <HasPermission can={Permissions.VenueThemeUpdate}>
                                                    <Button
                                                        appearance="subtle"
                                                        style={{ color: config.textColor, backgroundColor: "rgba(255,255,255,0.15)", border: "1px solid rgba(255,255,255,0.1)" }}
                                                        icon={<Edit20Regular />}
                                                        onClick={() => navigate(`/admin/themes/${theme.themeId}/edit`)}
                                                    />
                                                </HasPermission>
                                                <HasPermission can={Permissions.VenueThemeDelete}>
                                                    <Button
                                                        appearance="subtle"
                                                        style={{ color: config.textColor, backgroundColor: "rgba(255,255,255,0.15)", border: "1px solid rgba(255,255,255,0.1)" }}
                                                        icon={<Delete20Regular />}
                                                        onClick={() => handleDelete(theme.themeId)}
                                                    />
                                                </HasPermission>
                                            </div>
                                        </CardFooter>
                                        <div className="absolute inset-0 bg-black/0 group-hover:bg-black/5 transition-colors pointer-events-none z-20" />
                                    </Card>
                                );
                            })}
                        </div>

                        <div className="flex items-center justify-between flex-wrap gap-2">
                            <Body1>Показано {themes.length} из {totalCount}</Body1>
                            <Pagination
                                currentPage={currentPage}
                                totalPages={totalPages}
                                onPageChange={setCurrentPage}
                                pageSize={pageSize}
                                onPageSizeChange={setPageSize}
                                totalCount={totalCount}
                            />
                        </div>
                    </>
                )}
            </div>
        </RequirePermission>
    );
};
