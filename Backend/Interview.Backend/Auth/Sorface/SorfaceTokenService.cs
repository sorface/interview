using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Interview.Backend.Auth.Sorface;

public class SorfaceTokenService
{
    private const string AnErrorOccurredWhileRetrievingTheUserProfile =
        "An error occurred while retrieving the user profile.";

    private const string AnErrorOccurredWhileRetrievingTheRefreshToken =
        "An error occurred while retrieving refresh token.";

    private readonly AuthorizationService _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public SorfaceTokenService(AuthorizationService options, IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<RefreshTokenObject?> RefreshTokenAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new HttpRequestException(AnErrorOccurredWhileRetrievingTheRefreshToken);
        }

        var form = new List<KeyValuePair<string, string>> { new("refresh_token", $"{refreshToken}"), new("grant_type", "refresh_token"), };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint) { Content = new FormUrlEncodedContent(form), };

        var chars = $"{_options.ClientId}:{_options.ClientSecret}";

        var bytes = Encoding.UTF8.GetBytes(chars);
        var base64Secret = Convert.ToBase64String(bytes);

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Secret);

        using var response = await _httpClientFactory.CreateClient()
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

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.RevokeTokenEndpoint) { Content = new FormUrlEncodedContent(form), };

        var chars = $"{_options.ClientId}:{_options.ClientSecret}";

        var bytes = Encoding.UTF8.GetBytes(chars);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));

        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            await httpClient.SendAsync(request, httpContent.RequestAborted);
        }
        catch (Exception e)
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

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.UserInformationEndpoint) { Content = new FormUrlEncodedContent(form), };

        var chars = $"{_options.ClientId}:{_options.ClientSecret}";

        var bytes = Encoding.UTF8.GetBytes(chars);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));

        using var response = await _httpClientFactory.CreateClient()
            .SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(AnErrorOccurredWhileRetrievingTheUserProfile);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonDocument.Parse(content);
    }
}
