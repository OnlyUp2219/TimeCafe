import type { FC } from "react";
import { Button, Card, Title2, Body2 } from "@fluentui/react-components";
import { DismissRegular, ArrowLeftRegular } from "@fluentui/react-icons";

interface RejectedVisitStateProps {
    sizes: any;
    rejectionReason?: string;
    onGoToStart: () => void;
}

export const RejectedVisitState: FC<RejectedVisitStateProps> = ({
    sizes,
    rejectionReason,
    onGoToStart,
}) => {
    let reasonElement = null;
    if (rejectionReason) {
        reasonElement = <Body2>Причина: {rejectionReason}</Body2>;
    }

    return (
        <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center">
            <DismissRegular className="opacity-50 text-(--colorPaletteRedForeground1)" />
            <Title2>Заявка отклонена</Title2>
            {reasonElement}
            <Button appearance="primary" size={sizes.button} onClick={onGoToStart} icon={<ArrowLeftRegular />}>
                К выбору тарифа
            </Button>
        </Card>
    );
};
