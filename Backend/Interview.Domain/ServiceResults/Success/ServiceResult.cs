namespace Interview.Domain.ServiceResults.Success;

public class ServiceResult : IEquatable<ServiceResult>
{
    // ReSharper disable once InconsistentNaming
    private static readonly ServiceResult Instance = new();

    public static ServiceResult<T> Ok<T>(T value) => new OkServiceResult<T>(value);

    public static ServiceResult Ok() => Instance;

    public static ServiceResult<T> Created<T>(T value) => new CreateServiceResult<T>(value);

    public static ServiceResult<T> NoContent<T>() => new NoContentServiceResult<T>(default);

    public bool Equals(ServiceResult? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType();
    }

    public override bool Equals(object? obj) => Equals(obj as ServiceResult);

    public override int GetHashCode() => GetType().GetHashCode();
}

#pragma warning disable SA1402
public abstract class ServiceResult<T> : IEquatable<ServiceResult<T>>, IEquatable<T>
#pragma warning restore SA1402
{
    public T Value { get; }

    public ServiceResult(T value)
    {
        Value = value;
    }

    public abstract TRes Match<TRes>(
        Func<OkServiceResult<T>, TRes> ok,
        Func<CreateServiceResult<T>, TRes> create,
        Func<NoContentServiceResult<T>, TRes> noContent);

    public bool Equals(ServiceResult<T>? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public bool Equals(T? other) => other is not null && EqualityComparer<T>.Default.Equals(Value, other);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((ServiceResult<T>)obj);
    }

    public override int GetHashCode()
    {
        return Value is null ? 0 : EqualityComparer<T>.Default.GetHashCode(Value);
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;
}
