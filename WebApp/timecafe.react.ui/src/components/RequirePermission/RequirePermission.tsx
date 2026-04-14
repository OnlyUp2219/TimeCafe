import type {FC, ReactNode} from "react";
import {usePermissions} from "@hooks/usePermissions";
import type {Permission} from "@shared/auth/permissions";

interface RequirePermissionProps {
    permission?: Permission;
    any?: Permission[];
    all?: Permission[];
    fallback?: ReactNode;
    children: ReactNode;
}

export const RequirePermission: FC<RequirePermissionProps> = ({
    permission,
    any,
    all,
    fallback = null,
    children,
}) => {
    const {has, hasAny, hasAll, loaded} = usePermissions();

    if (!loaded) return null;

    if (permission && !has(permission)) return <>{fallback}</>;
    if (any && !hasAny(any)) return <>{fallback}</>;
    if (all && !hasAll(all)) return <>{fallback}</>;

    return <>{children}</>;
};
