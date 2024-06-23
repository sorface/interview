using Interview.Domain.Repository;

namespace Interview.Domain.Questions.CodeEditors;

public class QuestionCodeEditor : Entity
{
    public string Content { get; set; }

    public string Lang { get; set; }
}
