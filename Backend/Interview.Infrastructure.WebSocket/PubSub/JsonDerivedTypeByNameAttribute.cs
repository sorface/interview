using System.Text.Json.Serialization;

namespace Interview.Infrastructure.WebSocket.PubSub;

public class JsonDerivedTypeByNameAttribute<T> : JsonDerivedTypeAttribute
{
    public JsonDerivedTypeByNameAttribute() : base(typeof(T), typeof(T).Name)
    {
    }
}
