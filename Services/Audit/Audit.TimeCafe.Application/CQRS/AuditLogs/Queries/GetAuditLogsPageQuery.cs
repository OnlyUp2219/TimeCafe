namespace Audit.TimeCafe.Application.CQRS.AuditLogs.Queries;

public sealed record GetAuditLogsPageQuery(int Page, int PageSize, string? EventType, string? UserName)
    : IQuery<PagedResponse<AuditLogDto>>;


public sealed class GetAuditLogsPageQueryHandler(IUnitOfWork uow)
    : IQueryHandler<GetAuditLogsPageQuery, PagedResponse<AuditLogDto>>
{
    public async Task<Result<PagedResponse<AuditLogDto>>> Handle(
        GetAuditLogsPageQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (items, totalCount) = await uow.AuditLogs.GetPageAsync(
                request.Page, request.PageSize, request.EventType, request.UserName, cancellationToken);

            var dtos = items.ConvertAll(a => new AuditLogDto(
                a.Id,
                a.EventType,
                a.Action,
                a.UserName,
                a.MachineName,
                a.DomainName,
                a.Duration,
                a.CreatedAt,
                a.StartDate,
                a.EndDate,
                a.CorrelationId));

            var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

            return Result.Ok(new PagedResponse<AuditLogDto>(
                dtos,
                new PageMetadata(request.Page, request.PageSize, totalCount, totalPages)));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
