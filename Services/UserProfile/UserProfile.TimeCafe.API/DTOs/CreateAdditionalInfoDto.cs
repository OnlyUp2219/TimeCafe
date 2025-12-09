namespace UserProfile.TimeCafe.API.DTOs;

public record CreateAdditionalInfoDto(
    string UserId,
    string InfoText,
    string? CreatedBy);

public class CreateAdditionalInfoDtoExample : IExamplesProvider<CreateAdditionalInfoDto>
{
    public CreateAdditionalInfoDto GetExamples()
    {
        return new CreateAdditionalInfoDto(
            UserId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479").ToString(),
            InfoText: "Дополнительная информация о пользователе: VIP-клиент, предпочитает утренние посещения",
            CreatedBy: "admin-456"
        );
    }
}
