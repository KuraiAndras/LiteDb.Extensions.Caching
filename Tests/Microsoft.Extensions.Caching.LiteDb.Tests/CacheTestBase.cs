using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Caching.LiteDb;

public abstract class CacheTestBase : IAsyncLifetime
{
    private readonly ServiceProvider _sp;

    protected CacheTestBase()
    {
        const string cachesFolder = "Caches";

        if (!Directory.Exists(cachesFolder)) Directory.CreateDirectory(cachesFolder);

        _sp = new ServiceCollection()
            .AddLiteDbCache(Path.Combine(cachesFolder, $"{Guid.NewGuid()}.db"))
            .BuildServiceProvider(true);
        Cache = _sp.GetRequiredService<IDistributedCache>();
    }

    protected IDistributedCache Cache { get; }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _sp.DisposeAsync();
}
