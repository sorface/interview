namespace Interview.Backend.Auth;

public abstract record SecurePolicy
{
    public const string Manager = "manager";
    public const string User = "user";
}
