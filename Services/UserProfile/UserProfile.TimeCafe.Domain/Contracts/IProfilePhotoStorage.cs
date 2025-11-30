namespace UserProfile.TimeCafe.Domain.Contracts;

public interface IProfilePhotoStorage
{
    Task<PhotoUploadDto> UploadAsync(string userId, Stream data, string contentType, string fileName, CancellationToken cancellationToken);
    Task<PhotoStreamDto?> GetAsync(string userId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string userId, CancellationToken cancellationToken);
}