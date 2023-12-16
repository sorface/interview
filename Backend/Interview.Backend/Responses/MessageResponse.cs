namespace Interview.Backend.Responses
{
    public class MessageResponse
    {
        public static readonly MessageResponse Ok = new() { Message = "OK", };

#pragma warning disable SA1206
        public required string Message { get; init; } = string.Empty;
#pragma warning restore SA1206
    }
}
