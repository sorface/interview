using Serilog.Events;

namespace Interview.Backend.Logging;

public class LoggingSection
{
    public Dictionary<string, LogEventLevel> LogLevel { get; set; } = new();
}
