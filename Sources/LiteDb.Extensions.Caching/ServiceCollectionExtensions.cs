using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace LiteDb.Extensions.Caching;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection TryConfigure<TOptions>(this IServiceCollection services, Action<TOptions> setup) where TOptions : class
    {
        if (services.Any(d => d.ServiceType == typeof(IConfigureOptions<TOptions>))) return services;

        services.Configure(setup);

        return services;
    }

    public static IServiceCollection AddLiteDbCache(this IServiceCollection services, Action<LiteDbCacheOptions> setupAction)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (setupAction is null) throw new ArgumentNullException(nameof(setupAction));

        services.AddOptions();
        services.TryConfigure(setupAction);

        services.TryAddSingleton<ILiteDbCacheDateTimeService, LiteDbDateTimeService>();
        services.TryAddSingleton<LiteDbCache>();
        services.TryAddSingleton<IDistributedCache>(s => s.GetRequiredService<LiteDbCache>());
        services.TryAddSingleton<ILiteDbCache>(s => s.GetRequiredService<LiteDbCache>());

        services.AddMemoryCache();
        services.TryAddSingleton<IMultiLevelCache, MultilevelCache>();
        services.TryAddSingleton<IMultiLevelCacheSerializer, JsonMultiLevelCacheSerializer>();

        return services;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Too much nesing")]
    public static IServiceCollection AddLiteDbCache(this IServiceCollection services, string path)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (path is null) throw new ArgumentNullException(nameof(path));

        return AddLiteDbCache(services, options => options.CachePath = path);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Too much nesing")]
    public static IServiceCollection AddLiteDbCache(this IServiceCollection services, string path, string password)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (path is null) throw new ArgumentNullException(nameof(path));
        if (password is null) throw new ArgumentNullException(nameof(password));

        return AddLiteDbCache(services, options =>
        {
            options.CachePath = path;
            options.Password = password;
        });
    }
}
