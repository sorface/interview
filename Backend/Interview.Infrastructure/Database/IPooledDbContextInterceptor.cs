using Interview.Domain.Events.ChangeEntityProcessors;
using Interview.Domain.Users;
using Interview.Domain.Users.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Interview.Infrastructure.Database;

public interface IPooledDbContextInterceptor<in TContext>
    where TContext : DbContext
{
    void OnCreate(TContext dbContext);

    void OnReturn(TContext dbContext);
}

public class UserAccessorDbContextInterceptor : IPooledDbContextInterceptor<AppDbContext>
{
    private readonly IServiceProvider _serviceProvider;

    public UserAccessorDbContextInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void OnCreate(AppDbContext dbContext)
    {
        dbContext.Processors = new LazyPreProcessors(_serviceProvider);
    }

    public void OnReturn(AppDbContext dbContext)
    {
        dbContext.Processors = null!;
    }
}

public sealed class LazyPreProcessors
{
    private readonly Lazy<List<IEntityPreProcessor>> _preProcessors;

    private readonly Lazy<List<IEntityPostProcessor>> _postProcessors;

    public LazyPreProcessors(IServiceProvider serviceProvider)
    {
        _preProcessors = new Lazy<List<IEntityPreProcessor>>(
            () => serviceProvider.GetServices<IEntityPreProcessor>().ToList());
        _postProcessors = new Lazy<List<IEntityPostProcessor>>(
            () => serviceProvider.GetServices<IEntityPostProcessor>().ToList());
    }

    public List<IEntityPreProcessor> PreProcessors => _preProcessors.Value;

    public List<IEntityPostProcessor> PostProcessors => _postProcessors.Value;
}
