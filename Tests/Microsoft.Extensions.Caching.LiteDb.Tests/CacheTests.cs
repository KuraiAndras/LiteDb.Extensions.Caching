using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.LiteDb;

public class CacheTests
{
    [Fact]
    public void Cache_Is_Registered()
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
    }

    [Fact]
    public async Task Cached_Item_Can_Be_Retrieved()
    {
        const string cacheValue = "Hello there!";
        var cacheKey = Guid.NewGuid().ToString();

        var cache = CreateProvider().GetRequiredService<IDistributedCache>();

        await cache.SetStringAsync(cacheKey, cacheValue);

        var result = await cache.GetStringAsync(cacheKey);

        result.Should().Be(cacheValue);
    }

    private static IServiceProvider CreateProvider() => new ServiceCollection()
        .AddLiteDbCache(Guid.NewGuid().ToString() + ".db")
        .BuildServiceProvider(true);
}