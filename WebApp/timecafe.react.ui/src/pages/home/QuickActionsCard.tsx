import type { FC } from "react";
import { Button, Divider, Subtitle2Stronger, Tag, Text } from "@fluentui/react-components";
import { HoverTiltCard } from "@components/HoverTiltCard/HoverTiltCard";
import { useComponentSize } from "@hooks/useComponentSize";
import {
    Flash20Regular,
    Person20Regular,
    ShieldKeyhole20Regular,
    Money20Regular,
    History20Regular
} from "@fluentui/react-icons";

interface QuickActionsCardProps {
    onNavigateProfile: () => void;
    onNavigateBilling: () => void;
}

export const QuickActionsCard: FC<QuickActionsCardProps> = ({ onNavigateProfile, onNavigateBilling }) => {
    const { sizes } = useComponentSize();

    return (
        <HoverTiltCard className="flex flex-col justify-start gap-4 lg:col-span-5 min-h-[320px]" size={sizes.card}>
            <div className="flex flex-col gap-4">
                <div className="flex items-center gap-2 flex-wrap justify-between">
                    <div className="flex items-center gap-2">
                        <Flash20Regular className="text-[var(--colorStatusWarningForeground1)]" />
                        <Subtitle2Stronger>Быстрые действия</Subtitle2Stronger>
                    </div>
                    <Tag appearance="outline">Shortcuts</Tag>
                </div>
                <Divider className="divider grow-0" />
            </div>

            <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
                <Button appearance="primary" size={sizes.button} icon={<Person20Regular />} onClick={onNavigateProfile}>
                    <Text truncate wrap={false}>Профиль</Text>
                </Button>
                <Button appearance="outline" size={sizes.button} icon={<ShieldKeyhole20Regular className="text-[var(--colorBrandForeground1)]" />} onClick={onNavigateProfile}>
                    <Text truncate wrap={false}>Безопасность</Text>
                </Button>
                <Button appearance="outline" size={sizes.button} icon={<Money20Regular className="text-[var(--colorStatusSuccessForeground1)]" />} onClick={onNavigateBilling}>
                    <Text truncate wrap={false}>Баланс</Text>
                </Button>
                <Button appearance="outline" size={sizes.button} icon={<History20Regular className="text-[var(--colorBrandForeground1)]" />} onClick={onNavigateBilling}>
                    <Text truncate wrap={false}>Операции</Text>
                </Button>
            </div>
        </HoverTiltCard>
    );
};
