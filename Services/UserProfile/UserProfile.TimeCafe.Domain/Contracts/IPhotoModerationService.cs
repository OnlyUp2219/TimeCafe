using UserProfile.TimeCafe.Domain.DTOs;

namespace UserProfile.TimeCafe.Domain.Contracts;

public interface IPhotoModerationService
{
    Task<ModerationResult> ModeratePhotoAsync(Stream photoStream, CancellationToken cancellationToken = default);
}
