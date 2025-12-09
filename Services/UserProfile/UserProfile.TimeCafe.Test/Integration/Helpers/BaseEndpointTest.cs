using Amazon.S3;

namespace UserProfile.TimeCafe.Test.Integration.Helpers;

public abstract class BaseEndpointTest(IntegrationApiFactory factory) : IClassFixture<IntegrationApiFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client = factory.CreateClient();
    protected readonly IntegrationApiFactory Factory = factory;

    public async Task InitializeAsync()
    {
        foreach (var (id, firstName, lastName, gender) in TestData.GetAllExistingUsers())
        {
            await SeedProfileAsync(id, firstName, lastName, gender);
        }

        await SeedAdditionalInfoAsync(
            TestData.AdditionalInfoData.Info1Id,
            TestData.AdditionalInfoData.Info1UserId,
            TestData.AdditionalInfoData.Info1Text
        );

        await SeedAdditionalInfoAsync(
            TestData.AdditionalInfoData.Info2Id,
            TestData.AdditionalInfoData.Info2UserId,
            TestData.AdditionalInfoData.Info2Text
        );
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task SeedProfileAsync(Guid userId, string firstName, string lastName, Gender gender = Gender.NotSpecified)
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
    protected async Task SeedProfileAsync(string userIdStr, string firstName, string lastName, Gender gender = Gender.NotSpecified)
    {
        await SeedProfileAsync(Guid.Parse(userIdStr), firstName, lastName, gender);
    }

    protected async Task SeedAdditionalInfoAsync(string infoId, string userId, string infoText)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var infoGuid = Guid.Parse(infoId);
        var existing = await context.AdditionalInfos.FindAsync(infoGuid);

        if (existing == null)
        {
            var info = new AdditionalInfo
            {
                InfoId = infoGuid,
                UserId = Guid.Parse(userId),
                InfoText = infoText,
                CreatedAt = DateTime.UtcNow
            };
            context.AdditionalInfos.Add(info);
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
