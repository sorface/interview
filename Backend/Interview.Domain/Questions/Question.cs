using System.ComponentModel.DataAnnotations.Schema;
using Interview.Domain.Categories;
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

    public List<Tag> Tags { get; internal set; } = new List<Tag>();

    public SEQuestionType Type { get; internal set; }

    public Guid? CategoryId { get; internal set; }

    public Category? Category { get; set; }
}
