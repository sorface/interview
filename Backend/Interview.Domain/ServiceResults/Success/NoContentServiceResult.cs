namespace Interview.Domain.ServiceResults.Success;

public sealed class NoContentServiceResult<T>(T value) : ServiceResult<T>(value)
{
    public override TRes Match<TRes>(
        Func<OkServiceResult<T>, TRes> ok,
        Func<CreateServiceResult<T>, TRes> create,
        Func<NoContentServiceResult<T>, TRes> noContent)
        => noContent(this);
}
