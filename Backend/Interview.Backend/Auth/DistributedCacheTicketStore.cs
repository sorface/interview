using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace Interview.Backend.Auth;

public class DistributedCacheTicketStore : ITicketStore
{
    private readonly IDistributedCache _distributedCache;

    public DistributedCacheTicketStore(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString();
        await RenewAsync(key, ticket);
        return key;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var options = new DistributedCacheEntryOptions();
        var expiresUtc = ticket.Properties.ExpiresUtc;

        if (expiresUtc.HasValue)
        {
            options.SetAbsoluteExpiration(expiresUtc.Value);
        }

        var val = SerializeToBytes(ticket);
        _distributedCache.Set(key, val, options);
        return Task.FromResult(0);
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var bytes = _distributedCache.Get(key);
        var ticket = DeserializeFromBytes(bytes);
        return Task.FromResult(ticket);
    }

    public Task RemoveAsync(string key)
    {
        _distributedCache.Remove(key);
        return Task.FromResult(0);
    }

    private static byte[] SerializeToBytes(AuthenticationTicket source)
    {
        return TicketSerializer.Default.Serialize(source);
    }

    private static AuthenticationTicket? DeserializeFromBytes(byte[]? source)
    {
        return source is null ? null : TicketSerializer.Default.Deserialize(source)!;
    }
}
