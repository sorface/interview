using Ardalis.SmartEnum;
using Interview.Domain.Repository;

namespace Interview.Domain.Questions;

public class QuestionSubjectTree : ArchiveEntity
{
    public required Guid QuestionId { get; set; }

    public Question? Question { get; set; }

    public required SEQuestionSubjectTreeType Type { get; set; }

    public Guid? ParentQuestionSubjectTreeId { get; set; }

    public QuestionSubjectTree? ParentQuestionSubjectTree { get; set; }
}

public enum EVQuestionSubjectTreeType : byte
{
    /// <summary>
    /// Empty.
    /// </summary>
    Empty,

    /// <summary>
    /// Question.
    /// </summary>
    Question,
}

public sealed class SEQuestionSubjectTreeType : SmartEnum<SEQuestionSubjectTreeType, char>
{
    public static readonly SEQuestionSubjectTreeType Empty = new SEQuestionSubjectTreeType(EVQuestionSubjectTreeType.Empty, "Empty", 'E');

    public static readonly SEQuestionSubjectTreeType Question = new SEQuestionSubjectTreeType(EVQuestionSubjectTreeType.Question, "Question", 'Q');

    public SEQuestionSubjectTreeType(EVQuestionSubjectTreeType enumValue, string name, char value) : base(name, value)
    {
        EnumValue = enumValue;
    }

    public EVQuestionSubjectTreeType EnumValue { get; }
}
