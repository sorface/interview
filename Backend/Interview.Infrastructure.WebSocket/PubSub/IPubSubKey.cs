namespace Interview.Infrastructure.WebSocket.PubSub;

public interface IPubSubKey
{
    string BuildStringKey();
}

public record PubSubKey : IPubSubKey
{
    public string BuildStringKey() => ToString();
}
