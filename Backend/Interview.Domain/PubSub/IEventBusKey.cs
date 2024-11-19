namespace Interview.Domain.PubSub;

public interface IEventBusKey
{
    string BuildStringKey();
}

public record EventBusKey : IEventBusKey
{
    public string BuildStringKey() => ToString();
}
