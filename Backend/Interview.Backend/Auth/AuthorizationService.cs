namespace Interview.Backend.Auth;

public class AuthorizationService
{
    public string Id { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string AuthorizationEndPoint { get; set; } = string.Empty;

    public string TokenEndpoint { get; set; } = string.Empty;

    public string UserInformationEndpoint { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string ClaimsIssuer { get; set; } = string.Empty;

    public string CookieDomain { get; set; } = string.Empty;

    public string CookieName { get; set; } = string.Empty;

    public PathString CallbackPath { get; set; } = PathString.Empty;

    public List<string> AvailableLoginRedirects { get; set; } = new();

    public CorrelationCookie? CorrelationCookie { get; set; } = null;
}

public class CorrelationCookie
{
    public string Domain { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
