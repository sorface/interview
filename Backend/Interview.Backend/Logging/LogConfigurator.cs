using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Formatting.Json;

namespace Interview.Backend.Logging;

public class LogConfigurator
{
    public static void Configure(IHostBuilder host, IServiceCollection serviceConfigurator)
    {
        host.UseSerilog((context, configuration) =>
        {
            if (context.HostingEnvironment.IsPreProduction())
            {
                configuration.WriteTo.Console(new AppJsonFormatter());
            }
            else
            {
                const string logFolder = "logs";
                try
                {
                    Directory.CreateDirectory(logFolder);
                }
                catch (Exception)
                {
                    // ignore
                }

                var logFileName = Path.Combine(logFolder, "log.txt");

                var loggingSection = new LoggingSection();
                context.Configuration.GetSection("Logging").Bind(loggingSection);

                foreach (var pair in loggingSection.LogLevel)
                {
                    configuration.MinimumLevel.Override(pair.Key, pair.Value);
                }

                configuration
                    .WriteTo.Console(outputTemplate: "{Timestamp:dd:MM:yyyy HH:mm:ss.sss} {Level:u4} [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                    .Enrich.FromLogContext()
                    .WriteTo.File(new JsonFormatter(), logFileName, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 1048576, rollOnFileSizeLimit: true);
            }
        });
    }
}
