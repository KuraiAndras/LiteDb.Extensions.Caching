# LiteDb.Extensions.Caching [![Nuget](https://img.shields.io/nuget/v/LiteDb.Extensions.Caching)](https://www.nuget.org/packages/LiteDb.Extensions.Caching) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=KuraiAndras_LiteDb.Extensions.Caching&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=KuraiAndras_LiteDb.Extensions.Caching) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=KuraiAndras_LiteDb.Extensions.Caching&metric=coverage)](https://sonarcloud.io/summary/new_code?id=KuraiAndras_LiteDb.Extensions.Caching)

An `IDistributedCache` implementation for [LiteDB](https://www.litedb.org/).

## Usage

Registering the cache:

```csharp

IServiceCollection services = //...

services.AddLiteDbCache("MyCache.db")
// or
services.AddLiteDbCache("MyCache.db", "MySuperStrongPassword1111!!!")
// or
services.AddLiteDbCache(options =>
{
    options.CachePath = "MyCache.db";
    options.Password = "MySuperStrongPassword1111!!!";
});

```

After this the `IDistributedCache` can be used as usual;

## Fine controls

The package also allows you to have some manual controls for managing the cache through the `ILiteDbCache` interface:

```csharp
public interface ILiteDbCache : IDistributedCache
{
    int CacheItemCount();
    Task<int> CacheCountAsync(CancellationToken cancellationToken = default);

    void Clear();
    Task ClearAsync(CancellationToken cancellationToke = default);

    ulong CacheSizeInBytes();
    Task<ulong> CacheSizeInBytesAsync();
}
```

## Multi level cache

The library also adds a multi level cache implementation through the `IMultiLevelCache`, which first tries to retrieve an object from the `IMemoryCache`, then from the `IDistributedCache`. For serialization it uses `System.Text.Json`. Another string-based serialized can be used by implementing the `IMultiLevelCacheSerializer` interface and then registering it through DI.

```csharp
var key = Guid.NewGuid().ToString();
var value = new TestData(Guid.NewGuid().ToString());

var storedValue = await MultiLevelCache.GetOrSetAsync(key, (_) => Task.FromResult(value), new MemoryCacheEntryOptions(), new DistributedCacheEntryOptions());
```

## Contributing

Issues and pull requests are welcome.

**If you find a bug, please try to recreate it with a test case in the test project and put it in the newly created issue** (not mandatory)
