using UserProfile.TimeCafe.Domain.DTOs;

namespace UserProfile.TimeCafe.API.Extensions;

public static class S3Extensions
{
    public static IServiceCollection AddS3(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<S3Options>(configuration, "S3");

        services.AddValidatedOptions<PhotoOptions>(
            configuration,
            "Photo",
            options => options.AllowedContentTypes is { Length: > 0 } && options.AllowedContentTypes.All(contentType => !string.IsNullOrWhiteSpace(contentType)),
            "Photo:AllowedContentTypes must contain at least one non-empty content type.");

        services.AddScoped<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptionsSnapshot<S3Options>>().Value;
            return new AmazonS3Client(options.AccessKey, options.SecretKey, new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = true
            });
        });


        services.AddScoped<IProfilePhotoStorage>(sp =>
        {
            var s3Opts = sp.GetRequiredService<IOptionsSnapshot<S3Options>>().Value;
            var s3Client = sp.GetRequiredService<IAmazonS3>();
            return new S3ProfilePhotoStorage(s3Client, s3Opts);
        });

        return services;
    }
}
