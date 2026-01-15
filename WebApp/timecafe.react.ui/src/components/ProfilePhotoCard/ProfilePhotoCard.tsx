import {useEffect, useRef, useState} from "react";
import {Avatar, Button, Card, Caption1, Field, Title2, tokens} from "@fluentui/react-components";
import {Delete24Regular, ImageAdd24Regular} from "@fluentui/react-icons";

interface ProfilePhotoCardProps {
    displayName: string;
    onPhotoUrlChange?: (url: string | null) => void;
    className?: string;
}

export function ProfilePhotoCard({displayName, onPhotoUrlChange, className}: ProfilePhotoCardProps) {
    const [photoUrl, setPhotoUrl] = useState<string | null>(null);
    const [selectedFile, setSelectedFile] = useState<File | null>(null);

    const objectUrlRef = useRef<string | null>(null);

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
        onPhotoUrlChange?.(url);
    };

    const clearPhoto = () => {
        revokeObjectUrl();
        setPhotoUrl(null);
        onPhotoUrlChange?.(null);
    };

    useEffect(() => {
        return () => {
            revokeObjectUrl();
        };
    }, []);

    return (
        <Card className={className}>
            <div className="flex items-start justify-between gap-3">
                <Title2>Фото профиля</Title2>
            </div>

            <div className="mt-3 flex items-center gap-4">
                <Avatar
                    name={displayName}
                    size={72}
                    color="colorful"
                    image={photoUrl ? {src: photoUrl} : undefined}
                />

                <div className="flex flex-col gap-1 min-w-0">
                    <Caption1 style={{color: tokens.colorNeutralForeground2}}>
                        JPG/PNG. Сейчас это только UI-превью (без загрузки).
                    </Caption1>
                </div>
            </div>

            <div className="mt-4 flex flex-col gap-2">
                <Field label="Файл">
                    <input
                        type="file"
                        accept="image/*"
                        disabled={false}
                        onChange={(e) => {
                            const file = e.target.files?.[0] ?? null;
                            setSelectedFile(file);
                            if (file) {
                                setPhotoFromBlob(file);
                            }
                        }}
                    />
                </Field>

                <div className="flex flex-wrap gap-2">
                    <Button
                        appearance="primary"
                        icon={<ImageAdd24Regular/>}
                        disabled={!selectedFile}
                        onClick={() => {
                            if (selectedFile) setPhotoFromBlob(selectedFile);
                        }}
                    >
                        Применить
                    </Button>
                    <Button
                        appearance="secondary"
                        icon={<Delete24Regular/>}
                        disabled={!photoUrl}
                        onClick={() => {
                            setSelectedFile(null);
                            clearPhoto();
                        }}
                    >
                        Удалить
                    </Button>
                </div>
            </div>
        </Card>
    );
}
