import type { FC } from "react";
import { Dialog, DialogTrigger, DialogSurface, DialogBody, DialogContent, DialogTitle, Button } from "@fluentui/react-components";
import { Dismiss24Regular } from "@fluentui/react-icons";
import { VirtualReceipt } from "./VirtualReceipt";
import type { Invoice } from "@store/api/billingApi";

export interface VirtualReceiptDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    invoice: Invoice;
    tariffName?: string;
}

export const VirtualReceiptDialog: FC<VirtualReceiptDialogProps> = ({ open, onOpenChange, invoice, tariffName }) => {
    if (!invoice.fiscalReceiptNumber) return null;

    return (
        <Dialog open={open} onOpenChange={(_, data) => onOpenChange(data.open)}>
            <DialogSurface>
                <DialogBody>
                    <DialogTitle
                        action={
                            <DialogTrigger action="close">
                                <Button
                                    appearance="subtle"
                                    aria-label="close"
                                    icon={<Dismiss24Regular />}
                                />
                            </DialogTrigger>
                        }
                    >
                        Электронный чек
                    </DialogTitle>
                    <DialogContent>
                        <VirtualReceipt
                            receiptNumber={invoice.fiscalReceiptNumber}
                            amount={invoice.totalAmount}
                            date={invoice.paidAt || invoice.createdAt}
                            tariffName={tariffName}
                            visitId={invoice.visitId}
                        />
                    </DialogContent>
                </DialogBody>
            </DialogSurface>
        </Dialog>
    );
};
