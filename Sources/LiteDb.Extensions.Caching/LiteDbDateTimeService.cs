namespace LiteDb.Extensions.Caching;

public interface ILiteDbCacheDateTimeService
{
    public DateTimeOffset UtcNow { get; }
}

public sealed class LiteDbDateTimeService : ILiteDbCacheDateTimeService
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
