namespace Microsoft.Extensions.Caching.LiteDb;

public interface ILiteDbCacheDateTimeService
{
    public DateTimeOffset Now { get; }
}

public sealed class LiteDbDateTimeService : ILiteDbCacheDateTimeService
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
