using Microsoft.EntityFrameworkCore;
using TimeCafeWinUI3.Core.Models;

namespace TimeCafeWinUI3.Tests.MSTest.Services;

[TestClass]
public class BillingTypeServiceTests
{
    private TimeCafeContext _context;
    private BillingTypeService _service;

    [TestInitialize]
    public void Initialize()
    {
        var options = new DbContextOptionsBuilder<TimeCafeContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TimeCafeContext(options);
        _service = new BillingTypeService(_context);

        _context.BillingTypes.AddRange(
            new BillingType { BillingTypeId = 1, BillingTypeName = "Почасовая" },
            new BillingType { BillingTypeId = 2, BillingTypeName = "Поминутная" }
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
    public async Task GetBillingTypesAsync_ShouldReturnAllTypes()
    {
        var result = await _service.GetBillingTypesAsync();
        Assert.AreEqual(2, result.Count());
        Assert.IsTrue(result.Any(b => b.BillingTypeName == "Почасовая"));
        Assert.IsTrue(result.Any(b => b.BillingTypeName == "Поминутная"));
    }

    [TestMethod]
    public async Task GetBillingTypeByIdAsync_ShouldReturnCorrectType()
    {
        var result = await _service.GetBillingTypeByIdAsync(2);
        Assert.IsNotNull(result);
        Assert.AreEqual("Поминутная", result.BillingTypeName);
    }

    [TestMethod]
    public async Task GetBillingTypeByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetBillingTypeByIdAsync(999);
        Assert.IsNull(result);
    }
}
