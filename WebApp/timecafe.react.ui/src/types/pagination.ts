export interface PageMetadata {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export interface PagedResponse<T> {
    items: T[];
    metadata: PageMetadata;
}
