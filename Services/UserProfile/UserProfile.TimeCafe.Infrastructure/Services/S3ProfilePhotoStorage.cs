using Amazon.S3;
using Amazon.S3.Model;

using System.Net;

using UserProfile.TimeCafe.Domain.DTOs;

namespace UserProfile.TimeCafe.Infrastructure.Services;

public class S3ProfilePhotoStorage(IAmazonS3 s3, S3Options s3Options, PhotoOptions photoOptions) : IProfilePhotoStorage
{
    private readonly IAmazonS3 _s3 = s3;
    private readonly S3Options _s3Options = s3Options;
    private readonly PhotoOptions _photoOptions = photoOptions;

    public async Task<PhotoUploadDto> UploadAsync(Guid userId, Stream data, string contentType, string fileName, CancellationToken cancellationToken)
    {
        var key = BuildKey(userId);
        try
        {
            var put = new PutObjectRequest
            {
                BucketName = _s3Options.BucketName,
                Key = key,
                InputStream = data,
                AutoCloseStream = false,
                ContentType = contentType
            };
            put.Metadata["original-filename"] = fileName;
            put.Metadata["uploaded-at-utc"] = DateTime.UtcNow.ToString("O");

            var resp = await _s3.PutObjectAsync(put, cancellationToken);
            if (resp.HttpStatusCode != HttpStatusCode.OK)
                return new PhotoUploadDto(false);

            var url = _s3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _s3Options.BucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(_photoOptions.PresignedUrlExpirationMinutes)
            });

            return new PhotoUploadDto(true, key, url, data.Length, contentType);
        }
        catch (Exception ex)
        {
            return new PhotoUploadDto(false);
        }
        finally
        {
            data.Dispose();
        }
    }

    public async Task<PhotoStreamDto?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var key = BuildKey(userId);
        try
        {
            var resp = await _s3.GetObjectAsync(new GetObjectRequest
            {
                BucketName = _s3Options.BucketName,
                Key = key
            }, cancellationToken);
            if (resp.HttpStatusCode != HttpStatusCode.OK)
                return null;
            return new PhotoStreamDto(resp.ResponseStream, resp.Headers["Content-Type"]);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        var key = BuildKey(userId);
        try
        {
            var resp = await _s3.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _s3Options.BucketName,
                Key = key
            }, cancellationToken);
            return resp.HttpStatusCode == HttpStatusCode.NoContent;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private static string BuildKey(Guid userId) => $"profiles/{userId}/photo";
}