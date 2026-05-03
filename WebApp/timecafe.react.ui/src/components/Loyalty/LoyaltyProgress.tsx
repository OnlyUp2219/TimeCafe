import React from 'react';
import { ProgressBar, Tooltip, Body1, Body1Strong, Body2, Caption1 } from '@fluentui/react-components';

export const LOYALTY_TIERS = [
    { requiredVisits: 5, discount: 5, name: "Бронза" },
    { requiredVisits: 10, discount: 10, name: "Серебро" },
    { requiredVisits: 20, discount: 15, name: "Золото" }
];

export interface LoyaltyProgressProps {
    visitCount: number;
    currentDiscount: number;
    className?: string;
}

export const LoyaltyProgress: React.FC<LoyaltyProgressProps> = ({ visitCount, currentDiscount, className }) => {
    const nextTier = LOYALTY_TIERS.find(t => t.requiredVisits > visitCount);

    if (!nextTier) {
        return (
            <div className={`flex flex-col gap-2 ${className || ''}`}>
                <div className="flex justify-between items-end">
                    <Body1Strong>Максимальная скидка!</Body1Strong>
                    <Body1>{currentDiscount}%</Body1>
                </div>
                <ProgressBar value={1} max={1} thickness="large" color="success" />
                <Caption1>
                    У вас максимальный уровень лояльности ({visitCount} визитов)
                </Caption1>
            </div>
        );
    }

    const prevTier = [...LOYALTY_TIERS].reverse().find(t => t.requiredVisits <= visitCount) || { requiredVisits: 0, discount: 0 };

    const visitsInCurrentLevel = visitCount - prevTier.requiredVisits;
    const visitsNeededForNextLevel = nextTier.requiredVisits - prevTier.requiredVisits;
    const progress = visitsInCurrentLevel / visitsNeededForNextLevel;

    return (
        <div className={`flex flex-col gap-2 ${className || ''}`}>
            <div className="flex justify-between items-end">
                <Body2>
                    {nextTier.name} (Скидка {nextTier.discount}%)
                </Body2>
                <Body2>
                    {visitCount} / {nextTier.requiredVisits} визитов
                </Body2>
            </div>

            <Tooltip content={`Осталось ${nextTier.requiredVisits - visitCount} визитов до скидки ${nextTier.discount}%`} relationship="label">
                <ProgressBar value={progress} max={1} thickness="large" color="brand" />
            </Tooltip>

            <Caption1>
                Текущая скидка: {currentDiscount > 0 ? `${currentDiscount}%` : 'Нет'}
            </Caption1>
        </div>
    );
};
