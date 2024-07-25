using Serilog;

namespace Interview.Backend.Logging;

public class LogConfigurator
{
    public static void Configure(IHostBuilder host)
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
                configuration
                    .WriteTo.Console()
                    .WriteTo.File(logFileName, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 1048576, rollOnFileSizeLimit: true);
            }
        });
    }
}
