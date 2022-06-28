using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace LiteDb.Extensions.Caching;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLiteDbCache(this IServiceCollection services, Action<LiteDbCacheOptions> setupAction)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (setupAction is null) throw new ArgumentNullException(nameof(setupAction));

        services.AddOptions();
        services.AddSingleton<LiteDbCache>();
        services.AddSingleton<ILiteDbCacheDateTimeService, LiteDbDateTimeService>();
        services.AddSingleton<IDistributedCache, LiteDbCache>(services => services.GetRequiredService<LiteDbCache>());
        services.Configure(setupAction);
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
