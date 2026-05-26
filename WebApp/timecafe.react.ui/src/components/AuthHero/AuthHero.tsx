import { tokens } from "@fluentui/react-components";
import type {FC} from "react";

interface AuthHeroProps {
    title: string;
    subtitle: string;
}

export const AuthHero: FC<AuthHeroProps> = ({title, subtitle}) => (
    <div 
        className="auth-hero relative hidden overflow-hidden sm:flex items-center justify-center"
        style={{ 
            background: `linear-gradient(135deg, ${tokens.colorBrandBackground}, ${tokens.colorBrandBackgroundHover})` 
        }}
    >
        <div className="auth-hero__circle auth-hero__circle--large" />
        <div className="auth-hero__circle auth-hero__circle--small" />
        <div className="relative z-10 text-center px-8 max-w-md">
            <h2 className="text-5xl font-bold mb-4 leading-tight" style={{ color: tokens.colorNeutralForegroundOnBrand }}>{title}</h2>
            <p className="text-base leading-relaxed" style={{ color: tokens.colorNeutralForegroundOnBrand }}>{subtitle}</p>
        </div>
    </div>
);
