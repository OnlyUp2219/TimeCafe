namespace Auth.TimeCafe.API.Endpoints.ExternalAuth;

public class ExternalProviders : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/authenticate/login/google", async (
            [FromQuery] string returnUrl,
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", $"/auth/authenticate/login/google/callback?returnUrl={Uri.EscapeDataString(returnUrl)}");
            properties.Items.Add("prompt", "select_account");
            return Results.Challenge(properties, ["Google"]);
        })
            .WithName("GoogleLogin")
            .WithSummary("Инициирует вход через Google")
            .WithDescription("Перенаправляет пользователя на страницу аутентификации Google. После успешной аутентификации пользователь будет перенаправлен на callback URL.")
            .WithTags("ExternalProviders");

        app.MapGet("/authenticate/login/microsoft", async (
            [FromQuery] string returnUrl,
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Microsoft", $"/auth/authenticate/login/microsoft/callback?returnUrl={Uri.EscapeDataString(returnUrl)}");
            properties.Items.Add("prompt", "select_account");
            return Results.Challenge(properties, ["Microsoft"]);
        })
            .WithName("MicrosoftLogin")
            .WithSummary("Инициирует вход через Microsoft")
            .WithDescription("Перенаправляет пользователя на страницу аутентификации Microsoft. После успешной аутентификации пользователь будет перенаправлен на callback URL.")
            .WithTags("ExternalProviders");

        app.MapGet("/authenticate/login/google/callback", async (
            [FromQuery] string returnUrl,
            HttpContext context,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromServices] IJwtService jwtService,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] ApplicationDbContext db,
            [FromServices] ILogger<ExternalProviders> logger) =>
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

            var externalUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (externalUserId == null)
            {
                return Results.BadRequest("External user ID is null");
            }

            var existingUserWithLogin = await userManager.FindByLoginAsync("Google", externalUserId);

            if (existingUserWithLogin != null)
            {
                var tokens = await jwtService.GenerateTokens(existingUserWithLogin);
                return Results.Redirect($"{returnUrl}#access_token={tokens.AccessToken}&refresh_token={tokens.RefreshToken}&emailConfirmed=true");
            }
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
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
                bool removedPassword = false;
                if (await userManager.HasPasswordAsync(user))
                {
                    var removeResult = await userManager.RemovePasswordAsync(user);
                    removedPassword = removeResult.Succeeded;
                    if (!removeResult.Succeeded)
                    {
                        logger.LogWarning("Не удалось удалить локальный пароль для пользователя {Email}: {Errors}", email, string.Join("; ", removeResult.Errors.Select(e => e.Description)));
                    }
                }
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    logger.LogWarning("Не удалось обновить пользователя {Email} после подтверждения email: {Errors}", email, string.Join("; ", updateResult.Errors.Select(e => e.Description)));
                }

                var tokensToRevoke = db.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked).ToList();
                if (tokensToRevoke.Count > 0)
                {
                    foreach (var t in tokensToRevoke) t.IsRevoked = true;
                    await db.SaveChangesAsync();
                    logger.LogInformation("Revoked {Count} refresh tokens for externally confirmed user {Email}", tokensToRevoke.Count, email);
                }
                logger.LogInformation("External email confirmation завершена для {Email}. Пароль удалён: {Removed}", email, removedPassword);
            }

            var info = new UserLoginInfo("Google", externalUserId, "Google");
            var loginResult = await userManager.AddLoginAsync(user, info);

            if (!loginResult.Succeeded)
            {
                return Results.BadRequest(string.Join(", ", loginResult.Errors.Select(e => e.Description)));
            }

            var userTokens = await jwtService.GenerateTokens(user);
            var hasPassword = await userManager.HasPasswordAsync(user) ? "true" : "false";
            return Results.Redirect($"{returnUrl}#access_token={userTokens.AccessToken}&refresh_token={userTokens.RefreshToken}&emailConfirmed=true&hasPassword={hasPassword}");
        })
            .WithName("GoogleLoginCallback")
            .WithSummary("Обрабатывает callback от Google после аутентификации")
            .WithDescription("Принимает ответ от Google, создает или обновляет пользователя в системе, генерирует JWT токены и перенаправляет на указанный URL с токенами.")
            .WithTags("ExternalProviders");

        app.MapGet("/authenticate/login/microsoft/callback", async (
            [FromQuery] string returnUrl,
            HttpContext context,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromServices] IJwtService jwtService,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] ApplicationDbContext db,
            [FromServices] ILogger<ExternalProviders> logger) =>
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

            var externalUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (externalUserId == null)
            {
                return Results.BadRequest("External user ID is null");
            }

            var existingUserWithLogin = await userManager.FindByLoginAsync("Microsoft", externalUserId);

            if (existingUserWithLogin != null)
            {
                var tokens = await jwtService.GenerateTokens(existingUserWithLogin);
                return Results.Redirect($"{returnUrl}#access_token={tokens.AccessToken}&refresh_token={tokens.RefreshToken}&emailConfirmed=true");
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
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
                bool removedPassword = false;
                if (await userManager.HasPasswordAsync(user))
                {
                    var removeResult = await userManager.RemovePasswordAsync(user);
                    removedPassword = removeResult.Succeeded;
                    if (!removeResult.Succeeded)
                    {
                        logger.LogWarning("Не удалось удалить локальный пароль для пользователя {Email}: {Errors}", email, string.Join("; ", removeResult.Errors.Select(e => e.Description)));
                    }
                }
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    logger.LogWarning("Не удалось обновить пользователя {Email} после подтверждения email: {Errors}", email, string.Join("; ", updateResult.Errors.Select(e => e.Description)));
                }
                var tokensToRevoke = db.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked).ToList();
                if (tokensToRevoke.Count > 0)
                {
                    foreach (var t in tokensToRevoke) t.IsRevoked = true;
                    await db.SaveChangesAsync();
                    logger.LogInformation("Revoked {Count} refresh tokens for externally confirmed user {Email}", tokensToRevoke.Count, email);
                }
                logger.LogInformation("External email confirmation завершена для {Email}. Пароль удалён: {Removed}", email, removedPassword);
            }
            var info = new UserLoginInfo("Microsoft", externalUserId, "Microsoft");
            var loginResult = await userManager.AddLoginAsync(user, info);

            if (!loginResult.Succeeded)
            {
                return Results.BadRequest(string.Join(", ", loginResult.Errors.Select(e => e.Description)));
            }

            var userTokens = await jwtService.GenerateTokens(user);
            var hasPassword = await userManager.HasPasswordAsync(user) ? "true" : "false";
            return Results.Redirect($"{returnUrl}#access_token={userTokens.AccessToken}&refresh_token={userTokens.RefreshToken}&emailConfirmed=true&hasPassword={hasPassword}");
        })
            .WithName("MicrosoftLoginCallback")
            .WithSummary("Обрабатывает callback от Microsoft после аутентификации")
            .WithDescription("Принимает ответ от Microsoft, создает или обновляет пользователя в системе, генерирует JWT токены и перенаправляет на указанный URL с токенами.")
            .WithTags("ExternalProviders");
    }
}