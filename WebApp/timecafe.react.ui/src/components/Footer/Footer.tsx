import type {FC} from "react";
import {Link, tokens} from "@fluentui/react-components";

export const Footer: FC = () => {
    return (
        <footer
            className="mt-auto border-t w-full"
            style={{
                backgroundColor: tokens.colorNeutralBackground1,
                borderColor: tokens.colorNeutralStroke2,
            }}
        >
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
