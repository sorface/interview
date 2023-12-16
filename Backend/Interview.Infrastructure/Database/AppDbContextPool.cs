using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Interview.Infrastructure.Database;

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Custom factory")]
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:Element parameters should be documented", Justification = "Skip")]
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1616:Element return value documentation should have text", Justification = "Skip")]
public sealed class AppDbContextPool<TContext> : IDbContextPool<TContext>, IDisposable, IAsyncDisposable
    where TContext : DbContext
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public const int DefaultPoolSize = 1024;

    private readonly Func<TContext> _contextFactory;

    private readonly ConcurrentQueue<IDbContextPoolable> _pool = new();

    private int _maxSize;
    private int _count;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContextPool{TContext}"/> class.
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public AppDbContextPool(DbContextOptions<TContext> options, Func<TContext> contextFactory)
    {
        _maxSize = options.FindExtension<CoreOptionsExtension>()?.MaxPoolSize ?? DefaultPoolSize;

        if (_maxSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(CoreOptionsExtension.MaxPoolSize), CoreStrings.InvalidPoolSize);
        }

        options.Freeze();
        _contextFactory = contextFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <returns></returns>
    public IDbContextPoolable Rent()
    {
        if (_pool.TryDequeue(out var context))
        {
            Interlocked.Decrement(ref _count);

            Debug.Assert(_count >= 0, $"_count is {_count}");

            return context;
        }

        context = _contextFactory();

        context.SnapshotConfiguration();

        return context;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Return(IDbContextPoolable context)
    {
        if (Interlocked.Increment(ref _count) <= _maxSize)
        {
            context.ResetState();

            _pool.Enqueue(context);
        }
        else
        {
            PooledReturn(context);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <returns></returns>
    public async ValueTask ReturnAsync(IDbContextPoolable context, CancellationToken cancellationToken = default)
    {
        if (Interlocked.Increment(ref _count) <= _maxSize)
        {
            await context.ResetStateAsync(cancellationToken).ConfigureAwait(false);

            _pool.Enqueue(context);
        }
        else
        {
            PooledReturn(context);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Dispose()
    {
        _maxSize = 0;

        while (_pool.TryDequeue(out var context))
        {
            context.ClearLease();
            context.Dispose();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        _maxSize = 0;

        while (_pool.TryDequeue(out var context))
        {
            context.ClearLease();
            await context.DisposeAsync().ConfigureAwait(false);
        }
    }

    private void PooledReturn(IDbContextPoolable context)
    {
        Interlocked.Decrement(ref _count);

        Debug.Assert(_maxSize == 0 || _pool.Count <= _maxSize, $"_maxSize is {_maxSize}");

        context.ClearLease();
        context.Dispose();
    }
}
