namespace UserProfile.TimeCafe.API.DTOs;

public record UpdateAdditionalInfoDto(
    int InfoId,
    string UserId,
    string InfoText,
    string? CreatedBy);

public class UpdateAdditionalInfoDtoExample : IExamplesProvider<UpdateAdditionalInfoDto>
{
    public UpdateAdditionalInfoDto GetExamples()
    {
        return new UpdateAdditionalInfoDto(
            InfoId: 1,
            UserId: "user-123-abc",
            InfoText: "Обновлённая информация: клиент теперь предпочитает вечерние посещения",
            CreatedBy: "admin-456"
        );
    }
}
