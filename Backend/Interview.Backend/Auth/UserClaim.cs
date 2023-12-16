namespace Interview.Backend.Auth
{
    public class UserClaim
    {
        public Guid Identity { get; set; }

        public string Nickname { get; set; } = string.Empty;

        public string Avatar { get; set; } = string.Empty;

        public string TwitchIdentity { get; set; } = string.Empty;

        public List<string>? Roles { get; set; }
    }
}
