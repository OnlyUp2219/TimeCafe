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
            UserId: "user-123-abc",
            InfoText: "Дополнительная информация о пользователе: VIP-клиент, предпочитает утренние посещения",
            CreatedBy: "admin-456"
        );
    }
}
