using LiteDB;

namespace LiteDb.Extensions.Caching;

public class LiteDbCacheOptions
{
    public string CachePath { get; set; } = "LiteDbCache.db";
    public string? Password { get; set; }
    public ConnectionType? Connection { get; set; }
}
