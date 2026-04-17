import {useCallback, useState} from "react";
import {useParams, useNavigate} from "react-router-dom";
import {
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    Checkbox,
    MessageBar,
    MessageBarBody,
    Spinner,
    Title2,
    Title3,
} from "@fluentui/react-components";
import {ArrowLeft20Regular, Save20Regular} from "@fluentui/react-icons";
import {useGetRoleClaimsByNameQuery, useUpdateRoleClaimsMutation, useGetPermissionsQuery} from "@store/api/adminApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {useComponentSize} from "@hooks/useComponentSize";

export const RoleClaimsPage = () => {
    const {roleName} = useParams<{roleName: string}>();
    const navigate = useNavigate();
    const {sizes} = useComponentSize();

    const {data: roleClaimsData, isLoading: claimsLoading, error: claimsError} = useGetRoleClaimsByNameQuery(roleName!, {skip: !roleName});
    const {data: permissionsData, isLoading: permsLoading} = useGetPermissionsQuery();
    const [updateRoleClaims, {isLoading: saving}] = useUpdateRoleClaimsMutation();

    const currentClaims = roleClaimsData?.roleClaim?.claims ?? [];
    const allPermissions = permissionsData?.permissions ?? [];

    const [selected, setSelected] = useState<Set<string> | null>(null);
    const effectiveSelected = selected ?? new Set(currentClaims);

    const [mutationError, setMutationError] = useState<string | null>(null);
    const [saved, setSaved] = useState(false);

    const toggle = useCallback((perm: string) => {
        setSelected(prev => {
            const s = new Set(prev ?? currentClaims);
            if (s.has(perm)) s.delete(perm);
            else s.add(perm);
            return s;
        });
        setSaved(false);
    }, [currentClaims]);

    const handleSave = useCallback(async () => {
        if (!roleName) return;
        setMutationError(null);
        try {
            await updateRoleClaims({roleName, claims: Array.from(effectiveSelected)}).unwrap();
            setSaved(true);
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось сохранить");
        }
    }, [roleName, effectiveSelected, updateRoleClaims]);

    const claimsError2 = claimsError ? getRtkErrorMessage(claimsError as FetchBaseQueryError) : null;

    if (claimsLoading || permsLoading) return <div className="flex justify-center p-8"><Spinner /></div>;

    const groupedPermissions = allPermissions.reduce<Record<string, string[]>>((acc, perm) => {
        const prefix = perm.split(".").slice(0, 2).join(".");
        if (!acc[prefix]) acc[prefix] = [];
        acc[prefix].push(perm);
        return acc;
    }, {});

    return (
        <div>
            <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/roles")} className="mb-4">
                Назад к ролям
            </Button>

            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Permissions роли</Title2>
                    <Body2 block>
                        <Badge appearance="outline" size="large">{roleName}</Badge>
                        {" "}{effectiveSelected.size} из {allPermissions.length} выбрано
                    </Body2>
                </div>
                <Button
                    appearance="primary"
                    size={sizes.button}
                    icon={saving ? <Spinner size="tiny" /> : <Save20Regular />}
                    onClick={handleSave}
                    disabled={saving}
                >
                    Сохранить
                </Button>
            </div>

            {(claimsError2 || mutationError) && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{claimsError2 || mutationError}</MessageBarBody>
                </MessageBar>
            )}

            {saved && (
                <MessageBar intent="success" className="mb-4">
                    <MessageBarBody>Permissions сохранены</MessageBarBody>
                </MessageBar>
            )}

            <div className="flex flex-col gap-4">
                {Object.entries(groupedPermissions).map(([group, perms]) => (
                    <Card key={group} size={sizes.card}>
                        <Title3 className="mb-3">{group}</Title3>
                        <div className="flex flex-wrap gap-2">
                            {perms.map(perm => (
                                <Checkbox
                                    key={perm}
                                    label={perm}
                                    checked={effectiveSelected.has(perm)}
                                    onChange={() => toggle(perm)}
                                />
                            ))}
                        </div>
                    </Card>
                ))}
            </div>

            {allPermissions.length === 0 && (
                <Body1>Нет доступных permissions</Body1>
            )}
        </div>
    );
};
