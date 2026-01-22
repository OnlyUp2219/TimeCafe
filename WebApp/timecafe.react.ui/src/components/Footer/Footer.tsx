import type {FC} from "react";

export const Footer: FC = () => {
    return (
        <footer
            className="mt-auto border-t w-full border-slate-200 bg-white/90 backdrop-blur
            supports-[backdrop-filter]:bg-white/90">
            <div className="mx-auto max-w-6xl px-2 py-4 sm:px-3 sm:py-6 text-sm">
                © 2025 – Все права защищены.
            </div>
        </footer>
    );
};
