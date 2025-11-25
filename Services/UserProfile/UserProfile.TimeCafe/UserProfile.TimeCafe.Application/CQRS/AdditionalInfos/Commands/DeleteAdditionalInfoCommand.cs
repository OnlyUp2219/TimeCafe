namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;

public record DeleteAdditionalInfoCommand(int InfoId) : IRequest<DeleteAdditionalInfoResult>;

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
            .GreaterThan(0).WithMessage("InfoId должен быть больше 0");
    }
}

public class DeleteAdditionalInfoCommandHandler(IAdditionalInfoRepository repository) : IRequestHandler<DeleteAdditionalInfoCommand, DeleteAdditionalInfoResult>
{
    private readonly IAdditionalInfoRepository _repository = repository;

    public async Task<DeleteAdditionalInfoResult> Handle(DeleteAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _repository.DeleteAdditionalInfoAsync(request.InfoId, cancellationToken);

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
