using Ardalis.SmartEnum;

namespace Interview.Domain.Questions;

/// <summary>
/// Question type.
/// </summary>
public sealed class SEQuestionType : SmartEnum<SEQuestionType>
{
    /// <summary>
    /// Public.
    /// </summary>
    public static readonly SEQuestionType Public = new("Public", EVQuestionType.Public);

    /// <summary>
    /// Private.
    /// </summary>
    public static readonly SEQuestionType Private = new("Private", EVQuestionType.Private);

    private SEQuestionType(string name, EVQuestionType type)
        : base(name, (int)type)
    {
    }

    public EVQuestionType EnumValue => (EVQuestionType)Value;
}
