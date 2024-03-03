using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Interview.Domain.Database;

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "For support scoped dependency.")]
public sealed class AppScopedDbContextLease<TContext> : IScopedDbContextLease<TContext>, IDisposable, IAsyncDisposable
    where TContext : DbContext
{
    private readonly IPooledDbContextInterceptor<TContext> _interceptor;
    private DbContextLease _lease;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppScopedDbContextLease{TContext}"/> class.
    /// This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /// the same compatibility standards as public APIs. It may be changed or removed without notice in
    /// any release. You should only use it directly in your code with extreme caution and knowing that
    /// doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <param name="contextPool">Db pool.</param>
    /// <param name="interceptor">Interceptor.</param>
    public AppScopedDbContextLease(IDbContextPool<TContext> contextPool, IPooledDbContextInterceptor<TContext> interceptor)
    {
        _interceptor = interceptor;
        _lease = new DbContextLease(contextPool, standalone: false);
        _lease.Context.SetLease(_lease);
    }

    /// <summary>
    ///     Gets this is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TContext Context
    {
        get
        {
            var ctx = (TContext)_lease.Context;
            _interceptor.OnCreate(ctx);
            return ctx;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    void IDisposable.Dispose()
    {
        var ctx = (TContext)_lease.Context;
        _lease.Release();
        _interceptor.OnReturn(ctx);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <returns>Value task.</returns>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        var ctx = (TContext)_lease.Context;
        await _lease.ReleaseAsync();
        _interceptor.OnReturn(ctx);
    }
}
