import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
    Button,
    Card,
    Title2,
    Subtitle2,
    Body1,
    Body2,
    Badge,
    Caption1,
    Divider,
    MessageBar,
    MessageBarBody,
} from "@fluentui/react-components";
import {
    Grid20Regular,
    Person20Regular,
    Clock20Regular,
    Money20Regular,
    Add20Regular,
    Eye20Regular,
} from "@fluentui/react-icons";
import { useGetActiveVisitsQuery, useGetResourcesQuery, useGetResourceGroupsQuery } from "@store/api/venueApi";
import { useGetProfileByUserIdQuery } from "@store/api/profileApi";
import { WalkInVisitDialog } from "@components/Admin/WalkInVisitDialog/WalkInVisitDialog";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { CURRENCY_SYMBOL } from "@shared/const/currency";
import { useComponentSize } from "@hooks/useComponentSize";
import { PageLoader } from "@components/PageLoader/PageLoader";

const ActiveUserLabel = ({ userId }: { userId: string | null }) => {
    const { data: profile } = useGetProfileByUserIdQuery(userId ?? "", { skip: !userId });
    if (!userId) return <span>Анонимный гость</span>;
    const userFullName = profile && (profile.firstName || profile.lastName)
        ? [profile.lastName, profile.firstName].filter(Boolean).join(" ")
        : "Загрузка...";
    return <span>{profile ? userFullName : `${userId.slice(0, 8)}…`}</span>;
};

export const ResourcesPage = () => {
    const navigate = useNavigate();
    const { sizes } = useComponentSize();

    const { data: activeVisits, isLoading: loadingVisits, error: errorVisits, refetch } = useGetActiveVisitsQuery();
    const { data: dbResources, isLoading: loadingResources, error: errorResources } = useGetResourcesQuery();
    const { data: dbResourceGroups, isLoading: loadingResourceGroups, error: errorResourceGroups } = useGetResourceGroupsQuery();

    const [walkInOpen, setWalkInOpen] = useState(false);
    const [selectedResourceId, setSelectedResourceId] = useState<string | null>(null);

    const handleOpenWalkIn = (resId: string) => {
        setSelectedResourceId(resId);
        setWalkInOpen(true);
    };

    const isLoading = loadingVisits || loadingResources || loadingResourceGroups;
    const error = errorVisits || errorResources || errorResourceGroups;

    if (isLoading) {
        return <PageLoader label="Загрузка карты столов..." />;
    }

    if (error) {
        const errMsg = getRtkErrorMessage(error as FetchBaseQueryError) || "Ошибка загрузки данных заведения";
        return (
            <MessageBar intent="error">
                <MessageBarBody>{errMsg}</MessageBarBody>
            </MessageBar>
        );
    }

    const resources = dbResources || [];
    const resourceGroups = dbResourceGroups || [];

    return (
        <div className="flex flex-col gap-6">
            <div className="flex items-center justify-between flex-wrap gap-4">
                <div className="flex flex-col gap-1">
                    <Title2>Интерактивная карта столов</Title2>
                    <Caption1 className="text-[var(--colorNeutralForeground3)]">
                        Мониторинг занятости ресурсов и быстрая посадка гостей
                    </Caption1>
                </div>
                <Button
                    appearance="primary"
                    size={sizes.button}
                    icon={<Add20Regular />}
                    onClick={() => handleOpenWalkIn("")}
                >
                    Быстрая посадка (Walk-in)
                </Button>
            </div>

            <div className="flex flex-col gap-8">
                {resourceGroups.map((group) => {
                    const zoneResources = resources.filter((r) => r.resourceGroupId === group.resourceGroupId);
                    return (
                        <div key={group.resourceGroupId} className="flex flex-col gap-4">
                            <div className="flex flex-col gap-1 border-b pb-2">
                                <Subtitle2 className="text-[var(--colorBrandForegroundLink)]">
                                    {group.name}
                                </Subtitle2>
                                {group.description && (
                                    <Caption1 className="text-[var(--colorNeutralForeground3)] italic">
                                        {group.description}
                                    </Caption1>
                                )}
                            </div>
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                                {zoneResources.map((res) => {
                                    const activeVisit = activeVisits?.find(
                                        (v) => v.resourceId === res.resourceId || v.resourceId === res.name
                                    );
                                    const isOccupied = !!activeVisit;

                                    return (
                                        <Card
                                            key={res.resourceId}
                                            size={sizes.card}
                                            className={`transition-all duration-200 border-2 ${isOccupied
                                                    ? "border-[var(--colorPaletteRedBorderActive)] bg-[var(--colorNeutralBackground2)]"
                                                    : "border-[var(--colorPaletteGreenBorderActive)] hover:shadow-md"
                                                }`}
                                        >
                                            <div className="flex flex-col gap-3">
                                                <div className="flex items-center justify-between">
                                                    <div className="flex items-center gap-2">
                                                        <Grid20Regular />
                                                        <Body1 className="font-semibold">
                                                            {res.name}
                                                        </Body1>
                                                    </div>
                                                    <Badge
                                                        color={isOccupied ? "danger" : "success"}
                                                        appearance="filled"
                                                        size={sizes.badge}
                                                    >
                                                        {isOccupied ? "Занят" : "Свободен"}
                                                    </Badge>
                                                </div>

                                                <Divider />

                                                {isOccupied ? (
                                                    <div className="flex flex-col gap-2 my-1">
                                                        <div className="flex items-center gap-2">
                                                            <Person20Regular className="text-[var(--colorNeutralForeground2)]" />
                                                            <Body2 className="font-medium text-[var(--colorNeutralForeground1)]">
                                                                <ActiveUserLabel userId={activeVisit.userId} />
                                                            </Body2>
                                                        </div>
                                                        <div className="flex items-center gap-2">
                                                            <Clock20Regular className="text-[var(--colorNeutralForeground2)]" />
                                                            <Body2 className="text-[var(--colorNeutralForeground2)]">
                                                                Вход: {new Date(activeVisit.entryTime).toLocaleTimeString("ru-RU", {
                                                                    hour: "2-digit",
                                                                    minute: "2-digit"
                                                                })}
                                                            </Body2>
                                                        </div>
                                                        <div className="flex items-center gap-2">
                                                            <Money20Regular className="text-[var(--colorNeutralForeground2)]" />
                                                            <Body2 className="text-[var(--colorNeutralForeground2)]">
                                                                Тариф: {activeVisit.tariffName} (
                                                                {activeVisit.calculatedCost != null
                                                                    ? `${activeVisit.calculatedCost.toFixed(2)} ${CURRENCY_SYMBOL}`
                                                                    : "—"}
                                                                )
                                                            </Body2>
                                                        </div>
                                                    </div>
                                                ) : (
                                                    <div className="flex items-center justify-center py-4">
                                                        <Body2 className="text-[var(--colorNeutralForeground3)] italic">
                                                            Готов к посадке гостей (Вместимость: {res.capacity})
                                                        </Body2>
                                                    </div>
                                                )}

                                                <Divider />

                                                <div className="flex justify-end gap-2 mt-1">
                                                    {isOccupied ? (
                                                        <Button
                                                            size={sizes.button}
                                                            appearance="primary"
                                                            icon={<Eye20Regular />}
                                                            onClick={() => navigate(`/admin/visits/${activeVisit.visitId}`)}
                                                        >
                                                            Управление
                                                        </Button>
                                                    ) : (
                                                         <Button
                                                            size={sizes.button}
                                                            appearance="outline"
                                                            icon={<Add20Regular />}
                                                            onClick={() => handleOpenWalkIn(res.resourceId)}
                                                        >
                                                            Посадить
                                                        </Button>
                                                    )}
                                                </div>
                                            </div>
                                        </Card>
                                    );
                                })}
                            </div>
                        </div>
                    );
                })}
            </div>

            <WalkInVisitDialog
                open={walkInOpen}
                onOpenChange={setWalkInOpen}
                initialResourceId={selectedResourceId}
                onSuccess={() => void refetch()}
            />
        </div>
    );
};
