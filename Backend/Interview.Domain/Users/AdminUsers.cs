namespace Interview.Domain.Users;

public class AdminUsers
{
    public string[] TwitchNicknames { get; set; } = Array.Empty<string>();

    public bool IsAdmin(string nickname) => Array.IndexOf(TwitchNicknames, nickname) >= 0;
}
