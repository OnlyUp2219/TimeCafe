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
                CreatedAt = DateTimeOffset.UtcNow
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
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.AdditionalInfos.Add(info);
            await context.SaveChangesAsync();
        }
    }

    protected static byte[] LoadTestImage()
    {
        var header = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
            0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41,
            0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
            0x00, 0x00, 0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC,
            0x33, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E,
            0x44, 0xAE, 0x42, 0x60, 0x82
        };
        return header;
    }


    protected static async Task<bool> VerifyS3ObjectExistsAsync(string bucketName, string key, IAmazonS3 s3Client)
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
