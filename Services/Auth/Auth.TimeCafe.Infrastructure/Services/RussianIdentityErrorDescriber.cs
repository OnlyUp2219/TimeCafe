using Microsoft.AspNetCore.Identity;

namespace Auth.TimeCafe.Infrastructure.Services;

public class RussianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateUserName(string userName) 
        => new() { 
            Code = nameof(DuplicateUserName), 
            Description = $"Имя пользователя '{userName}' уже используется."
        };

    public override IdentityError DuplicateEmail(string email) 
        => new() { 
            Code = nameof(DuplicateEmail), 
            Description = $"Email '{email}' уже используется." 
        };

    public override IdentityError PasswordRequiresLower()
          => new IdentityError
          {
              Code = nameof(PasswordRequiresLower),
              Description = "Пароль должен содержать хотя бы одну строчную букву ('a'-'z', 'а'-'я')."
          };

    public override IdentityError PasswordRequiresUpper()
        => new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = "Пароль должен содержать хотя бы одну заглавную букву."
        };

    public override IdentityError PasswordRequiresDigit()
        => new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = "Пароль должен содержать хотя бы одну цифру."
        };

    public override IdentityError PasswordTooShort(int length)
        => new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = $"Пароль должен содержать не менее {length} символов."
        };
}
