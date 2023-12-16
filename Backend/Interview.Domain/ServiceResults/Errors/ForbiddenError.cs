using Interview.Domain.Errors;

namespace Interview.Domain.ServiceResults.Errors;

public sealed class ForbiddenError : ServiceError
{
    public ForbiddenError(string message)
        : base(message)
    {
    }

    public override TRes Match<TRes>(Func<ServiceError, TRes> appError, Func<NotFoundServiceError, TRes> notFoundError, Func<ForbiddenError, TRes> forbiddenError)
        => forbiddenError(this);
}
