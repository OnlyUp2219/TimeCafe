import { useCallback, useMemo, useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
    Badge,
    Body2,
    Button,
    Card,
    MessageBar,
    MessageBarBody,
    Spinner,
    Title2,
    FlatTree,
    FlatTreeItem,
    TreeItemLayout,
    CounterBadge,
    useHeadlessFlatTree_unstable,
} from "@fluentui/react-components";
import type {
    TreeCheckedChangeData,
    TreeItemValue,
    HeadlessFlatTreeItemProps,
} from "@fluentui/react-components";
import { ArrowLeft20Regular, Save20Regular } from "@fluentui/react-icons";
import { useGetRoleClaimsByNameQuery, useUpdateRoleClaimsMutation, useGetPermissionsQuery } from "@store/api/adminApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useComponentSize } from "@hooks/useComponentSize";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions } from "@shared/auth/permissions";

const SERVICE_LABELS: Record<string, string> = {
    "userprofile": "👤 UserProfile",
    "billing": "💳 Billing",
    "venue": "🏠 Venue",
    "auth": "🔑 Auth",
};

const ENTITY_LABELS: Record<string, string> = {
    "profile": "Профиль",
    "additionalinfo": "Доп. информация",
    "photo": "Фото",
    "balance": "Баланс",
    "debt": "Задолженность",
    "transaction": "Транзакции",
    "payment": "Платежи",
    "tariff": "Тарифы",
    "promotion": "Акции",
    "theme": "Темы",
    "visit": "Визиты",
    "account": "Аккаунт",
    "rbac": "RBAC",
    "role": "Роли",
    "permission": "Права",
    "userrole": "Роли пользователей",
};

const ACTION_LABELS: Record<string, string> = {
    "create": "Создание",
    "read": "Чтение",
    "update": "Обновление",
    "delete": "Удаление",
    "activate": "Активация",
    "deactivate": "Деактивация",
    "initialize": "Инициализация",
    "assign": "Назначение",
    "remove": "Удаление назначения",
    "change": "Изменение",
    "save": "Сохранение",
    "clear": "Очистка",
    "generate": "Генерация",
    "verify": "Верификация",
    "self": "Своё",
    "admin": "Администраторское",
    "claims": "Клеймы",
    "end": "Завершение",
    "status": "Статус",
};

function formatPermissionLabel(parts: string[]): string {
    return parts.map(p => ACTION_LABELS[p] ?? p).join(" → ");
}

export const RoleClaimsPage = () => {
    const { roleName } = useParams<{ roleName: string }>();
    const navigate = useNavigate();
    const { sizes } = useComponentSize();

    const normalizedRoleName = roleName?.trim() || null;

    const { data: roleClaimsData, isLoading: claimsLoading, error: claimsError } = useGetRoleClaimsByNameQuery(normalizedRoleName!, { skip: !normalizedRoleName, refetchOnMountOrArgChange: true });
    const { data: permissionsData, isLoading: permsLoading } = useGetPermissionsQuery();
    const [updateRoleClaims, { isLoading: saving }] = useUpdateRoleClaimsMutation();

    const currentClaims = roleClaimsData?.roleClaim?.claims ?? [];
    const allPermissions = permissionsData?.permissions ?? [];

    const groupedPermissions = useMemo(() => {
        const groups: Record<string, Record<string, string[]>> = {};
        allPermissions.forEach(perm => {
            const parts = perm.split(".");
            const service = parts[0] ?? "other";
            const entity = parts.slice(1, -1).join(".") || "other";
            if (!groups[service]) groups[service] = {};
            if (!groups[service][entity]) groups[service][entity] = [];
            groups[service][entity].push(perm);
        });
        return groups;
    }, [allPermissions]);

    const flatTreeItems = useMemo(() => {
        const items: (HeadlessFlatTreeItemProps & { content: React.ReactNode; service?: string; entity?: string })[] = [];
        Object.entries(groupedPermissions).forEach(([service, entities]) => {
            const serviceLabel = SERVICE_LABELS[service] ?? service;
            items.push({
                value: service,
                content: serviceLabel,
                itemType: "branch",
                service
            });

            Object.entries(entities).forEach(([entity, perms]) => {
                const entityValue = `${service}.${entity}`;
                items.push({
                    value: entityValue,
                    parentValue: service,
                    content: ENTITY_LABELS[entity] ?? entity,
                    itemType: "branch",
                    service,
                    entity
                });

                perms.forEach(perm => {
                    const actionParts = perm.split(".").slice(service === "auth" ? 2 : 1);
                    items.push({
                        value: perm,
                        parentValue: entityValue,
                        content: (
                            <>
                                {formatPermissionLabel(actionParts)}
                                <span className="text-xs text-[var(--colorNeutralForeground3)] ml-2 font-mono">{perm}</span>
                            </>
                        ),
                        itemType: "leaf"
                    });
                });
            });
        });
        return items;
    }, [groupedPermissions]);

    const [selectedLeaves, setSelectedLeaves] = useState<Set<string>>(new Set());
    const [initializedRole, setInitializedRole] = useState<string | null>(null);

    useEffect(() => {
        if (allPermissions.length > 0 && !claimsLoading && initializedRole !== normalizedRoleName) {
            setSelectedLeaves(new Set(currentClaims));
            setInitializedRole(normalizedRoleName);
            setSaved(false);
        }
    }, [currentClaims, allPermissions, normalizedRoleName, initializedRole, claimsLoading]);

    const checkedItemsMap = useMemo(() => {
        const map = new Map<TreeItemValue, "checked" | "mixed">();

        selectedLeaves.forEach(leaf => map.set(leaf, "checked"));

        Object.entries(groupedPermissions).forEach(([service, entities]) => {
            let serviceTotal = 0;
            let serviceChecked = 0;

            Object.entries(entities).forEach(([entity, perms]) => {
                let entityChecked = 0;
                perms.forEach(p => {
                    if (selectedLeaves.has(p)) entityChecked++;
                });

                if (entityChecked === perms.length && perms.length > 0) {
                    map.set(`${service}.${entity}`, "checked");
                } else if (entityChecked > 0) {
                    map.set(`${service}.${entity}`, "mixed");
                }

                serviceTotal += perms.length;
                serviceChecked += entityChecked;
            });

            if (serviceChecked === serviceTotal && serviceTotal > 0) {
                map.set(service, "checked");
            } else if (serviceChecked > 0) {
                map.set(service, "mixed");
            }
        });

        return map;
    }, [selectedLeaves, groupedPermissions]);

    const handleCheckedChange = useCallback((_: unknown, data: TreeCheckedChangeData) => {
        setSaved(false);
        const val = String(data.value);
        const isChecking = data.checked !== false;

        setSelectedLeaves(prev => {
            const next = new Set(prev);

            const applyChange = (perm: string) => {
                if (isChecking) next.add(perm);
                else next.delete(perm);
            };

            if (allPermissions.includes(val)) {
                applyChange(val);
            } else if (groupedPermissions[val]) {
                Object.values(groupedPermissions[val]).flat().forEach(applyChange);
            } else {
                const firstDotIndex = val.indexOf(".");
                if (firstDotIndex > 0) {
                    const s = val.substring(0, firstDotIndex);
                    const ent = val.substring(firstDotIndex + 1);
                    if (groupedPermissions[s]?.[ent]) {
                        groupedPermissions[s][ent].forEach(applyChange);
                    }
                }
            }
            return next;
        });
    }, [groupedPermissions, allPermissions]);

    const flatTree = useHeadlessFlatTree_unstable(flatTreeItems, {
        selectionMode: "multiselect",
        checkedItems: checkedItemsMap,
        onCheckedChange: handleCheckedChange,
        defaultOpenItems: Object.keys(groupedPermissions),
    });

    const [mutationError, setMutationError] = useState<string | null>(null);
    const [saved, setSaved] = useState(false);


    const handleSave = useCallback(async () => {
        if (!normalizedRoleName) return;
        setMutationError(null);
        try {
            await updateRoleClaims({ roleName: normalizedRoleName, claims: Array.from(selectedLeaves) }).unwrap();
            setSaved(true);
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось сохранить");
        }
    }, [normalizedRoleName, selectedLeaves, updateRoleClaims]);

    const claimsError2 = claimsError ? getRtkErrorMessage(claimsError as FetchBaseQueryError) : null;

    if (claimsLoading || permsLoading) {
        return <div className="flex justify-center p-20"><Spinner label="Загрузка прав..." /></div>;
    }

    if (claimsError2) {
        return (
            <div className="p-8">
                <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/roles")} className="mb-4">
                    Назад к ролям
                </Button>
                <MessageBar intent="error">
                    <MessageBarBody>{claimsError2}</MessageBarBody>
                </MessageBar>
            </div>
        );
    }

    if (!normalizedRoleName) {
        return (
            <div className="p-8">
                <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/roles")} className="mb-4">
                    Назад к ролям
                </Button>
                <Card size={sizes.card}>
                    <Title2>Роль не указана</Title2>
                    <Body2 block>Переход выполнен без имени роли, поэтому редактирование permissions недоступно.</Body2>
                </Card>
            </div>
        );
    }

    return (
        <div>
            <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/roles")} className="mb-4">
                Назад к ролям
            </Button>

            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Permissions роли</Title2>
                    <Body2 block className="flex items-center gap-2 mt-1">
                        <Badge appearance="filled" size="large">{normalizedRoleName}</Badge>
                        <span className="text-[var(--colorNeutralForeground3)]">{selectedLeaves.size} из {allPermissions.length} выбрано</span>
                    </Body2>
                </div>
                <HasPermission can={Permissions.RbacRoleClaimsUpdate}>
                    <Button
                        appearance="primary"
                        size={sizes.button}
                        icon={saving ? <Spinner size="tiny" /> : <Save20Regular />}
                        onClick={handleSave}
                        disabled={saving}
                    >
                        Сохранить
                    </Button>
                </HasPermission>
            </div>

            {mutationError && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{mutationError}</MessageBarBody>
                </MessageBar>
            )}

            {saved && (
                <MessageBar intent="success" className="mb-4">
                    <MessageBarBody>Permissions сохранены</MessageBarBody>
                </MessageBar>
            )}

            {allPermissions.length === 0 ? (
                <Body2>Нет доступных permissions</Body2>
            ) : (
                <FlatTree {...flatTree.getTreeProps()} aria-label="Permissions">
                    {Array.from(flatTree.items(), (item) => {
                        const { content, service, entity, ...treeItemProps } = item.getTreeItemProps() as any;

                        let badgeCount = 0;
                        if (service && !entity) {
                            const servicePerms = Object.values(groupedPermissions[service]).flat();
                            badgeCount = servicePerms.filter(p => selectedLeaves.has(p)).length;
                        } else if (service && entity) {
                            const entityPerms = groupedPermissions[service][entity];
                            badgeCount = entityPerms.filter(p => selectedLeaves.has(p)).length;
                        }

                        return (
                            <FlatTreeItem {...treeItemProps} key={item.value}>
                                <TreeItemLayout
                                    aside={
                                        badgeCount > 0 ? (
                                            <CounterBadge
                                                count={badgeCount}
                                                color="brand"
                                                size="small"
                                                overflowCount={99}
                                            />
                                        ) : undefined
                                    }
                                >
                                    {content}
                                </TreeItemLayout>
                            </FlatTreeItem>
                        );
                    })}
                </FlatTree>
            )}
        </div>
    );
};
