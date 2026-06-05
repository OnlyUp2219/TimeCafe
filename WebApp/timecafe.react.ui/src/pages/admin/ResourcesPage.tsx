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
    Dialog,
    DialogActions,
    DialogBody,
    DialogContent,
    DialogSurface,
    DialogTitle,
    Field,
    Input,
} from "@fluentui/react-components";
import { DismissableError } from "@components/DismissableError/DismissableError";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions } from "@shared/auth/permissions";
import {
    Grid20Regular,
    Person20Regular,
    Clock20Regular,
    Money20Regular,
    Add20Regular,
    Eye20Regular,
    Edit20Regular,
    Delete20Regular,
} from "@fluentui/react-icons";
import {
    useGetActiveVisitsQuery,
    useGetResourcesQuery,
    useGetResourceGroupsQuery,
    useCreateResourceMutation,
    useUpdateResourceMutation,
    useDeleteResourceMutation,
    useCreateResourceGroupMutation,
    useUpdateResourceGroupMutation,
    useDeleteResourceGroupMutation,
    type Resource,
    type ResourceGroup,
} from "@store/api/venueApi";
import { useGetProfileByUserIdQuery } from "@store/api/profileApi";
import { WalkInVisitDialog } from "@components/Admin/WalkInVisitDialog/WalkInVisitDialog";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { CURRENCY_SYMBOL } from "@shared/const/currency";
import { useComponentSize } from "@hooks/useComponentSize";
import { PageLoader } from "@components/PageLoader/PageLoader";
import { RequirePermission } from "@app/components/RequirePermission/RequirePermission";

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
    const { data: dbResources, isLoading: loadingResources, error: errorResources, refetch: refetchResources } = useGetResourcesQuery();
    const { data: dbResourceGroups, isLoading: loadingResourceGroups, error: errorResourceGroups, refetch: refetchGroups } = useGetResourceGroupsQuery();

    const [createResource] = useCreateResourceMutation();
    const [updateResource] = useUpdateResourceMutation();
    const [deleteResource] = useDeleteResourceMutation();

    const [createResourceGroup] = useCreateResourceGroupMutation();
    const [updateResourceGroup] = useUpdateResourceGroupMutation();
    const [deleteResourceGroup] = useDeleteResourceGroupMutation();

    const [walkInOpen, setWalkInOpen] = useState(false);
    const [selectedResourceId, setSelectedResourceId] = useState<string | null>(null);

    const [groupDialogOpen, setGroupDialogOpen] = useState(false);
    const [selectedGroup, setSelectedGroup] = useState<ResourceGroup | null>(null);
    const [groupName, setGroupName] = useState("");
    const [groupDescription, setGroupDescription] = useState("");
    const [groupError, setGroupError] = useState<string | null>(null);

    const [resourceDialogOpen, setResourceDialogOpen] = useState(false);
    const [selectedResource, setSelectedResource] = useState<Resource | null>(null);
    const [resourceGroupIdForNew, setResourceGroupIdForNew] = useState("");
    const [resourceName, setResourceName] = useState("");
    const [resourceCapacity, setResourceCapacity] = useState(4);
    const [resourceError, setResourceError] = useState<string | null>(null);

    const handleOpenWalkIn = (resId: string) => {
        setSelectedResourceId(resId);
        setWalkInOpen(true);
    };

    const handleOpenGroupDialog = (group: ResourceGroup | null) => {
        setSelectedGroup(group);
        setGroupName(group?.name ?? "");
        setGroupDescription(group?.description ?? "");
        setGroupError(null);
        setGroupDialogOpen(true);
    };

    const handleSaveGroup = async () => {
        setGroupError(null);
        if (!groupName.trim()) {
            setGroupError("Название зоны обязательно");
            return;
        }

        try {
            if (selectedGroup) {
                await updateResourceGroup({
                    ...selectedGroup,
                    name: groupName.trim(),
                    description: groupDescription.trim() || null,
                }).unwrap();
            } else {
                await createResourceGroup({
                    name: groupName.trim(),
                    description: groupDescription.trim() || null,
                    capacity: 0,
                    isActive: true,
                }).unwrap();
            }
            setGroupDialogOpen(false);
            void refetchGroups();
        } catch (err) {
            setGroupError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось сохранить зону");
        }
    };

    const handleDeleteGroup = async (groupId: string) => {
        if (!window.confirm("Вы уверены, что хотите удалить эту зону?")) return;
        try {
            await deleteResourceGroup(groupId).unwrap();
            void refetchGroups();
        } catch (err) {
            alert(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось удалить зону");
        }
    };

    const handleOpenResourceDialog = (res: Resource | null, groupId?: string) => {
        setSelectedResource(res);
        setResourceGroupIdForNew(groupId ?? "");
        setResourceName(res?.name ?? "");
        setResourceCapacity(res?.capacity ?? 4);
        setResourceError(null);
        setResourceDialogOpen(true);
    };

    const handleSaveResource = async () => {
        setResourceError(null);
        if (!resourceName.trim()) {
            setResourceError("Название стола обязательно");
            return;
        }

        try {
            if (selectedResource) {
                await updateResource({
                    ...selectedResource,
                    name: resourceName.trim(),
                    capacity: resourceCapacity,
                }).unwrap();
            } else {
                await createResource({
                    resourceGroupId: resourceGroupIdForNew,
                    name: resourceName.trim(),
                    capacity: resourceCapacity,
                    isActive: true,
                }).unwrap();
            }
            setResourceDialogOpen(false);
            void refetchResources();
        } catch (err) {
            setResourceError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось сохранить стол");
        }
    };

    const handleDeleteResource = async (resId: string) => {
        if (!window.confirm("Вы уверены, что хотите удалить этот стол?")) return;
        try {
            await deleteResource(resId).unwrap();
            void refetchResources();
        } catch (err) {
            alert(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось удалить стол");
        }
    };

    const isLoading = loadingVisits || loadingResources || loadingResourceGroups;
    const error = errorVisits || errorResources || errorResourceGroups;

    if (isLoading) {
        return <PageLoader label="Загрузка карты столов..." />;
    }

    const errorMessage = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const resources = dbResources || [];
    const resourceGroups = dbResourceGroups || [];

    return (
        <RequirePermission can={Permissions.VenueResourceRead}>
            <div className="flex flex-col gap-6">
                <div className="flex items-center justify-between flex-wrap gap-4">
                    <div className="flex flex-col gap-1">
                        <Title2>Интерактивная карта столов</Title2>
                        <Caption1 className="text-(--colorNeutralForeground3)">
                            Мониторинг занятости ресурсов и быстрая посадка гостей
                        </Caption1>
                    </div>
                    <div className="flex gap-2">
                        <HasPermission can={Permissions.VenueResourceCreate}>
                            <Button
                                appearance="outline"
                                size={sizes.button}
                                icon={<Add20Regular />}
                                onClick={() => handleOpenGroupDialog(null)}
                            >
                                Добавить зону
                            </Button>
                        </HasPermission>
                        <HasPermission can={Permissions.VenueVisitCreate}>
                            <Button
                                appearance="primary"
                                size={sizes.button}
                                icon={<Add20Regular />}
                                onClick={() => handleOpenWalkIn("")}
                            >
                                Быстрая посадка (Walk-in)
                            </Button>
                        </HasPermission>
                    </div>
                </div>

                <DismissableError error={errorMessage} className="mb-4" />

                <div className="flex flex-col gap-8">
                    {resourceGroups.map((group) => {
                        const zoneResources = resources.filter((r) => r.resourceGroupId === group.resourceGroupId);
                        return (
                            <div key={group.resourceGroupId} className="flex flex-col gap-4">
                                <div className="flex items-center justify-between border-b pb-2 flex-wrap gap-2">
                                    <div className="flex flex-col gap-1">
                                        <Subtitle2 className="text-(--colorBrandForegroundLink) text-lg">
                                            {group.name}
                                        </Subtitle2>
                                        {group.description && (
                                            <Caption1 className="text-(--colorNeutralForeground3) italic">
                                                {group.description}
                                            </Caption1>
                                        )}
                                    </div>
                                    <div className="flex gap-2">
                                        <HasPermission can={Permissions.VenueResourceCreate}>
                                            <Button
                                                size="small"
                                                appearance="subtle"
                                                icon={<Add20Regular />}
                                                onClick={() => handleOpenResourceDialog(null, group.resourceGroupId)}
                                            >
                                                Добавить стол
                                            </Button>
                                        </HasPermission>
                                        <HasPermission can={Permissions.VenueResourceUpdate}>
                                            <Button
                                                size="small"
                                                appearance="subtle"
                                                icon={<Edit20Regular />}
                                                onClick={() => handleOpenGroupDialog(group)}
                                            >
                                                Редактировать зону
                                            </Button>
                                        </HasPermission>
                                        <HasPermission can={Permissions.VenueResourceDelete}>
                                            <Button
                                                size="small"
                                                appearance="subtle"
                                                icon={<Delete20Regular />}
                                                onClick={() => handleDeleteGroup(group.resourceGroupId)}
                                            >
                                                Удалить зону
                                            </Button>
                                        </HasPermission>
                                    </div>
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
                                                    ? "border-(--colorPaletteRedBorderActive) bg-(--colorNeutralBackground2)"
                                                    : "border-(--colorPaletteGreenBorderActive) hover:shadow-md"
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
                                                        <div className="flex items-center gap-2">
                                                            <Badge
                                                                color={isOccupied ? "danger" : "success"}
                                                                appearance="filled"
                                                                size={sizes.badge}
                                                            >
                                                                {isOccupied ? "Занят" : "Свободен"}
                                                            </Badge>
                                                        </div>
                                                    </div>

                                                    <Divider />

                                                    {isOccupied ? (
                                                        <div className="flex flex-col gap-2 my-1">
                                                            <div className="flex items-center gap-2">
                                                                <Person20Regular className="text-(--colorNeutralForeground2)" />
                                                                <Body2 className="font-medium text-(--colorNeutralForeground1)">
                                                                    <ActiveUserLabel userId={activeVisit.userId} />
                                                                </Body2>
                                                            </div>
                                                            <div className="flex items-center gap-2">
                                                                <Clock20Regular className="text-(--colorNeutralForeground2)" />
                                                                <Body2 className="text-(--colorNeutralForeground2)">
                                                                    Вход: {new Date(activeVisit.entryTime).toLocaleTimeString("ru-RU", {
                                                                        hour: "2-digit",
                                                                        minute: "2-digit"
                                                                    })}
                                                                </Body2>
                                                            </div>
                                                            <div className="flex items-center gap-2">
                                                                <Money20Regular className="text-(--colorNeutralForeground2)" />
                                                                <Body2 className="text-(--colorNeutralForeground2)">
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
                                                            <Body2 className="text-(--colorNeutralForeground3) italic">
                                                                Готов к посадке гостей (Вместимость: {res.capacity})
                                                            </Body2>
                                                        </div>
                                                    )}

                                                    <Divider />

                                                    <div className="flex justify-between items-center mt-1">
                                                        <div className="flex gap-1">
                                                            <HasPermission can={Permissions.VenueResourceUpdate}>
                                                                <Button
                                                                    size="small"
                                                                    appearance="subtle"
                                                                    icon={<Edit20Regular />}
                                                                    onClick={() => handleOpenResourceDialog(res)}
                                                                    title="Редактировать стол"
                                                                />
                                                            </HasPermission>
                                                            <HasPermission can={Permissions.VenueResourceDelete}>
                                                                <Button
                                                                    size="small"
                                                                    appearance="subtle"
                                                                    icon={<Delete20Regular />}
                                                                    onClick={() => handleDeleteResource(res.resourceId)}
                                                                    disabled={isOccupied}
                                                                    title="Удалить стол"
                                                                />
                                                            </HasPermission>
                                                        </div>
                                                        <div className="flex gap-2">
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
                                                                <HasPermission can={Permissions.VenueVisitCreate}>
                                                                    <Button
                                                                        size={sizes.button}
                                                                        appearance="outline"
                                                                        icon={<Add20Regular />}
                                                                        onClick={() => handleOpenWalkIn(res.resourceId)}
                                                                    >
                                                                        Посадить
                                                                    </Button>
                                                                </HasPermission>
                                                            )}
                                                        </div>
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

                <Dialog open={groupDialogOpen} onOpenChange={(_, data) => setGroupDialogOpen(data.open)}>
                    <DialogSurface>
                        <DialogBody>
                            <DialogTitle>{selectedGroup ? "Редактировать зону" : "Добавить зону"}</DialogTitle>

                            <DialogContent>
                                <div className="flex flex-col gap-4 py-2">
                                    <DismissableError error={groupError} className="mb-3" />
                                    <Field label="Название зоны" required>
                                        <Input
                                            value={groupName}
                                            onChange={(_, data) => setGroupName(data.value)}
                                            placeholder="Например: VIP, Общий зал"
                                        />
                                    </Field>
                                    <Field label="Описание">
                                        <Input
                                            value={groupDescription}
                                            onChange={(_, data) => setGroupDescription(data.value)}
                                            placeholder="Например: Тихая зона с приставками"
                                        />
                                    </Field>
                                </div>
                            </DialogContent>
                            <DialogActions>
                                <Button appearance="primary" onClick={handleSaveGroup}>
                                    Сохранить
                                </Button>
                                <Button appearance="secondary" onClick={() => setGroupDialogOpen(false)}>
                                    Отмена
                                </Button>
                            </DialogActions>
                        </DialogBody>
                    </DialogSurface>
                </Dialog>

                <Dialog open={resourceDialogOpen} onOpenChange={(_, data) => setResourceDialogOpen(data.open)}>
                    <DialogSurface>
                        <DialogBody>
                            <DialogTitle>{selectedResource ? "Редактировать стол" : "Добавить стол"}</DialogTitle>
                            <DialogContent>
                                <div className="flex flex-col gap-4 py-2">
                                    <DismissableError error={resourceError} className="mb-3" />
                                    <Field label="Название / Номер стола" required>
                                        <Input
                                            value={resourceName}
                                            onChange={(_, data) => setResourceName(data.value)}
                                            placeholder="Например: Стол №5"
                                        />
                                    </Field>
                                    <Field label="Вместимость (человек)" required>
                                        <Input
                                            type="number"
                                            value={String(resourceCapacity)}
                                            onChange={(_, data) => setResourceCapacity(Math.max(1, Number(data.value)))}
                                            min={1}
                                        />
                                    </Field>
                                </div>
                            </DialogContent>
                            <DialogActions>
                                <Button appearance="primary" onClick={handleSaveResource}>
                                    Сохранить
                                </Button>
                                <Button appearance="secondary" onClick={() => setResourceDialogOpen(false)}>
                                    Отмена
                                </Button>
                            </DialogActions>
                        </DialogBody>
                    </DialogSurface>
                </Dialog>
            </div>
        </RequirePermission>
    );
};
