using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Moq;

using System.Text.Json;

using UserProfile.TimeCafe.Domain.Constants;
using UserProfile.TimeCafe.Domain.Models;
using UserProfile.TimeCafe.Infrastructure.Data;
using UserProfile.TimeCafe.Infrastructure.Repositories;

namespace UserProfile.TimeCafe.Test
{
    public class UserRepositoriesTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<UserRepositories>> _loggerMock;
        private readonly UserRepositories _repository;
        private readonly List<Profile> _testProfiles;
        private bool _disposed;

        public UserRepositoriesTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<UserRepositories>>();
            _repository = new UserRepositories(_context, _cacheMock.Object, _loggerMock.Object);

            _testProfiles =
            [
                new() { UserId = "1", FirstName = "John", CreatedAt = DateTime.UtcNow },
                new() { UserId = "2", FirstName = "Jane", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { UserId = "3", FirstName = "Bob", CreatedAt = DateTime.UtcNow.AddDays(-2) }
            ];
        }

        private async Task SeedProfilesAsync()
        {
            _context.Profiles.AddRange(_testProfiles);
            await _context.SaveChangesAsync();
        }

        private void SetupCache<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            _cacheMock.Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);
        }

        [Fact]
        public async Task GetAllProfilesAsync_ReturnsAllProfiles_WhenNoCache()
        {
            // Arrange
            await SeedProfilesAsync();

            // Act
            var result = await _repository.GetAllProfilesAsync(CancellationToken.None);

            // Assert
            result.Should().HaveCount(_testProfiles.Count)
                .And.BeEquivalentTo(_testProfiles, options => options
                    .Including(p => p.UserId)
                    .Including(p => p.FirstName));
        }

        [Fact]
        public async Task GetAllProfilesAsync_ReturnsCachedProfiles_WhenCacheExists()
        {
            // Arrange
            SetupCache(CacheKeys.Profile_All, _testProfiles);
            _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _repository.GetAllProfilesAsync(CancellationToken.None);

            // Assert
            result.Should().HaveCount(_testProfiles.Count)
                .And.BeEquivalentTo(_testProfiles, options => options
                    .Including(p => p.UserId)
                    .Including(p => p.FirstName));
            _cacheMock.Verify(c => c.GetAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
            _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetProfilesPageAsync_ReturnsPagedProfiles_WhenNoCache()
        {
            // Arrange
            await SeedProfilesAsync();

            // Act
            var result = await _repository.GetProfilesPageAsync(1, 2, CancellationToken.None);

            // Assert
            result.Should().NotBeNull("Result should not be null")
                .And.HaveCount(2)
                .And.Contain(p => p.UserId == "1")
                .And.Contain(p => p.UserId == "2");
        }

        [Fact]
        public async Task GetProfilesPageAsync_ReturnsCachedPagedProfiles_WhenCacheExists()
        {
            // Arrange
            var pageProfiles = _testProfiles.Take(2).ToList();
            SetupCache(CacheKeys.Profile_Page(1, 1), pageProfiles);
            _cacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));
            _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _repository.GetProfilesPageAsync(1, 2, CancellationToken.None);

            // Assert
            result.Should().NotBeNull()
                .And.HaveCount(2)
                .And.BeEquivalentTo(pageProfiles, options => options
                    .Including(p => p.UserId)
                    .Including(p => p.FirstName));
            _cacheMock.Verify(c => c.GetAsync(CacheKeys.Profile_Page(1, 1), It.IsAny<CancellationToken>()), Times.Once());
            _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetTotalPageAsync_ReturnsCorrectCount()
        {
            // Arrange
            await SeedProfilesAsync();

            // Act
            var result = await _repository.GetTotalPageAsync(CancellationToken.None);

            // Assert
            result.Should().Be(_testProfiles.Count);
        }

        [Fact]
        public async Task GetProfileByIdAsync_ReturnsProfile_WhenExists()
        {
            // Arrange
            await SeedProfilesAsync();

            // Act
            var result = await _repository.GetProfileByIdAsync("1", CancellationToken.None);

            // Assert
            result.Should().NotBeNull()
                .And.BeEquivalentTo(_testProfiles[0], options => options
                    .Including(p => p.UserId)
                    .Including(p => p.FirstName));
        }

        [Fact]
        public async Task GetProfileByIdAsync_ReturnsCachedProfile_WhenCacheExists()
        {
            // Arrange
            var profile = _testProfiles[0];
            SetupCache(CacheKeys.Profile_ById("1"), profile);
            _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _repository.GetProfileByIdAsync("1", CancellationToken.None);

            // Assert
            result.Should().NotBeNull()
                .And.BeEquivalentTo(profile, options => options
                    .Including(p => p.UserId)
                    .Including(p => p.FirstName));
            _cacheMock.Verify(c => c.GetAsync(CacheKeys.Profile_ById("1"), It.IsAny<CancellationToken>()), Times.Once());
            _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateProfileAsync_AddsProfileAndInvalidatesCache()
        {
            // Arrange
            var profile = new Profile { UserId = "4", FirstName = "Alice" };
            _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _cacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));
            _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _repository.CreateProfileAsync(profile, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            var dbProfile = await _context.Profiles.FindAsync("4");
            dbProfile.Should().NotBeNull()
                .And.BeEquivalentTo(profile, options => options
                    .Including(p => p.UserId)
                    .Including(p => p.FirstName));
            _cacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
            _cacheMock.Verify(c => c.SetAsync(CacheKeys.ProfilePagesVersion(), It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == "2"), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateProfileAsync_UpdatesProfileAndInvalidatesCache()
        {
            // Arrange
            await SeedProfilesAsync();
            var updatedProfile = new Profile { UserId = "1", FirstName = "Jane" };
            _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _cacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));
            _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _repository.UpdateProfileAsync(updatedProfile, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            var dbProfile = await _context.Profiles.FindAsync("1");
            dbProfile.FirstName.Should().NotBeNull("Result should not be null").And.Be("Jane");
            _cacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
            _cacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_ById("1"), It.IsAny<CancellationToken>()), Times.Once());
            _cacheMock.Verify(c => c.SetAsync(CacheKeys.ProfilePagesVersion(), It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == "2"), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeleteProfileAsync_RemovesProfileAndInvalidatesCache()
        {
            // Arrange
            await SeedProfilesAsync();
            _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _cacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));
            _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.DeleteProfileAsync("1", CancellationToken.None);

            // Assert
            var dbProfile = await _context.Profiles.FindAsync("1");
            dbProfile.Should().BeNull();
            _cacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
            _cacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_ById("1"), It.IsAny<CancellationToken>()), Times.Once());
            _cacheMock.Verify(c => c.SetAsync(CacheKeys.ProfilePagesVersion(), It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == "2"), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateEmptyAsync_CreatesEmptyProfileAndInvalidatesCache_WhenNotExists()
        {
            // Arrange
            var userId = "4";
            _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _cacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));
            _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.CreateEmptyAsync(userId, CancellationToken.None);

            // Assert
            var dbProfile = await _context.Profiles.FindAsync(userId);
            dbProfile.Should().NotBeNull();
            dbProfile.UserId.Should().Be(userId);
            dbProfile.FirstName.Should().BeEmpty();
            dbProfile.ProfileStatus.Should().Be(ProfileStatus.Pending);
            _cacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
            _cacheMock.Verify(c => c.SetAsync(CacheKeys.ProfilePagesVersion(), System.Text.Encoding.UTF8.GetBytes("2"), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateEmptyAsync_DoesNothing_WhenProfileExists()
        {
            // Arrange
            await SeedProfilesAsync();

            // Act
            await _repository.CreateEmptyAsync("1", CancellationToken.None);

            // Assert
            var dbProfiles = await _context.Profiles.ToListAsync();
            dbProfiles.Should().HaveCount(_testProfiles.Count);
            dbProfiles.First(p => p.UserId == "1").FirstName.Should().Be("John");
            _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
            _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _context?.Dispose();
            }

            _disposed = true;
        }

        ~UserRepositoriesTests()
        {
            Dispose(false);
        }
    }
}