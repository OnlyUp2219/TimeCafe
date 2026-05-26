import {useState} from "react";
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
} from "@fluentui/react-components";
import type {VisitWithTariff} from "@app-types/visitWithTariff";

interface ApproveVisitDialogProps {
    open: boolean;
    visit: VisitWithTariff | null;
    onOpenChange: (open: boolean) => void;
    onApprove: (visitId: string) => void;
    onReject: (visitId: string, reason: string) => void;
    approving?: boolean;
    rejecting?: boolean;
}

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

    return (
        <Dialog open={open} onOpenChange={(_, data) => handleOpenChange(data.open)}>
            <DialogSurface aria-describedby={undefined}>
                <DialogBody>
                    <DialogTitle>
                        {mode === "approve" ? "Подтвердить визит" : "Отклонить визит"}
                    </DialogTitle>
                    <DialogContent>
                        {visit && (
                            <div className="flex flex-col gap-2">
                                <div className="flex items-center justify-between">
                                    <span>Визит:</span>
                                    <span className="font-semibold">{visit.tariffName}</span>
                                </div>
                                {mode === "reject" && (
                                    <Field label="Причина отклонения" required>
                                        <Input
                                            value={reason}
                                            onChange={(_, data) => setReason(data.value)}
                                            placeholder="Укажите причину"
                                        />
                                    </Field>
                                )}
                            </div>
                        )}
                    </DialogContent>
                    <DialogActions>
                        {mode === "approve" ? (
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
                        ) : (
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
                        )}
                    </DialogActions>
                </DialogBody>
            </DialogSurface>
        </Dialog>
    );
};
