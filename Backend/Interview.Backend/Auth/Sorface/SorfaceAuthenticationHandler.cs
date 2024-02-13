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
    public SorfaceAuthenticationHandler(
        IOptionsMonitor<SorfaceAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticationTicket> CreateTicketAsync(
        ClaimsIdentity identity,
        AuthenticationProperties properties,
        OAuthTokenResponse tokens)
    {
        var form = new List<KeyValuePair<string, string>>
        {
            new("token", $"{tokens.AccessToken}"),
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, Options.UserInformationEndpoint)
        {
            Content = new FormUrlEncodedContent(form),
        };

        var chars = $"{Options.ClientId}:{Options.ClientSecret}";

        var bytes = Encoding.UTF8.GetBytes(chars);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));

        using var response = await Backchannel
            .SendAsync(request, HttpCompletionOption.ResponseContentRead, Context.RequestAborted);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("An error occurred while retrieving the user profile.");
        }

        var content = await response.Content.ReadAsStringAsync(Context.RequestAborted);

        using var payload = JsonDocument.Parse(content);

        var principal = new ClaimsPrincipal(identity);

        var context = new OAuthCreatingTicketContext(
            principal, properties, Context, Scheme, Options, Backchannel, tokens, payload.RootElement);

        context.RunClaimActions();

        await Events.CreatingTicket(context);

        return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
    }
}
