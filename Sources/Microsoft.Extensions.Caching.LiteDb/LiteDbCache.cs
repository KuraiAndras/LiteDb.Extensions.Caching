using LiteDB;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.LiteDb;

public class LiteDbCache : IDistributedCache
{
    private const string CacheCollection = "cache";

    private readonly IOptions<LiteDbCacheOptions> _options;
    private readonly ILiteDbCacheDateTimeService _date;

    public LiteDbCache(IOptions<LiteDbCacheOptions> options, ILiteDbCacheDateTimeService date)
    {
        _options = options;
        _date = date;
    }

    public byte[] Get(string key)
    {
        using var db = new LiteDatabase(_options.Value.CachePath);

        var collection = db.GetCollection<LiteDbCacheEntry>(CacheCollection);

        var entry = collection.Query()
            .Where(e => e.Key == key)
            .SingleOrDefault();

        if (entry is null) return Array.Empty<byte>();

        if (IsExpired(entry))
        {
            Remove(key);

            return Array.Empty<byte>();
        }

        return entry.Value;
    }

    public Task<byte[]> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));

    public void Refresh(string key)
    {
        throw new NotImplementedException();
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        Refresh(key);

        return Task.CompletedTask;
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

        collection.Insert(new LiteDbCacheEntry(key, value, options));

        collection.EnsureIndex(e => e.Key);
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        Set(key, value, options);

        return Task.CompletedTask;
    }

    private bool IsExpired(LiteDbCacheEntry entry)
    {
        var now = _date.Now;

        if (entry.Options.AbsoluteExpiration.HasValue)
        {
            var absolute = entry.Options.AbsoluteExpiration.Value;
            var relativeToNow = absolute - now;

            return relativeToNow.TotalMilliseconds <= 0;
        }
        else if (entry.Options.SlidingExpiration.HasValue)
        {
            throw new NotImplementedException();
        }
        else if (entry.Options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            throw new NotImplementedException();
        }
        else
        {
            return false;
        }
    }
}
