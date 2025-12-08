namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;

public record DeleteAdditionalInfoCommand(string InfoId) : IRequest<DeleteAdditionalInfoResult>;

public record DeleteAdditionalInfoResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static DeleteAdditionalInfoResult InfoNotFound() =>
        new(false, Code: "AdditionalInfoNotFound", Message: "Дополнительная информация не найдена", StatusCode: 404);

    public static DeleteAdditionalInfoResult DeleteFailed() =>
        new(false, Code: "DeleteAdditionalInfoFailed", Message: "Не удалось удалить дополнительную информацию", StatusCode: 500);

    public static DeleteAdditionalInfoResult DeleteSuccess() =>
        new(true, Message: "Дополнительная информация успешно удалена");
}

public class DeleteAdditionalInfoCommandValidator : AbstractValidator<DeleteAdditionalInfoCommand>
{
    public DeleteAdditionalInfoCommandValidator()
    {
        RuleFor(x => x.InfoId)
            .NotEmpty().WithMessage("Идентификатор информации не указан")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Идентификатор информации не указан")
            .Must(x => Guid.TryParse(x, out _)).WithMessage("Некорректный идентификатор информации");
    }
}

public class DeleteAdditionalInfoCommandHandler(IAdditionalInfoRepository repository) : IRequestHandler<DeleteAdditionalInfoCommand, DeleteAdditionalInfoResult>
{
    private readonly IAdditionalInfoRepository _repository = repository;

    public async Task<DeleteAdditionalInfoResult> Handle(DeleteAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var infoId = Guid.Parse(request.InfoId);
            var deleted = await _repository.DeleteAdditionalInfoAsync(infoId, cancellationToken);

            if (!deleted)
                return DeleteAdditionalInfoResult.InfoNotFound();

            return DeleteAdditionalInfoResult.DeleteSuccess();
        }
        catch (Exception)
        {
            return DeleteAdditionalInfoResult.DeleteFailed();
        }
    }
}
