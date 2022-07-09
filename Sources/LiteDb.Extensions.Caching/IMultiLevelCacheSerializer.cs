namespace LiteDb.Extensions.Caching;

public interface IMultiLevelCacheSerializer
{
    string Serialize<T>(T item);
    T? DeSerialize<T>(string item);
}
