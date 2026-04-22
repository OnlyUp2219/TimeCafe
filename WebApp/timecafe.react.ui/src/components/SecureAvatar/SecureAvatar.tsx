import React, { useEffect, useState, useRef } from "react";
import { Avatar, type AvatarProps } from "@fluentui/react-components";
import { ProfileApi } from "@api/profile/profileApi";
import { getApiBaseUrl } from "@shared/api/apiBaseUrl";

interface SecureAvatarProps extends AvatarProps {
    photoUrl?: string | null;
}

export const SecureAvatar: React.FC<SecureAvatarProps> = ({ photoUrl, ...props }) => {
    const [imageSrc, setImageSrc] = useState<string | undefined>(undefined);
    const [isVisible, setIsVisible] = useState(false);
    const objectUrlRef = useRef<string | null>(null);
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const observer = new IntersectionObserver(
            ([entry]) => {
                if (entry.isIntersecting) {
                    setIsVisible(true);
                    observer.disconnect();
                }
            },
            { rootMargin: "50px" }
        );

        if (containerRef.current) {
            observer.observe(containerRef.current);
        }

        return () => observer.disconnect();
    }, [photoUrl]);

    useEffect(() => {
        if (!photoUrl || !isVisible) {
            setImageSrc(undefined);
            return;
        }

        let isMounted = true;
        const fullUrl = photoUrl.startsWith("http") ? photoUrl : `${getApiBaseUrl()}${photoUrl}`;

        const loadPhoto = async () => {
            try {
                const blob = await ProfileApi.getProfilePhotoBlob(fullUrl);
                if (isMounted) {
                    if (objectUrlRef.current) {
                        URL.revokeObjectURL(objectUrlRef.current);
                    }
                    const url = URL.createObjectURL(blob);
                    objectUrlRef.current = url;
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
            if (objectUrlRef.current) {
                URL.revokeObjectURL(objectUrlRef.current);
                objectUrlRef.current = null;
            }
        };
    }, [photoUrl, isVisible]);

    return (
        <div ref={containerRef} style={{ display: "inline-flex" }}>
            <Avatar
                {...props}
                image={imageSrc ? { src: imageSrc } : undefined}
            />
        </div>
    );
};
