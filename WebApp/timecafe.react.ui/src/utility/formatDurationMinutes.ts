export const formatDurationMinutes = (totalMinutes: number) => {
    const minutes = Math.max(0, Math.floor(totalMinutes));
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;

    if (h <= 0) return `${m} мин`;
    if (m <= 0) return `${h} ч`;
    return `${h} ч ${m} мин`;
};
