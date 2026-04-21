import {useParams, useNavigate} from "react-router-dom";
import {
    Avatar,
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    Caption1,
    MessageBar,
    MessageBarBody,
    Spinner,
    Title2,
    Title3,
} from "@fluentui/react-components";
import {ArrowLeft20Regular, Checkmark20Regular} from "@fluentui/react-icons";
import {useGetVisitByIdQuery, useEndVisitMutation} from "@store/api/venueApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {useComponentSize} from "@hooks/useComponentSize";
import {VisitStatus} from "@app-types/visit";
import {useState} from "react";
import {HasPermission} from "@components/Guard/HasPermission";
import {Permissions} from "@shared/auth/permissions";

import {CURRENCY_SYMBOL} from "@shared/const/currency";
import {NO_DATA} from "@shared/const/placeholders";

const formatDateTime = (iso: string | null) => {
    if (!iso) return NO_DATA;
    return new Date(iso).toLocaleString("ru-RU", {
        day: "2-digit", month: "2-digit", year: "numeric",
        hour: "2-digit", minute: "2-digit", second: "2-digit"
    });
};

const formatMoney = (v: number | null) => v != null ? `${v.toFixed(2)} ${CURRENCY_SYMBOL}` : NO_DATA;

const statusLabel = (s: number) => s === VisitStatus.Active ? "Активен" : "Завершён";
const statusColor = (s: number): "success" | "informative" => s === VisitStatus.Active ? "success" : "informative";

export const VisitDetailPage = () => {
    const {id} = useParams<{id: string}>();
    const navigate = useNavigate();
    const {sizes} = useComponentSize();
    const [endError, setEndError] = useState<string | null>(null);

    const {data: visitData, isLoading, error} = useGetVisitByIdQuery(id!, {skip: !id});
    const [endVisit, {isLoading: ending}] = useEndVisitMutation();

    const visit = visitData?.visit;
    const errorMessage = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const handleEnd = async () => {
        if (!id) return;
        setEndError(null);
        try {
            await endVisit(id).unwrap();
        } catch (err) {
            setEndError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось завершить визит");
        }
    };

    if (isLoading) return <div className="flex justify-center p-8"><Spinner /></div>;

    if (errorMessage) return (
        <div>
            <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/visits")} className="mb-4">
                Назад к визитам
            </Button>
            <MessageBar intent="error">
                <MessageBarBody>{errorMessage}</MessageBarBody>
            </MessageBar>
        </div>
    );

    if (!visit) return (
        <div>
            <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/visits")}>
                Назад
            </Button>
            <MessageBar intent="warning" className="mt-4">
                <MessageBarBody>Визит не найден</MessageBarBody>
            </MessageBar>
        </div>
    );

    return (
        <div>
            <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/visits")} className="mb-4">
                Назад к визитам
            </Button>

            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <Title2>Визит</Title2>
                <HasPermission can={Permissions.VenueVisitEnd}>
                    {visit.status === VisitStatus.Active && (
                        <Button
                            appearance="primary"
                            icon={ending ? <Spinner size="tiny" /> : <Checkmark20Regular />}
                            onClick={handleEnd}
                            disabled={ending}
                        >
                            Завершить визит
                        </Button>
                    )}
                </HasPermission>
            </div>

            {endError && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{endError}</MessageBarBody>
                </MessageBar>
            )}

            <div className="flex flex-col gap-4">
                <Card size={sizes.card}>
                    <div className="flex items-center gap-4 flex-wrap">
                        <Avatar name={visit.userId} size={48} />
                        <div className="min-w-0 flex-1">
                            <Body2 block>Пользователь</Body2>
                            <Caption1 block className="font-mono">{visit.userId}</Caption1>
                        </div>
                        <Badge appearance="filled" color={statusColor(visit.status)} size="large">
                            {statusLabel(visit.status)}
                        </Badge>
                    </div>
                </Card>

                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    <Card size={sizes.card}>
                        <Body2 block>Тариф</Body2>
                        <Title3>{visit.tariffName || NO_DATA}</Title3>
                    </Card>
                    <Card size={sizes.card}>
                        <Body2 block>Вход</Body2>
                        <Body1 block>{formatDateTime(visit.entryTime)}</Body1>
                    </Card>
                    <Card size={sizes.card}>
                        <Body2 block>Выход</Body2>
                        <Body1 block>{formatDateTime(visit.exitTime)}</Body1>
                    </Card>
                    <Card size={sizes.card}>
                        <Body2 block>Стоимость</Body2>
                        <Title3>{formatMoney(visit.calculatedCost)}</Title3>
                    </Card>
                    <Card size={sizes.card}>
                        <Body2 block>ID визита</Body2>
                        <Caption1 block className="font-mono">{visit.visitId}</Caption1>
                    </Card>
                </div>
            </div>
        </div>
    );
};
