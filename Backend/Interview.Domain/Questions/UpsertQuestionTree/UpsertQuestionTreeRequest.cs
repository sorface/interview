namespace Interview.Domain.Questions.UpsertQuestionTree;

public class UpsertQuestionTreeRequest
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public int Order { get; set; }

    public string? ThemeAiDescription { get; set; }

    public Guid? ParentQuestionTreeId { get; set; }

    public required List<UpsertQuestionSubjectTreeRequest> Tree { get; set; }

    public bool IsValidTree(out string errorMessage)
    {
        errorMessage = string.Empty;
        if (Tree.Count == 0)
        {
            errorMessage = "Tree has no nodes";
            return false;
        }

        var countOfParentNodes = Tree.Count(e => e.ParentQuestionSubjectTreeId is null);
        if (countOfParentNodes == 0)
        {
            errorMessage = "Tree has no parent node";
            return false;
        }

        if (countOfParentNodes > 1)
        {
            errorMessage = "Tree has more than one parent node";
            return false;
        }

        if (Tree.All(e => e.Type != EVQuestionSubjectTreeType.Question))
        {
            errorMessage = "Tree has no question";
            return false;
        }

        if (Tree.Any(e => e is { Type: EVQuestionSubjectTreeType.Empty, QuestionId: not null }))
        {
            errorMessage = "Tree has empty node with question";
            return false;
        }

        if (Tree.Any(e => e is { Type: EVQuestionSubjectTreeType.Question, QuestionId: null }))
        {
            errorMessage = "Tree has question node without question";
            return false;
        }

        var nodeDict = new Dictionary<Guid, UpsertQuestionSubjectTreeRequest>();
        foreach (var node in Tree)
        {
            if (!nodeDict.TryAdd(node.Id, node))
            {
                // Если узел с таким Id уже существует, это ошибка
                errorMessage = $"Duplicate node id: {node.Id}";
                return false;
            }
        }

        // Проверка на циклические ссылки и однонаправленность
        foreach (var node in Tree)
        {
            if (!node.ParentQuestionSubjectTreeId.HasValue)
            {
                continue;
            }

            if (!nodeDict.TryGetValue(node.ParentQuestionSubjectTreeId.Value, out var value))
            {
                // Если ParentId ссылается на несуществующий узел, это ошибка
                errorMessage = $"Unknown parent node id {node.ParentQuestionSubjectTreeId.Value}";
                return false;
            }

            var current = node;
            while (current.ParentQuestionSubjectTreeId.HasValue)
            {
                if (current.ParentQuestionSubjectTreeId == node.Id)
                {
                    // Найдена циклическая ссылка
                    errorMessage = "Tree has cycle";
                    return false;
                }

                // Обновляем current на родительский узел
                if (!nodeDict.TryGetValue(current.ParentQuestionSubjectTreeId.Value, out var parent))
                {
                    // Если ParentId ссылается на несуществующий узел, это ошибка
                    errorMessage = $"Unknown parent node id {current.ParentQuestionSubjectTreeId.Value}";
                    return false;
                }

                current = parent;
            }
        }

        return true;
    }
}
