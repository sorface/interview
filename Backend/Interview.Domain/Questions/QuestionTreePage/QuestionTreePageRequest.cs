namespace Interview.Domain.Questions.QuestionTreePage;

/// <summary>
/// Question tree page request.
/// </summary>
public class QuestionTreePageRequest : FilteredRequest<QuestionTreePageRequestFilter>;

public class QuestionTreePageRequestFilter
{
    public string? Name { get; set; }

    public Guid? ParentQuestionTreeId { get; set; }

    public bool? ParentlessOnly { get; set; }
}
