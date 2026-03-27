import type {FC} from "react";
import {Button, Divider, Subtitle2Stronger, Tag, Text} from "@fluentui/react-components";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";

interface QuickActionsCardProps {
    onNavigateProfile: () => void;
    onNavigateBilling: () => void;
}

export const QuickActionsCard: FC<QuickActionsCardProps> = ({onNavigateProfile, onNavigateBilling}) => (
    <HoverTiltCard className="flex flex-col justify-between lg:col-span-5">
        <div className="flex flex-col gap-4">
            <div className="flex items-center gap-2 flex-wrap justify-between">
                <Subtitle2Stronger>Быстрые действия</Subtitle2Stronger>
                <Tag appearance="outline">Shortcuts</Tag>
            </div>
            <Divider className="divider grow-0"/>
        </div>

        <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
            <Button appearance="primary" onClick={onNavigateProfile}>
                <Text truncate wrap={false}>Профиль</Text>
            </Button>
            <Button appearance="secondary" onClick={onNavigateProfile}>
                Безопасность
            </Button>
            <Button appearance="secondary" onClick={onNavigateBilling}>
                <Text truncate wrap={false}>Баланс</Text>
            </Button>
            <Button appearance="secondary" onClick={onNavigateBilling}>
                Операции
            </Button>
        </div>
    </HoverTiltCard>
);
