import type {FC} from "react";
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
    Divider,
    Title3,
} from "@fluentui/react-components";
import {Shield20Regular} from "@fluentui/react-icons";
import {formatMoneyByN} from "@utility/formatMoney";

interface VisitEndDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onConfirm: () => void;
    tariffName: string;
    duration: string;
    estimateTotal: number;
    estimateBreakdown: string;
    confirming: boolean;
}

export const VisitEndDialog: FC<VisitEndDialogProps> = ({
    open,
    onOpenChange,
    onConfirm,
    tariffName,
    duration,
    estimateTotal,
    estimateBreakdown,
    confirming,
}) => (
    <Dialog open={open} onOpenChange={(_, data) => onOpenChange(data.open)}>
        <DialogSurface className="w-[calc(100vw-32px)] max-w-[520px]">
            <DialogBody>
                <DialogTitle data-testid="visit-end-dialog-title">
                    <div className="flex items-center gap-2">
                        <Shield20Regular/>
                        Завершить визит
                    </div>
                </DialogTitle>
                <DialogContent>
                    <div className="flex flex-col gap-4">
                        <Body1 block>
                            Подтвердите выход — будет рассчитана финальная стоимость и списан баланс.
                        </Body1>
                        <Body1 block>
                            Финальная стоимость будет рассчитана и отправлена в биллинг.
                        </Body1>

                        <Card>
                            <div className="flex flex-col gap-3">
                                <div className="flex items-center justify-between gap-3">
                                    <Body1>Тариф</Body1>
                                    <Body1>{tariffName}</Body1>
                                </div>
                                <div className="flex items-center justify-between gap-3">
                                    <Body1>Длительность</Body1>
                                    <Body1>{duration}</Body1>
                                </div>

                                <Divider/>

                                <div className="flex items-center justify-between gap-3">
                                    <Body1 block>Итого к списанию</Body1>
                                    <Title3>{formatMoneyByN(estimateTotal)}</Title3>
                                </div>
                                <Body1>{estimateBreakdown}</Body1>
                            </div>
                        </Card>
                    </div>
                </DialogContent>

                <DialogActions>
                    <Button appearance="secondary" data-testid="visit-end-cancel" onClick={() => onOpenChange(false)}>
                        Отмена
                    </Button>
                    <Button
                        appearance="primary"
                        data-testid="visit-end-confirm"
                        disabled={confirming}
                        onClick={onConfirm}
                    >
                        {confirming ? "Завершение..." : "Завершить визит"}
                    </Button>
                </DialogActions>
            </DialogBody>
        </DialogSurface>
    </Dialog>
);
