using FluentAssertions;
using Interview.Backend;
using Interview.DependencyInjection;
using Interview.Domain.Users;
using Interview.Infrastructure.WebSocket;
using Interview.Infrastructure.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Interview.Test.Units
{
    public class ServiceProviderTest
    {
        [Fact]
        public void BuildServiceProvider()
        {
            var webAppBuilder = Host.CreateApplicationBuilder();
            webAppBuilder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            }));
            var option = new DependencyInjectionAppServiceOption
            {
                DbConfigurator = builder => builder.UseSqlite("DataSource=:memory:"),
                AdminUsers = new AdminUsers(),
                EventStorageConfigurator = builder => builder.UseEmpty(),
            };
            webAppBuilder.Services.AddAppServices(option);
            webAppBuilder.Services.AddWebSocketServices(configuration => configuration.UseInMemory());

            var webApp = webAppBuilder.Build();
            webApp.Should().NotBeNull();
        }
    }
}
