import React from 'react';
import { Badge, Tooltip } from '@fluentui/react-components';
import { StarRegular, StarFilled } from '@fluentui/react-icons';

export interface LoyaltyBadgeProps {
    discountPercent: number;
    className?: string;
    size?: 'small' | 'medium' | 'large' | 'extra-large';
}

export const LoyaltyBadge: React.FC<LoyaltyBadgeProps> = ({ discountPercent, className, size = 'medium' }) => {
    let tierName = "Нет скидки";
    let color: "danger" | "important" | "informative" | "severe" | "subtle" | "success" | "warning" | "brand" = "subtle";
    let Icon = StarRegular;

    if (discountPercent >= 15) {
        tierName = "Золотой уровень";
        color = "warning";
        Icon = StarFilled;
    } else if (discountPercent >= 10) {
        tierName = "Серебряный уровень";
        color = "informative";
        Icon = StarFilled;
    } else if (discountPercent >= 5) {
        tierName = "Бронзовый уровень";
        color = "important";
        Icon = StarFilled;
    }

    if (discountPercent === 0) {
        return (
            <Badge appearance="outline" color={color} className={className} size={size}>
                0% скидка
            </Badge>
        );
    }

    return (
        <Tooltip content={tierName} relationship="label">
            <Badge appearance="filled" color={color} icon={<Icon />} className={className} size={size}>
                Скидка {discountPercent}%
            </Badge>
        </Tooltip>
    );
};
