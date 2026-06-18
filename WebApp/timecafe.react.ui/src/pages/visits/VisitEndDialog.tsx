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
    Divider,
    Title3,
    Switch,
    Body1Stronger,
} from "@fluentui/react-components";
import { Shield20Regular, Wallet20Regular } from "@fluentui/react-icons";
import { formatMoneyByN } from "@utility/formatMoney";

interface VisitEndDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onConfirm: (payFromBalance: boolean) => void;
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
    userBalance: number;
    payFromBalance: boolean;
    onPayFromBalanceChange: (checked: boolean) => void;
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
    userBalance,
    payFromBalance,
    onPayFromBalanceChange,
}) => {
    const isBalanceSufficient = userBalance >= estimateTotal;

    return (
        <Dialog open={open} onOpenChange={(_, data) => onOpenChange(data.open)}>
            <DialogSurface className="w-[calc(100vw-32px)] max-w-[520px]">
                <DialogBody>
                    <DialogTitle data-testid="visit-end-dialog-title">
                        <div className="flex items-center gap-2">
                            <Shield20Regular />
                            Завершить визит
                        </div>
                    </DialogTitle>
                    <DialogContent>
                        <div className="flex flex-col gap-4">
                            <Body1 block>
                                Подтвердите выход — таймер продолжит идти до фиксации администратором на кассе.
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

                                    <Divider />

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
                                                    <div className="flex justify-between items-center text-(--colorNeutralForeground2)">
                                                        <Body1>Скидка лояльности:</Body1>
                                                        <Body1Stronger>-{personalDiscount}%</Body1Stronger>
                                                    </div>
                                                )}
                                                {Math.max(globalDiscount, tariffDiscount) > 0 && (
                                                    <div className="flex justify-between items-center text-(--colorNeutralForeground2)">
                                                        <Body1>Акционная скидка:</Body1>
                                                        <Body1Stronger>-{Math.max(globalDiscount, tariffDiscount)}%</Body1Stronger>
                                                    </div>
                                                )}
                                                <Divider className="my-1" />
                                                <div className="flex justify-between items-center text-(--colorBrandForeground1)">
                                                    <Body1Stronger>Итоговая скидка:</Body1Stronger>
                                                    <Body1Stronger>-{estimateAppliedDiscountPercent}% (-{formatMoneyByN(estimateDiscountTotal)})</Body1Stronger>
                                                </div>
                                            </div>
                                        )}
                                    </div>

                                    <Divider />

                                    <div className="flex items-center justify-between gap-3 bg-(--colorNeutralBackground2) p-3 rounded">
                                        <div className="flex items-center gap-2">
                                            <Wallet20Regular />
                                            <div className="flex flex-col">
                                                <Body1>Списать с баланса</Body1>
                                                <span className="text-xs text-(--colorNeutralForeground3)">Баланс: {formatMoneyByN(userBalance)}</span>
                                            </div>
                                        </div>
                                        <Switch
                                            checked={payFromBalance}
                                            onChange={(_, data) => onPayFromBalanceChange(data.checked)}
                                            disabled={!isBalanceSufficient}
                                        />
                                    </div>
                                    {!isBalanceSufficient && (
                                        <div className="text-xs text-(--colorPaletteRedForeground1)">
                                            Недостаточно средств на балансе для автоматического списания.
                                        </div>
                                    )}
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
                            onClick={() => onConfirm(payFromBalance)}
                        >
                            {confirming ? "Завершение..." : "Подтвердить выход"}
                        </Button>
                    </DialogActions>
                </DialogBody>
            </DialogSurface>
        </Dialog>
    );
};

