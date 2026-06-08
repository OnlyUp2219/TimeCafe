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
    estimateBaseTotal: number;
    estimateDiscountTotal: number;
    estimateAppliedDiscountPercent: number;
    estimateIsDiscounted: boolean;
    personalDiscount: number;
    globalDiscount: number;
    tariffDiscount: number;
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
    estimateBaseTotal,
    estimateDiscountTotal,
    estimateAppliedDiscountPercent,
    estimateIsDiscounted,
    personalDiscount,
    globalDiscount,
    tariffDiscount,
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
                                    <div className="flex flex-col items-end gap-1">
                                        {estimateIsDiscounted && (
                                            <span className="line-through text-(--colorNeutralForeground3) text-sm">
                                                {formatMoneyByN(estimateBaseTotal)}
                                            </span>
                                        )}
                                        <Title3>{formatMoneyByN(estimateTotal)}</Title3>
                                    </div>
                                </div>
                                <div className="flex flex-col gap-1 items-end w-full">
                                    <Body1 className="text-right">{estimateBreakdown}</Body1>
                                    {estimateIsDiscounted && (
                                        <div className="flex flex-col gap-1 text-xs bg-(--colorNeutralBackground3) p-2 rounded border border-(--colorNeutralStroke3) mt-2 w-full text-left">
                                            {personalDiscount > 0 && (
                                                <div className="flex justify-between text-(--colorNeutralForeground2)">
                                                    <span>Скидка лояльности:</span>
                                                    <span className="font-semibold">-{personalDiscount}%</span>
                                                </div>
                                            )}
                                            {Math.max(globalDiscount, tariffDiscount) > 0 && (
                                                <div className="flex justify-between text-(--colorNeutralForeground2)">
                                                    <span>Акционная скидка:</span>
                                                    <span className="font-semibold">-{Math.max(globalDiscount, tariffDiscount)}%</span>
                                                </div>
                                            )}
                                            <Divider className="my-1" />
                                            <div className="flex justify-between text-(--colorBrandForeground1) font-semibold">
                                                <span>Итоговая скидка:</span>
                                                <span>-{estimateAppliedDiscountPercent}% (-{formatMoneyByN(estimateDiscountTotal)})</span>
                                            </div>
                                        </div>
                                    )}
                                </div>
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
