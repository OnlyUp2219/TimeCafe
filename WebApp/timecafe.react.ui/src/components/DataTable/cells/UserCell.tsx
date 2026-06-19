import { Avatar, Body1, Body2, Caption1 } from "@fluentui/react-components";
import { TableCellLayout } from "@fluentui/react-components";
import { useGetProfileByUserIdQuery, useGetProfileByUserIdReadOnlyQuery } from "@store/api/profileApi";
import { getUserFullName } from "@utility/userUtils";

interface UserCellProps {
    userId: string | null;
    variant?: "compact" | "detailed";
    readOnly?: boolean;
    showAvatar?: boolean;
}

export const UserCell = ({ userId, variant = "compact", readOnly = false, showAvatar = false }: UserCellProps) => {
    const skip = !userId;
    const { data: writableProfile } = useGetProfileByUserIdQuery(userId ?? "", { skip: skip || readOnly });
    const { data: readOnlyProfile } = useGetProfileByUserIdReadOnlyQuery(userId ?? "", { skip: skip || !readOnly });
    const profile = readOnly ? readOnlyProfile : writableProfile;

    if (!userId) {
        return (
            <TableCellLayout truncate title="Анонимный гость (Walk-in)">
                <Body2 className="text-xs text-(--colorNeutralForeground3)">Анонимный гость (Walk-in)</Body2>
            </TableCellLayout>
        );
    }

    const displayName = getUserFullName(profile, userId);
    const title = `${displayName} (ID: ${userId})`;

    if (variant === "detailed") {
        return (
            <TableCellLayout
                truncate
                media={showAvatar ? <Avatar name={displayName || userId} size={28} /> : undefined}
                title={title}
            >
                <div className="flex flex-col min-w-0">
                    <Body1 truncate>{displayName || userId}</Body1>
                    <Caption1 className="font-mono text-(--colorNeutralForeground4)" style={{ fontSize: "10px" }}>
                        {userId}
                    </Caption1>
                </div>
            </TableCellLayout>
        );
    }

    return (
        <TableCellLayout truncate title={title}>
            <Body2 className="text-xs">{profile ? displayName : `${userId.slice(0, 8)}…`}</Body2>
        </TableCellLayout>
    );
};
