using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.LiteDb;

public class CacheTests
{
    private const string CacheValue = "Hello there!";

    [Fact]
    public async Task Cache_Is_Registered()
    {
        const string dbPath = "TestDb";

        var sp = new ServiceCollection()
            .AddLiteDbCache(dbPath)
            .BuildServiceProvider(true);

        var cache = sp.GetService<IDistributedCache>();
        var options = sp.GetService<IOptions<LiteDbCacheOptions>>()?.Value;

        cache.Should().NotBeNull();
        options.Should().NotBeNull();

        options!.CachePath.Should().Be(dbPath);

        await sp.DisposeAsync();
    }

    [Fact]
    public async Task Cached_Item_Can_Be_Retrieved()
    {
        var cacheKey = Guid.NewGuid().ToString();

        var sp = CreateProvider();
        var cache = sp.GetRequiredService<IDistributedCache>();

        await cache.SetStringAsync(cacheKey, CacheValue);

        var result = await cache.GetStringAsync(cacheKey);

        result.Should().Be(CacheValue);

        await sp.DisposeAsync();
    }

    [Fact]
    public async Task Removed_Item_Is_Removed()
    {
        var cacheKey = Guid.NewGuid().ToString();

        var sp = CreateProvider();
        var cache = sp.GetRequiredService<IDistributedCache>();

        await cache.SetStringAsync(cacheKey, CacheValue);

        await cache.RemoveAsync(cacheKey);

        var result = await cache.GetStringAsync(cacheKey);

        result.Should().BeNullOrWhiteSpace();

        await sp.DisposeAsync();
    }

    private static ServiceProvider CreateProvider() => new ServiceCollection()
        .AddLiteDbCache(Guid.NewGuid().ToString() + ".db")
        .BuildServiceProvider(true);
}
