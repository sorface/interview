using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;

namespace Interview.Backend.Auth.Sorface;

public partial class SorfaceAuthenticationHandler : OAuthHandler<SorfaceAuthenticationOptions>
{
    private readonly SorfaceTokenValidateHandler _sorfaceTokenValidateHandler;

    public SorfaceAuthenticationHandler(
        IOptionsMonitor<SorfaceAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        SorfaceTokenValidateHandler sorfaceTokenValidateHandler)
        : base(options, logger, encoder, clock)
    {
        _sorfaceTokenValidateHandler = sorfaceTokenValidateHandler;
    }

    protected override async Task<AuthenticationTicket> CreateTicketAsync(
        ClaimsIdentity identity,
        AuthenticationProperties properties,
        OAuthTokenResponse tokens)
    {
        using var document = await _sorfaceTokenValidateHandler.GetTokenPrincipalAsync(tokens.AccessToken, Context.RequestAborted);

        var principal = new ClaimsPrincipal(identity);

        var context = new OAuthCreatingTicketContext(
            principal, properties, Context, Scheme, Options, Backchannel, tokens, document.RootElement);

        context.RunClaimActions();

        await Events.CreatingTicket(context);

        return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
    }
}
