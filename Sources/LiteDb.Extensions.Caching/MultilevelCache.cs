using System.Collections.Concurrent;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace LiteDb.Extensions.Caching;

public interface IMultiLevelCache
{
    Task<T?> GetAsync<T>
    (
        string key,
        IMultiLevelCacheSerializer? serializerOverride = null,
        CancellationToken cancellationToken = default
    );

    Task SetAsync<T>
    (
        string key,
        T item,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        IMultiLevelCacheSerializer? serializerOverride = null,
        CancellationToken cancellationToken = default
    );

    Task<T> GetOrSetAsync<T>
    (
        string key,
        Func<CancellationToken, Task<T>> factory,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        IMultiLevelCacheSerializer? serializerOverride = null,
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

    public async Task<T?> GetAsync<T>(string key, IMultiLevelCacheSerializer? serializerOverride = null, CancellationToken cancellationToken = default)
    {
        var memoryItem = _memoryCache.Get(key);

        if (memoryItem is null || memoryItem.Equals(default))
        {
            var itemString = await _distributedCache.GetStringAsync(key, cancellationToken);

#pragma warning disable IDE0046 // Convert to conditional expression
            if (!string.IsNullOrWhiteSpace(itemString))
            {
                return serializerOverride is null
                    ? _serializer.DeSerialize<T>(itemString)
                    : serializerOverride.DeSerialize<T>(itemString);
            }
            else
            {
                return default;
            }
#pragma warning restore IDE0046 // Convert to conditional expression
        }
        else
        {
            return (T)memoryItem;
        }
    }

    public async Task SetAsync<T>
    (
        string key,
        T item,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        IMultiLevelCacheSerializer? serializerOverride = null,
        CancellationToken cancellationToken = default
    )
    {
        var semaphore = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);

        try
        {
            _memoryCache.Set(key, item, memoryEntry);

            var serializedItem = serializerOverride is null
                    ? _serializer.Serialize(item)
                    : serializerOverride.Serialize(item);

            await _distributedCache.SetStringAsync(key, serializedItem, persistentEntry, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<T> GetOrSetAsync<T>
    (
        string key,
        Func<CancellationToken, Task<T>> factory,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        IMultiLevelCacheSerializer? serializerOverride = null,
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

                    await _distributedCache.SetStringAsync(key, _serializer.Serialize(item), persistentEntry, cancellationToken);
                }
                else
                {
                    item = serializerOverride is null
                        ? _serializer.DeSerialize<T>(itemString)!
                        : serializerOverride.DeSerialize<T>(itemString)!;
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
