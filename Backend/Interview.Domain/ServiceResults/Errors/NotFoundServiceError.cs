namespace Interview.Domain.ServiceResults.Errors;

public sealed class NotFoundServiceError : ServiceError
{
    public NotFoundServiceError(string message)
        : base(message)
    {
    }

    public override TRes Match<TRes>(Func<ServiceError, TRes> appError, Func<NotFoundServiceError, TRes> notFoundError, Func<ForbiddenError, TRes> forbiddenError)
        => notFoundError(this);
}
