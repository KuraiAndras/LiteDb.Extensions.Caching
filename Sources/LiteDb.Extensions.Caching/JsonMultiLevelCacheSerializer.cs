namespace LiteDb.Extensions.Caching;

public class JsonMultiLevelCacheSerializer : IMultiLevelCacheSerializer
{
    public T? DeSerialize<T>(string item) => System.Text.Json.JsonSerializer.Deserialize<T?>(item);

    public string Serialize<T>(T item) => System.Text.Json.JsonSerializer.Serialize<T?>(item);
}
