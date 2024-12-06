using Interview.Domain.Database.Processors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Interview.Domain.Database;

public interface IPooledDbContextInterceptor<in TContext>
    where TContext : DbContext
{
    void OnCreate(TContext dbContext);

    void OnReturn(TContext dbContext);
}

public class UserAccessorDbContextInterceptor(IServiceProvider serviceProvider) : IPooledDbContextInterceptor<AppDbContext>
{
    public void OnCreate(AppDbContext dbContext)
    {
        dbContext.Processors = new LazyPreProcessors(serviceProvider);
    }

    public void OnReturn(AppDbContext dbContext)
    {
        dbContext.Processors = null!;
    }
}

#pragma warning disable SA1402
public sealed class LazyPreProcessors
#pragma warning restore SA1402
    (IServiceProvider serviceProvider)
{
    private readonly Lazy<List<IEntityPreProcessor>> _preProcessors = new(
        () => serviceProvider.GetServices<IEntityPreProcessor>().ToList());

    private readonly Lazy<List<IEntityPostProcessor>> _postProcessors = new(
        () => serviceProvider.GetServices<IEntityPostProcessor>().ToList());

    public List<IEntityPreProcessor> PreProcessors => _preProcessors.Value;

    public List<IEntityPostProcessor> PostProcessors => _postProcessors.Value;
}
