using Microsoft.EntityFrameworkCore;
using TimeCafeWinUI3.Core.Models;

namespace TimeCafeWinUI3.Tests.MSTest.Services;

[TestClass]
public class FinancialServiceTests
{
    private TimeCafeContext _context;
    private FinancialService _service;
    private Client _testClient;

    [TestInitialize]
    public void Initialize()
    {
        var options = new DbContextOptionsBuilder<TimeCafeContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TimeCafeContext(options);
        _service = new FinancialService(_context);

        // Добавляем справочные данные
        _context.TransactionTypes.AddRange(
            new TransactionType { TransactionTypeId = 1, TransactionTypeName = "Пополнение" },
            new TransactionType { TransactionTypeId = 2, TransactionTypeName = "Списание" }
        );
        _context.Clients.Add(_testClient = new Client
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+375 (29) 123 4567",
            Email = "test@example.com",
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
    public async Task DepositAsync_ShouldIncreaseBalance()
    {
        var result = await _service.DepositAsync(_testClient.ClientId, 100);
        Assert.IsNotNull(result);
        Assert.AreEqual(100, await _service.GetClientBalanceAsync(_testClient.ClientId));
    }

    [TestMethod]
    public async Task DeductAsync_ShouldDecreaseBalance()
    {
        await _service.DepositAsync(_testClient.ClientId, 200);
        var result = await _service.DeductAsync(_testClient.ClientId, 50);
        Assert.IsNotNull(result);
        Assert.AreEqual(150, await _service.GetClientBalanceAsync(_testClient.ClientId));
    }

    [TestMethod]
    public async Task GetClientDebtAsync_ShouldReturnDebt_WhenNegativeBalance()
    {
        await _service.DeductAsync(_testClient.ClientId, 50);
        var debt = await _service.GetClientDebtAsync(_testClient.ClientId);
        Assert.AreEqual(50, debt);
    }

    [TestMethod]
    public async Task HasSufficientBalanceAsync_ShouldReturnTrue_WhenEnough()
    {
        await _service.DepositAsync(_testClient.ClientId, 100);
        var hasEnough = await _service.HasSufficientBalanceAsync(_testClient.ClientId, 50);
        Assert.IsTrue(hasEnough);
    }

    [TestMethod]
    public async Task HasSufficientBalanceAsync_ShouldReturnFalse_WhenNotEnough()
    {
        await _service.DepositAsync(_testClient.ClientId, 30);
        var hasEnough = await _service.HasSufficientBalanceAsync(_testClient.ClientId, 50);
        Assert.IsFalse(hasEnough);
    }
}
