import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
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
    Title2,
    Title3,
} from "@fluentui/react-components";
import { Search20Regular } from "@fluentui/react-icons";
import { useGetAdditionalInfosByUserIdQuery } from "@store/api/profileApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useComponentSize } from "@hooks/useComponentSize";
import { Pagination } from "@components/Pagination/Pagination";

import { DismissableError } from "@components/DismissableError/DismissableError";
import { PageLoader } from "@components/PageLoader/PageLoader";
import { EmptyState } from "@components/EmptyState/EmptyState";
import { formatDateTime } from "@utility/dateUtils";

import { usePagination } from "@hooks/usePagination";

export const AdditionalInfosPage = () => {
    const { sizes } = useComponentSize();
    const [searchParams] = useSearchParams();
    const [userId, setUserId] = useState(searchParams.get("userId") ?? "");
    const [searchUserId, setSearchUserId] = useState(searchParams.get("userId") ?? "");
    const { page, size: PAGE_SIZE, setPage } = usePagination("adminAdditionalInfos", 1, 10);

    useEffect(() => {
        const uid = searchParams.get("userId");
        if (uid) {
            setUserId(uid);
            setSearchUserId(uid);
        }
    }, [searchParams]);

    const { data: notesData, isLoading, error } = useGetAdditionalInfosByUserIdQuery(
        { userId: searchUserId, page: page, pageSize: PAGE_SIZE },
        { skip: !searchUserId }
    );
    const infos = notesData?.items ?? [];
    const totalCount = notesData?.metadata?.totalCount ?? 0;
    const totalPages = notesData?.metadata?.totalPages ?? 0;

    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const handleSearch = () => {
        setSearchUserId(userId.trim());
    };

    return (
        <div className="flex flex-col gap-2">
            <div className="flex flex-col">
                <Title2>Доп. информация</Title2>
                <Body2>Заметки и дополнительные данные по пользователям</Body2>
            </div>

            <div className="flex gap-3 items-end flex-wrap">
                <Field label="ID пользователя" size={sizes.field}>
                    <Input
                        size={sizes.input}
                        value={userId}
                        onChange={(e) => setUserId(e.target.value)}
                        onKeyDown={(e) => e.key === "Enter" && handleSearch()}
                        placeholder="Введите userId..."
                        style={{ minWidth: 320 }}
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

            <DismissableError error={queryError} />

            {isLoading && <PageLoader label="Загрузка..." />}

            {!searchUserId && (
                <MessageBar intent="info">
                    <MessageBarBody>Введите ID пользователя для просмотра доп. информации</MessageBarBody>
                </MessageBar>
            )}

            {searchUserId && !isLoading && infos.length === 0 && !queryError && (
                <EmptyState
                    title="Заметки отсутствуют"
                    description="Для указанного пользователя еще не добавлено никакой дополнительной информации."
                />
            )}

            {infos.length > 0 && (
                <div className="flex flex-col gap-3">
                    <Title3>{totalCount} записей</Title3>
                    {infos.map((info: { infoId: string; userId: string; infoText: string; createdBy: string; createdAt: string }) => (
                        <Card key={info.infoId} size={sizes.card}>
                            <Body1>{info.infoText}</Body1>
                            <div className="flex gap-4 mt-2 flex-wrap">
                                <Caption1 style={{ color: "var(--colorNeutralForeground3)" }}>
                                    Автор: {info.createdBy}
                                </Caption1>
                                <Caption1 style={{ color: "var(--colorNeutralForeground3)" }}>
                                    {formatDateTime(info.createdAt)}
                                </Caption1>
                            </div>
                        </Card>
                    ))}

                    {totalCount > PAGE_SIZE && (
                        <div className="flex justify-center">
                            <Pagination
                                currentPage={page}
                                totalPages={totalPages}
                                onPageChange={setPage}
                            />
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};
