using Microsoft.Extensions.Caching.Distributed;

namespace LiteDb.Extensions.Caching;

public sealed class Manual_Mode_Tests : CacheTestBase
{
    [Fact]
    public async Task Cache_Count_Is_Exact()
    {
        // Arrange
        var cache = (ILiteDbCache)Cache;

        // Act
        await cache.SetAsync("Test0", Array.Empty<byte>(), new());
        await cache.SetAsync("Test1", Array.Empty<byte>(), new());
        await cache.SetAsync("Test2", Array.Empty<byte>(), new());

        await cache.RemoveAsync("Test0");

        var count = await cache.CacheCountAsync();

        // Assert

        count.Should().Be(2);
    }

    [Fact]
    public async Task Cache_Can_Be_Cleared()
    {
        // Arrange
        var cache = (ILiteDbCache)Cache;

        // Act
        await cache.SetAsync("Test0", Array.Empty<byte>(), new());
        await cache.SetAsync("Test1", Array.Empty<byte>(), new());
        await cache.SetAsync("Test2", Array.Empty<byte>(), new());

        await cache.ClearAsync();

        var item = await cache.GetStringAsync("Test0");
        var count = await cache.CacheCountAsync();


        // Assert
        item.Should().BeNullOrWhiteSpace();
        count.Should().Be(0);
    }

    [Fact]
    public async Task Cache_Size_Is_Right()
    {
        // Arrange
        var cache = (ILiteDbCache)Cache;

        // Act
        await cache.SetAsync("Test", new byte[] { 0, 1 });
        await cache.SetAsync("Test2", new byte[] { 0, 1 });

        var size = await cache.CacheSizeInBytesAsync();

        // Assert
        size.Should().Be(4UL);
    }
}
