namespace Interview.Backend.WebSocket.Configuration;

public class WebSocketAuthorizationOptions
{
    public const string DefaultCookieName = "sorinv_session_id";

    public string CookieName { get; set; } = string.Empty;

    public string WebSocketQueryName { get; set; } = string.Empty;
}
