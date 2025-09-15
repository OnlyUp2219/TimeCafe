using Microsoft.EntityFrameworkCore;
using TimeCafeWinUI3.Core.Enums;
using TimeCafeWinUI3.Core.Models;

namespace TimeCafeWinUI3.Tests.MSTest.Services;

[TestClass]
public class ClientAdditionalInfoServiceTests
{
    private TimeCafeContext _context;
    private ClientAdditionalInfoService _service;
    private Client _testClient;

    [TestInitialize]
    public void Initialize()
    {
        var options = new DbContextOptionsBuilder<TimeCafeContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TimeCafeContext(options);
        _service = new ClientAdditionalInfoService(_context);

        // Добавляем необходимые справочные данные
        _context.ClientStatuses.AddRange(
            new ClientStatus { StatusId = (int)EClientStatusType.Draft, StatusName = "Черновик" },
            new ClientStatus { StatusId = (int)EClientStatusType.Active, StatusName = "Активный" }
        );
        _context.Genders.AddRange(
            new Gender { GenderId = 1, GenderName = "Женский" },
            new Gender { GenderId = 2, GenderName = "Мужской" }
        );

        // Создаем тестового клиента
        _testClient = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
            StatusId = (int)EClientStatusType.Draft,
            CreatedAt = DateTime.Now
        };
        _context.Clients.Add(_testClient);
        _context.SaveChanges();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [TestMethod]
    public async Task CreateAdditionalInfo_ShouldCreateNewInfo()
    {
        var info = new ClientAdditionalInfo
        {
            ClientId = _testClient.ClientId,
            InfoText = "Test info"
        };

        var result = await _service.CreateAdditionalInfoAsync(info);

        Assert.IsNotNull(result);
        Assert.AreEqual(_testClient.ClientId, result.ClientId);
        Assert.AreEqual("Test info", result.InfoText);
        Assert.AreNotEqual(default, result.CreatedAt);
    }

    [TestMethod]
    public async Task UpdateAdditionalInfo_ShouldUpdateExistingInfo()
    {
        var info = new ClientAdditionalInfo
        {
            ClientId = _testClient.ClientId,
            InfoText = "Original info"
        };
        _context.ClientAdditionalInfos.Add(info);
        _context.SaveChanges();

        info.InfoText = "Updated info";
        var result = await _service.UpdateAdditionalInfoAsync(info);

        Assert.IsNotNull(result);
        Assert.AreEqual("Updated info", result.InfoText);
    }

    [TestMethod]
    public async Task DeleteAdditionalInfo_ShouldDeleteInfo()
    {
        var info = new ClientAdditionalInfo
        {
            ClientId = _testClient.ClientId,
            InfoText = "Test info"
        };
        _context.ClientAdditionalInfos.Add(info);
        _context.SaveChanges();

        var result = await _service.DeleteAdditionalInfoAsync(info.InfoId);

        Assert.IsTrue(result);
        Assert.IsNull(await _context.ClientAdditionalInfos.FindAsync(info.InfoId));
    }

    [TestMethod]
    public async Task GetClientAdditionalInfos_ShouldReturnAllInfosForClient()
    {
        var infos = new List<ClientAdditionalInfo>
        {
            new() { ClientId = _testClient.ClientId, InfoText = "Info 1" },
            new() { ClientId = _testClient.ClientId, InfoText = "Info 2" }
        };
        _context.ClientAdditionalInfos.AddRange(infos);
        _context.SaveChanges();

        var result = await _service.GetClientAdditionalInfosAsync(_testClient.ClientId);

        Assert.AreEqual(2, result.Count());
        Assert.IsTrue(result.All(i => i.ClientId == _testClient.ClientId));
    }

    [TestMethod]
    public async Task GetAdditionalInfoById_ShouldReturnCorrectInfo()
    {
        var info = new ClientAdditionalInfo
        {
            ClientId = _testClient.ClientId,
            InfoText = "Test info"
        };
        _context.ClientAdditionalInfos.Add(info);
        _context.SaveChanges();

        var result = await _service.GetAdditionalInfoByIdAsync(info.InfoId);

        Assert.IsNotNull(result);
        Assert.AreEqual(info.InfoId, result.InfoId);
        Assert.AreEqual("Test info", result.InfoText);
        Assert.IsNotNull(result.Client);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public async Task UpdateAdditionalInfo_WithNonExistentId_ShouldThrowException()
    {
        var info = new ClientAdditionalInfo
        {
            InfoId = 999,
            ClientId = _testClient.ClientId,
            InfoText = "Test info"
        };

        await _service.UpdateAdditionalInfoAsync(info);
    }

    [TestMethod]
    public async Task DeleteAdditionalInfo_WithNonExistentId_ShouldReturnFalse()
    {
        var result = await _service.DeleteAdditionalInfoAsync(999);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task GetAdditionalInfoById_WithNonExistentId_ShouldReturnNull()
    {
        var result = await _service.GetAdditionalInfoByIdAsync(999);
        Assert.IsNull(result);
    }
}