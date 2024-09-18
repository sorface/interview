using Microsoft.Extensions.Caching.Distributed;

namespace Interview.Backend.Auth;

public class DistributedLockStorage : IDistributedLockStorage
{
    private static readonly string DEFAULTPREFIX = "LOCK:";

    private static readonly byte[] EMPTYBYTEARRAY = { 0 };

    private readonly IDistributedCache _distributedCache;

    public DistributedLockStorage(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public Task LockAsync(string key, TimeSpan lifetimeLockRecord, CancellationToken cancellationToken = default)
    {
        var entryOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = lifetimeLockRecord };
        return _distributedCache.SetAsync(DEFAULTPREFIX + key, EMPTYBYTEARRAY, entryOptions, cancellationToken);
    }

    public async Task<bool> IsLockAsync(string key, CancellationToken cancellationToken = default)
    {
        return (await _distributedCache.GetAsync(DEFAULTPREFIX + key, cancellationToken)) != null;
    }
}
