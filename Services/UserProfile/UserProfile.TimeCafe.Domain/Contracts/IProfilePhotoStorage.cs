using UserProfile.TimeCafe.Domain.DTOs;

namespace UserProfile.TimeCafe.Domain.Contracts;

public interface IProfilePhotoStorage
{
    Task<PhotoUploadDto> UploadAsync(Guid userId, Stream data, string contentType, string fileName, CancellationToken cancellationToken = default);
    Task<PhotoStreamDto?> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}