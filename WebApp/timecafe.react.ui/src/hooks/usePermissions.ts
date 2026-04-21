import {useCallback, useEffect} from "react";
import {useAppDispatch, useAppSelector} from "@store/hooks";
import {selectPermissions, selectPermissionsLoaded, setPermissions, clearPermissions} from "@store/permissionsSlice";
import {useLazyGetMyPermissionsQuery} from "@store/api/authApi";
import {Permissions, type Permission} from "@shared/auth/permissions";

export const usePermissions = () => {
    const dispatch = useAppDispatch();
    const permissions = useAppSelector(selectPermissions);
    const loaded = useAppSelector(selectPermissionsLoaded);
    const accessToken = useAppSelector((s) => s.auth.accessToken);
    const [fetchPermissions] = useLazyGetMyPermissionsQuery();

    useEffect(() => {
        if (!accessToken) {
            dispatch(clearPermissions());
            return;
        }
        if (!loaded) {
            fetchPermissions()
                .unwrap()
                .then((data) => dispatch(setPermissions(data.permissions)))
                .catch(() => dispatch(clearPermissions()));
        }
    }, [accessToken, loaded, fetchPermissions, dispatch]);

    const isSuperAdmin = permissions.includes(Permissions.RbacSuperAdmin);

    const has = useCallback(
        (permission: Permission) => isSuperAdmin || permissions.includes(permission),
        [isSuperAdmin, permissions],
    );

    const hasAny = useCallback(
        (perms: Permission[]) => isSuperAdmin || perms.some((p) => permissions.includes(p)),
        [isSuperAdmin, permissions],
    );

    const hasAll = useCallback(
        (perms: Permission[]) => isSuperAdmin || perms.every((p) => permissions.includes(p)),
        [isSuperAdmin, permissions],
    );

    return {permissions, loaded, has, hasAny, hasAll, isSuperAdmin};
};
