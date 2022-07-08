using System.Collections.Concurrent;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace LiteDb.Extensions.Caching;

public interface IMultiLevelCache
{
    Task<T> GetOrSetAsync<T>
    (
        string key,
        Func<CancellationToken, Task<T>> factory,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
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

    public async Task<T> GetOrSetAsync<T>
    (
        string key,
        Func<CancellationToken, Task<T>> factory,
        MemoryCacheEntryOptions memoryEntry,
        DistributedCacheEntryOptions persistentEntry,
        CancellationToken cancellationToken = default
    )
    {
        var semaphore = _locks.GetOrAdd(key, new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);

        try
        {
            var memoryItem = _memoryCache.Get(key);

            if (memoryItem is null || memoryItem.Equals(default))
            {
                var itemString = await _distributedCache.GetStringAsync(key, cancellationToken);

                T? item;
                if (string.IsNullOrWhiteSpace(itemString))
                {
                    item = await factory(cancellationToken);

                    await _distributedCache.SetStringAsync(key, _serializer.Serialize(item), persistentEntry, cancellationToken);
                }
                else
                {
                    item = _serializer.DeSerialize<T>(itemString);
                }

                _memoryCache.Set(key, item, memoryEntry);

                return item!;
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
