import type { FC } from "react";
import { Body1, Body2, Card } from "@fluentui/react-components";
import { Warning20Regular } from "@fluentui/react-icons";
import { useComponentSize } from "@hooks/useComponentSize";

interface WarningsCardProps {
    warnings: string[];
    className?: string;
    asCard?: boolean;
}

export const WarningsCard: FC<WarningsCardProps> = ({ warnings, className, asCard = true }) => {
    const { sizes } = useComponentSize();
    if (warnings.length === 0) return null;

    const content = (
        <div className="flex flex-col gap-2">
            <Body2 className="font-semibold flex items-center gap-2">
                <Warning20Regular className="text-(--colorStatusWarningForeground1)" />
                <span>Предупреждения</span>
            </Body2>
            <div className="flex flex-col gap-1">
                {warnings.map((w, i) => (
                    <Body1 key={i} className="text-(--colorStatusWarningForeground1)">• {w}</Body1>
                ))}
            </div>
        </div>
    );

    if (asCard) {
        return (
            <Card
                size={sizes.card}
                className={`border-(--colorStatusWarningBorderActive) bg-(--colorStatusWarningBackground1) ${className || ""}`}
            >
                {content}
            </Card>
        );
    }

    return (
        <div
            className={`border border-(--colorStatusWarningBorderActive) bg-(--colorStatusWarningBackground1) p-3 rounded-lg flex flex-col gap-2 ${className || ""}`}
        >
            {content}
        </div>
    );
};
