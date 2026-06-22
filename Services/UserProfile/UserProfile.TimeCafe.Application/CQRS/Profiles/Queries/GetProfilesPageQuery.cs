namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfilesPageQuery(int Page, int PageSize) : IQuery<PagedResponse<ProfileDto>>;

public record ProfileDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhotoUrl,
    DateOnly? BirthDate,
    int Gender,
    int ProfileStatus,
    DateTimeOffset CreatedAt,
    int VisitCount,
    decimal? PersonalDiscountPercent);

public class GetProfilesPageQueryHandler(IUnitOfWork uow) : IQueryHandler<GetProfilesPageQuery, PagedResponse<ProfileDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<PagedResponse<ProfileDto>>> Handle(GetProfilesPageQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            var profiles = await _uow.Profiles.GetPageAsync(page, pageSize, cancellationToken);
            var totalCount = await _uow.Profiles.GetTotalPageAsync(cancellationToken);

            var dtos = ProfilePhotoUrlMapper.WithApiUrl(profiles.Where(p => p != null)!)
                .Select(p => new ProfileDto(
                    p.UserId,
                    p.FirstName,
                    p.LastName,
                    p.MiddleName,
                    p.PhotoUrl,
                    p.BirthDate,
                    (int)p.Gender,
                    (int)p.ProfileStatus,
                    p.CreatedAt,
                    p.VisitCount,
                    p.PersonalDiscountPercent));

            var totalPages = (totalCount + pageSize - 1) / pageSize;

            return Result.Ok(new PagedResponse<ProfileDto>(
                dtos, 
                new PageMetadata(page, pageSize, totalCount, totalPages)));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
