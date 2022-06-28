using LiteDB;

namespace Microsoft.Extensions.Caching.LiteDb;

public sealed class LiteDbCacheEntry
{
    public LiteDbCacheEntry()
    {
    }

    public LiteDbCacheEntry(string key, byte[] value, DateTimeOffset? expiry, TimeSpan? renewal)
    {
        Key = key;
        Value = value;
        Expiry = expiry;
        Renewal = renewal;
    }

    public DateTimeOffset? Expiry { get; set; }
    public TimeSpan? Renewal { get; set; }

    public byte[] Value { get; set; } = Array.Empty<byte>();

    [BsonId]
    public string Key { get; set; } = string.Empty;
}
