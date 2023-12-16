using Serilog.Events;
using Serilog.Formatting.Elasticsearch;

namespace Interview.Backend.Logging;

public class AppJsonFormatter : ElasticsearchJsonFormatter
{
    protected override void WriteLevel(LogEventLevel level, ref string delim, TextWriter output)
    {
        var strName = FormatLogEventLevel(level);
        WriteJsonProperty(nameof(level), strName, ref delim, output);
    }

    private static string FormatLogEventLevel(LogEventLevel level)
    {
        switch (level)
        {
            case LogEventLevel.Verbose:
                return "TRACE";
            case LogEventLevel.Debug:
                return "DEBUG";
            case LogEventLevel.Information:
                return "INFO";
            case LogEventLevel.Warning:
                return "WARN";
            case LogEventLevel.Error:
                return "ERROR";
            case LogEventLevel.Fatal:
                return "FATAL";
            default:
                return level.ToString();
        }
    }
}
