namespace Interview.Domain.Users;

public sealed class CurrentUserAccessor(User? currentUser) : IEditableCurrentUserAccessor
{
    private User? _currentUser = currentUser;

    public CurrentUserAccessor()
        : this(null)
    {
    }

    public Guid? UserId => _currentUser?.Id;

    public User? UserDetailed => _currentUser;

    public void SetUser(User user) => _currentUser = user;
}
