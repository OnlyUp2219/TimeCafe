namespace BuildingBlocks.Contracts.CQRS;

public record PagedResponse<T>(IEnumerable<T> Items, PageMetadata Metadata);

public record PageMetadata(int Page, int PageSize, int TotalCount, int TotalPages);
