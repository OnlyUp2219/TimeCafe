import {createElement, useState, type FC} from "react";
import {Badge, Body1Strong, Body2, Button, Card, Tag, Title2, Tooltip} from "@fluentui/react-components";
import {Edit20Filled, MailRegular} from "@fluentui/react-icons";
import {EmailVerificationModal} from "../EmailVerificationModal/EmailVerificationModal";
import {useDispatch, useSelector} from "react-redux";
import type {RootState} from "../../store";
import {hydrateAuthFromCurrentUser} from "../../shared/auth/hydrateAuthFromCurrentUser";
import {getPersonalDataStatusClass, getPersonalDataStatusIcon} from "./personalDataStatus";

export interface EmailFormCardProps {
    loading?: boolean;
    className?: string;
}

export const EmailFormCard: FC<EmailFormCardProps> = ({loading = false, className}) => {
    const dispatch = useDispatch();
    const authEmail = useSelector((state: RootState) => state.auth.email);
    const authEmailConfirmed = useSelector((state: RootState) => state.auth.emailConfirmed);
    const [showEmailModal, setShowEmailModal] = useState(false);

    const effectiveEmail = (authEmail ?? "").trim();
    const hasEmail = Boolean(effectiveEmail);
    const effectiveConfirmed: boolean | null = hasEmail ? authEmailConfirmed : null;
    const actionLabel =
        !hasEmail ? "Заполнить" : effectiveConfirmed === true ? "Изменить" : "Подтвердить";

    const handleEmailVerified = async () => {
        try {
            await hydrateAuthFromCurrentUser(dispatch);
        } catch {
            void 0;
        }
    };

    return (
        <Card className={className}>
            <Title2 block className="!flex items-center gap-2">
                <Badge appearance="tint" shape="rounded" size="extra-large" className="brand-badge">
                    <MailRegular className="size-5" />
                </Badge>
                Почта
            </Title2>
            <Body2 className="!line-clamp-2">
                Нужна для важных уведомлений и восстановления доступа.
            </Body2>

            <div className="flex flex-col gap-3">
                <div className="flex flex-col gap-2">
                    <div className="flex flex-col sm:items-center sm:flex-row justify-between gap-2 min-w-0">
                        <div className="flex items-center gap-2 min-w-0">
                            <Tooltip content={`Почта: ${effectiveEmail || "не указан"}`} relationship="label">
                                <Body1Strong className="!line-clamp-1 max-w-[25ch] md:max-w-[40ch] !truncate">
                                    {effectiveEmail || "—"}
                                </Body1Strong>
                            </Tooltip>
                            <Tooltip
                                content={!hasEmail ? "Почта не указана" : (effectiveConfirmed === true ? "Email подтверждён" : "Email не подтверждён")}
                                relationship="description"
                            >
                                <Tag
                                    appearance="outline"
                                    icon={createElement(getPersonalDataStatusIcon(effectiveConfirmed))}
                                    className={`custom-tag ${getPersonalDataStatusClass(effectiveConfirmed)}`}
                                />
                            </Tooltip>
                        </div>
                        <Button
                            appearance="primary"
                            icon={<Edit20Filled/>}
                            onClick={() => setShowEmailModal(true)}
                            disabled={loading}
                        >
                            {actionLabel}
                        </Button>
                    </div>
                </div>
            </div>

            <EmailVerificationModal
                isOpen={showEmailModal}
                onClose={() => setShowEmailModal(false)}
                currentEmail={effectiveEmail}
                currentEmailConfirmed={effectiveConfirmed === true}
                onSuccess={async (_verifiedEmail) => {
                    try {
                        await handleEmailVerified();
                    } finally {
                        setShowEmailModal(false);
                    }
                }}
            />
        </Card>
    );
};
