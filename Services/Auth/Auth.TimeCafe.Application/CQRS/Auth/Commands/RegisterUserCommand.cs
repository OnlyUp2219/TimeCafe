namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record RegisterUserCommand(string Username, string Email, string Password, bool SendEmail = true) : IRequest<RegisterUserResult>;

public record RegisterUserResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    string? CallbackUrl = null) : ICqrsResult
{
    public static RegisterUserResult Error(List<ErrorItem>? errorItems) =>
        new(false, Code: "RegistrationError", Message: "Ошибка при регистрации", StatusCode: 400,
            Errors: errorItems);
    public static RegisterUserResult SuccessResult(string callbackUrl) =>
        new(true, Message: "Пользователь создан и письмо отправлено", CallbackUrl: callbackUrl);
}

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Логин обязателен");

        RuleFor(x => x.Email).ValidEmail();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен");
    }
}

public class RegisterUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender,
    IOptionsSnapshot<PostmarkOptions> postmarkOptions,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork uow) : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailSender<ApplicationUser> _emailSender = emailSender;
    private readonly IOptionsSnapshot<PostmarkOptions> _postmarkOptions = postmarkOptions;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IUnitOfWork _uow = uow;

    // TODO : добавить транзакцию, чтобы при ошибке отправки письма удалять пользователя и публиковать событие только после успешной отправки письма
    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {

        using var transaction = await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                EmailConfirmed = false,
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                List<ErrorItem> errs = [.. createResult.Errors.Select(e => new ErrorItem(e.Code, e.Description))];
                return RegisterUserResult.Error(errs);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, Roles.Client);
            if (!addToRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                List<ErrorItem> errs = [.. addToRoleResult.Errors.Select(e => new ErrorItem(e.Code, e.Description))];
                return RegisterUserResult.Error(errs);
            }

            await _publishEndpoint.Publish(new UserRegisteredEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty
            }, cancellationToken);

            // TODO : перенести отправку письм после успешного завершения событий
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var callbackUrl = $"{_postmarkOptions.Value.FrontendBaseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";

            if (request.SendEmail)
            {
                await _emailSender.SendConfirmationLinkAsync(user, request.Email, callbackUrl);
            }

            await transaction.CommitAsync(cancellationToken);

            return RegisterUserResult.SuccessResult(callbackUrl);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return RegisterUserResult.Error(new List<ErrorItem> { new("Ошибка ", ex.ToString()) });
        }
    }
}
