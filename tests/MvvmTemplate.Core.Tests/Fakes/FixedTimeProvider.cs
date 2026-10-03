namespace MvvmTemplate.Core.Tests.Fakes;

/// <summary>A clock that always returns the same moment, in UTC.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
