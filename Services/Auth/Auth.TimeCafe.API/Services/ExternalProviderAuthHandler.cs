namespace Auth.TimeCafe.API.Services;

internal static class ExternalProviderAuthHandler
{
    private const string ClientRole = "client";

    public static async Task<IResult> HandleCallbackAsync(
        string provider,
        string returnUrl,
        HttpContext context,
        IJwtService jwtService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        var authResult = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (!authResult.Succeeded || authResult.Principal is null)
            return Results.Unauthorized();

        var principal = authResult.Principal;
        var email = principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            return Results.BadRequest("Email is null");

        var externalUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(externalUserId))
            return Results.BadRequest("External user ID is null");

        var existingUserWithLogin = await userManager.FindByLoginAsync(provider, externalUserId);
        if (existingUserWithLogin != null)
        {
            var existingUserTokens = await jwtService.GenerateTokens(existingUserWithLogin);
            return Results.Redirect(BuildRedirectUrl(returnUrl, existingUserTokens.AccessToken, existingUserTokens.RefreshToken, true));
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return Results.BadRequest(string.Join(", ", createResult.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(user, ClientRole);
        }
        else if (!user.EmailConfirmed)
        {
            await ConfirmUserFromExternalProviderAsync(email, user, userManager, db, logger);
        }

        var addLoginResult = await userManager.AddLoginAsync(user, new UserLoginInfo(provider, externalUserId, provider));
        if (!addLoginResult.Succeeded)
            return Results.BadRequest(string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));

        var userTokens = await jwtService.GenerateTokens(user);
        var hasPassword = await userManager.HasPasswordAsync(user);

        return Results.Redirect(BuildRedirectUrl(returnUrl, userTokens.AccessToken, userTokens.RefreshToken, hasPassword));
    }

    private static async Task ConfirmUserFromExternalProviderAsync(
        string email,
        ApplicationUser user,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        user.EmailConfirmed = true;

        var removedPassword = false;
        if (await userManager.HasPasswordAsync(user))
        {
            var removePasswordResult = await userManager.RemovePasswordAsync(user);
            removedPassword = removePasswordResult.Succeeded;

            if (!removePasswordResult.Succeeded)
            {
                logger.LogWarning(
                    "Не удалось удалить локальный пароль для пользователя {Email}: {Errors}",
                    email,
                    string.Join("; ", removePasswordResult.Errors.Select(e => e.Description)));
            }
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            logger.LogWarning(
                "Не удалось обновить пользователя {Email} после подтверждения email: {Errors}",
                email,
                string.Join("; ", updateResult.Errors.Select(e => e.Description)));
        }

        var activeRefreshTokens = await db.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync();

        if (activeRefreshTokens.Count > 0)
        {
            foreach (var token in activeRefreshTokens)
                token.IsRevoked = true;

            await db.SaveChangesAsync();
            logger.LogInformation(
                "Revoked {Count} refresh tokens for externally confirmed user {Email}",
                activeRefreshTokens.Count,
                email);
        }

        logger.LogInformation(
            "External email confirmation завершена для {Email}. Пароль удалён: {Removed}",
            email,
            removedPassword);
    }

    private static string BuildRedirectUrl(
        string returnUrl,
        string accessToken,
        string refreshToken,
        bool hasPassword)
    {
        var hasPasswordValue = hasPassword ? "true" : "false";
        return $"{returnUrl}#access_token={accessToken}&refresh_token={refreshToken}&emailConfirmed=true&hasPassword={hasPasswordValue}";
    }
}
