using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Interview.Backend.WebSocket.Configuration;

public class WebSocketAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    private readonly WebSocketAuthorizationOptions _options;

    public WebSocketAuthorizationMiddleware(RequestDelegate next, IOptions<WebSocketAuthorizationOptions> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options.Value;
    }

    public Task Invoke(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest || context.Request.Cookies.ContainsKey(_options.CookieName))
        {
            return _next(context);
        }

        var query = context.Request.Query;

        if (!query.TryGetValue(_options.WebSocketQueryName, out var value))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Request.Headers.Cookie = $"{_options.CookieName}={value}";
        context.Request.Cookies = new CustomRequestCookieCollection { { $"{_options.CookieName}", value! } };

        return _next(context);
    }

    private sealed class CustomRequestCookieCollection : IRequestCookieCollection
    {
        private readonly Dictionary<string, string> _store = new();

        public int Count => _store.Count;

        public void Add(string key, string value) => _store.Add(key, value);

        public bool ContainsKey(string key) => _store.ContainsKey(key);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => _store.TryGetValue(key, out value);

        public ICollection<string> Keys => this.Select(e => e.Key).ToList();

        public string? this[string key] => _store.TryGetValue(key, out var value) ? value : null;

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _store.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
