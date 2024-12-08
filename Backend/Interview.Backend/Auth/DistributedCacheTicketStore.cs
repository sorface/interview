using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace Interview.Backend.Auth;

public class DistributedCacheTicketStore(
    IDistributedCache distributedCache,
    ILogger<DistributedCacheTicketStore> logger)
    : ITicketStore
{
    private static readonly DistributedCacheEntryOptions DISTRIBUTEDCACHEENTRYOPTIONS = new() { SlidingExpiration = TimeSpan.FromDays(5), };

    public Task<string> StoreAsync(AuthenticationTicket ticket) => StoreAsync(ticket, CancellationToken.None);

    public async Task<string> StoreAsync(AuthenticationTicket ticket, CancellationToken cancellationToken)
    {
        var key = Guid.NewGuid().ToString();

        logger.LogInformation($"Creating a new auth session in distributed cache with id -> {key}");

        await SetupValue(key, ticket, cancellationToken);

        return key;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket) => RenewAsync(key, ticket, CancellationToken.None);

    public Task RenewAsync(string key, AuthenticationTicket ticket, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Update a auth session in distributed cache with id -> {key}");

        return SetupValue(key, ticket, cancellationToken);
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key) => RetrieveAsync(key, CancellationToken.None);

    public async Task<AuthenticationTicket?> RetrieveAsync(string key, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Get a auth session in distributed cache by id -> {key}");

        var bytes = await distributedCache.GetAsync(key, cancellationToken);
        var ticket = DeserializeFromBytes(bytes);
        return ticket;
    }

    public Task RemoveAsync(string key) => RemoveAsync(key, CancellationToken.None);

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Remove a auth session in distributed cache by id -> {key}");

        return distributedCache.RemoveAsync(key, cancellationToken);
    }

    private static byte[] SerializeToBytes(AuthenticationTicket source)
    {
        return TicketSerializer.Default.Serialize(source);
    }

    private static AuthenticationTicket? DeserializeFromBytes(byte[]? source)
    {
        return source is null ? null : TicketSerializer.Default.Deserialize(source)!;
    }

    private Task SetupValue(string key, AuthenticationTicket ticket, CancellationToken cancellationToken)
    {
        var val = SerializeToBytes(ticket);

        return distributedCache.SetAsync(key, val, DISTRIBUTEDCACHEENTRYOPTIONS, cancellationToken);
    }
}
