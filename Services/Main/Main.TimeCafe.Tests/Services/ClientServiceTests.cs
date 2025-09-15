using Microsoft.EntityFrameworkCore;
using TimeCafeWinUI3.Core.Enums;
using TimeCafeWinUI3.Core.Models;

namespace TimeCafeWinUI3.Tests.MSTest.Services;

[TestClass]
public class ClientServiceTests
{
    private TimeCafeContext _context;
    private ClientUtilities _service;

    [TestInitialize]
    public void Initialize()
    {
        var options = new DbContextOptionsBuilder<TimeCafeContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TimeCafeContext(options);
        _service = new ClientService(_context);

        // Добавляем необходимые справочные данные
        _context.ClientStatuses.AddRange(
            new ClientStatus { StatusId = (int)EClientStatusType.Draft, StatusName = "Черновик" },
            new ClientStatus { StatusId = (int)EClientStatusType.Active, StatusName = "Активный" }
        );
        _context.Genders.AddRange(
            new Gender { GenderId = 1, GenderName = "Женский" },
            new Gender { GenderId = 2, GenderName = "Мужской" }
        );
        _context.Clients.Add(new Client
        {
            ClientId = 1,
            FirstName = "Test",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Active,
            GenderId = 1,
            CreatedAt = DateTime.Now
        });
        _context.SaveChanges();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [TestMethod]
    public async Task CreateClient_ShouldCreateNewClient()
    {
        var client = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Draft
        };

        var result = await _service.CreateClientAsync(client);

        Assert.IsNotNull(result);
        Assert.AreNotEqual(0, result.ClientId);
        Assert.AreEqual("Test", result.FirstName);
        Assert.AreEqual("User", result.LastName);
        Assert.AreNotEqual(default, result.CreatedAt);
    }

    [TestMethod]
    public async Task UpdateClient_ShouldUpdateExistingClient()
    {
        var client = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Draft
        };
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        client.FirstName = "Updated";
        var result = await _service.UpdateClientAsync(client);

        Assert.IsNotNull(result);
        Assert.AreEqual("Updated", result.FirstName);
    }

    [TestMethod]
    public async Task DeleteClient_ShouldDeleteClient()
    {
        var client = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Draft
        };
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteClientAsync(client.ClientId);

        Assert.IsTrue(result);
        Assert.IsNull(await _context.Clients.FindAsync(client.ClientId));
    }

    [TestMethod]
    public async Task GetClientById_ShouldReturnCorrectClient()
    {
        var client = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Draft
        };
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var result = await _service.GetClientByIdAsync(client.ClientId);

        Assert.IsNotNull(result);
        Assert.AreEqual(client.ClientId, result.ClientId);
        Assert.AreEqual("Test", result.FirstName);
        Assert.AreEqual("User", result.LastName);
    }

    [DataTestMethod]
    [DataRow("", false, "Empty phone number should be invalid")]
    [DataRow(null, false, "Null phone number should be invalid")]
    [DataRow("1234567890", false, "Incorrect format should be invalid")]
    [DataRow("+375 (33) 987 6543", true, "Correct format should be valid")]
    [DataRow("+375 (29) 123 4567", false, "Existing phone number should be invalid")]
    public async Task ValidatePhoneNumber_VariousInputs_ReturnsExpectedResult(string phoneNumber, bool expectedResult, string message)
    {
        var result = await _service.ValidatePhoneNumberAsync(phoneNumber);
        Assert.AreEqual(expectedResult, result, message);
    }

    [DataTestMethod]
    [DataRow("", false, "Empty phone number should be invalid")]
    [DataRow(null, false, "Null phone number should be invalid")]
    [DataRow("1234567890", false, "Incorrect format should be invalid")]
    [DataRow("+375 (33) 987 6543", true, "Correct format should be valid")]
    [DataRow("+375-33-9876543", true, "New format should be valid")]
    [DataRow("+375 (29) 123 4567", true, "Existing phone number should be valid for format check")]
    public async Task ValidatePhoneNumberFormat_VariousInputs_ReturnsExpectedResult(string phoneNumber, bool expectedResult, string message)
    {
        var result = await _service.ValidatePhoneNumberFormatAsync(phoneNumber);
        Assert.AreEqual(expectedResult, result, message);
    }

    [TestMethod]
    public async Task ValidatePhoneNumber_ShouldCheckUniqueness()
    {
        // Arrange
        var existingPhone = "+375 (29) 123 4567";
        var client = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = existingPhone,
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Draft
        };
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ValidatePhoneNumberAsync(existingPhone);

        // Assert
        Assert.IsFalse(result, "Existing phone number should be invalid");
    }

    [TestMethod]
    public async Task ValidatePhoneNumberFormat_ShouldNotCheckUniqueness()
    {
        // Arrange
        var existingPhone = "+375 (29) 123 4567";
        var client = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = existingPhone,
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Draft
        };
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ValidatePhoneNumberFormatAsync(existingPhone);

        // Assert
        Assert.IsTrue(result, "Existing phone number should be valid for format check");
    }

    [DataTestMethod]
    [DataRow("new@example.com", true, "Correct email format should be valid")]
    [DataRow("invalid-email", false, "Incorrect email format should be invalid")]
    [DataRow("test@example.com", false, "Existing email should be invalid")]
    [DataRow("test@@example.com", false, "Email with multiple @ should be invalid")]
    public async Task ValidateEmail_VariousInputs_ReturnsExpectedResult(string email, bool expectedResult, string message)
    {
        var result = await _service.ValidateEmailAsync(email);
        Assert.AreEqual(expectedResult, result, message);
    }

    [TestMethod]
    public async Task ValidateAccessCardNumber_ShouldValidateCorrectFormat()
    {
        var validCard = "ABCD1234EFGH5678IJKL";
        var invalidCard = "123";

        var validResult = await _service.ValidateAccessCardNumberAsync(validCard);
        var invalidResult = await _service.ValidateAccessCardNumberAsync(invalidCard);

        Assert.IsTrue(validResult);
        Assert.IsFalse(invalidResult);
    }

    [TestMethod]
    public async Task GenerateAccessCardNumber_ShouldGenerateUniqueNumber()
    {
        var cardNumber1 = await _service.GenerateAccessCardNumberAsync();
        var cardNumber2 = await _service.GenerateAccessCardNumberAsync();

        Assert.AreEqual(20, cardNumber1.Length);
        Assert.AreEqual(20, cardNumber2.Length);
        Assert.AreNotEqual(cardNumber1, cardNumber2);
    }

    [TestMethod]
    public async Task SetClientDraft_ShouldClearAccessCardNumber()
    {
        // Arrange
        var client = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Active,
            AccessCardNumber = "TEST1234567890123456"
        };
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SetClientDraftAsync(client.ClientId);

        // Assert
        Assert.IsTrue(result);
        var updatedClient = await _context.Clients.FindAsync(client.ClientId);
        Assert.AreEqual((int)EClientStatusType.Draft, updatedClient.StatusId);
        Assert.IsNull(updatedClient.AccessCardNumber);
    }

    [TestMethod]
    public async Task SetClientRejected_ShouldClearAccessCardNumber()
    {
        // Arrange
        var client = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Active,
            AccessCardNumber = "TEST1234567890123456"
        };
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SetClientRejectedAsync(client.ClientId, "Test reason");

        // Assert
        Assert.IsTrue(result);
        var updatedClient = await _context.Clients.FindAsync(client.ClientId);
        Assert.AreEqual((int)EClientStatusType.Rejected, updatedClient.StatusId);
        Assert.AreEqual("Test reason", updatedClient.RefusalReason);
        Assert.IsNull(updatedClient.AccessCardNumber);
    }

    [TestMethod]
    public async Task SetClientActive_ShouldGenerateAccessCardNumber()
    {
        // Arrange
        var client = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Draft,
            AccessCardNumber = null
        };
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SetClientActiveAsync(client.ClientId);

        // Assert
        Assert.IsTrue(result);
        var updatedClient = await _context.Clients.FindAsync(client.ClientId);
        Assert.AreEqual((int)EClientStatusType.Active, updatedClient.StatusId);
        Assert.IsNotNull(updatedClient.AccessCardNumber);
        Assert.AreEqual(20, updatedClient.AccessCardNumber.Length);
    }

    [TestMethod]
    public async Task SetClientActive_WithExistingCardNumber_ShouldKeepCardNumber()
    {
        // Arrange
        var existingCardNumber = "TEST1234567890123456";
        var client = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Draft,
            AccessCardNumber = existingCardNumber
        };
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SetClientActiveAsync(client.ClientId);

        // Assert
        Assert.IsTrue(result);
        var updatedClient = await _context.Clients.FindAsync(client.ClientId);
        Assert.AreEqual((int)EClientStatusType.Active, updatedClient.StatusId);
        Assert.AreEqual(existingCardNumber, updatedClient.AccessCardNumber);
    }

}