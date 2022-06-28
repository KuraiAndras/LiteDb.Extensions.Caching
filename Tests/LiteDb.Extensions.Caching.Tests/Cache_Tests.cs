using Microsoft.Extensions.Caching.Distributed;

namespace LiteDb.Extensions.Caching;

public class Cache_Tests : CacheTestBase
{
    [Fact]
    public async Task Cached_Item_Can_Be_Retrieved()
    {
        // Arrange
        const string cacheValue = "Hello there!";
        var cacheKey = Guid.NewGuid().ToString();

        // Act
        await Cache.SetStringAsync(cacheKey, cacheValue);

        var result = await Cache.GetStringAsync(cacheKey);

        // Assert
        result.Should().Be(cacheValue);
    }

    [Fact]
    public async Task Removed_Item_Is_Removed()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();

        // Act
        await Cache.SetStringAsync(cacheKey, "Test");

        await Cache.RemoveAsync(cacheKey);

        var result = await Cache.GetStringAsync(cacheKey);

        // Assert
        result.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task The_Right_Item_Is_Retrieved()
    {
        // Arrange
        var cacheKey1 = Guid.NewGuid().ToString();
        var cacheKey2 = Guid.NewGuid().ToString();
        const string cacheValue1 = "Test1";
        const string cacheValue2 = "Test2";

        // Act
        await Cache.SetStringAsync(cacheKey1, cacheValue1);
        await Cache.SetStringAsync(cacheKey2, cacheValue2);

        var result1 = await Cache.GetStringAsync(cacheKey1);
        var result2 = await Cache.GetStringAsync(cacheKey2);

        // Assert
        result1.Should().Be(cacheValue1);
        result2.Should().Be(cacheValue2);
    }

    [Fact]
    public async Task The_Right_Item_Is_Removed()
    {
        // Arrange
        var cacheKey1 = Guid.NewGuid().ToString();
        var cacheKey2 = Guid.NewGuid().ToString();
        const string cacheValue1 = "Test1";
        const string cacheValue2 = "Test2";

        // Act
        await Cache.SetStringAsync(cacheKey1, cacheValue1);
        await Cache.SetStringAsync(cacheKey2, cacheValue2);

        await Cache.RemoveAsync(cacheKey1);

        var result1 = await Cache.GetStringAsync(cacheKey1);
        var result2 = await Cache.GetStringAsync(cacheKey2);

        // Assert
        result1.Should().BeNullOrWhiteSpace();
        result2.Should().Be(cacheValue2);
    }

    [Fact]
    public async Task Same_Key_Overwrties_Old_Value()
    {
        // Arrange
        var cacheKey = Guid.NewGuid().ToString();
        const string cacheValue1 = "Test1";
        const string cacheValue2 = "Test2";

        // Act
        await Cache.SetStringAsync(cacheKey, cacheValue1);
        await Cache.SetStringAsync(cacheKey, cacheValue2);

        var result = await Cache.GetStringAsync(cacheKey);

        // Assert
        result.Should().Be(cacheValue2);
    }
}
