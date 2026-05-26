import { createElement, useState, type FC } from "react";
import { Body1Strong, Body2, Button, Card, Title2, Tooltip, Caption1 } from "@fluentui/react-components";
import { Edit20Filled, MailRegular } from "@fluentui/react-icons";
import { EmailVerificationModal } from "@components/EmailVerificationModal/EmailVerificationModal";
import { useAppDispatch, useAppSelector } from "@store/hooks";
import { hydrateAuthFromCurrentUser } from "@shared/auth/hydrateAuthFromCurrentUser";
import { useComponentSize } from "@hooks/useComponentSize";
import { getPersonalDataStatusIcon } from "@components/PersonalDataForm/personalDataStatus";

export interface EmailFormCardProps {
    loading?: boolean;
    className?: string;
}

export const EmailFormCard: FC<EmailFormCardProps> = ({ loading = false, className }) => {
    const { sizes } = useComponentSize();
    const dispatch = useAppDispatch();
    const authEmail = useAppSelector((state) => state.auth.email);
    const authEmailConfirmed = useAppSelector((state) => state.auth.emailConfirmed);
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
        <Card className={className} size={sizes.card}>
            <Title2 block className="!flex items-center gap-2">
                <MailRegular className="text-[var(--colorBrandForeground1)]" fontSize={24} />
                Электронная почта
            </Title2>
            <Body2 className="!line-clamp-2">
                Используется для входа, сброса пароля и чеков.
            </Body2>

            <div>
                <div className="flex flex-col gap-2">
                    <div className="flex flex-col sm:items-center sm:flex-row justify-between gap-2 min-w-0">
                        <div className="flex items-center gap-2 min-w-0 flex-wrap">
                            <Tooltip content={`Email: ${effectiveEmail || "не указан"}`} relationship="label">
                                <Body1Strong className="!line-clamp-1 max-w-[25ch] md:max-w-[40ch] !truncate">
                                    {effectiveEmail || "—"}
                                </Body1Strong>
                            </Tooltip>
                            <div className="flex items-center gap-1.5 sm:ml-2 shrink-0">
                                {createElement(getPersonalDataStatusIcon(effectiveConfirmed), {
                                    className: effectiveConfirmed ? "text-[var(--colorStatusSuccessForeground1)]" : (effectiveConfirmed === false ? "text-[var(--colorStatusDangerForeground1)]" : "text-[var(--colorNeutralForeground3)]"),
                                    fontSize: 16
                                })}
                                <Caption1 className={effectiveConfirmed ? "text-[var(--colorStatusSuccessForeground1)]" : (effectiveConfirmed === false ? "text-[var(--colorStatusDangerForeground1)]" : "text-[var(--colorNeutralForeground3)]")}>
                                    {!hasEmail ? "Не указан" : (effectiveConfirmed ? "Подтверждён" : "Не подтверждён")}
                                </Caption1>
                            </div>
                        </div>
                        <Button
                            appearance="primary"
                            icon={<Edit20Filled />}
                            onClick={() => setShowEmailModal(true)}
                            disabled={loading}
                            size={sizes.button}
                            className="w-full sm:w-auto"
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
                onSuccess={async () => {
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
