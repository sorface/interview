namespace Interview.Backend.Auth;

public interface IDistributedLockStorage
{
    public Task LockAsync(string key, TimeSpan lifetimeLockRecord, CancellationToken cancellationToken = default);

    public Task<bool> IsLockAsync(string key, CancellationToken cancellationToken = default);
}
