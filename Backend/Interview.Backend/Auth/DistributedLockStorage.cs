using Microsoft.Extensions.Caching.Distributed;

namespace Interview.Backend.Auth;

public class DistributedLockStorage(IDistributedCache distributedCache) : IDistributedLockStorage
{
    private static readonly string DEFAULTPREFIX = "LOCK:";

    private static readonly byte[] EMPTYBYTEARRAY = [0];

    public Task LockAsync(string key, TimeSpan lifetimeLockRecord, CancellationToken cancellationToken = default)
    {
        var entryOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = lifetimeLockRecord };
        return distributedCache.SetAsync(DEFAULTPREFIX + key, EMPTYBYTEARRAY, entryOptions, cancellationToken);
    }

    public async Task<bool> IsLockAsync(string key, CancellationToken cancellationToken = default)
    {
        return (await distributedCache.GetAsync(DEFAULTPREFIX + key, cancellationToken)) != null;
    }
}
