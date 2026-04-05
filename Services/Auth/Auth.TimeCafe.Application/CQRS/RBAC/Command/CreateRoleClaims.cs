namespace Auth.TimeCafe.Application.CQRS.RBAC.Command;

public sealed class CreateRoleClaimsCommand : ICommand
{
    public string RoleName { get; set; }
    public List<string> Claims { get; set; }
};

public sealed class CreateRoleClaimsValidator : AbstractValidator<CreateRoleClaimsCommand>
{
    public CreateRoleClaimsValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty();
        RuleFor(x => x.Claims).NotEmpty();
    }
}

public sealed class CreateRoleClaimsCommandHandler(IRbacRepository rbacRepository) : ICommandHandler<CreateRoleClaimsCommand>
{
    private readonly IRbacRepository _rbacRepository = rbacRepository;

    public async Task<Result> Handle(CreateRoleClaimsCommand request, CancellationToken cancellationToken)
    {
        if (await _rbacRepository.RoleExistsAsync(request.RoleName))
            return Result.Fail(new RoleExistError(request.RoleName));

        return await _rbacRepository.CreateRoleClaimsAsync(request.RoleName, request.Claims);
    }
}
