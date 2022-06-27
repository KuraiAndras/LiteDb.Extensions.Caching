using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Caching.LiteDb;

public static class TestHelper
{
    public static ServiceProvider CreateProvider() => new ServiceCollection()
        .AddLiteDbCache(Guid.NewGuid().ToString() + ".db")
        .BuildServiceProvider(true);
}
