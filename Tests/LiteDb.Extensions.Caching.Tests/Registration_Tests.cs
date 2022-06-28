using LiteDB;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LiteDb.Extensions.Caching;

public class Registration_Tests
{
    [Fact]
    public async Task Cache_Is_Registered()
    {
        // Arrange
        var dbPath = Path.Combine("Caches", $"{Guid.NewGuid()}.db");

        var sp = new ServiceCollection()
            .AddLiteDbCache(dbPath)
            .BuildServiceProvider(true);

        // Act
        var cache = sp.GetService<IDistributedCache>();
        var options = sp.GetService<IOptions<LiteDbCacheOptions>>()!.Value;

        // Assert
        cache.Should().NotBeNull();
        options.Should().NotBeNull();

        options.CachePath.Should().Be(dbPath);

        await sp.DisposeAsync();

        File.Delete(dbPath);
    }

    [Fact]
    public async Task Password_Configuration_Works()
    {
        // Arrange
        var dbPath = Path.Combine("Caches", $"{Guid.NewGuid()}.db");

        const string password = "Test1234!";

        var sp = new ServiceCollection()
            .AddLiteDbCache(dbPath, password)
            .BuildServiceProvider(true);

        // Act
        sp.GetRequiredService<IDistributedCache>();

        await sp.DisposeAsync();

        var wrongPassword = () =>
        {
            var db = new LiteDatabase(new ConnectionString(dbPath) { Password = "Wrong" });
            db.Dispose();
        };
        var rightPassword = () =>
        {
            var db = new LiteDatabase(new ConnectionString(dbPath) { Password = password });
            db.Dispose();
        };

        // Assert
        wrongPassword.Should().Throw<LiteException>();
        rightPassword.Should().NotThrow();

        // Cleanup
        File.Delete(dbPath);
    }

    [Fact]
    public async Task All_Instances_Are_The_Same()
    {
        // Arrange
        var dbPath = Path.Combine("Caches", $"{Guid.NewGuid()}.db");

        var sp = new ServiceCollection()
            .AddLiteDbCache(dbPath)
            .BuildServiceProvider(true);

        // Act
        var cache = sp.GetService<IDistributedCache>();
        var liteDbCacheInterface = sp.GetService<ILiteDbCache>();
        var liteDbCache = sp.GetService<LiteDbCache>();

        // Assert
        cache.Should().NotBeNull();

        cache.Should().Be(liteDbCacheInterface);
        cache.Should().Be(liteDbCache);

        await sp.DisposeAsync();

        File.Delete(dbPath);
    }
}
