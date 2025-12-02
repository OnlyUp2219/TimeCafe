using Amazon.S3;
using Amazon.S3.Model;

namespace UserProfile.TimeCafe.Test.Integration.Helpers;

public abstract class BaseEndpointTest(IntegrationApiFactory factory) : IClassFixture<IntegrationApiFactory>
{
    protected readonly HttpClient Client = factory.CreateClient();
    protected readonly IntegrationApiFactory Factory = factory;

    protected async Task SeedProfileAsync(string userId, string firstName, string lastName, Gender gender = Gender.NotSpecified)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var existing = await context.Profiles.FindAsync(userId);
        if (existing == null)
        {
            var profile = new Profile
            {
                UserId = userId,
                FirstName = firstName,
                LastName = lastName,
                Gender = gender,
                ProfileStatus = ProfileStatus.Completed,
                CreatedAt = DateTime.UtcNow
            };
            context.Profiles.Add(profile);
            await context.SaveChangesAsync();
        }
    }

    protected byte[] LoadTestImage()
    {
        var imagePath = @"f:\IT\TimeCafe\diagrams\Er-diagrama.png";

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"Тестовое изображение не найдено: {imagePath}");
        }

        return File.ReadAllBytes(imagePath);
    }


    protected async Task<bool> VerifyS3ObjectExistsAsync(string bucketName, string key, IAmazonS3 s3Client)
    {
        try
        {
            await s3Client.GetObjectMetadataAsync(bucketName, key);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
