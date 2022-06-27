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
        const string dbPath = "TestDb";

        var sp = new ServiceCollection()
            .AddLiteDbCache(dbPath)
            .BuildServiceProvider(true);

        // Act
        var cache = sp.GetService<IDistributedCache>();
        var options = sp.GetService<IOptions<LiteDbCacheOptions>>()?.Value;

        // Assert
        cache.Should().NotBeNull();
        options.Should().NotBeNull();

        options!.CachePath.Should().Be(dbPath);

        await sp.DisposeAsync();
    }
}
