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
import { BillingType } from "@app-types/tariff";
import { VisitStatus } from "@app-types/visit";
import { useGetProfileByUserIdQuery } from "@store/api/profileApi";
import { useGetBalanceQuery } from "@store/api/billingApi";
import { useGetResourcesQuery } from "@store/api/venueApi";

interface ApproveVisitDialogProps {
    open: boolean;
    visit: VisitWithTariff | null;
    onOpenChange: (open: boolean) => void;
    onApprove: (visitId: string) => void;
    onReject: (visitId: string, reason: string) => void;
    approving?: boolean;
    rejecting?: boolean;
}

const formatDateTime = (iso: string | null) => {
    if (!iso) return "—";
    const d = new Date(iso);
    return d.toLocaleString("ru-RU", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit"
    });
};

const getRelativeTime = (iso: string | null) => {
    if (!iso) return "";
    const diffMs = Date.now() - new Date(iso).getTime();
    const diffMins = Math.floor(diffMs / 60000);
    if (diffMins < 1) return "только что";
    if (diffMins < 60) return `${diffMins} мин. назад`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} ч. назад`;
    return "";
};

const getGuestsWord = (count: number) => {
    const mod10 = count % 10;
    const mod100 = count % 100;
    if (mod10 === 1 && mod100 !== 11) return "гость";
    if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return "гостя";
    return "гостей";
};

const formatRoundingRule = (rule: string | null | undefined) => {
    if (!rule) return "нет";
    if (rule === "FiveMinutes") return "до 5 мин.";
    if (rule === "FifteenMinutes") return "до 15 мин.";
    if (rule === "SixtyMinutes") return "до 1 ч.";
    return rule;
};

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

    const { data: profile } = useGetProfileByUserIdQuery(visit?.userId ?? "", { skip: !visit?.userId });
    const { data: balanceObj } = useGetBalanceQuery(visit?.userId ?? "", { skip: !visit?.userId });
    const { data: resources } = useGetResourcesQuery();
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
                        onClick={handleApprove}
                        disabled={approving}
                    >
                        {approving ? "Подтверждение..." : "Подтвердить"}
                    </Button>
                    <Button
                        appearance="secondary"
                        onClick={() => setMode("reject")}
                    >
                        Отклонить
                    </Button>
                    <Button
                        appearance="subtle"
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
                    onClick={handleReject}
                    disabled={rejecting || !reason.trim()}
                >
                    {rejecting ? "Отклонение..." : "Отклонить"}
                </Button>
                <Button
                    appearance="secondary"
                    onClick={() => setMode("approve")}
                >
                    Назад
                </Button>
                <Button
                    appearance="subtle"
                    onClick={() => handleOpenChange(false)}
                >
                    Отмена
                </Button>
            </>
        );
    };

    const userFullName = profile && (profile.firstName || profile.lastName)
        ? [profile.firstName, profile.middleName, profile.lastName].filter(Boolean).join(" ")
        : "Имя не указано";

    const balance = balanceObj?.currentBalance ?? 0;
    const isNegativeBalance = balance < 0;
    const isZeroBalance = balance === 0;

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
                                            {visit.tariffBillingType === 0 ? "Поминутно" : "Почасово"}
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
                                            {visit.tariffPricePerMinute} ₽ / мин
                                            {visit.tariffBillingType === 1 && ` (${visit.tariffPricePerMinute * 60} ₽/ч)`}
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
                                            color={isNegativeBalance ? "danger" : isZeroBalance ? "warning" : "success"}
                                            appearance="filled"
                                        >
                                            {balance.toLocaleString("ru-RU")} ₽
                                        </Badge>
                                    </div>
                                </div>

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

                                    {visit.resourceId && (
                                        <div className="flex items-center justify-between">
                                            <span className="text-(--colorNeutralForeground3) flex items-center gap-1.5">
                                                <People20Regular /> Столик / Зона:
                                            </span>
                                            <span className="font-medium">
                                                {resource ? resource.name : `ID: ${visit.resourceId.slice(0, 8)}`}
                                            </span>
                                        </div>
                                    )}

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

                                {mode === "reject" && (
                                    <div className="mt-2">
                                        <Field label="Причина отклонения" required>
                                            <Input
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
