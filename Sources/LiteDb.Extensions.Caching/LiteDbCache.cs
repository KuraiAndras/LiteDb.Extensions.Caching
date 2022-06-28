using LiteDB;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace LiteDb.Extensions.Caching;

public interface ILiteDbCache : IDistributedCache
{
    int CacheItemCount();
    Task<int> CacheCountAsync(CancellationToken cancellationToken = default);

    void Clear();
    Task ClearAsync(CancellationToken cancellationToke = default);

    ulong CacheSizeInBytes();
    Task<ulong> CacheSizeInBytesAsync();
}

public sealed class LiteDbCache : ILiteDbCache, IDisposable
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
        var now = _date.UtcNow;

        var collection = _db.GetCollection<LiteDbCacheEntry>(CacheCollection);

        var entry = collection.Query()
            .Where(e => e.Key == key)
            .SingleOrDefault();

        if (entry is null) return Array.Empty<byte>();

        if (IsExpired(entry, now))
        {
            Remove(key);

            return Array.Empty<byte>();
        }

        RefreshInternal(key, now, collection, entry);

        return entry.Value;
    }

    public Task<byte[]> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));

    public void Refresh(string key)
    {
        var now = _date.UtcNow;

        var collection = _db.GetCollection<LiteDbCacheEntry>(CacheCollection);

        var entry = collection.Query()
            .Where(e => e.Key == key)
            .SingleOrDefault();

        RefreshInternal(key, now, collection, entry);
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        Refresh(key);

        return Task.CompletedTask;
    }

    private void RefreshInternal(string key, DateTimeOffset now, ILiteCollection<LiteDbCacheEntry> collection, LiteDbCacheEntry entry)
    {
        if (!entry.Renewal.HasValue) return;

        if (IsExpired(entry, now))
        {
            Remove(key);

            return;
        }

        entry.Expiry = now + entry.Renewal;

        collection.Update(entry);
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
        var now = _date.UtcNow;

        var collection = _db.GetCollection<LiteDbCacheEntry>(CacheCollection);

        var oldItem = collection.Query()
            .Where(c => c.Key == key)
            .SingleOrDefault();

        if (oldItem is not null) Remove(key);

        DateTimeOffset? expiry = null;
        TimeSpan? renewal = null;

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
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        Set(key, value, options);

        return Task.CompletedTask;
    }

    public int CacheItemCount() => _db.GetCollection<LiteDbCacheEntry>(CacheCollection).Query().Count();

    public Task<int> CacheCountAsync(CancellationToken cancellationToken = default) => Task.FromResult(CacheItemCount());

    public void Clear() => _db.GetCollection<LiteDbCacheEntry>(CacheCollection).DeleteAll();

    public Task ClearAsync(CancellationToken cancellationToke = default)
    {
        Clear();

        return Task.CompletedTask;
    }

    public ulong CacheSizeInBytes() => _db
        .GetCollection<LiteDbCacheEntry>(CacheCollection)
        .Query()
        .ToEnumerable()
        .Aggregate(0UL, static (sum, next) => sum + (ulong)next.Value.Length);

    public Task<ulong> CacheSizeInBytesAsync() => Task.FromResult(CacheSizeInBytes());

    private static bool IsExpired(LiteDbCacheEntry entry, DateTimeOffset now) => entry.Expiry.HasValue && now >= entry.Expiry;
}