import type {FC} from "react";
import {Link} from "@fluentui/react-components";

export const Footer: FC = () => {
    return (
        <footer
            className="mt-auto border-t w-full border-slate-200 bg-white/90 backdrop-blur
            supports-[backdrop-filter]:bg-white/90">
            <div className="page-content flex items-center justify-between text-sm">
                <span>© 2026 TimeCafe</span>
                <div className="flex gap-4">
                    <Link href="#" appearance="subtle">Поддержка</Link>
                    <Link href="#" appearance="subtle">Политика конфиденциальности</Link>
                </div>
            </div>
        </footer>
    );
};
