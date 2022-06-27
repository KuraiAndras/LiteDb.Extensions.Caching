
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.Caching.LiteDb;

public sealed class LiteDbCacheEntry
{
    public LiteDbCacheEntry()
    {
    }

    public LiteDbCacheEntry(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        Key = key;
        Value = value;
        Options = options;
    }

    public DistributedCacheEntryOptions Options { get; set; } = new();

    public byte[] Value { get; set; } = Array.Empty<byte>();

    public string Key { get; set; } = string.Empty;
}
