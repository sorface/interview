using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms;

public class SERoomType : SmartEnum<SERoomType, char>
{
    public static readonly SERoomType Standard = new("Standard", 'S');

    public static readonly SERoomType AI = new("AI", 'A');

    private SERoomType(string name, char value) : base(name, value)
    {
    }
}
