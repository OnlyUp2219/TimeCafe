import type {FC, ReactNode} from "react";
import {usePermissions} from "@hooks/usePermissions";
import type {Permission} from "@shared/auth/permissions";

interface RequirePermissionProps {
    permission?: Permission;
    can?: Permission;
    any?: Permission[];
    anyOf?: Permission[];
    all?: Permission[];
    fallback?: ReactNode;
    children: ReactNode;
}

export const RequirePermission: FC<RequirePermissionProps> = ({
    permission,
    can,
    any,
    anyOf,
    all,
    fallback = null,
    children,
}) => {
    const {has, hasAny, hasAll, loaded} = usePermissions();

    if (!loaded) return null;

    const perm = permission ?? can;
    const anyPerms = any ?? anyOf;

    if (perm && !has(perm)) return <>{fallback}</>;
    if (anyPerms && !hasAny(anyPerms)) return <>{fallback}</>;
    if (all && !hasAll(all)) return <>{fallback}</>;

    return <>{children}</>;
};
