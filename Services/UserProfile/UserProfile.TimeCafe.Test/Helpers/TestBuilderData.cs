namespace UserProfile.TimeCafe.Test.Helpers;

public class ProfileBuilder
{
    private Guid _userId = Guid.Parse(TestData.NewProfiles.NewUser1Id);
    private string _firstName = TestData.NewProfiles.NewUser1FirstName;
    private string _lastName = TestData.NewProfiles.NewUser1LastName;
    private string? _middleName;
    private string? _photoUrl;
    private DateOnly? _birthDate;
    private Gender _gender = Gender.NotSpecified;

    public ProfileBuilder WithUserId(Guid id) { _userId = id; return this; }
    public ProfileBuilder WithFirstName(string name) { _firstName = name; return this; }
    public ProfileBuilder WithLastName(string name) { _lastName = name; return this; }
    public ProfileBuilder WithMiddleName(string? name) { _middleName = name; return this; }
    public ProfileBuilder WithPhotoUrl(string? url) { _photoUrl = url; return this; }
    public ProfileBuilder WithBirthDate(DateOnly? date) { _birthDate = date; return this; }
    public ProfileBuilder WithGender(Gender gender) { _gender = gender; return this; }
    public ProfileBuilder AsExistingUser1()
    {
        _userId = Guid.Parse(TestData.ExistingUsers.User1Id);
        _firstName = TestData.ExistingUsers.User1FirstName;
        _lastName = TestData.ExistingUsers.User1LastName;
        _gender = TestData.ExistingUsers.User1Gender;
        return this;
    }

    public Profile Build() => new()
    {
        UserId = _userId,
        FirstName = _firstName,
        LastName = _lastName,
        MiddleName = _middleName,
        PhotoUrl = _photoUrl,
        BirthDate = _birthDate,
        Gender = _gender,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public CreateProfileCommand BuildCommand() => new(_userId, _firstName, _lastName, _gender);
}

public class AdditionalInfoBuilder
{
    private Guid _infoId = Guid.NewGuid();
    private Guid _userId = Guid.Parse(TestData.ExistingUsers.User1Id);
    private string _infoText = TestData.AdditionalInfoData.NewInfoText;
    private string? _createdBy;

    public AdditionalInfoBuilder WithId(Guid id) { _infoId = id; return this; }
    public AdditionalInfoBuilder WithUserId(Guid id) { _userId = id; return this; }
    public AdditionalInfoBuilder WithText(string text) { _infoText = text; return this; }
    public AdditionalInfoBuilder WithCreatedBy(string? by) { _createdBy = by; return this; }

    public AdditionalInfo Build() => new()
    {
        InfoId = _infoId,
        UserId = _userId,
        InfoText = _infoText,
        CreatedBy = _createdBy,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public CreateAdditionalInfoCommand BuildCommand() => new(_userId, _infoText, _createdBy);
}

