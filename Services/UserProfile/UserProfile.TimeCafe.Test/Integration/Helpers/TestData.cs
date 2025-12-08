namespace UserProfile.TimeCafe.Test.Integration.Helpers;

/// <summary>
/// Централизованное хранилище тестовых данных для интеграционных тестов.
/// Изменяйте только этот файл при необходимости обновления тестовых данных.
/// </summary>
public static class TestData
{
    /// <summary>
    /// Существующие пользователи с профилями (используются в GET-тестах)
    /// </summary>
    public static class ExistingUsers
    {
        public static readonly string User1Id = "11111111-1111-1111-1111-111111111111";
        public static readonly string User1FirstName = "Иван";
        public static readonly string User1LastName = "Петров";
        public static readonly Gender User1Gender = Gender.Male;

        public static readonly string User2Id = "22222222-2222-2222-2222-222222222222";
        public static readonly string User2FirstName = "Мария";
        public static readonly string User2LastName = "Сидорова";
        public static readonly Gender User2Gender = Gender.Female;

        public static readonly string User3Id = "33333333-3333-3333-3333-333333333333";
        public static readonly string User3FirstName = "Алексей";
        public static readonly string User3LastName = "Иванов";
        public static readonly Gender User3Gender = Gender.Male;
    }
    
    public static class NonExistingUsers
    {
        public static readonly string UserId1 = "99999999-9999-9999-9999-999999999999";
        public static readonly string UserId2 = "88888888-8888-8888-8888-888888888888";
    }

    public static class InvalidIds
    {
        public static readonly string NotAGuid = "not-a-guid";
        public static readonly string EmptyString = "";
        public static readonly string JustNumbers = "12345";
        public static readonly string InvalidFormat = "123e4567-e89b-12d3-a456-42661417400g"; // 'g' в конце
    }

    public static class NewProfiles
    {
        public static readonly string NewUser1Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
        public static readonly string NewUser1FirstName = "Новый";
        public static readonly string NewUser1LastName = "Пользователь";
        
        public static readonly string NewUser2Id = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        public static readonly string NewUser2FirstName = "Тест";
        public static readonly string NewUser2LastName = "Тестов";
    }

    public static class AdditionalInfoData
    {
        public static readonly string Info1Id = "cccccccc-cccc-cccc-cccc-cccccccccccc";
        public static readonly string Info1Text = "Любит кофе";
        public static readonly string Info1UserId = ExistingUsers.User1Id;

        public static readonly string Info2Id = "dddddddd-dddd-dddd-dddd-dddddddddddd";
        public static readonly string Info2Text = "Предпочитает чай";
        public static readonly string Info2UserId = ExistingUsers.User2Id;

        public static readonly string NewInfoText = "Новая дополнительная информация";
        public static readonly string UpdatedInfoText = "Обновленная информация";
    }

    public static class UpdateData
    {
        public static readonly string UpdatedFirstName = "ОбновленноеИмя";
        public static readonly string UpdatedLastName = "ОбновленнаяФамилия";
        public static readonly Gender UpdatedGender = Gender.Female;
    }

    public static IEnumerable<(string Id, string FirstName, string LastName, Gender Gender)> GetAllExistingUsers()
    {
        yield return (ExistingUsers.User1Id, ExistingUsers.User1FirstName, ExistingUsers.User1LastName, ExistingUsers.User1Gender);
        yield return (ExistingUsers.User2Id, ExistingUsers.User2FirstName, ExistingUsers.User2LastName, ExistingUsers.User2Gender);
        yield return (ExistingUsers.User3Id, ExistingUsers.User3FirstName, ExistingUsers.User3LastName, ExistingUsers.User3Gender);
    }
}
