namespace Interview.Domain.Users;

public sealed class CurrentUserAccessor : IEditableCurrentUserAccessor
{
    private User? _currentUser;

    public CurrentUserAccessor()
        : this(null)
    {
    }

    public CurrentUserAccessor(User? currentUser)
    {
        _currentUser = currentUser;
    }

    public Guid? UserId => _currentUser?.Id;

    public User? UserDetailed => _currentUser;

    public void SetUser(User user) => _currentUser = user;
}
