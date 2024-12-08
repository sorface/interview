using System.Text.Json.Serialization;

namespace Interview.Domain.PubSub;

public class JsonDerivedTypeByNameAttribute<T>() : JsonDerivedTypeAttribute(typeof(T), typeof(T).Name);
