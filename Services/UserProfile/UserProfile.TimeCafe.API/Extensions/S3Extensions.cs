namespace UserProfile.TimeCafe.API.Extensions;

public static class S3Extensions
{
    public static IServiceCollection AddS3(this IServiceCollection services, IConfiguration configuration)
    {
        var s3 = configuration.GetSection("S3");
        if (!s3.Exists())
            throw new InvalidOperationException("S3 configuration section is missing.");

        services.Configure<S3Options>(options =>
        {
            options.AccessKey = s3["AccessKey"] ?? throw new InvalidOperationException("S3:AccessKey is not configured.");
            options.SecretKey = s3["SecretKey"] ?? throw new InvalidOperationException("S3:SecretKey is not configured.");
            options.ServiceUrl = s3["ServiceUrl"] ?? throw new InvalidOperationException("S3:ServiceUrl is not configured.");
            options.BucketName = s3["BucketName"] ?? throw new InvalidOperationException("S3:BucketName is not configured.");
        });

        var photo = configuration.GetSection("Photo");
        if (!photo.Exists())
            throw new InvalidOperationException("Photo configuration section is missing.");

        services.Configure<PhotoOptions>(photo);

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
            return new AmazonS3Client(options.AccessKey, options.SecretKey, new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = true
            });
        });


        services.AddScoped<IProfilePhotoStorage>(sp =>
        {
            var s3Opts = sp.GetRequiredService<IOptions<S3Options>>().Value;
            var photoOpts = sp.GetRequiredService<IOptions<PhotoOptions>>().Value;
            var s3Client = sp.GetRequiredService<IAmazonS3>();
            return new S3ProfilePhotoStorage(s3Client, s3Opts, photoOpts);
        });

        return services;
    }
}
