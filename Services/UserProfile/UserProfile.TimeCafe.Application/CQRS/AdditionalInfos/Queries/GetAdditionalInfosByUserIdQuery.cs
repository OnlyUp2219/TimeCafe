namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Queries;

public record GetAdditionalInfosByUserIdQuery(Guid UserId, int PageNumber = 1, int PageSize = 10) : IQuery<GetAdditionalInfosByUserIdResponse>;

public record GetAdditionalInfosByUserIdResponse(IEnumerable<AdditionalInfo> Infos, int TotalCount);

public class GetAdditionalInfosByUserIdQueryValidator : AbstractValidator<GetAdditionalInfosByUserIdQuery>
{
    public GetAdditionalInfosByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");
    }
}

public class GetAdditionalInfosByUserIdQueryHandler(IAdditionalInfoRepository repository, IUserRepositories userRepository) : IQueryHandler<GetAdditionalInfosByUserIdQuery, GetAdditionalInfosByUserIdResponse>
{
    private readonly IAdditionalInfoRepository _repository = repository;
    private readonly IUserRepositories _userRepository = userRepository;

    public async Task<Result<GetAdditionalInfosByUserIdResponse>> Handle(GetAdditionalInfosByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _userRepository.GetProfileByIdAsync(request.UserId, cancellationToken);
            if (profile == null)
                return Result.Fail(new ProfileNotFoundError());

            var (infos, totalCount) = await _repository.GetPagedAdditionalInfosByUserIdAsync(request.UserId, request.PageNumber, request.PageSize, cancellationToken);

            return Result.Ok(new GetAdditionalInfosByUserIdResponse(infos, totalCount));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
