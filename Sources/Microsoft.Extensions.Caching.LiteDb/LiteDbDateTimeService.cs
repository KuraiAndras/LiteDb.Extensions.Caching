namespace Microsoft.Extensions.Caching.LiteDb;

public interface ILiteDbCacheDateTimeService
{
    public DateTimeOffset UtcNow { get; }
}

public sealed class LiteDbDateTimeService : ILiteDbCacheDateTimeService
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
