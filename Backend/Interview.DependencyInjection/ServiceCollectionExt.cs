using Interview.Domain;
using Interview.Domain.Certificates;
using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events;
using Interview.Domain.Events.DatabaseProcessors;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Domain.Events.Storage;
using Interview.Domain.Repository;
using Interview.Domain.Users;
using Interview.Infrastructure.Certificates.Pdf;
using Interview.Infrastructure.Events;
using Interview.Infrastructure.Users;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using NSpecifications;

namespace Interview.DependencyInjection;

public static class ServiceCollectionExt
{
    public static IServiceCollection AddAppServices(this IServiceCollection self, DependencyInjectionAppServiceOption option)
    {
        self.AddSingleton<Func<AppDbContext>>(provider => () => ActivatorUtilities.CreateInstance<AppDbContext>(provider));
#pragma warning disable EF1001
        self.AddSingleton<IDbContextPool<AppDbContext>, AppDbContextPool<AppDbContext>>();
        self.AddScoped<IScopedDbContextLease<AppDbContext>, AppScopedDbContextLease<AppDbContext>>();
        self.AddScoped<IPooledDbContextInterceptor<AppDbContext>, UserAccessorDbContextInterceptor>();
#pragma warning restore EF1001
        self.AddDbContextPool<AppDbContext>(option.DbConfigurator);

        self.AddSingleton<ICertificateGenerator, PdfCertificateGenerator>();
        self.AddSingleton<IRoomEventDispatcher, RoomEventDispatcher>();
        self.AddSingleton<ISystemClock, SystemClock>();
        self.AddSingleton(option.AdminUsers);
        var serializer = new JsonRoomEventSerializer();
        self.AddSingleton<IRoomEventSerializer>(serializer);
        self.AddSingleton<IRoomEventDeserializer>(serializer);

        self.AddScoped(typeof(ArchiveService<>));

        self.Scan(selector =>
        {
            var assemblies = new[] { typeof(UserRepository).Assembly, typeof(RoomQuestionReactionPostProcessor).Assembly, typeof(RoomQuestionPostProcessor).Assembly };
            selector.FromAssemblies(assemblies.Distinct())
                .AddClasses(filter => filter.AssignableTo(typeof(IRepository<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()

                .AddClasses(filter => filter.AssignableTo<IEntityPostProcessor>())
                .As<IEntityPostProcessor>()
                .WithScopedLifetime()

                .AddClasses(filter => filter.AssignableTo<IEntityPreProcessor>())
                .As<IEntityPreProcessor>()
                .WithScopedLifetime()

                .AddClasses(filter => filter.AssignableTo<IService>().Where(f => !f.IsAssignableTo(typeof(IServiceDecorator))))
                .AsImplementedInterfaces(type => type != typeof(IService))
                .WithScopedLifetime()

                .AddClasses(filter => filter.AssignableTo<IServiceDecorator>())
                .As<IServiceDecorator>()
                .WithScopedLifetime()

                .AddClasses(filter => filter.AssignableTo<ISelfScopeService>())
                .AsSelf()
                .WithScopedLifetime();
        });

        var decorators = self.Where(e => e.ServiceType == typeof(IServiceDecorator)).ToList();
        foreach (var serviceDescriptor in decorators)
        {
            foreach (var decorateInterface in serviceDescriptor.ImplementationType!.GetInterfaces().Where(e => e != typeof(IServiceDecorator) && e != typeof(IService)))
            {
                self.Decorate(decorateInterface, serviceDescriptor.ImplementationType);
            }
        }

        decorators.ForEach(e => self.Remove(e));

        self.AddScoped<CurrentUserAccessor>();
        self.AddScoped<IEditableCurrentUserAccessor>(provider => provider.GetRequiredService<CurrentUserAccessor>());
        self.Decorate<IEditableCurrentUserAccessor, CachedCurrentUserAccessor>();
        self.AddScoped<ICurrentUserAccessor>(e => e.GetRequiredService<IEditableCurrentUserAccessor>());

        self.AddScoped<ICurrentPermissionAccessor, CurrentPermissionAccessor>();
        self.AddEventServices(option);
        self.AddScoped<EventStorage2DatabaseService>();
        self.AddScoped<RoomCodeEditorChangeEventHandler>();
        return self;
    }

    private static void AddEventServices(
        this IServiceCollection self,
        DependencyInjectionAppServiceOption dependencyInjectionAppServiceOption)
    {
        self.AddSingleton<IEventSenderAdapter, DefaultEventSenderAdapter>();

        var builder = new EventStorageOptionBuilder();
        dependencyInjectionAppServiceOption.EventStorageConfigurator?.Invoke(builder);
        if (string.IsNullOrWhiteSpace(builder.RedisConnectionString))
        {
            self.AddSingleton<IHotEventStorage, EmptyHotEventStorage>();
        }
        else
        {
            var redisStorage = new RedisHotEventStorage(new RedisEventStorageConfiguration
            {
                ConnectionString = builder.RedisConnectionString,
            });
            self.AddSingleton<IHotEventStorage, RedisHotEventStorage>(_ => redisStorage);
            redisStorage.CreateIndexes();
        }

        self.Decorate<IEventSenderAdapter, StoreEventSenderAdapter>();
    }
}
