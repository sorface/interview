using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Interview.Backend.Auth
{
    public class SemaphoreLockProvider<T>
        where T : notnull
    {
        private readonly ConcurrentDictionary<T, SemaphoreSlim> _lockStore = new();
        private readonly ILogger<SemaphoreLockProvider<T>> _logger;

        public SemaphoreLockProvider(ILogger<SemaphoreLockProvider<T>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Asynchronously puts thread to wait (according to the given ID)
        /// until it can enter the LockProvider.
        /// </summary>
        /// <param name="id">the unique ID to perform the lock.</param>
        /// <returns>A <see cref="Task"/>representing the asynchronous operation.</returns>
        public ILockWaiter CreateWaiter(T id)
        {
            var semaphoreSlim = _lockStore.GetOrAdd(id, static _ => new SemaphoreSlim(1, 1));
            return new SemaphoreSlimReleaser(id, semaphoreSlim, this);
        }

        private class SemaphoreSlimReleaser(T key, SemaphoreSlim semaphoreSlim, SemaphoreLockProvider<T> semaphoreLockProvider) : ILockWaiter
        {
            public void Dispose()
            {
                semaphoreSlim.Release();

                semaphoreLockProvider._logger.LogInformation("current capacity semaphore store {Count}, Release {Key}", semaphoreLockProvider._lockStore.Count, key);

                if (semaphoreSlim.CurrentCount == 0)
                {
                    semaphoreLockProvider._logger.LogInformation("delete the semaphoreSlim {Key}", key);
                    semaphoreLockProvider._lockStore.TryRemove(key, out _);
                }
            }

            public Task WaitAsync(CancellationToken cancellationToken)
            {
                return semaphoreSlim.WaitAsync(cancellationToken);
            }
        }
    }
}

#pragma warning disable CA1050
public interface ILockWaiter : IDisposable
{
    Task WaitAsync(CancellationToken cancellationToken);
}
#pragma warning restore CA1050
