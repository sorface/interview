using Interview.Domain.Categories;
using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Repository;
using Interview.Domain.Tags;

namespace Interview.Domain.Questions;

public class Question(string value) : ArchiveEntity
{
    private Question()
        : this(string.Empty)
    {
    }

    public string Value { get; internal set; } = value;

    public Guid? CodeEditorId { get; internal set; }

    public QuestionCodeEditor? CodeEditor { get; set; }

    public List<Tag> Tags { get; internal set; } = new();

    public List<QuestionAnswer> Answers { get; set; } = new();

    public SEQuestionType Type { get; internal set; } = default!;

    public Guid? CategoryId { get; set; }

    public Category? Category { get; set; }
}
