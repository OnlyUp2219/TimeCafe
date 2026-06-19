import { useState } from "react";
import {
    Button,
    Dialog,
    DialogActions,
    DialogBody,
    DialogContent,
    DialogSurface,
    DialogTitle,
    Field,
    Input,
    Divider,
    Badge,
    Caption1,
    Avatar,
    Body2,
} from "@fluentui/react-components";
import {
    Clock20Regular,
    Money20Regular,
    Person20Regular,
    Document20Regular,
    People20Regular,
    Wallet20Regular,
} from "@fluentui/react-icons";
import type { VisitWithTariff } from "@app-types/visitWithTariff";

import { VisitStatus } from "@app-types/visit";
import { useGetProfileByUserIdQuery } from "@store/api/profileApi";
import { useGetBalanceQuery } from "@store/api/billingApi";
import { useGetResourcesQuery, useGetAllPromotionsQuery, useGetUserLoyaltyQuery } from "@store/api/venueApi";

interface ApproveVisitDialogProps {
    open: boolean;
    visit: VisitWithTariff | null;
    onOpenChange: (open: boolean) => void;
    onApprove: (visitId: string) => void;
    onReject: (visitId: string, reason: string) => void;
    approving?: boolean;
    rejecting?: boolean;
}

import { formatDateTime, getRelativeTime } from "@utility/dateUtils";
import { getGuestsWord, formatRoundingRule, formatMoney } from "@utility/formatUtils";
import { getBalanceColor } from "@utility/billingUtils";
import { getUserFullName } from "@utility/userUtils";
import { calcVisitEstimate } from "@utility/visitEstimate";
import { CURRENCY_SYMBOL } from "@shared/const/currency";
import { useComponentSize } from "@hooks/useComponentSize";
import { useVisitDiscounts } from "@hooks/useVisitDiscounts";
import { VisitDiscountBreakdown } from "@components/Billing/VisitDiscountBreakdown";
import { WarningsCard } from "@components/Admin/WarningsCard/WarningsCard";

export const ApproveVisitDialog = ({
    open,
    visit,
    onOpenChange,
    onApprove,
    onReject,
    approving = false,
    rejecting = false,
}: ApproveVisitDialogProps) => {
    const [reason, setReason] = useState("");
    const [mode, setMode] = useState<"approve" | "reject">("approve");
    const { sizes } = useComponentSize();

    const { data: profile } = useGetProfileByUserIdQuery(visit?.userId ?? "", { skip: !visit?.userId });
    const { data: balanceObj } = useGetBalanceQuery(visit?.userId ?? "", { skip: !visit?.userId });
    const { data: resources } = useGetResourcesQuery();
    const { data: loyalty } = useGetUserLoyaltyQuery(visit?.userId ?? "", { skip: !visit?.userId });
    const { data: promotions } = useGetAllPromotionsQuery();
    const resource = resources?.find(r => r.resourceId === visit?.resourceId);

    const handleApprove = () => {
        if (!visit) return;
        onApprove(visit.visitId);
    };

    const handleReject = () => {
        if (!visit || !reason.trim()) return;
        onReject(visit.visitId, reason.trim());
    };

    const handleOpenChange = (openValue: boolean) => {
        if (!openValue) {
            setReason("");
            setMode("approve");
        }
        onOpenChange(openValue);
    };

    const renderActions = () => {
        if (!visit) {
            return null;
        }

        if (visit.status !== VisitStatus.Pending) {
            return (
                <Button
                    appearance="secondary"
                    size={sizes.button}
                    onClick={() => handleOpenChange(false)}
                >
                    Закрыть
                </Button>
            );
        }

        if (mode === "approve") {
            return (
                <>
                    <Button
                        appearance="primary"
                        size={sizes.button}
                        onClick={handleApprove}
                        disabled={approving}
                    >
                        {approving ? "Подтверждение..." : "Подтвердить"}
                    </Button>
                    <Button
                        appearance="secondary"
                        size={sizes.button}
                        onClick={() => setMode("reject")}
                    >
                        Отклонить
                    </Button>
                    <Button
                        appearance="subtle"
                        size={sizes.button}
                        onClick={() => handleOpenChange(false)}
                    >
                        Отмена
                    </Button>
                </>
            );
        }

        return (
            <>
                <Button
                    appearance="primary"
                    size={sizes.button}
                    onClick={handleReject}
                    disabled={rejecting || !reason.trim()}
                >
                    {rejecting ? "Отклонение..." : "Отклонить"}
                </Button>
                <Button
                    appearance="secondary"
                    size={sizes.button}
                    onClick={() => setMode("approve")}
                >
                    Назад
                </Button>
                <Button
                    appearance="subtle"
                    size={sizes.button}
                    onClick={() => handleOpenChange(false)}
                >
                    Отмена
                </Button>
            </>
        );
    };

    const userFullName = getUserFullName(profile, visit?.userId);

    const balance = balanceObj?.currentBalance ?? 0;
    const balanceColor = getBalanceColor(balance);

    const warnings: string[] = [];
    if (visit && resource?.capacity != null && (visit.guestsCount || 1) > resource.capacity) {
        warnings.push(`Превышена вместимость (макс: ${resource.capacity} чел., гостей: ${visit.guestsCount || 1})`);
    }
    if (visit?.userId && balanceObj && balance < 0) {
        warnings.push(`Отрицательный баланс (${formatMoney(balance)})`);
    }

    const { globalDiscount, tariffDiscount, personalDiscount } = useVisitDiscounts(
        promotions,
        loyalty,
        visit?.tariffId
    );

    const plannedMinutes = visit?.plannedMinutes ?? (visit?.tariffMinSessionMinutes ?? 60);
    const estimate = visit ? calcVisitEstimate(plannedMinutes, visit.tariffBillingType as any, visit.tariffPricePerMinute, visit.tariffMinSessionMinutes ?? null, visit.tariffRoundingRule ?? null, globalDiscount, tariffDiscount, personalDiscount) : null;

    return (
        <Dialog open={open} onOpenChange={(_, data) => handleOpenChange(data.open)}>
            <DialogSurface aria-describedby={undefined}>
                <DialogBody>
                    <DialogTitle>
                        {mode === "approve" ? "Подтвердить визит" : "Отклонить визит"}
                    </DialogTitle>
                    <DialogContent>
                        {visit && (
                            <div className="flex flex-col gap-4 py-2">
                                <div className="bg-(--colorNeutralBackground2) p-3 rounded-lg flex flex-col gap-2 border border-(--colorNeutralStroke2)">
                                    <div className="flex justify-between items-center">
                                        <div className="flex items-center gap-2">
                                            <Document20Regular className="text-(--colorBrandForegroundLink)" />
                                            <span className="font-semibold text-base">{visit.tariffName}</span>
                                        </div>
                                        <Badge color="brand" appearance="tint">
                                            {visit.tariffBillingType === 1 ? "Почасово" : "Поминутно"}
                                        </Badge>
                                    </div>
                                    {visit.tariffDescription && (
                                        <div className="flex">
                                            <Caption1 className="text-(--colorNeutralForeground2) italic">
                                                {visit.tariffDescription}
                                            </Caption1>
                                        </div>
                                    )}
                                    <Divider className="my-1" />
                                    <div className="flex items-center justify-between text-sm">
                                        <span className="text-(--colorNeutralForeground3) flex items-center gap-1.5">
                                            <Money20Regular /> Стоимость:
                                        </span>
                                        <span className="font-semibold text-(--colorBrandForegroundLink)">
                                            {visit.tariffPricePerMinute} {CURRENCY_SYMBOL} / мин
                                            {visit.tariffBillingType === 1 && ` (${visit.tariffPricePerMinute * 60} ${CURRENCY_SYMBOL}/ч)`}
                                        </span>
                                    </div>
                                    {(visit.tariffMinSessionMinutes || visit.tariffRoundingRule) && (
                                        <>
                                            <Divider className="my-1" />
                                            <div className="flex flex-wrap gap-2 mt-1">
                                                {visit.tariffMinSessionMinutes && (
                                                    <Badge appearance="outline" size="small">
                                                        Минимум: {visit.tariffMinSessionMinutes} мин.
                                                    </Badge>
                                                )}
                                                {visit.tariffRoundingRule && (
                                                    <Badge appearance="outline" size="small">
                                                        Округление: {formatRoundingRule(visit.tariffRoundingRule)}
                                                    </Badge>
                                                )}
                                            </div>
                                        </>
                                    )}
                                </div>

                                {visit.userId ? (
                                    <div className="bg-(--colorNeutralBackground3) p-3 rounded-lg flex flex-col gap-3 border border-(--colorNeutralStroke3)">
                                        <div className="flex items-center gap-3">
                                            <Avatar
                                                name={userFullName}
                                                image={profile?.photoUrl ? { src: profile.photoUrl } : undefined}
                                                size={36}
                                            />
                                            <div className="flex flex-col min-w-0">
                                                <span className="font-medium text-sm truncate">{userFullName}</span>
                                                <span className="text-xs text-(--colorNeutralForeground3) truncate font-mono">
                                                    ID: {visit.userId}
                                                </span>
                                            </div>
                                        </div>

                                        <Divider />

                                        <div className="flex items-center justify-between text-sm">
                                            <span className="text-(--colorNeutralForeground3) flex items-center gap-1.5">
                                                <Wallet20Regular /> Текущий баланс:
                                            </span>
                                            <Badge
                                                color={balanceColor}
                                                appearance="filled"
                                            >
                                                {formatMoney(balance)}
                                            </Badge>
                                        </div>
                                    </div>
                                ) : (
                                    <div className="bg-(--colorNeutralBackground3) p-3 rounded-lg flex items-center gap-3 border border-(--colorNeutralStroke3)">
                                        <Person20Regular className="text-(--colorNeutralForeground3)" />
                                        <Body2 className="text-(--colorNeutralForeground3)">Анонимный гость (Walk-in)</Body2>
                                    </div>
                                )}

                                {resource && (
                                    <div className="bg-(--colorNeutralBackground2) p-3 rounded-lg flex items-center justify-between text-sm border border-(--colorNeutralStroke2)">
                                        <span className="text-(--colorNeutralForeground3) flex items-center gap-1.5">
                                            <People20Regular /> Зона / Столик:
                                        </span>
                                        <div className="flex items-center gap-2">
                                            <span className="font-medium">{resource.name}</span>
                                            {resource.capacity > 0 && (
                                                <Badge appearance="outline" size="small">макс: {resource.capacity} чел.</Badge>
                                            )}
                                        </div>
                                    </div>
                                )}

                                <div className="flex flex-col gap-2.5 text-sm px-1">
                                    <div className="flex items-center justify-between">
                                        <span className="text-(--colorNeutralForeground3) flex items-center gap-1.5">
                                            <Clock20Regular /> Время запроса:
                                        </span>
                                        <div className="flex flex-col items-end">
                                            <span className="font-medium">{formatDateTime(visit.entryTime)}</span>
                                            {getRelativeTime(visit.entryTime) && (
                                                <div className="flex">
                                                    <Caption1 className="text-(--colorBrandForegroundLink)">
                                                        {getRelativeTime(visit.entryTime)}
                                                    </Caption1>
                                                </div>
                                            )}
                                        </div>
                                    </div>

                                    <div className="flex items-center justify-between">
                                        <span className="text-(--colorNeutralForeground3) flex items-center gap-1.5">
                                            <People20Regular /> Количество гостей:
                                        </span>
                                        <span className="font-medium">
                                            {visit.guestsCount} {getGuestsWord(visit.guestsCount)}
                                        </span>
                                    </div>

                                    {visit.plannedMinutes && (
                                        <div className="flex items-center justify-between">
                                            <span className="text-(--colorNeutralForeground3) flex items-center gap-1.5">
                                                <Clock20Regular /> Планируемое время:
                                            </span>
                                            <span className="font-medium">
                                                {visit.plannedMinutes} мин. (~{(visit.plannedMinutes / 60).toFixed(1)} ч.)
                                            </span>
                                        </div>
                                    )}
                                </div>

                                {estimate && (
                                    <VisitDiscountBreakdown
                                        estimate={estimate}
                                        personalDiscount={personalDiscount}
                                        globalDiscount={globalDiscount}
                                        tariffDiscount={tariffDiscount}
                                        balance={visit.userId && balanceObj ? balance : undefined}
                                    />
                                )}

                                <WarningsCard warnings={warnings} asCard={false} />

                                {mode === "reject" && (
                                    <div className="mt-2">
                                        <Field label="Причина отклонения" required size={sizes.field}>
                                            <Input
                                                size={sizes.input}
                                                value={reason}
                                                onChange={(_, data) => setReason(data.value)}
                                                placeholder="Укажите причину"
                                                className="w-full"
                                            />
                                        </Field>
                                    </div>
                                )}
                            </div>
                        )}
                    </DialogContent>
                    <DialogActions>
                        {renderActions()}
                    </DialogActions>
                </DialogBody>
            </DialogSurface>
        </Dialog>
    );
};
