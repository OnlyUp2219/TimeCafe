namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhotoUrl,
    DateOnly? BirthDate,
    Gender Gender);

public class UpdateProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/profiles/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId,
            [FromBody] UpdateProfileRequest request) =>
        {
            var profile = new Profile
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                PhotoUrl = request.PhotoUrl,
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                ProfileStatus = ProfileStatus.Pending
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
