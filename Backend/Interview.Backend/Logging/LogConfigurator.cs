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
                configuration.WriteTo.Console();
            }
        });
    }
}
