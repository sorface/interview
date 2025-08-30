namespace Interview.Backend.Auth;

public class OpenIdConnectOptions
{
    public string Issuer { get; set; } = string.Empty;

    public string MetadataPath { get; set; } = string.Empty;

    public bool RequireHttpsMetadata { get; set; } = false;
}
