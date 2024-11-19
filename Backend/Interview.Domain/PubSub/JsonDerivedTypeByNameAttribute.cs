using System.Text.Json.Serialization;

namespace Interview.Domain.PubSub;

public class JsonDerivedTypeByNameAttribute<T> : JsonDerivedTypeAttribute
{
    public JsonDerivedTypeByNameAttribute() : base(typeof(T), typeof(T).Name)
    {
    }
}
