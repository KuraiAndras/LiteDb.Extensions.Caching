using LiteDB;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LiteDb.Extensions.Caching;

public class Registration_Tests
{
    [Fact]
    public async Task Cache_Is_Registered()
    {
        // Arrange
        var dbPath = Path.Combine("Caches", $"{Guid.NewGuid()}.db");

        var sp = new ServiceCollection()
            .AddLiteDbCache(dbPath)
            .BuildServiceProvider(true);

        // Act
        var cache = sp.GetService<IDistributedCache>();
        var options = sp.GetService<IOptions<LiteDbCacheOptions>>()!.Value;

        // Assert
        cache.Should().NotBeNull();
        options.Should().NotBeNull();

        options.CachePath.Should().Be(dbPath);

        await sp.DisposeAsync();

        File.Delete(dbPath);
    }

    [Fact]
    public async Task Cache_Is_Registered_From_Config()
    {
        // Arrange
        var dbPath = Path.Combine("Caches", $"{Guid.NewGuid()}.db");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { $"{nameof(LiteDbCacheOptions)}:{nameof(LiteDbCacheOptions.CachePath)}", dbPath },
            })
            .Build();

        var sp = new ServiceCollection()
            .Configure<LiteDbCacheOptions>(config.GetSection(nameof(LiteDbCacheOptions)))
            .AddLiteDbCache()
            .BuildServiceProvider(true);

        // Act
        var options = sp.GetService<IOptions<LiteDbCacheOptions>>()!.Value;

        // Assert
        options.Should().NotBeNull();

        options.CachePath.Should().Be(dbPath);

        await sp.DisposeAsync();

        File.Delete(dbPath);
    }

    [Fact]
    public async Task Password_Configuration_Works()
    {
        // Arrange
        var dbPath = Path.Combine("Caches", $"{Guid.NewGuid()}.db");

        const string password = "Test1234!";

        var sp = new ServiceCollection()
            .AddLiteDbCache(dbPath, password)
            .BuildServiceProvider(true);

        // Act
        sp.GetRequiredService<IDistributedCache>();

        await sp.DisposeAsync();

        var wrongPassword = () =>
        {
            var db = new LiteDatabase(new ConnectionString(dbPath) { Password = "Wrong" });
            db.Dispose();
        };
        var rightPassword = () =>
        {
            var db = new LiteDatabase(new ConnectionString(dbPath) { Password = password });
            db.Dispose();
        };

        // Assert
        wrongPassword.Should().Throw<LiteException>();
        rightPassword.Should().NotThrow();

        // Cleanup
        File.Delete(dbPath);
    }

    [Fact]
    public async Task All_Instances_Are_The_Same()
    {
        // Arrange
        var dbPath = Path.Combine("Caches", $"{Guid.NewGuid()}.db");

        var sp = new ServiceCollection()
            .AddLiteDbCache(dbPath)
            .BuildServiceProvider(true);

        // Act
        var cache = sp.GetService<IDistributedCache>();
        var liteDbCacheInterface = sp.GetService<ILiteDbCache>();
        var liteDbCache = sp.GetService<LiteDbCache>();

        // Assert
        cache.Should().NotBeNull();

        cache.Should().Be(liteDbCacheInterface);
        cache.Should().Be(liteDbCache);

        await sp.DisposeAsync();

        File.Delete(dbPath);
    }

    [Fact]
    public void Calling_Add_Does_Not_Add_Duplicates()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var firstCount = services.AddLiteDbCache(string.Empty).Count;
        var secondCount = services.AddLiteDbCache(string.Empty).Count;

        // Assert
        firstCount.Should().Be(secondCount);
    }

    [Fact]
    public void Setup_Throws_On_Null_Param()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var setups = new Action[]
        {
            () => (null as IServiceCollection)!.AddLiteDbCache(setupAction: null!),
            () => services.AddLiteDbCache(setupAction: null!),

            () => (null as IServiceCollection)!.AddLiteDbCache(path: null!),
            () => services.AddLiteDbCache(path: null!),

            () => (null as IServiceCollection)!.AddLiteDbCache(path: null!, password: null!),
            () => services.AddLiteDbCache(path: null!, password: null!),
            () => services.AddLiteDbCache(string.Empty, null!),
        };

        // Assert
        foreach (var setup in setups)
        {
            setup.Should().Throw<ArgumentNullException>();
        }
    }
}
