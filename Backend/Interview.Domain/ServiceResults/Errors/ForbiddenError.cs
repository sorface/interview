namespace Interview.Domain.ServiceResults.Errors;

public sealed class ForbiddenError(string message) : ServiceError(message)
{
    public override TRes Match<TRes>(Func<ServiceError, TRes> appError, Func<NotFoundServiceError, TRes> notFoundError, Func<ForbiddenError, TRes> forbiddenError)
        => forbiddenError(this);
}
