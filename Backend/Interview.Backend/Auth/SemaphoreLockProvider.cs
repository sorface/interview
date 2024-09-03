using System.Collections.Concurrent;

namespace Interview.Backend.Auth
{
    public class SemaphoreLockProvider<T>
        where T : notnull
    {
        private static readonly ConcurrentDictionary<T, SemaphoreSlim> LOCKSTORE = new();

        private readonly ILogger<SemaphoreLockProvider<T>> _logger;

        public SemaphoreLockProvider(ILogger<SemaphoreLockProvider<T>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Asynchronously puts thread to wait (according to the given ID)
        /// until it can enter the LockProvider
        /// </summary>
        /// <param name="id">the unique ID to perform the lock</param>
        /// <param name="cancellationToken">cancellationToken for aborted process</param>
        /// <returns>A <see cref="Task"/>representing the asynchronous operation</returns>
        public async Task WaitAsync(T id, CancellationToken cancellationToken)
        {
            await LOCKSTORE.GetOrAdd(id, new SemaphoreSlim(1, 1)).WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Releases the lock (according to the given ID)
        /// </summary>
        /// <param name="id">the unique ID to unlock</param>
        public void Release(T id)
        {
            if (!LOCKSTORE.TryGetValue(id, out var semaphore))
            {
                return;
            }

            semaphore.Release();

            if (LOCKSTORE.TryRemove(id, out _))
            {
                _logger.LogDebug($@"drop semaphore from semaphore store {LOCKSTORE.Count}");
            }

            _logger.LogDebug($@"current capacity semaphore store {LOCKSTORE.Count}");
        }
    }
}
