using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace LiteDb.Extensions.Caching;

public abstract class CacheTestBase : IAsyncLifetime
{
    private readonly ServiceProvider _sp;
    private readonly string _cacheDbPath;

    protected CacheTestBase()
    {
        const string cachesFolder = "Caches";

        if (!Directory.Exists(cachesFolder)) Directory.CreateDirectory(cachesFolder);

        _cacheDbPath = Path.Combine(cachesFolder, $"{Guid.NewGuid()}.db");

        _sp = new ServiceCollection()
            .AddLiteDbCache(_cacheDbPath)
            .BuildServiceProvider(true);

        Cache = _sp.GetRequiredService<IDistributedCache>();
    }

    protected IDistributedCache Cache { get; }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _sp.DisposeAsync();

        File.Delete(_cacheDbPath);
    }
}