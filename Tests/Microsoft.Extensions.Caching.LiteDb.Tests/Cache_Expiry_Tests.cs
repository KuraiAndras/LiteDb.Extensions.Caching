using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Caching.LiteDb;

public class Cache_Expiry_Tests
{
    [Fact]
    public async Task Absolute_Expired_Entry_Is_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();

        var sp = CreateProvider();
        var cache = sp.GetRequiredService<IDistributedCache>();

        // Act
        const int expiration = 500;

        await cache.SetStringAsync(cacheKey, "Test", new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(expiration) });

        await Task.Delay(expiration);

        var value = await cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().BeNullOrWhiteSpace();

        await sp.DisposeAsync();
    }

    [Fact]
    public async Task Absolute_Not_Expired_Entry_Is_Not_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();
        const string cacheValue = "Test";

        var sp = CreateProvider();
        var cache = sp.GetRequiredService<IDistributedCache>();

        // Act
        await cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(1000) });

        await Task.Delay(500);

        var value = await cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().Be(cacheValue);

        await sp.DisposeAsync();
    }

    [Fact]
    public async Task Absolute_From_Now_Expired_Entry_Is_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();

        var sp = CreateProvider();
        var cache = sp.GetRequiredService<IDistributedCache>();

        // Act
        const int expiration = 500;

        await cache.SetStringAsync(cacheKey, "Test", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(expiration) });

        await Task.Delay(expiration);

        var value = await cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().BeNullOrWhiteSpace();

        await sp.DisposeAsync();
    }

    [Fact]
    public async Task Absolute_From_Now_Not_Expired_Entry_Is_Not_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();
        const string cacheValue = "Test";

        var sp = CreateProvider();
        var cache = sp.GetRequiredService<IDistributedCache>();

        // Act
        await cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(1000) });

        await Task.Delay(500);

        var value = await cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().Be(cacheValue);

        await sp.DisposeAsync();
    }
}
