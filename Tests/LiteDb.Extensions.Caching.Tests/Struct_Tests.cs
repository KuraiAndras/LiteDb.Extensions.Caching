using System.Text.Json.Serialization;

namespace LiteDb.Extensions.Caching;

public class Struct_Tests : CacheTestBase
{
    private readonly struct TestData
    {
        [JsonInclude]
        public readonly Guid Value;

        [JsonConstructor]
        public TestData(Guid value) => Value = value;
    }

    [Fact]
    public async Task Not_Set_Struct_Returns_Default()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();

        // Act
        var storedValue = await MultiLevelCache.GetAsync<TestData>(key);

        // Assert
        storedValue.Should().Be(default(TestData));
    }

    [Fact]
    public async Task Not_Set_Nullable_Struct_Returns_Null()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();

        // Act
        var storedValue = await MultiLevelCache.GetAsync<TestData?>(key);

        // Assert
        storedValue.Should().Be(null);
    }

    [Theory]
    [InlineData(0.1)] // in memory
    [InlineData(0.6)] // in file
    public async Task Struct_Is_Retrieved(double delay)
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var value = new TestData(Guid.NewGuid());

        // Act
        await MultiLevelCache.SetAsync(key, value, new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(0.5) }, new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1) });

        await Task.Delay(TimeSpan.FromSeconds(delay));

        var storedValue = await MultiLevelCache.GetAsync<TestData>(key);

        // Assert
        storedValue.Should().Be(value);
    }
}
