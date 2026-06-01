import { useState, useEffect } from "react";
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
    Select,
    MessageBar,
    MessageBarBody,
    Spinner,
} from "@fluentui/react-components";
import { useGetActiveTariffsQuery, useWalkInVisitMutation } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";

interface WalkInVisitDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSuccess?: () => void;
    initialResourceId?: string | null;
}

export const WalkInVisitDialog = ({ open, onOpenChange, onSuccess, initialResourceId }: WalkInVisitDialogProps) => {
    const { data: tariffs, isLoading: loadingTariffs } = useGetActiveTariffsQuery();
    const [walkInVisit, { isLoading: submitting }] = useWalkInVisitMutation();

    const [tariffId, setTariffId] = useState("");
    const [guestsCount, setGuestsCount] = useState(1);
    const [resourceId, setResourceId] = useState("");
    const [userId, setUserId] = useState("");
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (tariffs && tariffs.length > 0 && !tariffId) {
            setTariffId(tariffs[0].tariffId);
        }
    }, [tariffs, tariffId]);

    useEffect(() => {
        if (open) {
            setResourceId(initialResourceId || "");
        }
    }, [open, initialResourceId]);

    const handleOpenChange = (openValue: boolean) => {
        if (!openValue) {
            setGuestsCount(1);
            setResourceId("");
            setUserId("");
            setError(null);
            if (tariffs && tariffs.length > 0) {
                setTariffId(tariffs[0].tariffId);
            }
        }
        onOpenChange(openValue);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        if (!tariffId) {
            setError("Пожалуйста, выберите тариф");
            return;
        }

        try {
            await walkInVisit({
                tariffId,
                guestsCount,
                resourceId: resourceId.trim() || null,
                userId: userId.trim() || null,
            }).unwrap();

            handleOpenChange(false);
            if (onSuccess) {
                onSuccess();
            }
        } catch (err) {
            setError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось зарегистрировать посадку");
        }
    };

    return (
        <Dialog open={open} onOpenChange={(_, data) => handleOpenChange(data.open)}>
            <DialogSurface aria-describedby={undefined}>
                <DialogBody>
                    <DialogTitle>Быстрая посадка гостя (Walk-in)</DialogTitle>
                    <form onSubmit={handleSubmit}>
                        <DialogContent>
                            <div className="flex flex-col gap-4 py-2">
                                {error && (
                                    <MessageBar intent="error">
                                        <MessageBarBody>{error}</MessageBarBody>
                                    </MessageBar>
                                )}

                                <Field label="Тариф" required>
                                    <Select
                                        value={tariffId}
                                        onChange={(_, data) => setTariffId(data.value)}
                                        disabled={loadingTariffs || submitting}
                                    >
                                        {loadingTariffs ? (
                                            <option value="">Загрузка тарифов...</option>
                                        ) : tariffs && tariffs.length > 0 ? (
                                            tariffs.map((t) => (
                                                <option key={t.tariffId} value={t.tariffId}>
                                                    {t.name} ({t.pricePerMinute.toFixed(2)} ₽/мин)
                                                </option>
                                            ))
                                        ) : (
                                            <option value="">Нет активных тарифов</option>
                                        )}
                                    </Select>
                                </Field>

                                <Field label="Количество гостей" required>
                                    <Input
                                        type="number"
                                        min={1}
                                        value={guestsCount}
                                        onChange={(_, data) => setGuestsCount(Number(data.value))}
                                        disabled={submitting}
                                    />
                                </Field>

                                <Field label="Столик / Ресурс (ID, опционально)">
                                    <Input
                                        value={resourceId}
                                        onChange={(_, data) => setResourceId(data.value)}
                                        placeholder="Например, Стол №5"
                                        disabled={submitting}
                                    />
                                </Field>

                                <Field label="ID Пользователя (опционально, для зарегистрированных гостей)">
                                    <Input
                                        value={userId}
                                        onChange={(_, data) => setUserId(data.value)}
                                        placeholder="Введите UUID пользователя при наличии"
                                        disabled={submitting}
                                    />
                                </Field>
                            </div>
                        </DialogContent>
                        <DialogActions>
                            <Button
                                appearance="primary"
                                type="submit"
                                disabled={submitting || loadingTariffs || !tariffId}
                            >
                                {submitting ? <Spinner size="tiny" label="Оформление..." /> : "Посадить гостя"}
                            </Button>
                            <Button
                                appearance="secondary"
                                onClick={() => handleOpenChange(false)}
                                disabled={submitting}
                            >
                                Отмена
                            </Button>
                        </DialogActions>
                    </form>
                </DialogBody>
            </DialogSurface>
        </Dialog>
    );
};
