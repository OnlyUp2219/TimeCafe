import type { FC } from "react";
import { Card, Title2, Body2 } from "@fluentui/react-components";
import { Clock24Regular } from "@fluentui/react-icons";
import { VisitStatusBadge } from "@components/VisitStatusBadge";
import { VisitStatus } from "@app-types/visit";

interface ApprovedVisitStateProps {
    sizes: any;
}

export const ApprovedVisitState: FC<ApprovedVisitStateProps> = ({
    sizes,
}) => {
    return (
        <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center">
            <Clock24Regular className="opacity-50 text-(--colorPaletteGreenForeground1)" />
            <Title2>Визит подтверждён</Title2>
            <Body2>Можете входить в заведение!</Body2>
            <VisitStatusBadge status={VisitStatus.Approved} size="large" />
        </Card>
    );
};
