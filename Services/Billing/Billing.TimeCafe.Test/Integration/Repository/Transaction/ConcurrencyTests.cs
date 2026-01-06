namespace Billing.TimeCafe.Test.Integration.Repository.Transaction;

public class ConcurrencyTests : BaseTransactionRepositoryTest
{
    [Fact]
    public async Task Repository_ParallelGetByUserId_Should_HandleConcurrentReads()
    {
        await ClearCacheAsync();
        var userId = Defaults.UserId;
        await CreateTestTransactionAsync(userId);

        var tasks = new List<Task<List<TransactionModel>>>();

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var scope = CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                return await repository.GetByUserIdAsync(userId, page: 1, pageSize: 10);
            }));
        }

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.Should().NotBeEmpty());
        results.Should().AllSatisfy(r => r.Should().AllSatisfy(t => t.UserId.Should().Be(userId)));
    }

    [Fact]
    public async Task Repository_ParallelCreates_Should_HandleConcurrentCreations()
    {
        var userIds = new[] { Defaults.UserId, Defaults.UserId2, Defaults.UserId3 };

        var createTasks = userIds.Select(userId =>
        {
            return Task.Run(async () =>
            {
                using var scope = CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                var transaction = TransactionModel.CreateDeposit(
                    userId,
                    Defaults.DefaultAmount,
                    TransactionSource.Manual);
                return await repository.CreateAsync(transaction);
            });
        }).ToList();

        var results = await Task.WhenAll(createTasks);

        results.Should().HaveCount(3);
        results.Should().AllSatisfy(r => r.Should().NotBeNull());

        foreach (var userId in userIds)
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            var count = await repository.GetTotalCountByUserIdAsync(userId);
            count.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task Repository_ParallelCreatesMultiplePerUser_Should_InvalidateCacheCorrectly()
    {
        await ClearCacheAsync();
        var userId = Defaults.UserId;

        var createTasks = Enumerable.Range(0, 5).Select(i =>
        {
            return Task.Run(async () =>
            {
                using var scope = CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                var transaction = TransactionModel.CreateDeposit(
                    userId,
                    Defaults.SmallAmount * (i + 1),
                    TransactionSource.Manual);
                return await repository.CreateAsync(transaction);
            });
        }).ToList();

        var results = await Task.WhenAll(createTasks);

        results.Should().HaveCount(5);

        using var finalScope = CreateScope();
        var finalRepo = finalScope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var allTransactions = await finalRepo.GetByUserIdAsync(userId, page: 1, pageSize: 100);
        allTransactions.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task Repository_MixedReadCreateOperations_Should_HandleConcurrentMixedOperations()
    {
        await ClearCacheAsync();
        var userId = Defaults.UserId;
        await CreateTestTransactionAsync(userId);

        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            if (i % 2 == 0)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                    await repository.GetByUserIdAsync(userId, page: 1, pageSize: 10);
                }));
            }
            else
            {
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                    var transaction = TransactionModel.CreateWithdrawal(
                        userId,
                        Defaults.SmallAmount,
                        TransactionSource.Visit,
                        Defaults.TariffId);
                    await repository.CreateAsync(transaction);
                }));
            }
        }

        await Task.WhenAll(tasks);

        using var finalScope = CreateScope();
        var finalRepo = finalScope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var total = await finalRepo.GetTotalCountByUserIdAsync(userId);
        total.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Repository_ParallelGetBySource_Should_HandleConcurrentSourceQueries()
    {
        var sourceId = Defaults.TariffId;
        var transaction = TransactionModel.CreateWithdrawal(
            Defaults.UserId,
            Defaults.DefaultAmount,
            TransactionSource.Visit,
            sourceId);

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            await repository.CreateAsync(transaction);
        }

        var tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () =>
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            return await repository.GetBySourceAsync(TransactionSource.Visit, sourceId);
        })).ToList();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.Should().NotBeEmpty());
        results.Should().AllSatisfy(r => r.Should().AllSatisfy(t =>
        {
            t.Source.Should().Be(TransactionSource.Visit);
            t.SourceId.Should().Be(sourceId);
        }));
    }

    [Fact]
    public async Task Repository_ParallelExistsBySource_Should_HandleConcurrentExistenceChecks()
    {
        var sourceId = Defaults.PaymentId;
        var transaction = TransactionModel.CreateDeposit(
            Defaults.UserId,
            Defaults.DefaultAmount,
            TransactionSource.Payment,
            sourceId);

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            await repository.CreateAsync(transaction);
        }

        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
        {
            using var scope = CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            return await repository.ExistsBySourceAsync(TransactionSource.Payment, sourceId);
        })).ToList();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    [Fact]
    public async Task Repository_RapidCreateGetBySource_Should_MaintainConsistency()
    {
        var sourceIds = new[] { Defaults.TariffId, Defaults.PaymentId };
        var tasks = sourceIds.SelectMany(sourceId => new List<Task>
        {
            Task.Run(async () =>
            {
                using var scope = CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                var transaction = TransactionModel.CreateDeposit(
                    Defaults.UserId,
                    Defaults.SmallAmount,
                    TransactionSource.Visit,
                    sourceId);
                await repository.CreateAsync(transaction);
            }),
            Task.Run(async () =>
            {
                await Task.Delay(10);
                using var scope = CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                var result = await repository.GetBySourceAsync(TransactionSource.Visit, sourceId);
                result.Should().NotBeEmpty();
            })
        }).ToList();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task Repository_ConcurrentCreateAndGetTotalCount_Should_BeConsistent()
    {
        await ClearCacheAsync();
        var userId = Defaults.UserId;

        var tasks = new List<Task>();

        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var scope = CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                var transaction = TransactionModel.CreateDeposit(
                    userId,
                    Defaults.SmallAmount,
                    TransactionSource.Manual);
                await repository.CreateAsync(transaction);
            }));
        }

        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await Task.Delay(5);
                using var scope = CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                return await repository.GetTotalCountByUserIdAsync(userId);
            }));
        }

        await Task.WhenAll(tasks);

        using var finalScope = CreateScope();
        var finalRepo = finalScope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var total = await finalRepo.GetTotalCountByUserIdAsync(userId);
        total.Should().BeGreaterThanOrEqualTo(5);
    }
}
