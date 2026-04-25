namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;

public record CreateAdditionalInfoCommand(Guid UserId, string InfoText, string? CreatedBy = null) : ICommand<AdditionalInfo>;

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

public class CreateAdditionalInfoCommandHandler(IAdditionalInfoRepository repository, IUserRepositories userRepository) : ICommandHandler<CreateAdditionalInfoCommand, AdditionalInfo>
{
    private readonly IAdditionalInfoRepository _repository = repository;
    private readonly IUserRepositories _userRepository = userRepository;

    public async Task<Result<AdditionalInfo>> Handle(CreateAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _userRepository.GetProfileByIdAsync(request.UserId, cancellationToken);
            if (profile == null)
                return Result.Fail(new ProfileNotFoundError());

            var info = new AdditionalInfo
            {
                UserId = request.UserId,
                InfoText = request.InfoText,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var created = await _repository.CreateAdditionalInfoAsync(info, cancellationToken);

            return Result.Ok(created);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
