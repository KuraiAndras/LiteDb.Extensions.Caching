namespace LiteDb.Extensions.Caching;

public class MultiLevel_Tests : CacheTestBase
{
    private record TestData(string Message);

    [Fact]
    public async Task Item_Can_Be_Retreived_From_Empty_Cache()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var value = new TestData(Guid.NewGuid().ToString());

        // Act
        var storedValue = await MultiLevelCache.GetOrSetAsync(key, (_) => Task.FromResult(value), new(), new());

        // Assert
        storedValue.Should().Be(value);
    }

    [Fact]
    public async Task Item_Can_Be_Retreived_From_Non_Empty_Cache()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var value = new TestData(Guid.NewGuid().ToString());

        // Act
        _ = await MultiLevelCache.GetOrSetAsync(key, (_) => Task.FromResult(value), new(), new());
        var storedValue = await MultiLevelCache.GetOrSetAsync(key, (_) => Task.FromResult(value), new(), new());

        // Assert
        storedValue.Should().Be(value);
    }

    [Fact]
    public async Task Expiry_From_Memory_Is_Retrieved()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var value = new TestData(Guid.NewGuid().ToString());

        const int delay = 500;

        // Act
        _ = await MultiLevelCache.GetOrSetAsync(key, (_) => Task.FromResult(value), new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(delay) }, new());

        await Task.Delay(delay * 2);

        var storedValue = await MultiLevelCache.GetOrSetAsync(key, (_) => Task.FromResult(value), new(), new());

        // Assert
        storedValue.Should().Be(value);
    }
}
