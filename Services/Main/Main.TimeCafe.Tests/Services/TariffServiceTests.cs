using Microsoft.EntityFrameworkCore;
using TimeCafeWinUI3.Core.Models;

namespace TimeCafeWinUI3.Tests.MSTest.Services;

[TestClass]
public class TariffServiceTests
{
    private TimeCafeContext _context;
    private TariffService _service;
    private BillingType _billingType;
    private Theme _theme;

    [TestInitialize]
    public void Initialize()
    {
        var options = new DbContextOptionsBuilder<TimeCafeContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TimeCafeContext(options);
        _service = new TariffService(_context);

        _billingType = new BillingType { BillingTypeId = 1, BillingTypeName = "Почасовая" };
        _theme = new Theme { ThemeId = 1, ThemeName = "Classic", TechnicalName = "classic" };
        _context.BillingTypes.Add(_billingType);
        _context.Themes.Add(_theme);
        _context.SaveChanges();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [TestMethod]
    public async Task CreateTariffAsync_ShouldCreateTariff()
    {
        var tariff = new Tariff
        {
            TariffName = "Test Tariff",
            Price = 100,
            BillingTypeId = _billingType.BillingTypeId,
            ThemeId = _theme.ThemeId,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now
        };
        var result = await _service.CreateTariffAsync(tariff);
        Assert.IsNotNull(result);
        Assert.AreEqual("Test Tariff", result.TariffName);
    }

    [TestMethod]
    public async Task GetAllTariffsAsync_ShouldReturnAllTariffs()
    {
        _context.Tariffs.Add(new Tariff
        {
            TariffName = "Tariff1",
            Price = 50,
            BillingTypeId = _billingType.BillingTypeId,
            ThemeId = _theme.ThemeId,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now
        });
        _context.Tariffs.Add(new Tariff
        {
            TariffName = "Tariff2",
            Price = 70,
            BillingTypeId = _billingType.BillingTypeId,
            ThemeId = _theme.ThemeId,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now
        });
        _context.SaveChanges();
        var result = await _service.GetAllTariffsAsync();
        Assert.AreEqual(2, result.Count());
    }

    [TestMethod]
    public async Task GetTariffByIdAsync_ShouldReturnCorrectTariff()
    {
        var tariff = new Tariff
        {
            TariffName = "Tariff1",
            Price = 50,
            BillingTypeId = _billingType.BillingTypeId,
            ThemeId = _theme.ThemeId,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now
        };
        _context.Tariffs.Add(tariff);
        _context.SaveChanges();
        var result = await _service.GetTariffByIdAsync(tariff.TariffId);
        Assert.IsNotNull(result);
        Assert.AreEqual("Tariff1", result.TariffName);
    }

    [TestMethod]
    public async Task UpdateTariffAsync_ShouldUpdateTariff()
    {
        var tariff = new Tariff
        {
            TariffName = "Tariff1",
            Price = 50,
            BillingTypeId = _billingType.BillingTypeId,
            ThemeId = _theme.ThemeId,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now
        };
        _context.Tariffs.Add(tariff);
        _context.SaveChanges();
        tariff.TariffName = "Updated";
        var result = await _service.UpdateTariffAsync(tariff);
        Assert.AreEqual("Updated", result.TariffName);
    }

    [TestMethod]
    public async Task DeleteTariffAsync_ShouldDeleteTariff()
    {
        var tariff = new Tariff
        {
            TariffName = "Tariff1",
            Price = 50,
            BillingTypeId = _billingType.BillingTypeId,
            ThemeId = _theme.ThemeId,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now
        };
        _context.Tariffs.Add(tariff);
        _context.SaveChanges();
        var result = await _service.DeleteTariffAsync(tariff.TariffId);
        Assert.IsTrue(result);
        Assert.IsNull(await _context.Tariffs.FindAsync(tariff.TariffId));
    }
}
