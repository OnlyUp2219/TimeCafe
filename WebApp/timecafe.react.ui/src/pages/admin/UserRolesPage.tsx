import {useCallback, useState} from "react";
import {useParams, useNavigate} from "react-router-dom";
import {
    Avatar,
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    MessageBar,
    MessageBarBody,
    Spinner,
    Title2,
    Title3,
} from "@fluentui/react-components";
import {ArrowLeft20Regular, Add20Regular, Delete20Regular} from "@fluentui/react-icons";
import {
    useGetUserByIdQuery,
    useGetRolesQuery,
    useAssignRoleToUserMutation,
    useRemoveRoleFromUserMutation,
} from "@store/api/adminApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {useComponentSize} from "@hooks/useComponentSize";
import {HasPermission} from "@components/Guard/HasPermission";
import {Permissions} from "@shared/auth/permissions";

export const UserRolesPage = () => {
    const {id} = useParams<{id: string}>();
    const navigate = useNavigate();
    const {sizes} = useComponentSize();

    const {data: userData, isLoading: userLoading} = useGetUserByIdQuery(id!, {skip: !id, refetchOnMountOrArgChange: true});
    const {data: rolesData, isLoading: rolesLoading} = useGetRolesQuery(undefined, {refetchOnMountOrArgChange: true});
    const [assignRole] = useAssignRoleToUserMutation();
    const [removeRole] = useRemoveRoleFromUserMutation();

    const user = userData?.user;
    const allRoles = rolesData?.roles ?? [];
    const userRoles = user?.role ? user.role.split(", ").filter(Boolean) : [];

    const [mutationError, setMutationError] = useState<string | null>(null);

    const handleAssign = useCallback(async (roleName: string) => {
        if (!id) return;
        setMutationError(null);
        try {
            await assignRole({userId: id, roleName}).unwrap();
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось назначить роль");
        }
    }, [id, assignRole]);

    const handleRemove = useCallback(async (roleName: string) => {
        if (!id) return;
        setMutationError(null);
        try {
            await removeRole({userId: id, roleName}).unwrap();
        } catch (err) {
            setMutationError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось снять роль");
        }
    }, [id, removeRole]);

    if (userLoading || rolesLoading) {
        return <div className="flex justify-center p-20"><Spinner label="Загрузка ролей пользователя..." /></div>;
    }

    return (
        <div>
            <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate(`/admin/users/${id}`)} className="mb-4">
                Назад к пользователю
            </Button>

            <div className="mb-4">
                <Title2>Роли пользователя</Title2>
                {user && (
                    <div className="flex items-center gap-3 mt-2">
                        <Avatar name={user.name || user.email} size={32} />
                        <Body1>{user.name || user.email}</Body1>
                    </div>
                )}
            </div>

            {mutationError && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{mutationError}</MessageBarBody>
                </MessageBar>
            )}

            <div className="flex flex-col gap-4">
                <Card size={sizes.card}>
                    <Title3 className="mb-3">Текущие роли</Title3>
                    {userRoles.length === 0 ? (
                        <Body2>Нет назначенных ролей</Body2>
                    ) : (
                        <div className="flex flex-wrap gap-2">
                            {userRoles.map((role, idx) => (
                                <div key={`${role}-${idx}`} className="flex items-center gap-1">
                                    <Badge appearance="filled">{role}</Badge>
                                    {role !== "SuperAdmin" && (
                                        <HasPermission can={Permissions.RbacUserRoleRemove}>
                                            <Button
                                                appearance="subtle"
                                                size="small"
                                                icon={<Delete20Regular />}
                                                onClick={() => handleRemove(role)}
                                            />
                                        </HasPermission>
                                    )}
                                </div>
                            ))}
                        </div>
                    )}
                </Card>

                <Card size={sizes.card}>
                    <Title3 className="mb-3">Доступные роли</Title3>
                    <div className="flex flex-wrap gap-2">
                        {allRoles
                            .filter(r => !userRoles.includes(r.roleName) && r.roleName !== "SuperAdmin")
                            .map(r => (
                                <HasPermission can={Permissions.RbacUserRoleAssign}>
                                    <Button
                                        key={r.roleId}
                                        appearance="outline"
                                        size="small"
                                        icon={<Add20Regular />}
                                        onClick={() => handleAssign(r.roleName)}
                                    >
                                        {r.roleName}
                                    </Button>
                                </HasPermission>
                            ))}
                        {allRoles.filter(r => !userRoles.includes(r.roleName) && r.roleName !== "SuperAdmin").length === 0 && (
                            <Body2>Все роли уже назначены</Body2>
                        )}
                    </div>
                </Card>
            </div>
        </div>
    );
};
