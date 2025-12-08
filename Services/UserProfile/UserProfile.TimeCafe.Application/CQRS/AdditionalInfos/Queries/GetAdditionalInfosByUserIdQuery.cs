namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Queries;

public record GetAdditionalInfosByUserIdQuery(string UserId) : IRequest<GetAdditionalInfosByUserIdResult>;

public record GetAdditionalInfosByUserIdResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<AdditionalInfo>? AdditionalInfos = null) : ICqrsResultV2
{
    public static GetAdditionalInfosByUserIdResult GetFailed() =>
        new(false, Code: "GetAdditionalInfosFailed", Message: "Не удалось получить дополнительную информацию", StatusCode: 500);

    public static GetAdditionalInfosByUserIdResult GetSuccess(IEnumerable<AdditionalInfo> infos) =>
        new(true, Message: "Дополнительная информация получена", AdditionalInfos: infos);

    public static GetAdditionalInfosByUserIdResult ProfileNotFound() =>
        new(false, Code: "ProfileNotFound", Message: "Профиль не найден", StatusCode: 404);
}

public class GetAdditionalInfosByUserIdQueryValidator : AbstractValidator<GetAdditionalInfosByUserIdQuery>
{
    public GetAdditionalInfosByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Идентификатор пользователя не указан")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Идентификатор пользователя не указан")
            .Must(x => Guid.TryParse(x, out _)).WithMessage("Некорректный идентификатор пользователя");
    }
}

public class GetAdditionalInfosByUserIdQueryHandler(IAdditionalInfoRepository repository, IUserRepositories userRepository) : IRequestHandler<GetAdditionalInfosByUserIdQuery, GetAdditionalInfosByUserIdResult>
{
    private readonly IAdditionalInfoRepository _repository = repository;
    private readonly IUserRepositories _userRepository = userRepository;

    public async Task<GetAdditionalInfosByUserIdResult> Handle(GetAdditionalInfosByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(request.UserId);

            var profile = await _userRepository.GetProfileByIdAsync(userId, cancellationToken);
            if (profile == null)
                return GetAdditionalInfosByUserIdResult.ProfileNotFound();

            var infos = await _repository.GetAdditionalInfosByUserIdAsync(userId, cancellationToken);

            return GetAdditionalInfosByUserIdResult.GetSuccess(infos);
        }
        catch (Exception)
        {
            return GetAdditionalInfosByUserIdResult.GetFailed();
        }
    }
}
