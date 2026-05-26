import React, { useEffect, useState, useRef } from "react";
import { Avatar, type AvatarProps } from "@fluentui/react-components";
import { ProfileApi } from "@api/profile/profileApi";
import { getApiBaseUrl } from "@shared/api/apiBaseUrl";

interface SecureAvatarProps extends AvatarProps {
    photoUrl?: string | null;
}

const photoCache = new Map<string, string>();

export const SecureAvatar: React.FC<SecureAvatarProps> = ({ photoUrl, ...props }) => {
    const fullUrl = photoUrl ? (photoUrl.startsWith("http") ? photoUrl : `${getApiBaseUrl()}${photoUrl}`) : null;
    const initialSrc = fullUrl ? photoCache.get(fullUrl) : undefined;
    const [imageSrc, setImageSrc] = useState<string | undefined>(initialSrc);
    const [prevUrl, setPrevUrl] = useState(fullUrl);

    if (fullUrl !== prevUrl) {
        setPrevUrl(fullUrl);
        setImageSrc(initialSrc);
    }
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!fullUrl) {
            setImageSrc(undefined);
            return;
        }

        let isMounted = true;

        if (photoCache.has(fullUrl)) {
            setImageSrc(photoCache.get(fullUrl));
            return;
        }

        const loadPhoto = async () => {
            try {
                const blob = await ProfileApi.getProfilePhotoBlob(fullUrl);
                if (isMounted) {
                    const url = URL.createObjectURL(blob);
                    photoCache.set(fullUrl, url);
                    setImageSrc(url);
                }
            } catch (error) {
                if (isMounted) {
                    setImageSrc(undefined);
                }
            }
        };

        loadPhoto();

        return () => {
            isMounted = false;
        };
    }, [fullUrl]);

    return (
        <div ref={containerRef} style={{ display: "inline-flex" }}>
            <Avatar
                {...props}
                image={imageSrc ? { src: imageSrc } : undefined}
            />
        </div>
    );
};
