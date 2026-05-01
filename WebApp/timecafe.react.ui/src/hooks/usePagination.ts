import { useAppDispatch, useAppSelector } from "@store/hooks";
import { setPagination, setFilters as setUiFilters } from "@store/uiSlice";
import { useCallback } from "react";

export const usePagination = (key: string, defaultPage = 1, defaultSize = 20) => {
    const dispatch = useAppDispatch();
    const pagination = useAppSelector((state) => state.ui.pagination[key]);

    const page = pagination?.page ?? defaultPage;
    const size = pagination?.size ?? defaultSize;

    const setPage = useCallback((newPage: number) => {
        dispatch(setPagination({ key, page: newPage, size }));
    }, [dispatch, key, size]);

    const setSize = useCallback((newSize: number) => {
        dispatch(setPagination({ key, page: 1, size: newSize }));
    }, [dispatch, key]);

    const setPageAndSize = useCallback((newPage: number, newSize: number) => {
        dispatch(setPagination({ key, page: newPage, size: newSize }));
    }, [dispatch, key]);

    const setFilters = useCallback((filters: Record<string, any>) => {
        dispatch(setUiFilters({ key, filters }));
    }, [dispatch, key]);

    const filters = pagination?.filters ?? {};

    return {
        page,
        size,
        filters,
        setPage,
        setSize,
        setPageAndSize,
        setFilters,
    };
};
