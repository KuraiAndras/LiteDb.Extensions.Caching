using System.Collections.Concurrent;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace LiteDb.Extensions.Caching;

public interface IMultiLevelCache
{
    Task<T?> GetAsync<T>
    (
        string key,
        CancellationToken cancellationToken = default
    );

    Task<T?> GetAsync<T>
    (
        string key,
        IMultiLevelCacheSerializer serializerOverride,
        CancellationToken cancellationToken = default
    );

    Task<T?> GetAsync<T>
    (
        string key,
        Func<string, T> serializerOverride,
        CancellationToken cancellationToken = default
    );

    Task SetAsync<T>
    (
        string key,
        T item,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        CancellationToken cancellationToken = default
    );

    Task SetAsync<T>
    (
        string key,
        T item,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        Func<T, string> serializerOverride,
        CancellationToken cancellationToken = default
    );

    Task SetAsync<T>
    (
        string key,
        T item,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        IMultiLevelCacheSerializer serializerOverride,
        CancellationToken cancellationToken = default
    );

    Task<T> GetOrSetAsync<T>
    (
        string key,
        Func<CancellationToken, Task<T>> factory,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        CancellationToken cancellationToken = default
    );

    Task<T> GetOrSetAsync<T>
    (
        string key,
        Func<CancellationToken, Task<T>> factory,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        IMultiLevelCacheSerializer serializerOverride,
        CancellationToken cancellationToken = default
    );

    Task<T> GetOrSetAsync<T>
    (
        string key,
        Func<CancellationToken, Task<T>> factory,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        Func<T, string> serializeOverride,
        Func<string, T> deSerializeOverride,
        CancellationToken cancellationToken = default
    );
}

public class MultilevelCache : IMultiLevelCache
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly IMultiLevelCacheSerializer _serializer;

    public MultilevelCache(IMemoryCache memoryCache, IDistributedCache distributedCache, IMultiLevelCacheSerializer serializer)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _serializer = serializer;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) => GetAsync<T>(key, _serializer, cancellationToken);

    public Task<T?> GetAsync<T>(string key, IMultiLevelCacheSerializer serializerOverride, CancellationToken cancellationToken = default) =>
        GetAsync(key, item => serializerOverride.DeSerialize<T>(item)!, cancellationToken);

    public async Task<T?> GetAsync<T>(string key, Func<string, T> serializerOverride, CancellationToken cancellationToken = default)
    {
        var memoryItem = _memoryCache.Get(key);

        if (memoryItem is null || memoryItem.Equals(default))
        {
            var itemString = await _distributedCache.GetStringAsync(key, cancellationToken);

            return !string.IsNullOrWhiteSpace(itemString)
                ? serializerOverride(itemString)
                : default;
        }
        else
        {
            return (T)memoryItem;
        }
    }

    public Task SetAsync<T>
    (
        string key,
        T item,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        CancellationToken cancellationToken = default
    ) => SetAsync(key, item, memoryEntry, persistentEntry, _serializer, cancellationToken);

    public Task SetAsync<T>
    (
        string key,
        T item,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        IMultiLevelCacheSerializer serializerOverride,
        CancellationToken cancellationToken = default
    ) => SetAsync(key, item, memoryEntry, persistentEntry, x => serializerOverride.Serialize(x), cancellationToken);

    public async Task SetAsync<T>
    (
        string key,
        T item,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        Func<T, string> serializerOverride,
        CancellationToken cancellationToken = default
    )
    {
        var semaphore = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);

        try
        {
            _memoryCache.Set(key, item, memoryEntry);

            var serializedItem = serializerOverride(item);

            await _distributedCache.SetStringAsync(key, serializedItem, persistentEntry, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public Task<T> GetOrSetAsync<T>
    (
        string key,
        Func<CancellationToken, Task<T>> factory,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        CancellationToken cancellationToken = default
    ) => GetOrSetAsync(key, factory, memoryEntry, persistentEntry, _serializer, cancellationToken);

    public Task<T> GetOrSetAsync<T>
    (
        string key,
        Func<CancellationToken, Task<T>> factory,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        IMultiLevelCacheSerializer serializerOverride,
        CancellationToken cancellationToken = default
    ) => GetOrSetAsync(key, factory, memoryEntry, persistentEntry, x => serializerOverride.Serialize(x), x => serializerOverride.DeSerialize<T>(x)!, cancellationToken);

    public async Task<T> GetOrSetAsync<T>
    (
        string key,
        Func<CancellationToken, Task<T>> factory,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        Func<T, string> serializeOverride,
        Func<string, T> deSerializeOverride,
        CancellationToken cancellationToken = default
    )
    {
        var semaphore = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);

        try
        {
            var memoryItem = _memoryCache.Get(key);

            if (memoryItem is null || memoryItem.Equals(default))
            {
                var itemString = await _distributedCache.GetStringAsync(key, cancellationToken);

                T item;
                if (string.IsNullOrWhiteSpace(itemString))
                {
                    item = await factory(cancellationToken);

                    await _distributedCache.SetStringAsync(key, serializeOverride(item), persistentEntry, cancellationToken);
                }
                else
                {
                    item = deSerializeOverride(itemString);
                }

                _memoryCache.Set(key, item, memoryEntry);

                return item;
            }
            else
            {
                return (T)memoryItem;
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
}
