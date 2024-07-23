using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Interview.Infrastructure.Chat;

public class TwitchChatClient : IDisposable
{
    private readonly Guid _roomId;
    private readonly TwitchClient _client;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public TwitchChatClient(Guid roomId)
    {
        _roomId = roomId;
        _cancellationTokenSource = new CancellationTokenSource();
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30),
        };
        var customClient = new WebSocketClient(clientOptions);
        _client = new TwitchClient(customClient);
        _client.OnMessageReceived += ClientOnMessageReceived;
    }

    public void Connect(string username, string accessToken)
    {
        var credentials = new ConnectionCredentials(username, accessToken);
        _client.Initialize(credentials);
        _client.Connect();
    }

    public void Dispose()
    {
        try
        {
            _cancellationTokenSource.Cancel();
        }
        catch
        {
            // ignore
        }

        _client.OnMessageReceived -= ClientOnMessageReceived;
        try
        {
            _client.Disconnect();
        }
        catch
        {
            // ignore
        }
    }

    private void ClientOnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        var message = e.ChatMessage.Message ?? string.Empty;

        if (message.Contains("js"))
        {
            _client.SendMessage(e.ChatMessage.Channel, $"@{e.ChatMessage.Username}, God bless you");
        }
    }
}
