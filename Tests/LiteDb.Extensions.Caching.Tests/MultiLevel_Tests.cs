using System.Collections;

using Newtonsoft.Json;

namespace LiteDb.Extensions.Caching;

public class MultiLevel_Tests : CacheTestBase
{
    private record TestData(string Message);

    private class NewtonsoftSerializer : IMultiLevelCacheSerializer
    {
        public T? DeSerialize<T>(string item) => JsonConvert.DeserializeObject<T>(item);

        public string Serialize<T>(T item) => JsonConvert.SerializeObject(item);
    }

    private class SerializerData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new JsonMultiLevelCacheSerializer() };
            yield return new object[] { new NewtonsoftSerializer() };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Theory]
    [ClassData(typeof(SerializerData))]
    public async Task Item_Can_Be_Retreived_From_Empty_Cache(IMultiLevelCacheSerializer serializer)
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var value = new TestData(Guid.NewGuid().ToString());

        // Act
        var storedValue = await MultiLevelCache.GetOrSetAsync(key, (_) => Task.FromResult(value), new(), new(), serializer);

        // Assert
        storedValue.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(SerializerData))]
    public async Task Item_Can_Be_Retreived_From_Non_Empty_Cache(IMultiLevelCacheSerializer serializer)
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var value = new TestData(Guid.NewGuid().ToString());

        // Act
        _ = await MultiLevelCache.GetOrSetAsync(key, (_) => Task.FromResult(value), new(), new(), serializer);
        var storedValue = await MultiLevelCache.GetOrSetAsync(key, (_) => Task.FromResult(value), new(), new(), serializer);

        // Assert
        storedValue.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(SerializerData))]
    public async Task Expiry_From_Memory_Is_Retrieved(IMultiLevelCacheSerializer serializer)
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var value = new TestData(Guid.NewGuid().ToString());

        const int delay = 500;

        // Act
        _ = await MultiLevelCache.GetOrSetAsync(key, (_) => Task.FromResult(value), new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(delay) }, new(), serializer);

        await Task.Delay(delay * 2);

        var storedValue = await MultiLevelCache.GetOrSetAsync(key, (_) => Task.FromResult(value), new(), new(), serializer);

        // Assert
        storedValue.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(SerializerData))]
    public async Task Item_Can_Be_Retreived_From_Empty_Cache_Manual(IMultiLevelCacheSerializer serializer)
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var value = new TestData(Guid.NewGuid().ToString());

        // Act
        await MultiLevelCache.SetAsync(key, value, new(), new(), serializer);
        var storedValue = await MultiLevelCache.GetAsync<TestData>(key);

        // Assert
        storedValue.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(SerializerData))]
    public async Task Expiry_From_Memory_Is_Retrieved_Manual(IMultiLevelCacheSerializer serializer)
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var value = new TestData(Guid.NewGuid().ToString());

        const int delay = 500;

        // Act
        await MultiLevelCache.SetAsync(key, value, new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(delay) }, new(), serializer);

        await Task.Delay(delay * 2);

        var storedValue = await MultiLevelCache.GetAsync<TestData>(key);

        // Assert
        storedValue.Should().Be(value);
    }
}
