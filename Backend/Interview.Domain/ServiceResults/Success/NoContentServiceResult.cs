namespace Interview.Domain.ServiceResults.Success;

public sealed class NoContentServiceResult<T> : ServiceResult<T>
{
    public NoContentServiceResult(T value)
        : base(value)
    {
    }

    public override TRes Match<TRes>(
        Func<OkServiceResult<T>, TRes> ok,
        Func<CreateServiceResult<T>, TRes> create,
        Func<NoContentServiceResult<T>, TRes> noContent)
        => noContent(this);
}
