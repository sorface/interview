using System.Text.Json.Serialization;

namespace Interview.Domain.Dates;

[JsonConverter(typeof(DateConverter))]
public readonly struct Date : IEquatable<Date>, IParsable<Date>
{
    public long UnixTimeSeconds { get; }

    public DateTimeOffset DateTimeOffset => DateTimeOffset.FromUnixTimeSeconds(UnixTimeSeconds);

    public DateTime UtcDateTime => DateTimeOffset.UtcDateTime;

    public Date(long unixTimeSeconds)
    {
        UnixTimeSeconds = unixTimeSeconds;
    }

    public bool Equals(Date other) => UnixTimeSeconds == other.UnixTimeSeconds;

    public override bool Equals(object? obj) => obj is Date other && Equals(other);

    public override int GetHashCode() => UnixTimeSeconds.GetHashCode();

    public override string ToString() => DateTimeOffset.ToString();

    public static bool operator ==(Date left, Date right) => left.Equals(right);

    public static bool operator !=(Date left, Date right) => !(left == right);

    public static implicit operator Date(DateTimeOffset dateTimeOffset) => new(dateTimeOffset.ToUnixTimeSeconds());

    public static implicit operator long(Date date) => date.UnixTimeSeconds;

    public static implicit operator DateTimeOffset(Date date) => date.DateTimeOffset;

    public static implicit operator DateTime(Date date) => date.UtcDateTime;

    public static Date Parse(string value, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception("Value should not be empty");
        }

        if (!long.TryParse(value, out var unixTimeSeconds))
        {
            throw new Exception("Value should be a number");
        }

        return new Date(unixTimeSeconds);
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out Date result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        if (!long.TryParse(value, out var unixTimeSeconds))
        {
            result = default;
            return false;
        }

        result = new Date(unixTimeSeconds);
        return true;
    }
}
