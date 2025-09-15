using Microsoft.EntityFrameworkCore;
using TimeCafeWinUI3.Core.Models;


namespace TimeCafeWinUI3.Tests.MSTest.Services;

[TestClass]
public class VisitServiceTests
{
    private TimeCafeContext _context;
    private VisitService _service;
    private BillingTypeService _billingTypeService;
    private FinancialService _financialService;
    private Client _client;
    private Tariff _tariff;
    private BillingType _billingType;

    [TestInitialize]
    public void Initialize()
    {
        var options = new DbContextOptionsBuilder<TimeCafeContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TimeCafeContext(options);
        _billingTypeService = new BillingTypeService(_context);
        _financialService = new FinancialService(_context);
        _service = new VisitService(_context, _billingTypeService, _financialService);

        _billingType = new BillingType { BillingTypeId = 1, BillingTypeName = "Почасовая" };
        _context.BillingTypes.Add(_billingType);
        _tariff = new Tariff
        {
            TariffId = 1,
            TariffName = "Test Tariff",
            Price = 100,
            BillingTypeId = _billingType.BillingTypeId,
            BillingType = _billingType,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now
        };
        _context.Tariffs.Add(_tariff);
        var status = new ClientStatus { StatusId = 2, StatusName = "Активный" };
        _context.ClientStatuses.Add(status);
        _context.SaveChanges();
        _client = new Client
        {
            ClientId = 1,
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
            StatusId = 2,
            CreatedAt = DateTime.Now
        };
        _context.Clients.Add(_client);
        _context.TransactionTypes.AddRange(
            new TransactionType { TransactionTypeId = 1, TransactionTypeName = "Пополнение" },
            new TransactionType { TransactionTypeId = 2, TransactionTypeName = "Списание" }
        );
        _context.SaveChanges();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [TestMethod]
    public async Task EnterClientAsync_ShouldCreateVisit()
    {
        await _financialService.DepositAsync(_client.ClientId, 200); // чтобы хватило на вход
        var visit = await _service.EnterClientAsync(_client.ClientId, _tariff.TariffId, 60);
        Assert.IsNotNull(visit);
        Assert.AreEqual(_client.ClientId, visit.ClientId);
        Assert.AreEqual(_tariff.TariffId, visit.TariffId);
    }

    [TestMethod]
    public async Task ExitClientAsync_ShouldSetExitTimeAndCost()
    {
        await _financialService.DepositAsync(_client.ClientId, 200);
        var visit = await _service.EnterClientAsync(_client.ClientId, _tariff.TariffId, 60);
        var exited = await _service.ExitClientAsync(visit.VisitId);
        Assert.IsNotNull(exited.ExitTime);
        Assert.IsNotNull(exited.VisitCost);
    }

    [TestMethod]
    public async Task GetActiveVisitByClientAsync_ShouldReturnActiveVisit()
    {
        await _financialService.DepositAsync(_client.ClientId, 200);
        var visit = await _service.EnterClientAsync(_client.ClientId, _tariff.TariffId, 60);
        var active = await _service.GetActiveVisitByClientAsync(_client.ClientId);
        Assert.IsNotNull(active);
        Assert.AreEqual(visit.VisitId, active.VisitId);
    }

    [TestMethod]
    public async Task CalculateVisitCostAsync_ShouldReturnCorrectCost()
    {
        var now = DateTime.Now;
        var visit = new Visit
        {
            Tariff = _tariff,
            BillingTypeId = _billingType.BillingTypeId,
            EntryTime = now,
            ExitTime = now.AddHours(2)
        };
        var cost = await _service.CalculateVisitCostAsync(visit);
        Assert.AreEqual(200, cost); // 2 часа * 100
    }
}
