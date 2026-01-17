import type {FC} from "react";

export const Footer: FC = () => {
    return (
        <footer className="border-t w-full border-slate-200 bg-white/90 backdrop-blur supports-[backdrop-filter]:bg-white/70">
            <div className="mx-auto max-w-6xl px-4 py-6 text-sm sm:px-6">
                © 2025 – Все права защищены.
            </div>
        </footer>
    );
};
