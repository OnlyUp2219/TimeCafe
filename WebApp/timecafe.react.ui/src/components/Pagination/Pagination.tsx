import { useState } from "react";
import { Button, Input, Dropdown, Option, Body1 } from "@fluentui/react-components";
import { ChevronLeftRegular, ChevronRightRegular } from "@fluentui/react-icons";

const PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

interface PaginationProps {
    currentPage: number;
    totalPages: number;
    className?: string;
    onPageChange: (page: number) => void;
    pageSize?: number;
    onPageSizeChange?: (size: number) => void;
    totalCount?: number;
    size?: "small" | "medium" | "large";
}

export const Pagination = ({
    currentPage,
    totalPages,
    className = "",
    onPageChange,
    pageSize,
    onPageSizeChange,
    totalCount,
    size = "large",
}: PaginationProps) => {
    const [jumpPage, setJumpPage] = useState("");

    const handleJump = () => {
        const page = parseInt(jumpPage, 10);
        if (page >= 1 && page <= totalPages) {
            onPageChange(page);
            setJumpPage("");
        }
    };

    const renderPages = () => {
        const pages: (number | string)[] = [];

        if (totalPages <= 1) {
            pages.push(1);
        } else {
            const delta = 2;
            const left = Math.max(2, currentPage - delta);
            const right = Math.min(totalPages - 1, currentPage + delta);

            pages.push(1);

            if (left > 2) {
                pages.push("dots-left");
            }

            for (let i = left; i <= right; i++) {
                pages.push(i);
            }

            if (right < totalPages - 1) {
                pages.push("dots-right");
            }

            pages.push(totalPages);
        }

        return pages.map((item, idx) => {
            if (item === "dots-left" || item === "dots-right") {
                return (
                    <span key={`${item}-${idx}`} className="px-2 text-neutral-500 self-center">
                        ...
                    </span>
                );
            }
            const page = item as number;
            return (
                <Button
                    key={`page-${page}-${idx}`}
                    appearance={page === currentPage ? "primary" : "outline"}
                    onClick={() => onPageChange(page)}
                    size={size}
                    style={{ minWidth: 0 }}
                >
                    {page}
                </Button>
            );
        });
    };

    return (
        <div className={`flex items-center gap-3 flex-wrap ${className}`}>
            {onPageSizeChange && pageSize !== undefined && (
                <div className="flex items-center gap-2">
                    <Body1>Строк:</Body1>
                    <Dropdown
                        size={size}
                        value={String(pageSize)}
                        selectedOptions={[String(pageSize)]}
                        onOptionSelect={(_, d) => {
                            if (d.optionValue) {
                                onPageSizeChange(Number(d.optionValue));
                            }
                        }}
                        style={{ minWidth: '80px' }}
                    >
                        {PAGE_SIZE_OPTIONS.map(s => (
                            <Option key={s} value={String(s)} text={String(s)}>{s}</Option>
                        ))}
                    </Dropdown>
                    {totalCount !== undefined && (
                        <Body1>из {totalCount}</Body1>
                    )}
                </div>
            )}

            <Button
                icon={<ChevronLeftRegular />}
                disabled={currentPage <= 1}
                onClick={() => onPageChange(currentPage - 1)}
                size={size}
                style={{ minWidth: 0 }}
            />
            <div className="flex items-center gap-1">
                {renderPages()}
            </div>
            <Button
                icon={<ChevronRightRegular />}
                disabled={currentPage >= totalPages}
                onClick={() => onPageChange(currentPage + 1)}
                size={size}
                style={{ minWidth: 0 }}
            />
            <div className="flex items-center gap-2">
                <Input
                    type="number"
                    value={jumpPage}
                    onChange={(e) => setJumpPage(e.target.value)}
                    min={1}
                    max={totalPages}
                    size={size}
                    style={{ width: 72 }}
                />
                <Button appearance="secondary" size={size} onClick={handleJump}>
                    Перейти
                </Button>
            </div>
        </div>
    );
};
