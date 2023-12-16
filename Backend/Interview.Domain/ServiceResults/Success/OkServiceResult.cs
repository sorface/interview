using Interview.Domain.ServiceResults.Success;

namespace Interview.Domain.ServiceResults;

public sealed class OkServiceResult<T> : ServiceResult<T>
{
    public OkServiceResult(T value)
        : base(value)
    {
    }

    public override TRes Match<TRes>(
        Func<OkServiceResult<T>, TRes> ok,
        Func<CreateServiceResult<T>, TRes> create,
        Func<NoContentServiceResult<T>, TRes> noContent)
        => ok(this);
}
