using Microsoft.AspNetCore.Authentication;

namespace Interview.Backend.Auth.Sorface;

public static class SorfaceAuthenticationExtensions
{
    public static AuthenticationBuilder AddSorface(this AuthenticationBuilder builder)
    {
        return builder.AddSorface(SorfaceAuthenticationDefaults.AuthenticationScheme, options => { });
    }

    public static AuthenticationBuilder AddSorface(
        this AuthenticationBuilder builder, Action<SorfaceAuthenticationOptions> configuration)
    {
        return builder.AddSorface(SorfaceAuthenticationDefaults.AuthenticationScheme, configuration);
    }

    public static AuthenticationBuilder AddSorface(
        this AuthenticationBuilder builder, string scheme, Action<SorfaceAuthenticationOptions> configuration)
    {
        return builder.AddSorface(scheme, SorfaceAuthenticationDefaults.DisplayName, configuration);
    }

    public static AuthenticationBuilder AddSorface(
        this AuthenticationBuilder builder,
        string scheme,
        string caption,
        Action<SorfaceAuthenticationOptions> configuration)
    {
        return builder
            .AddOAuth<SorfaceAuthenticationOptions, SorfaceAuthenticationHandler>(scheme, caption, configuration);
    }
}
