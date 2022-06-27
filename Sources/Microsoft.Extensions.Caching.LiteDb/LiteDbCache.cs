using LiteDB;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.LiteDb;

public sealed class LiteDbCache : IDistributedCache, IDisposable
{
    private const string CacheCollection = "cache";

    private readonly ILiteDbCacheDateTimeService _date;
    private readonly LiteDatabase _db;

    public LiteDbCache(IOptions<LiteDbCacheOptions> options, ILiteDbCacheDateTimeService date)
    {
        _date = date;

        var liteDbOptions = options.Value;

        var connectionString = new ConnectionString(liteDbOptions.CachePath);
        if (liteDbOptions.Password is not null) connectionString.Password = liteDbOptions.Password;

        _db = new LiteDatabase(connectionString);
    }

    public void Dispose() => _db.Dispose();

    public byte[] Get(string key)
    {
        var collection = _db.GetCollection<LiteDbCacheEntry>(CacheCollection);

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
        var collection = _db.GetCollection<LiteDbCacheEntry>(CacheCollection);

        collection.DeleteMany(e => e.Key == key);
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key);

        return Task.CompletedTask;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        var collection = _db.GetCollection<LiteDbCacheEntry>(CacheCollection);

        var oldItem = collection.Query()
            .Where(c => c.Key == key)
            .SingleOrDefault();

        if (oldItem is not null) Remove(key);

        DateTimeOffset? expiry = null;
        TimeSpan? renewal = null;

        var now = _date.Now;

        if (options.AbsoluteExpiration.HasValue)
        {
            expiry = options.AbsoluteExpiration.Value.ToUniversalTime();
        }
        else if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            expiry = now.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }

        if (options.SlidingExpiration.HasValue)
        {
            renewal = options.SlidingExpiration.Value;
            expiry = (expiry ?? now) + renewal;
        }

        collection.Insert(new LiteDbCacheEntry(key, value, expiry, renewal));

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

        if (entry.Expiry.HasValue)
        {
            return now <= entry.Expiry;
        }
        else if (entry.Renewal.HasValue)
        {
            throw new NotImplementedException();
        }

        return false;
    }
}
