using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.LiteDb;

public class CacheTests
{
    [Fact]
    public async Task Cache_Is_Registered()
    {
        // Arrange
        const string dbPath = "TestDb";

        var sp = new ServiceCollection()
            .AddLiteDbCache(dbPath)
            .BuildServiceProvider(true);

        // Act
        var cache = sp.GetService<IDistributedCache>();
        var options = sp.GetService<IOptions<LiteDbCacheOptions>>()?.Value;

        // Assert
        cache.Should().NotBeNull();
        options.Should().NotBeNull();

        options!.CachePath.Should().Be(dbPath);

        await sp.DisposeAsync();
    }

    [Fact]
    public async Task Cached_Item_Can_Be_Retrieved()
    {
        // Arrange
        const string cacheValue = "Hello there!";
        var cacheKey = Guid.NewGuid().ToString();

        var sp = CreateProvider();
        var cache = sp.GetRequiredService<IDistributedCache>();

        // Act
        await cache.SetStringAsync(cacheKey, cacheValue);

        var result = await cache.GetStringAsync(cacheKey);

        // Assert
        result.Should().Be(cacheValue);

        await sp.DisposeAsync();
    }

    [Fact]
    public async Task Removed_Item_Is_Removed()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();

        var sp = CreateProvider();
        var cache = sp.GetRequiredService<IDistributedCache>();

        // Act
        await cache.SetStringAsync(cacheKey, "Test");

        await cache.RemoveAsync(cacheKey);

        var result = await cache.GetStringAsync(cacheKey);

        // Assert
        result.Should().BeNullOrWhiteSpace();

        await sp.DisposeAsync();
    }

    [Fact]
    public async Task The_Right_Item_Is_Retrieved()
    {
        // Arrange
        var cacheKey1 = Guid.NewGuid().ToString();
        var cacheKey2 = Guid.NewGuid().ToString();
        const string cacheValue1 = "Test1";
        const string cacheValue2 = "Test1";

        var sp = CreateProvider();
        var cache = sp.GetRequiredService<IDistributedCache>();

        // Act
        await cache.SetStringAsync(cacheKey1, cacheValue1);
        await cache.SetStringAsync(cacheKey2, cacheValue2);

        var result1 = await cache.GetStringAsync(cacheKey1);
        var result2 = await cache.GetStringAsync(cacheKey2);

        // Assert
        result1.Should().Be(cacheValue1);
        result2.Should().Be(cacheValue2);
    }

    private static ServiceProvider CreateProvider() => new ServiceCollection()
        .AddLiteDbCache(Guid.NewGuid().ToString() + ".db")
        .BuildServiceProvider(true);
}
