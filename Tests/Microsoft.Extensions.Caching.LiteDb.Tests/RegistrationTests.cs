using LiteDB;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.LiteDb;

public class RegistrationTests
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
}
