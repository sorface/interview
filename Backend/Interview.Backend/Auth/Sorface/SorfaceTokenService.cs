using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Interview.Backend.Auth.Sorface;

public class SorfaceTokenService(AuthorizationService options, IHttpClientFactory httpClientFactory)
{
    private const string AnErrorOccurredWhileRetrievingTheUserProfile =
        "An error occurred while retrieving the user profile.";

    private const string AnErrorOccurredWhileRetrievingTheRefreshToken =
        "An error occurred while retrieving refresh token.";

    public async Task<RefreshTokenObject?> RefreshTokenAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new HttpRequestException(AnErrorOccurredWhileRetrievingTheRefreshToken);
        }

        var form = new List<KeyValuePair<string, string>> { new("refresh_token", $"{refreshToken}"), new("grant_type", "refresh_token"), };

        using var request = new HttpRequestMessage(HttpMethod.Post, options.TokenEndpoint) { Content = new FormUrlEncodedContent(form), };

        var chars = $"{options.ClientId}:{options.ClientSecret}";

        var bytes = Encoding.UTF8.GetBytes(chars);
        var base64Secret = Convert.ToBase64String(bytes);

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Secret);

#pragma warning disable CA2000
        var httpClient = httpClientFactory.CreateClient();
#pragma warning restore CA2000
        using var response = await httpClient
            .SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(AnErrorOccurredWhileRetrievingTheRefreshToken);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var jsonDocument = JsonDocument.Parse(content);

        return jsonDocument.Deserialize<RefreshTokenObject>();
    }

    public async Task RevokeTokenAsync(HttpContext httpContent, string? accessToken)
    {
        if (accessToken is null)
        {
            return;
        }

        var form = new List<KeyValuePair<string, string>> { new("token", $"{accessToken}"), };

        using var request = new HttpRequestMessage(HttpMethod.Post, options.RevokeTokenEndpoint) { Content = new FormUrlEncodedContent(form), };

        var chars = $"{options.ClientId}:{options.ClientSecret}";

        var bytes = Encoding.UTF8.GetBytes(chars);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));

        try
        {
#pragma warning disable CA2000
            var httpClient = httpClientFactory.CreateClient();
#pragma warning restore CA2000
            await httpClient.SendAsync(request, httpContent.RequestAborted);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public async Task<JsonDocument> GetTokenPrincipalAsync(
        string? accessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new HttpRequestException(AnErrorOccurredWhileRetrievingTheUserProfile);
        }

        var form = new List<KeyValuePair<string, string>> { new("token", $"{accessToken}"), };

        using var request = new HttpRequestMessage(HttpMethod.Post, options.UserInformationEndpoint) { Content = new FormUrlEncodedContent(form), };

        var chars = $"{options.ClientId}:{options.ClientSecret}";

        var bytes = Encoding.UTF8.GetBytes(chars);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));

#pragma warning disable CA2000
        var httpClient = httpClientFactory.CreateClient();
#pragma warning restore CA2000
        using var response = await httpClient
            .SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(AnErrorOccurredWhileRetrievingTheUserProfile);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonDocument.Parse(content);
    }
}
