import type {FC, ReactNode} from "react";
import {usePermissions} from "@hooks/usePermissions";
import type {Permission} from "@shared/auth/permissions";

interface HasPermissionProps {
    can?: Permission;
    anyOf?: Permission[];
    allOf?: Permission[];
    fallback?: ReactNode;
    children: ReactNode;
}

export const HasPermission: FC<HasPermissionProps> = ({
    can,
    anyOf,
    allOf,
    fallback = null,
    children,
}) => {
    const {has, hasAny, hasAll, loaded} = usePermissions();

    if (!loaded) return null;

    let allowed = true;

    if (can) {
        allowed = has(can);
    } else if (anyOf) {
        allowed = hasAny(anyOf);
    } else if (allOf) {
        allowed = hasAll(allOf);
    }

    return allowed ? <>{children}</> : <>{fallback}</>;
};
