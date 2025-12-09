namespace Auth.TimeCafe.Infrastructure.Services;

public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        if (!password!.Any(ch => char.IsLower(ch) || (ch >= 'а' && ch <= 'я')))
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError()
            {
                Code = "PasswordRequiresLower",
                Description = "Пароль должен содержать хотя бы одну строчную букву ('a'-'z', 'а'-'я')."
            }));
        }
        return Task.FromResult(IdentityResult.Success);
    }
}
