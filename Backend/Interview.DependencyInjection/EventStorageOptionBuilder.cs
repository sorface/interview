namespace Interview.DependencyInjection;

public sealed class EventStorageOptionBuilder
{
    public string? RedisConnectionString { get; private set; }

    public EventStorageOptionBuilder UseRedis(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("Redis connection string should not be empty");
        }

        RedisConnectionString = connectionString;
        return this;
    }

    public EventStorageOptionBuilder UseEmpty()
    {
        RedisConnectionString = null;
        return this;
    }
}
