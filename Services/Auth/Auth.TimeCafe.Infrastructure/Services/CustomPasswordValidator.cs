using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.TimeCafe.Infrastructure.Services;

public class CustomPasswordValidator : IPasswordValidator<IdentityUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user, string? password)
    {
        if (!password.Any(ch => char.IsLower(ch) || (ch >= 'а' && ch <= 'я')))
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
