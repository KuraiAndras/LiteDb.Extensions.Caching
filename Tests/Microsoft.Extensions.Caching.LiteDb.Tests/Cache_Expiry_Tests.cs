using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.Caching.LiteDb;

public sealed class Cache_Expiry_Tests : CacheTestBase
{
    [Fact]
    public async Task Absolute_Expired_Entry_Is_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();

        // Act
        const int expiration = 500;

        await Cache.SetStringAsync(cacheKey, "Test", new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(expiration) });

        await Task.Delay(expiration * 2);

        var value = await Cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Absolute_Not_Expired_Entry_Is_Not_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();
        const string cacheValue = "Test";

        // Act
        const int expiration = 1000;

        await Cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(expiration) });

        await Task.Delay(expiration / 2);

        var value = await Cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().Be(cacheValue);
    }

    [Fact]
    public async Task Absolute_From_Now_Expired_Entry_Is_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();

        // Act
        const int expiration = 500;

        await Cache.SetStringAsync(cacheKey, "Test", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(expiration) });

        await Task.Delay(expiration * 2);

        var value = await Cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Absolute_From_Now_Not_Expired_Entry_Is_Not_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();
        const string cacheValue = "Test";

        // Act
        const int expiration = 1000;

        await Cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(expiration) });

        await Task.Delay(expiration / 2);

        var value = await Cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().Be(cacheValue);
    }
}
