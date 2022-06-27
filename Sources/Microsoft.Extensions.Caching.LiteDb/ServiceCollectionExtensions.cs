using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Caching.LiteDb;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLiteDbCache(this IServiceCollection services, Action<LiteDbCacheOptions> setupAction)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

        services.AddOptions();
        services.AddSingleton<LiteDbCache>();
        services.AddSingleton<IDistributedCache, LiteDbCache>(services => services.GetRequiredService<LiteDbCache>());
        services.Configure(setupAction);
        return services;
    }

    public static IServiceCollection AddLiteDbCache(this IServiceCollection services, string path) => AddLiteDbCache(services, options => options.CachePath = path);
}
