using System.ComponentModel.DataAnnotations.Schema;
using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Repository;
using Interview.Domain.Rooms;
using Interview.Domain.Tags;

namespace Interview.Domain.Questions;

public class Question : ArchiveEntity
{
    public Question(string value)
    {
        Value = value;
    }

    private Question()
        : this(string.Empty)
    {
    }

    public string Value { get; internal set; }

    public QuestionCodeEditor? CodeEditor { get; internal set; }

    public List<Tag> Tags { get; internal set; } = new();

    public List<QuestionAnswer> Answers { get; internal set; } = new();

    public SEQuestionType Type { get; internal set; }
}
