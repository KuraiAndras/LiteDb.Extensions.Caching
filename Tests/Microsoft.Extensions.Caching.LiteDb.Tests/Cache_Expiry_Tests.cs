using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.Caching.LiteDb;

public sealed class Cache_Expiry_Tests : CacheTestBase
{
    private const int Expiration = 300;

    [Fact]
    public async Task Absolute_Expired_Entry_Is_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();

        // Act
        await Cache.SetStringAsync(cacheKey, "Test", new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(Expiration) });

        await Task.Delay(Expiration * 2);

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
        await Cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(Expiration) });

        await Task.Delay(Expiration / 2);

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
        await Cache.SetStringAsync(cacheKey, "Test", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(Expiration) });

        await Task.Delay(Expiration * 2);

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
        await Cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(Expiration) });

        await Task.Delay(Expiration / 2);

        var value = await Cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().Be(cacheValue);
    }

    [Fact]
    public async Task Sliding_Expired_Is_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();

        // Act
        await Cache.SetStringAsync(cacheKey, "Test", new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMilliseconds(Expiration) });

        await Task.Delay(Expiration * 2);

        var value = await Cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Sliding_Not_Expired_Is_Not_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();
        const string cacheValue = "Test";

        // Act
        await Cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMilliseconds(Expiration) });

        await Task.Delay(Expiration / 2);

        var value = await Cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().Be(cacheValue);
    }

    [Fact]
    public async Task Sliding_Is_Renewed()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();
        const string cacheValue = "Test";

        // Act
        await Cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMilliseconds(Expiration) });

        await Task.Delay(Expiration / 2);

        var value1 = await Cache.GetStringAsync(cacheKey);

        await Task.Delay(Expiration / 2);

        var value2 = await Cache.GetStringAsync(cacheKey);

        await Task.Delay(Expiration / 2);

        await Cache.RefreshAsync(cacheKey);

        await Task.Delay(Expiration / 2);

        var value3 = await Cache.GetStringAsync(cacheKey);

        // Assert
        value1.Should().Be(cacheValue);
        value2.Should().Be(value1);
        value3.Should().Be(value2);
    }

    [Fact]
    public async Task Sliding_Expired_Refesh_Removes_Item()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();

        // Act
        await Cache.SetStringAsync(cacheKey, "Test", new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMilliseconds(Expiration) });

        await Task.Delay(Expiration * 2);

        await Cache.RefreshAsync(cacheKey);

        var value = await Cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Absolute_With_Sliding_Expired_Is_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();
        const int absoluteDelay = 300;

        // Act
        await Cache.SetStringAsync(cacheKey, "Test", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(absoluteDelay),
            SlidingExpiration = TimeSpan.FromMilliseconds(Expiration)
        });

        await Task.Delay((Expiration + absoluteDelay) * 2);

        var value = await Cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Absolute_With_Sliding_Not_Expired_Is_Not_Empty()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();
        const string cacheValue = "Test";
        const int absoluteDelay = 300;

        // Act
        await Cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(absoluteDelay),
            SlidingExpiration = TimeSpan.FromMilliseconds(Expiration)
        });

        await Task.Delay(Expiration / 2);

        var value = await Cache.GetStringAsync(cacheKey);

        // Assert
        value.Should().Be(cacheValue);
    }
}
