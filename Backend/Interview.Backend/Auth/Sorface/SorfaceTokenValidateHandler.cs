using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Interview.Backend.Auth.Sorface;

public class SorfaceTokenValidateHandler
{
    private const string AnErrorOccurredWhileRetrievingTheUserProfile =
        "An error occurred while retrieving the user profile.";

    private readonly AuthorizationService _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public SorfaceTokenValidateHandler(AuthorizationService options, IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
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

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.UserInformationEndpoint)
        {
            Content = new FormUrlEncodedContent(form),
        };

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
