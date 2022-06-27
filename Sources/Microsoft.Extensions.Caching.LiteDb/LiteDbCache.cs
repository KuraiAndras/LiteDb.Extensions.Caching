using LiteDB;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.LiteDb;

public class LiteDbCache : IDistributedCache
{
    private const string CacheCollection = "cache";

    private readonly IOptions<LiteDbCacheOptions> _options;

    public LiteDbCache(IOptions<LiteDbCacheOptions> options) => _options = options;

    public byte[] Get(string key)
    {
        using var db = new LiteDatabase(_options.Value.CachePath);

        var collection = db.GetCollection<LiteDbCacheEntry>(CacheCollection);

        var entry = collection.Query()
            .Where(e => e.Key == key)
            .SingleOrDefault();

        return entry?.Value ?? Array.Empty<byte>();
    }

    public Task<byte[]> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));

    public void Refresh(string key)
    {
        throw new NotImplementedException();
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public void Remove(string key)
    {
        using var db = new LiteDatabase(_options.Value.CachePath);

        var collection = db.GetCollection<LiteDbCacheEntry>(CacheCollection);

        collection.DeleteMany(e => e.Key == key);
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key);

        return Task.CompletedTask;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        using var db = new LiteDatabase(_options.Value.CachePath);

        var collection = db.GetCollection<LiteDbCacheEntry>(CacheCollection);

        collection.Insert(new LiteDbCacheEntry
        {
            Options = options,
            Key = key,
            Value = value,
        });

        collection.EnsureIndex(e => e.Key);
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        Set(key, value, options);

        return Task.CompletedTask;
    }
}

public sealed class LiteDbCacheEntry
{
    public DistributedCacheEntryOptions Options { get; set; } = new();

    public byte[] Value { get; set; } = Array.Empty<byte>();

    public string Key { get; set; } = string.Empty;
}
