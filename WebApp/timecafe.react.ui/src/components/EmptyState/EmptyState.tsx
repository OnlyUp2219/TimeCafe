import type {FC} from "react";
import {Body1, Caption1} from "@fluentui/react-components";
import {DatabaseSearch20Regular} from "@fluentui/react-icons";

interface EmptyStateProps {
    title?: string;
    description?: string;
    icon?: React.ReactElement;
}

export const EmptyState: FC<EmptyStateProps> = ({
    title = "Нет данных",
    description,
    icon = <DatabaseSearch20Regular style={{fontSize: 40, opacity: 0.4}} />,
}) => (
    <div
        style={{minHeight: 260}}
        className="flex flex-col items-center justify-center gap-2 text-center py-8"
    >
        <span className="text-[var(--colorNeutralForeground3)]">{icon}</span>
        <Body1 style={{color: "var(--colorNeutralForeground3)"}}>{title}</Body1>
        {description && (
            <Caption1 style={{color: "var(--colorNeutralForeground4)"}}>{description}</Caption1>
        )}
    </div>
);
