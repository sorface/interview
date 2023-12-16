using Microsoft.Extensions.Internal;

namespace Interview.Test;

public class TestSystemClock : ISystemClock
{
    public DateTimeOffset UtcNow { get; set; } = new DateTimeOffset(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
}
