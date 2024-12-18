using System.Collections.Concurrent;

namespace Interview.Backend.Auth
{
    public class SemaphoreLockProvider<T>(ILogger<SemaphoreLockProvider<T>> logger)
        where T : notnull
    {
        private static readonly ConcurrentDictionary<T, SemaphoreSlim> LOCKSTORE = new();

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

            logger.LogTrace("current capacity semaphore store {Count}", LOCKSTORE.Count);
        }
    }
}
