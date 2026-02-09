import {useEffect, useId, useRef, useState} from "react";
import {Avatar, Button, Card, Spinner, Text, Title2} from "@fluentui/react-components";
import {Delete24Regular, ImageAdd24Regular} from "@fluentui/react-icons";

interface ProfilePhotoCardProps {
    displayName: string;
    onPhotoUrlChange?: (url: string | null) => void;
    onUpload?: (file: File) => Promise<boolean>;
    onDelete?: () => Promise<boolean>;
    busy?: boolean;
    className?: string;
    asCard?: boolean;
    showTitle?: boolean;
    disabled?: boolean;
    variant?: "view" | "edit";
    initialPhotoUrl?: string | null;
}

export function ProfilePhotoCard({
                                    displayName,
                                    onPhotoUrlChange,
                                    onUpload,
                                    onDelete,
                                    busy = false,
                                    className,
                                    asCard = true,
                                    showTitle = true,
                                    disabled = false,
                                    variant = "edit",
                                    initialPhotoUrl = null,
                                }: ProfilePhotoCardProps) {
    const [photoUrl, setPhotoUrl] = useState<string | null>(initialPhotoUrl);
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const fileInputRef = useRef<HTMLInputElement | null>(null);
    const fileInputId = useId();
    const fileLabelId = useId();

    const objectUrlRef = useRef<string | null>(null);
    const onPhotoUrlChangeRef = useRef(onPhotoUrlChange);

    useEffect(() => {
        onPhotoUrlChangeRef.current = onPhotoUrlChange;
    }, [onPhotoUrlChange]);

    const revokeObjectUrl = () => {
        if (objectUrlRef.current) {
            URL.revokeObjectURL(objectUrlRef.current);
            objectUrlRef.current = null;
        }
    };

    const setPhotoFromBlob = (blob: Blob) => {
        revokeObjectUrl();
        const url = URL.createObjectURL(blob);
        objectUrlRef.current = url;
        setPhotoUrl(url);
        onPhotoUrlChangeRef.current?.(url);
    };

    const clearPhoto = () => {
        revokeObjectUrl();
        setPhotoUrl(null);
        onPhotoUrlChangeRef.current?.(null);
    };

    useEffect(() => {
        return () => {
            revokeObjectUrl();
        };
    }, []);

    useEffect(() => {
        if (objectUrlRef.current) {
            URL.revokeObjectURL(objectUrlRef.current);
            objectUrlRef.current = null;
        }
        setSelectedFile(null);
        setPhotoUrl(initialPhotoUrl);
        onPhotoUrlChangeRef.current?.(initialPhotoUrl);
    }, [initialPhotoUrl]);

    const body = variant === "view" ? (
        <div className="flex items-center gap-4">
            <Avatar
                name={displayName}
                size={72}
                color="colorful"
                image={photoUrl ? {src: photoUrl} : undefined}
            />
        </div>
    ) : (
        <>
            {showTitle && (
                <div className="flex items-start justify-between gap-3">
                    <Title2>Фото профиля</Title2>
                </div>
            )}

            <div className={showTitle ? "mt-3 flex items-center gap-4" : "flex items-center gap-4"}>
                <Avatar
                    name={displayName}
                    size={72}
                    color="colorful"
                    image={photoUrl ? {src: photoUrl} : undefined}
                />
            </div>

            <div className="mt-4 flex flex-col gap-2">
                <div className="flex flex-col gap-2">
                    <Text id={fileLabelId} size={200} weight="semibold">
                        Файл
                    </Text>
                    <input
                        id={fileInputId}
                        type="file"
                        accept="image/*"
                        disabled={disabled || busy}
                        aria-labelledby={fileLabelId}
                        title="Выберите файл"
                        className="sr-only"
                        ref={fileInputRef}
                        onChange={(e) => {
                            const file = e.target.files?.[0] ?? null;
                            setSelectedFile(file);
                            if (file) {
                                setPhotoFromBlob(file);
                            }
                        }}
                    />
                    <Button
                        appearance="secondary"
                        icon={<ImageAdd24Regular />}
                        disabled={disabled || busy}
                        onClick={() => fileInputRef.current?.click()}
                    >
                        Выбрать файл
                    </Button>
                </div>

                <div className="flex flex-wrap gap-2">
                    <Button
                        appearance="primary"
                        icon={busy ? <Spinner size="tiny" /> : <ImageAdd24Regular />}
                        disabled={disabled || busy || !selectedFile || !onUpload}
                        onClick={async () => {
                            if (!selectedFile || !onUpload) return;
                            const ok = await onUpload(selectedFile);
                            if (ok) {
                                setSelectedFile(null);
                            }
                        }}
                    >
                        Применить
                    </Button>
                    <Button
                        appearance="secondary"
                        icon={busy ? <Spinner size="tiny" /> : <Delete24Regular />}
                        disabled={disabled || busy || !photoUrl || !onDelete}
                        onClick={async () => {
                            if (!onDelete) return;
                            const ok = await onDelete();
                            if (ok) {
                                setSelectedFile(null);
                                clearPhoto();
                            }
                        }}
                    >
                        Удалить
                    </Button>
                </div>
            </div>
        </>
    );

    if (!asCard) {
        return <div className={className}>{body}</div>;
    }

    return (
        <Card className={className}>
            {body}
        </Card>
    );
}
