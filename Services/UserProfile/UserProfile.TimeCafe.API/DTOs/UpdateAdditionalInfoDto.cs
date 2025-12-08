namespace UserProfile.TimeCafe.API.DTOs;

public record UpdateAdditionalInfoDto(
    Guid InfoId,
    Guid UserId,
    string InfoText,
    string? CreatedBy);

public class UpdateAdditionalInfoDtoExample : IExamplesProvider<UpdateAdditionalInfoDto>
{
    public UpdateAdditionalInfoDto GetExamples()
    {
        return new UpdateAdditionalInfoDto(
            InfoId: Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            UserId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
            InfoText: "Обновлённая информация: клиент теперь предпочитает вечерние посещения",
            CreatedBy: "admin-456"
        );
    }
}
