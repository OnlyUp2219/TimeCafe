import {useState} from "react";
import {Button, Input, Select, Caption1} from "@fluentui/react-components";
import {ChevronLeftRegular, ChevronRightRegular} from "@fluentui/react-icons";

const PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

interface PaginationProps {
    currentPage: number;
    totalPages: number;
    className?: string;
    onPageChange: (page: number) => void;
    pageSize?: number;
    onPageSizeChange?: (size: number) => void;
    totalCount?: number;
}

export const Pagination = ({
    currentPage,
    totalPages,
    className = "",
    onPageChange,
    pageSize,
    onPageSizeChange,
    totalCount,
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
        const delta = 2;
        const range = [];
        const rangeWithDots = [];

        for (let i = Math.max(2, currentPage - delta); i <= Math.min(totalPages - 1, currentPage + delta); i++) {
            range.push(i);
        }

        if (currentPage - delta > 2) {
            rangeWithDots.push(1, "...");
        } else {
            rangeWithDots.push(1);
        }

        rangeWithDots.push(...range);

        if (currentPage + delta < totalPages - 1) {
            rangeWithDots.push("...", totalPages);
        } else if (totalPages > 1) {
            rangeWithDots.push(totalPages);
        }

        return rangeWithDots.map((page, index) => {
            if (page === "...") {
                return (
                    <span key={index} className="px-2 text-neutral-500">
                        ...
                    </span>
                );
            }
            return (
                <Button
                    key={page}
                    appearance={page === currentPage ? "primary" : "outline"}
                    onClick={() => onPageChange(page as number)}
                    size="large"
                    style={{minWidth: 0}}
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
                    <Caption1>Строк:</Caption1>
                    <Select
                        size="medium"
                        value={String(pageSize)}
                        onChange={(_, d) => {
                            onPageSizeChange(Number(d.value));
                            onPageChange(1);
                        }}
                        style={{width: 80}}
                    >
                        {PAGE_SIZE_OPTIONS.map(s => (
                            <option key={s} value={s}>{s}</option>
                        ))}
                    </Select>
                    {totalCount !== undefined && (
                        <Caption1>из {totalCount}</Caption1>
                    )}
                </div>
            )}

            <Button
                icon={<ChevronLeftRegular />}
                disabled={currentPage <= 1}
                onClick={() => onPageChange(currentPage - 1)}
                size="large"
                style={{minWidth: 0}}
            />
            <div className="flex items-center gap-1">
                {renderPages()}
            </div>
            <Button
                icon={<ChevronRightRegular />}
                disabled={currentPage >= totalPages}
                onClick={() => onPageChange(currentPage + 1)}
                size="large"
                style={{minWidth: 0}}
            />
            <div className="flex items-center gap-2">
                <Input
                    type="number"
                    value={jumpPage}
                    onChange={(e) => setJumpPage(e.target.value)}
                    min={1}
                    max={totalPages}
                    size="large"
                    style={{width: 72}}
                />
                <Button appearance="secondary" size="large" onClick={handleJump}>
                    Перейти
                </Button>
            </div>
        </div>
    );
};
