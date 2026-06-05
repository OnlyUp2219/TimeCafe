import {Badge} from "@fluentui/react-components";
import type {VisitStatus} from "@app-types/visit";

interface VisitStatusBadgeProps {
    status: VisitStatus;
    size?: "small" | "medium" | "large" | "extra-large";
}

const statusConfig: Record<VisitStatus, {label: string; color: "brand" | "danger" | "success" | "warning" | "important" | "informative"}> = {
    0: {label: "Ожидает подтверждения", color: "warning"},
    1: {label: "Подтверждён", color: "brand"},
    2: {label: "Отклонён", color: "danger"},
    3: {label: "Активен", color: "success"},
    4: {label: "Завершён", color: "informative"},
    5: {label: "Отменён", color: "important"},
    6: {label: "Ожидает оплаты", color: "warning"},
};

export const VisitStatusBadge = ({status, size = "medium"}: VisitStatusBadgeProps) => {
    const config = statusConfig[status] ?? {label: "Неизвестно", color: "important"};
    return (
        <Badge appearance="filled" color={config.color} size={size}>
            {config.label}
        </Badge>
    );
};
