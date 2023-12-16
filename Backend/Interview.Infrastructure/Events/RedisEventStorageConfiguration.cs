namespace Interview.Infrastructure.Events;

public sealed class RedisEventStorageConfiguration
{
    public required string ConnectionString { get; set; }
}
