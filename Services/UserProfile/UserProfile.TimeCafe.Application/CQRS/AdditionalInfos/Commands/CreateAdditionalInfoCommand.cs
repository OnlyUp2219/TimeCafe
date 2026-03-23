namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;

public record CreateAdditionalInfoCommand(Guid UserId, string InfoText, string? CreatedBy = null) : IRequest<CreateAdditionalInfoResult>;

public record CreateAdditionalInfoResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    AdditionalInfo? AdditionalInfo = null) : ICqrsResultV2
{
    public static CreateAdditionalInfoResult CreateFailed() =>
        new(false, Code: "CreateAdditionalInfoFailed", Message: "Не удалось создать дополнительную информацию", StatusCode: 500);

    public static CreateAdditionalInfoResult CreateSuccess(AdditionalInfo info) =>
        new(true, Message: "Дополнительная информация успешно создана", StatusCode: 201, AdditionalInfo: info);

    public static CreateAdditionalInfoResult ProfileNotFound() =>
        new(false, Code: "ProfileNotFound", Message: "Профиль не найден", StatusCode: 404);
}

public class CreateAdditionalInfoCommandValidator : AbstractValidator<CreateAdditionalInfoCommand>
{
    public CreateAdditionalInfoCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");

        RuleFor(x => x.InfoText).ValidInfoText();

        RuleFor(x => x.CreatedBy).ValidCreatedBy()
            .When(x => !string.IsNullOrEmpty(x.CreatedBy));
    }
}

public class CreateAdditionalInfoCommandHandler(IAdditionalInfoRepository repository, IUserRepositories userRepository) : IRequestHandler<CreateAdditionalInfoCommand, CreateAdditionalInfoResult>
{
    private readonly IAdditionalInfoRepository _repository = repository;
    private readonly IUserRepositories _userRepository = userRepository;

    public async Task<CreateAdditionalInfoResult> Handle(CreateAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _userRepository.GetProfileByIdAsync(request.UserId, cancellationToken);
            if (profile == null)
                return CreateAdditionalInfoResult.ProfileNotFound();

            var info = new AdditionalInfo
            {
                UserId = request.UserId,
                InfoText = request.InfoText,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var created = await _repository.CreateAdditionalInfoAsync(info, cancellationToken);

            return CreateAdditionalInfoResult.CreateSuccess(created);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(CreateAdditionalInfoResult.CreateFailed(), ex);
        }
    }
}
