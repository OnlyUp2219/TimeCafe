import type {FC} from "react";

interface AuthHeroProps {
    title: string;
    subtitle: string;
}

export const AuthHero: FC<AuthHeroProps> = ({title, subtitle}) => (
    <div className="auth-hero relative hidden overflow-hidden sm:flex items-center justify-center">
        <div className="auth-hero__circle auth-hero__circle--large" />
        <div className="auth-hero__circle auth-hero__circle--small" />
        <div className="relative z-10 text-center px-8 max-w-md">
            <h2 className="text-5xl font-bold text-white mb-4 leading-tight">{title}</h2>
            <p className="text-base text-white/90 leading-relaxed">{subtitle}</p>
        </div>
    </div>
);
