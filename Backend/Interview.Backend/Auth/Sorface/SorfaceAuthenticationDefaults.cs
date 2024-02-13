namespace Interview.Backend.Auth.Sorface;

public static class SorfaceAuthenticationDefaults
{
    public const string AuthenticationScheme = "sorface";

    public static readonly string DisplayName = "Sorface";

    public static readonly string Issuer = "Sorface";

    public static readonly string CallbackPath = "/oauth2/sorface";

    public static readonly string AuthorizationEndPoint = "http://localhost:8080/oauth2/authorize";

    public static readonly string TokenEndpoint = "http://localhost:8080/oauth2/token";

    public static readonly string UserInformationEndpoint = "http://localhost:8080/oauth2/introspect";
}
