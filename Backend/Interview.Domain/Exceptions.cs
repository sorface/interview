using Interview.Domain.Repository;

namespace Interview.Domain;

#pragma warning disable SA1649
public class UserException : Exception
#pragma warning restore SA1649
{
    public UserException(string message)
        : base(message)
    {
    }

    public UserException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

#pragma warning disable SA1402
public class NotFoundException : UserException
#pragma warning restore SA1402
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public static NotFoundException Create<T>(params (Guid Id, string EntityName)[] names)
        where T : Entity
    {
        var namesList = string.Join(", ", names.Select(e => $"{e.EntityName}=`{e.Id}`"));
        return new NotFoundException($"Not found '{typeof(T).Name}' by {namesList}");
    }

    public static NotFoundException Create<T>(Guid id)
        where T : Entity
    {
        return new NotFoundException($"Not found '{typeof(T).Name}' by Id '{id}'");
    }

    public static NotFoundException Create<T>(IEnumerable<Guid> ids)
        where T : Entity
    {
        return new NotFoundException($"Not found '{typeof(T).Name}' by ids '{string.Join(", ", ids)}'");
    }
}

#pragma warning disable SA1402
public class AccessDeniedException : UserException
#pragma warning restore SA1402
{
    public AccessDeniedException(string message)
        : base(message)
    {
    }

    public AccessDeniedException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public static AccessDeniedException CreateForAction(string resource)
        => new AccessDeniedException($"Action was denied for the '{resource.ToLower()}' resource.");
}

#pragma warning disable SA1402
public abstract class ExceptionMessage
#pragma warning restore SA1402
{
    public static string UserNotFound() => $"User not found";

    public static string UserNotFoundByNickname(string nickname) => $"Not found user with nickname [{nickname}]";

    public static string UserRoleNotFound() => "Not found \"User\" role";
}
