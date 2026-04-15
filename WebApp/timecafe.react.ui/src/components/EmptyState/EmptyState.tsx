import type {FC} from "react";
import {Subtitle2, Caption1} from "@fluentui/react-components";
import {DatabaseSearchRegular} from "@fluentui/react-icons";

interface EmptyStateProps {
    title?: string;
    description?: string;
    icon?: React.ReactElement;
}

export const EmptyState: FC<EmptyStateProps> = ({
    title = "Нет данных",
    description,
    icon = <DatabaseSearchRegular style={{fontSize: 56, opacity: 0.35}} />,
}) => (
    <div
        style={{minHeight: 260}}
        className="flex flex-col items-center justify-center gap-3 text-center py-10"
    >
        <span style={{color: "var(--colorNeutralForeground3)", display: "flex"}}>{icon}</span>
        <Subtitle2 style={{color: "var(--colorNeutralForeground3)"}}>{title}</Subtitle2>
        {description && (
            <Caption1 style={{color: "var(--colorNeutralForeground4)"}}>{description}</Caption1>
        )}
    </div>
);
