namespace Interview.Backend.Auth;

public class AuthorizationService
{
    public string Id { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string ClaimsIssuer { get; set; } = string.Empty;

    public string Domain { get; set; } = string.Empty; 

    public PathString CallbackPath { get; set; } = PathString.Empty;

    public List<string> AvailableLoginRedirects { get; set; } = new();
}
