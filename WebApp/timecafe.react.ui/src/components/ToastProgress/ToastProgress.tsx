import {
    Toaster,
    useToastController,
    Toast,
    ToastTitle,
    ToastBody,
    ProgressBar,
    Text,
    Link,
    useId, ToastTrigger,
} from "@fluentui/react-components";
import * as React from "react";
import "./ToastProgress.css"

type ToastIntent = "success" | "error" | "warning" | "info";

export const getToastColors = (intent: ToastIntent) => {
    switch (intent) {
        case "error":
            return "toast-error";
        case "success":
            return "toast-success";
        case "warning":
            return "toast-warning";
        case "info":
        default:
            return "toast-info";
    }
};

const intervalDelay = 100;

const ToastProgress: React.FC<{ onDownloadEnd: () => void; timeout: number }> = ({onDownloadEnd, timeout}) => {
    const [value, setValue] = React.useState(100);
    React.useEffect(() => {
        const step = 100 * (intervalDelay / timeout);
        const interval = setInterval(() => {
            setValue(v => {
                const next = v - step;
                if (next <= 0) {
                    clearInterval(interval);
                    setTimeout(() => onDownloadEnd(), 0);
                    return 0;
                }
                return next;
            });
        }, intervalDelay);

        return () => clearInterval(interval);
    }, [timeout, onDownloadEnd]);

    return <ProgressBar value={value} max={100}/>;
};

export const useProgressToast = () => {
    const toasterId = useId("toaster");
    const {dispatchToast, dismissToast} = useToastController(toasterId);
    const [limit, setLimit] = React.useState(3);

    const showToast = (
        message: string,
        intent: ToastIntent = "info",
        title?: string,
        timeout = 5000,
        limit?: number) => {

        const toastId = `toast-${Date.now()}`;
        const dismiss = () => dismissToast(toastId);
        setLimit(limit ?? 3);
        console.log(`Статус - ${intent}, стиль - ${getToastColors(intent)}`);
        console.log(`Сообщение - ${message}`);

        dispatchToast(
            <Toast className={getToastColors(intent)}>

                <ToastTitle
                    action={
                        <ToastTrigger>
                            <Link>Закрыть</Link>
                        </ToastTrigger>
                    }
                >
                    {title ?? "Уведомление"}
                </ToastTitle>
                <ToastBody>
                    <Text>{message}</Text>
                    <ToastProgress onDownloadEnd={dismiss} timeout={timeout}/>
                </ToastBody>
            </Toast>,
            {
                intent,
                timeout: timeout,
                toastId,
                position: "bottom",
            }
        );
    };

    const ToasterElement = <Toaster toasterId={toasterId} limit={limit}/>;

    return {showToast, ToasterElement};
};
