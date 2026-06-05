import type { FC } from "react";
import {
    Body1,
    Button,
    Card,
    Dialog,
    DialogActions,
    DialogBody,
    DialogContent,
    DialogSurface,
    DialogTitle,
} from "@fluentui/react-components";
import { Dismiss20Regular } from "@fluentui/react-icons";
import type { VisitWithTariff } from "@app-types/visitWithTariff";

interface VisitCancelDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onConfirm: () => void;
    visit: VisitWithTariff | null;
    cancelling: boolean;
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

const getGuestsWord = (count: number) => {
    const mod10 = count % 10;
    const mod100 = count % 100;
    if (mod10 === 1 && mod100 !== 11) return "гость";
    if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return "гостя";
    return "гостей";
};

export const VisitCancelDialog: FC<VisitCancelDialogProps> = ({
    open,
    onOpenChange,
    onConfirm,
    visit,
    cancelling,
}) => {
    if (!visit) return null;

    return (
        <Dialog open={open} onOpenChange={(_, data) => onOpenChange(data.open)}>
            <DialogSurface className="w-[calc(100vw-32px)] max-w-[520px]">
                <DialogBody>
                    <DialogTitle>
                        <div className="flex items-center gap-2">
                            <Dismiss20Regular />
                            Отменить заявку на визит
                        </div>
                    </DialogTitle>
                    <DialogContent>
                        <div className="flex flex-col gap-4 py-1">
                            <Body1 block>
                                Вы действительно хотите отменить вашу текущую заявку на визит? Это действие нельзя отменить.
                            </Body1>

                            <Card>
                                <div className="flex flex-col gap-3 text-sm">
                                    <div className="flex items-center justify-between gap-3">
                                        <span className="text-(--colorNeutralForeground3)">Тариф:</span>
                                        <span className="font-semibold">{visit.tariffName}</span>
                                    </div>
                                    <div className="flex items-center justify-between gap-3">
                                        <span className="text-(--colorNeutralForeground3)">Время запроса:</span>
                                        <span className="font-medium">{formatDateTime(visit.entryTime)}</span>
                                    </div>
                                    <div className="flex items-center justify-between gap-3">
                                        <span className="text-(--colorNeutralForeground3)">Количество гостей:</span>
                                        <span className="font-medium">
                                            {visit.guestsCount} {getGuestsWord(visit.guestsCount)}
                                        </span>
                                    </div>
                                    {visit.plannedMinutes && (
                                        <div className="flex items-center justify-between gap-3">
                                            <span className="text-(--colorNeutralForeground3)">Запланировано:</span>
                                            <span className="font-medium">
                                                {visit.plannedMinutes} мин. (~{(visit.plannedMinutes / 60).toFixed(1)} ч.)
                                            </span>
                                        </div>
                                    )}
                                </div>
                            </Card>
                        </div>
                    </DialogContent>

                    <DialogActions>
                        <Button appearance="secondary" onClick={() => onOpenChange(false)}>
                            Назад
                        </Button>
                        <Button
                            appearance="primary"
                            disabled={cancelling}
                            onClick={onConfirm}
                        >
                            {cancelling ? "Отмена..." : "Отменить заявку"}
                        </Button>
                    </DialogActions>
                </DialogBody>
            </DialogSurface>
        </Dialog>
    );
};
