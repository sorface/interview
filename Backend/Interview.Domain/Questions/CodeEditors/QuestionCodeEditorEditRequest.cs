using Interview.Domain.Repository;

namespace Interview.Domain.Questions.CodeEditors;

public class QuestionCodeEditorEditRequest
{
    public required string Content { get; set; }

    public required string Lang { get; set; }
}
