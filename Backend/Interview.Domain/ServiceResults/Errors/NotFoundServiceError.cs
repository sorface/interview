using Interview.Domain.ServiceResults.Errors;

namespace Interview.Domain.Errors;

public sealed class NotFoundServiceError : ServiceError
{
    public NotFoundServiceError(string message)
        : base(message)
    {
    }

    public override TRes Match<TRes>(Func<ServiceError, TRes> appError, Func<NotFoundServiceError, TRes> notFoundError, Func<ForbiddenError, TRes> forbiddenError)
        => notFoundError(this);
}
