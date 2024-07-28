using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;

namespace Interview.Backend.Auth.Sorface;

public class SorfaceAuthenticationHandler : OAuthHandler<SorfaceAuthenticationOptions>
{
    private readonly SorfaceTokenHandler _sorfaceTokenHandler;

    public SorfaceAuthenticationHandler(IOptionsMonitor<SorfaceAuthenticationOptions> options,
                                        ILoggerFactory logger,
                                        UrlEncoder encoder,
                                        ISystemClock clock,
                                        SorfaceTokenHandler sorfaceTokenHandler) : base(options, logger, encoder, clock)
    {
        _sorfaceTokenHandler = sorfaceTokenHandler;
    }

    protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
    {
        using var document = await _sorfaceTokenHandler.GetTokenPrincipalAsync(tokens.AccessToken, Context.RequestAborted);

        var principal = new ClaimsPrincipal(identity);

        var context = new OAuthCreatingTicketContext(
            principal,
            properties,
            Context,
            Scheme,
            Options,
            Backchannel,
            tokens,
            document.RootElement);

        context.RunClaimActions();

        await Events.CreatingTicket(context);

        return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
    }
}
