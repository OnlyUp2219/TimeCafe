namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class UpdateProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/profiles", async (
            [FromServices] ISender sender,
            [FromBody] UpdateProfileDto dto) =>
        {
            var profile = new Profile
            {
                UserId = dto.UserId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                MiddleName = dto.MiddleName,
                AccessCardNumber = dto.AccessCardNumber,
                PhotoUrl = dto.PhotoUrl,
                BirthDate = dto.BirthDate,
                Gender = dto.Gender,
                ProfileStatus = dto.ProfileStatus,
                BanReason = dto.BanReason
            };
            var command = new UpdateProfileCommand(profile);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message, profile = r.Profile }));
        })
        .WithTags("Profiles")
        .WithName("UpdateProfile")
        .WithSummary("Обновить профиль")
        .WithDescription("Обновляет существующий профиль пользователя.")
        .RequireAuthorization();
    }
}
