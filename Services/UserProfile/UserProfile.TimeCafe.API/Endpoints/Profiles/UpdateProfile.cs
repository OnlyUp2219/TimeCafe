namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public record UpdateProfileRequest(
    /// <example>Иван</example>
    string FirstName,
    /// <example>Иванов</example>
    string LastName,
    /// <example>Петрович</example>
    string? MiddleName,
    /// <example>https://s3.example.com/photos/user.jpg</example>
    string? PhotoUrl,
    /// <example>1990-05-15</example>
    DateOnly? BirthDate,
    /// <example>Male</example>
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
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message, profile = r.Profile }));
        })
        .WithTags("Profiles")
        .WithName("UpdateProfile")
        .WithSummary("Обновить профиль")
        .WithDescription("Обновляет существующий профиль пользователя.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization();
    }
}
