import { useState, useEffect, useMemo } from "react";
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
    Dropdown,
    Option,
    MessageBar,
    MessageBarBody,
    Spinner,
} from "@fluentui/react-components";
import { useGetActiveTariffsQuery, useWalkInVisitMutation, useGetResourcesQuery, useGetActiveVisitsQuery } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { CURRENCY_SYMBOL } from "@shared/const/currency";

interface WalkInVisitDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSuccess?: () => void;
    initialResourceId?: string | null;
}

export const WalkInVisitDialog = ({ open, onOpenChange, onSuccess, initialResourceId }: WalkInVisitDialogProps) => {
    const { data: tariffs, isLoading: loadingTariffs } = useGetActiveTariffsQuery();
    const [walkInVisit, { isLoading: submitting }] = useWalkInVisitMutation();
    const { data: resources, isLoading: loadingResources } = useGetResourcesQuery();
    const { data: activeVisits } = useGetActiveVisitsQuery();

    const [tariffId, setTariffId] = useState("");
    const [guestsCount, setGuestsCount] = useState(1);
    const [resourceId, setResourceId] = useState("");
    const [userId, setUserId] = useState("");
    const [error, setError] = useState<string | null>(null);

    const selectedTariff = useMemo(() => tariffs?.find(t => t.tariffId === tariffId), [tariffs, tariffId]);
    const maxGuestsLimit = selectedTariff?.maxGuests ?? 10;

    const freeResources = useMemo(() => {
        if (!resources) return [];
        const activeResourceIds = new Set(
            (activeVisits ?? [])
                .map((v) => v.resourceId)
                .filter((id): id is string => !!id)
        );
        return resources.filter(
            (res) => res.isActive && (!activeResourceIds.has(res.resourceId) || res.resourceId === initialResourceId)
        );
    }, [resources, activeVisits, initialResourceId]);

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

    useEffect(() => {
        if (selectedTariff) {
            const maxGuests = selectedTariff.maxGuests ?? 10;
            if (guestsCount > maxGuests) {
                setGuestsCount(maxGuests);
            }
        }
    }, [selectedTariff, guestsCount]);

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

    const handleSubmit = async () => {
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
                    <DialogContent>
                        <div className="flex flex-col gap-4 py-2">
                            {error && (
                                <MessageBar intent="error">
                                    <MessageBarBody>{error}</MessageBarBody>
                                </MessageBar>
                            )}

                            <Field label="Тариф" required>
                                <Dropdown
                                    value={selectedTariff ? `${selectedTariff.name} (${selectedTariff.pricePerMinute.toFixed(2)} ${CURRENCY_SYMBOL}/мин)` : (loadingTariffs ? "Загрузка тарифов..." : "Выберите тариф")}
                                    selectedOptions={tariffId ? [tariffId] : []}
                                    onOptionSelect={(_, data) => setTariffId(data.optionValue || "")}
                                    disabled={loadingTariffs || submitting}
                                >
                                    {tariffs && tariffs.length > 0 ? (
                                        tariffs.map((t) => (
                                            <Option key={t.tariffId} value={t.tariffId} text={`${t.name} (${t.pricePerMinute.toFixed(2)} ${CURRENCY_SYMBOL}/мин)`}>
                                                {t.name} ({t.pricePerMinute.toFixed(2)} {CURRENCY_SYMBOL}/мин)
                                            </Option>
                                        ))
                                    ) : (
                                        <Option value="" text="Нет активных тарифов">Нет активных тарифов</Option>
                                    )}
                                </Dropdown>
                            </Field>

                            <Field label="Количество гостей" hint={`Введите количество гостей (от 1 до ${maxGuestsLimit})`} required>
                                <Input
                                    type="number"
                                    min={1}
                                    max={maxGuestsLimit}
                                    value={guestsCount.toString()}
                                    onChange={(_, data) => setGuestsCount(Math.min(maxGuestsLimit, Math.max(1, Number(data.value))))}
                                    disabled={submitting}
                                />
                            </Field>

                            <Field label="Столик / Ресурс (опционально)">
                                <Dropdown
                                    placeholder="Не выбран (Любой свободный)"
                                    value={
                                        resourceId
                                            ? freeResources.find(r => r.resourceId === resourceId)?.name || ""
                                            : "Не выбран (Любой свободный)"
                                    }
                                    selectedOptions={resourceId ? [resourceId] : []}
                                    onOptionSelect={(_, data) => setResourceId(data.optionValue || "")}
                                    disabled={loadingResources || submitting}
                                >
                                    <Option value="" text="Не выбран (Любой свободный)">
                                        Не выбран (Любой свободный)
                                    </Option>
                                    {freeResources.map((res) => (
                                        <Option
                                            key={res.resourceId}
                                            value={res.resourceId}
                                            text={`${res.name} (мест: ${res.capacity})`}
                                        >
                                            {res.name} (мест: {res.capacity})
                                        </Option>
                                    ))}
                                </Dropdown>
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
                            onClick={handleSubmit}
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
                </DialogBody>
            </DialogSurface>
        </Dialog>
    );
};
