import {useState, useEffect} from "react";
import {useSearchParams} from "react-router-dom";
import {
    Body1,
    Body2,
    Button,
    Card,
    Caption1,
    Field,
    Input,
    MessageBar,
    MessageBarBody,
    Spinner,
    Title2,
    Title3,
} from "@fluentui/react-components";
import {Search20Regular} from "@fluentui/react-icons";
import {useGetAdditionalInfosByUserIdQuery} from "@store/api/profileApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {useComponentSize} from "@hooks/useComponentSize";

const formatDate = (iso: string) =>
    new Date(iso).toLocaleString("ru-RU", {day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit"});

export const AdditionalInfosPage = () => {
    const {sizes} = useComponentSize();
    const [searchParams] = useSearchParams();
    const [userId, setUserId] = useState(searchParams.get("userId") ?? "");
    const [searchUserId, setSearchUserId] = useState(searchParams.get("userId") ?? "");

    useEffect(() => {
        const uid = searchParams.get("userId");
        if (uid) {
            setUserId(uid);
            setSearchUserId(uid);
        }
    }, [searchParams]);

    const {data: infos = [], isLoading, error} = useGetAdditionalInfosByUserIdQuery(
        searchUserId,
        {skip: !searchUserId}
    );

    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const handleSearch = () => {
        setSearchUserId(userId.trim());
    };

    return (
        <div>
            <div className="mb-4">
                <Title2>Доп. информация</Title2>
                <Body2 block>Заметки и дополнительные данные по пользователям</Body2>
            </div>

            <div className="flex gap-3 items-end mb-6 flex-wrap">
                <Field label="ID пользователя" size={sizes.field}>
                    <Input
                        size={sizes.input}
                        value={userId}
                        onChange={(e) => setUserId(e.target.value)}
                        onKeyDown={(e) => e.key === "Enter" && handleSearch()}
                        placeholder="Введите userId..."
                        style={{minWidth: 320}}
                    />
                </Field>
                <Button
                    appearance="primary"
                    size={sizes.button}
                    icon={<Search20Regular />}
                    onClick={handleSearch}
                    disabled={!userId.trim()}
                >
                    Найти
                </Button>
            </div>

            {queryError && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{queryError}</MessageBarBody>
                </MessageBar>
            )}

            {isLoading && <Spinner label="Загрузка..." />}

            {!searchUserId && (
                <MessageBar intent="info">
                    <MessageBarBody>Введите ID пользователя для просмотра доп. информации</MessageBarBody>
                </MessageBar>
            )}

            {searchUserId && !isLoading && infos.length === 0 && !queryError && (
                <MessageBar intent="warning">
                    <MessageBarBody>Нет доп. информации для этого пользователя</MessageBarBody>
                </MessageBar>
            )}

            {infos.length > 0 && (
                <div className="flex flex-col gap-3">
                    <Title3>{infos.length} записей</Title3>
                    {infos.map((info) => (
                        <Card key={info.infoId} size={sizes.card}>
                            <Body1 block>{info.infoText}</Body1>
                            <div className="flex gap-4 mt-2 flex-wrap">
                                <Caption1 style={{color: "var(--colorNeutralForeground3)"}}>
                                    Автор: {info.createdBy}
                                </Caption1>
                                <Caption1 style={{color: "var(--colorNeutralForeground3)"}}>
                                    {formatDate(info.createdAt)}
                                </Caption1>
                            </div>
                        </Card>
                    ))}
                </div>
            )}
        </div>
    );
};
