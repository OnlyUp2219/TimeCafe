namespace Auth.TimeCafe.API.Endpoints;

public class ExternalProviders : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/authenticate/login/google", async ([FromQuery] string returnUrl, SignInManager<IdentityUser> signInManager) =>
        {
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", $"/authenticate/login/google/callback?returnUrl={Uri.EscapeDataString(returnUrl)}");
            properties.Items.Add("prompt", "select_account");
            return Results.Challenge(properties, ["Google"]);
        })
            .WithTags("ExternalProviders"); ;
        app.MapGet("/authenticate/login/microsoft", async ([FromQuery] string returnUrl, SignInManager<IdentityUser> signInManager) =>
        {
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Microsoft", $"/authenticate/login/microsoft/callback?returnUrl={Uri.EscapeDataString(returnUrl)}");
            properties.Items.Add("prompt", "select_account");
            return Results.Challenge(properties, ["Microsoft"]);
        })
            .WithTags("ExternalProviders");

        app.MapGet("/authenticate/login/google/callback", async ([FromQuery] string returnUrl, HttpContext context, SignInManager<IdentityUser> signInManager, IJwtService jwtService, UserManager<IdentityUser> userManager) =>
        {
            var result = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            var principal = result.Principal;

            var email = principal.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                return Results.BadRequest("Email is null");
            }

            var user = await userManager.FindByEmailAsync(email);


            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    return Results.BadRequest(string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }

                await userManager.AddToRoleAsync(user, "client");
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user);
            }

            var info = new UserLoginInfo("Google", principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty, "Google");

            var existingUserWithLogin = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (existingUserWithLogin != null)
            {
                if (existingUserWithLogin.Id != user.Id)
                {
                    return Results.BadRequest("This external login is already associated with another account.");
                }
            }
            else
            {
                var loginResult = await userManager.AddLoginAsync(user, info);

                if (!loginResult.Succeeded)
                {
                    return Results.BadRequest(string.Join(", ", loginResult.Errors.Select(e => e.Description)));
                }
            }

            var tokens = await jwtService.GenerateTokens(user);

            return Results.Redirect($"{returnUrl}#access_token={tokens.AccessToken}&refresh_token={tokens.RefreshToken}");
        })
            .WithTags("ExternalProviders");
        app.MapGet("/authenticate/login/microsoft/callback", async ([FromQuery] string returnUrl, HttpContext context, SignInManager<IdentityUser> signInManager, IJwtService jwtService, UserManager<IdentityUser> userManager) =>
        {
            var result = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            var principal = result.Principal;

            var email = principal.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                return Results.BadRequest("Email is null");
            }

            var user = await userManager.FindByEmailAsync(email);


            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    return Results.BadRequest(string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }

                await userManager.AddToRoleAsync(user, "client");
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user);
            }

            var info = new UserLoginInfo("Microsoft", principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty, "Microsoft");

            var existingUserWithLogin = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (existingUserWithLogin != null)
            {
                if (existingUserWithLogin.Id != user.Id)
                {
                    return Results.BadRequest("This external login is already associated with another account.");
                }
            }
            else
            {
                var loginResult = await userManager.AddLoginAsync(user, info);

                if (!loginResult.Succeeded)
                {
                    return Results.BadRequest(string.Join(", ", loginResult.Errors.Select(e => e.Description)));
                }
            }

            var tokens = await jwtService.GenerateTokens(user);

            return Results.Redirect($"{returnUrl}#access_token={tokens.AccessToken}&refresh_token={tokens.RefreshToken}");
        })
            .WithTags("ExternalProviders");
    }
}