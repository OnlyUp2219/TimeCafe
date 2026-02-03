import {createElement, useEffect, useState, type FC} from "react";
import {Badge, Body1Strong, Body2, Button, Card, Tag, Title2, Tooltip} from "@fluentui/react-components";
import {CheckmarkFilled, DismissFilled, Edit20Filled, MailRegular, type FluentIcon} from "@fluentui/react-icons";
import type {Profile} from "../../types/profile";
import {EmailVerificationModal} from "../EmailVerificationModal/EmailVerificationModal";
import {useSelector} from "react-redux";
import type {RootState} from "../../store";

export interface EmailFormCardProps {
    profile: Profile;
    loading?: boolean;
    className?: string;
    onSave?: (patch: Partial<Profile>) => void;
}

const getStatusClass = (confirmed?: boolean | null): string => {
    if (confirmed === true) return "dark-green";
    if (confirmed === false) return "pumpkin";
    if (confirmed == null) return "beige";
    return "dark-red";
};

const getStatusIcon = (confirmed?: boolean | null): FluentIcon => {
    if (confirmed) return CheckmarkFilled;
    return DismissFilled;
};

export const EmailFormCard: FC<EmailFormCardProps> = ({profile, loading = false, className, onSave}) => {
    const authEmail = useSelector((state: RootState) => state.auth.email);
    const [email, setEmail] = useState(profile.email);
    const [showEmailModal, setShowEmailModal] = useState(false);

    useEffect(() => {
        setEmail(profile.email);
    }, [profile.email]);

    const profileEmail = (email ?? "").trim();
    const fallbackEmail = (authEmail ?? "").trim();
    const effectiveEmail = profileEmail || fallbackEmail;
    const hasEmail = Boolean(effectiveEmail);
    const effectiveConfirmed: boolean | null =
        !hasEmail ? null : profileEmail ? (profile.emailConfirmed ?? null) : null;
    const actionLabel =
        !hasEmail ? "Заполнить" : effectiveConfirmed === true ? "Изменить" : "Подтвердить";

    const handleEmailVerified = (verifiedEmail: string) => {
        setEmail(verifiedEmail);
        onSave?.({email: verifiedEmail, emailConfirmed: true});
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
                                    icon={createElement(getStatusIcon(effectiveConfirmed))}
                                    className={`custom-tag ${getStatusClass(effectiveConfirmed)}`}
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
                onSuccess={handleEmailVerified}
            />
        </Card>
    );
};
