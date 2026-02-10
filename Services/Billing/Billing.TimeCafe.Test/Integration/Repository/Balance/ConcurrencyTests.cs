
using Billing.TimeCafe.Test.TestData;

namespace Billing.TimeCafe.Test.Integration.Repository.Balance;

public class ConcurrencyTests : BaseBalanceRepositoryTest
{
    [Fact]
    public async Task Repository_ParallelGetByUserId_Should_HandleConcurrentReads()
    {
        await ClearCacheAsync();
        var userId = DefaultsGuid.UserId;
        await CreateTestBalanceAsync(userId);

        var tasks = new List<Task<BalanceModel?>>();

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var scope = CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
                return await repository.GetByUserIdAsync(userId);
            }));
        }

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.Should().NotBeNull());
        results.Should().AllSatisfy(r => r!.UserId.Should().Be(userId));
        results.Should().AllSatisfy(r => r!.CurrentBalance.Should().Be(DefaultsGuid.DefaultAmount));
    }

    [Fact]
    public async Task Repository_ParallelCreates_Should_HandleConcurrentCreations()
    {

        var userIds = new[] { DefaultsGuid.UserId, DefaultsGuid.UserId2, DefaultsGuid.UserId3 };

        var createTasks = userIds.Select(userId =>
        {
            return Task.Run(async () =>
            {
                using var scope = CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
                var balance = new BalanceModel(userId)
                {
                    CurrentBalance = DefaultsGuid.DefaultAmount,
                    TotalDeposited = DefaultsGuid.DefaultAmount
                };
                return await repository.CreateAsync(balance);
            });
        }).ToList();

        var results = await Task.WhenAll(createTasks);

        results.Should().HaveCount(3);
        results.Should().AllSatisfy(r => r.Should().NotBeNull());

        foreach (var userId in userIds)
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            var exists = await repository.ExistsAsync(userId);
            exists.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Repository_ParallelUpdates_Should_HandleConcurrentUpdates()
    {

        var userId = DefaultsGuid.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        var updateTasks = Enumerable.Range(1, 5).Select(async i =>
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            var current = await repository.GetByUserIdAsync(userId);
            current!.CurrentBalance = DefaultsGuid.DefaultAmount + (i * DefaultsGuid.SmallAmount);
            current.LastUpdated = DateTimeOffset.UtcNow;
            return await repository.UpdateAsync(current);
        }).ToList();

        var results = await Task.WhenAll(updateTasks);

        using var finalScope = CreateScope();
        var finalRepo = finalScope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var final = await finalRepo.GetByUserIdAsync(userId);
        final.Should().NotBeNull();
        final!.CurrentBalance.Should().BeGreaterThanOrEqualTo(DefaultsGuid.DefaultAmount);
    }

    [Fact]
    public async Task Repository_MixedOperations_Should_HandleConcurrentReadWriteUpdates()
    {

        var userId = DefaultsGuid.UserId;
        await CreateTestBalanceAsync(userId);

        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            if (i % 3 == 0)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
                    await repository.GetByUserIdAsync(userId);
                }));
            }
            else if (i % 3 == 1)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
                    var current = await repository.GetByUserIdAsync(userId);
                    if (current != null)
                    {
                        current.CurrentBalance += DefaultsGuid.SmallAmount;
                        current.LastUpdated = DateTimeOffset.UtcNow;
                        await repository.UpdateAsync(current);
                    }
                }));
            }
            else
            {
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
                    await repository.ExistsAsync(userId);
                }));
            }
        }

        await Task.WhenAll(tasks);

        using var finalScope = CreateScope();
        var finalRepo = finalScope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var final = await finalRepo.GetByUserIdAsync(userId);
        final.Should().NotBeNull();
        final!.CurrentBalance.Should().BeGreaterThanOrEqualTo(DefaultsGuid.DefaultAmount);
    }

    [Fact]
    public async Task Repository_ParallelGetDebtors_Should_HandleConcurrentQueries()
    {
        await ClearCacheAsync();
        var debtorIds = new[] { DefaultsGuid.UserId, DefaultsGuid.UserId2, DefaultsGuid.UserId3 };
        foreach (var debtorId in debtorIds)
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            await repository.CreateAsync(new BalanceModel(debtorId) { Debt = DefaultsGuid.DebtAmount });
        }

        using (var cacheScope = CreateScope())
        {
            var cache = cacheScope.ServiceProvider.GetRequiredService<IDistributedCache>();
            await cache.RemoveAsync(CacheKeys.Debtors_All);
        }

        var tasks = Enumerable.Range(0, 5).Select(_ => Task.Run(async () =>
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            return await repository.GetUsersWithDebtAsync();
        })).ToList();
        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.Should().HaveCount(3));
        results.Should().AllSatisfy(r => r.Should().OnlyContain(b => b.Debt > 0));
    }

    [Fact]
    public async Task Repository_RapidCreateGetUpdate_Should_MaintainConsistency()
    {

        var userIds = new[] { DefaultsGuid.UserId, DefaultsGuid.UserId2 };
        var createTasks = userIds.Select(userId => Task.Run(async () =>
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            var balance = new BalanceModel(userId) { CurrentBalance = DefaultsGuid.SmallAmount };
            await repository.CreateAsync(balance);
        })).ToList();

        await Task.WhenAll(createTasks);

        var updateTasks = userIds.Select(userId => Task.Run(async () =>
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            var balance = await repository.GetByUserIdAsync(userId);
            if (balance != null)
            {
                balance.CurrentBalance = DefaultsGuid.DefaultAmount;
                balance.LastUpdated = DateTimeOffset.UtcNow;
                await repository.UpdateAsync(balance);
            }
        })).ToList();

        await Task.WhenAll(updateTasks);

        var verifyTasks = userIds.Select(userId => Task.Run(async () =>
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            var balance = await repository.GetByUserIdAsync(userId);
            balance.Should().NotBeNull();
            balance!.CurrentBalance.Should().BeGreaterThanOrEqualTo(DefaultsGuid.SmallAmount);
        })).ToList();

        await Task.WhenAll(verifyTasks);

        foreach (var userId in userIds)
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            var exists = await repository.ExistsAsync(userId);
            exists.Should().BeTrue();

            var balance = await repository.GetByUserIdAsync(userId);
            balance.Should().NotBeNull();
            balance!.CurrentBalance.Should().BeGreaterThanOrEqualTo(DefaultsGuid.SmallAmount);
        }
    }

    [Fact]
    public async Task Repository_ConcurrentCacheInvalidation_Should_MaintainCacheConsistency()
    {
        await ClearCacheAsync();
        var userId = DefaultsGuid.UserId;
        await CreateTestBalanceAsync(userId);

        var updateTasks = Enumerable.Range(0, 10).Select(async i =>
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            var balance = await repository.GetByUserIdAsync(userId);
            if (balance != null)
            {
                balance.CurrentBalance = DefaultsGuid.DefaultAmount + (i * DefaultsGuid.SmallAmount);
                balance.LastUpdated = DateTimeOffset.UtcNow;
                await repository.UpdateAsync(balance);
            }
        }).ToList();

        var readTasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            return await repository.GetByUserIdAsync(userId);
        }).ToList();

        await Task.WhenAll(updateTasks.Concat(readTasks));

        using var finalScope = CreateScope();
        var finalRepo = finalScope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var final = await finalRepo.GetByUserIdAsync(userId);
        final.Should().NotBeNull();
        final!.CurrentBalance.Should().BeGreaterThanOrEqualTo(DefaultsGuid.DefaultAmount);
    }

    [Fact]
    public async Task Repository_StressTest_ParallelOperations()
    {

        var operationCount = 50;
        var userIds = new[] { DefaultsGuid.UserId, DefaultsGuid.UserId2, DefaultsGuid.UserId3 };

        var tasks = new List<Task>();

        for (int i = 0; i < operationCount; i++)
        {
            var userId = userIds[i % userIds.Length];

            switch (i % 4)
            {
                case 0:
                    tasks.Add(Task.Run(async () =>
                    {
                        using var scope = CreateScope();
                        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
                        var exists = await repository.ExistsAsync(userId);
                        if (!exists)
                        {
                            await repository.CreateAsync(new BalanceModel(userId) { CurrentBalance = DefaultsGuid.SmallAmount });
                        }
                    }));
                    break;

                case 1:
                    tasks.Add(Task.Run(async () =>
                    {
                        using var scope = CreateScope();
                        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
                        await repository.GetByUserIdAsync(userId);
                    }));
                    break;

                case 2:
                    tasks.Add(Task.Run(async () =>
                    {
                        using var scope = CreateScope();
                        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
                        var balance = await repository.GetByUserIdAsync(userId);
                        if (balance != null)
                        {
                            balance.CurrentBalance += DefaultsGuid.SmallAmount;
                            balance.LastUpdated = DateTimeOffset.UtcNow;
                            await repository.UpdateAsync(balance);
                        }
                    }));
                    break;

                case 3:
                    tasks.Add(Task.Run(async () =>
                    {
                        using var scope = CreateScope();
                        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
                        await repository.ExistsAsync(userId);
                    }));
                    break;
            }
        }

        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var scope = CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
                await repository.GetUsersWithDebtAsync();
            }));
        }

        await Task.WhenAll(tasks);

        using var finalScope = CreateScope();
        var finalRepo = finalScope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var allDebtors = await finalRepo.GetUsersWithDebtAsync();
        var createdCount = 0;

        foreach (var userId in userIds)
        {
            using var existsScope = CreateScope();
            var existsRepo = existsScope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            if (await existsRepo.ExistsAsync(userId))
            {
                createdCount++;
            }
        }

        createdCount.Should().BeGreaterThan(0);
    }
}
