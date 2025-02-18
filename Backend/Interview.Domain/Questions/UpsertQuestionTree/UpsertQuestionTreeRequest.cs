namespace Interview.Domain.Questions.UpsertQuestionTree;

public class UpsertQuestionTreeRequest
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public int Order { get; set; }

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
            if (node.ParentQuestionSubjectTreeId.HasValue)
            {
                if (!nodeDict.TryGetValue(node.ParentQuestionSubjectTreeId.Value, out var value))
                {
                    // Если ParentId ссылается на несуществующий узел, это ошибка
                    errorMessage = $"Unknown parent node id {node.ParentQuestionSubjectTreeId.Value}";
                    return false;
                }

                // Проверка на циклические ссылки
                var current = node;
                while (current.ParentQuestionSubjectTreeId.HasValue)
                {
                    if (current.ParentQuestionSubjectTreeId == node.Id)
                    {
                        // Найдена циклическая ссылка
                        errorMessage = "Tree has cycle";
                        return false;
                    }

                    current = value;
                }
            }
        }

        return true;
    }
}
