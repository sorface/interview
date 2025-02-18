using System.Linq.Expressions;
using Interview.Domain.Database;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Questions.UpsertQuestionTree;

public class QuestionTreeUpsert(AppDbContext db) : ISelfScopeService
{
    public async Task UpsertQuestionTreeAsync(UpsertQuestionTreeRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new UserException("Name is required");
        }

        request.Name = request.Name.Trim();

        if (!request.IsValidTree(out var errorMessage))
        {
            throw new UserException(errorMessage);
        }

        var hasDuplicates = await db.QuestionTree
            .AsNoTracking()
            .Where(e => e.Id != request.Id && e.Name == request.Name)
            .Where(BuildSpecByParent(request.ParentQuestionTreeId))
            .AnyAsync(cancellationToken);
        if (hasDuplicates)
        {
            throw new UserException("Duplicate question tree by name");
        }

        var tree = await db.QuestionTree.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        await db.RunTransactionAsync(async ct =>
        {
            // Shift tree order
            await db.QuestionTree
                .Where(BuildSpecByParent(request.ParentQuestionTreeId))
                .Where(e => e.Order >= request.Order)
                .ExecuteUpdateAsync(e => e.SetProperty(p => p.Order, p => p.Order + 1), cancellationToken);

            if (tree is null)
            {
                await CreateAsync(request, cancellationToken);
            }
            else
            {
                await UpdateAsync(tree, request, cancellationToken);
            }

            await db.SaveChangesAsync(ct);
            return DBNull.Value;
        },
            cancellationToken);
    }

    private async Task UpdateAsync(QuestionTree tree, UpsertQuestionTreeRequest request, CancellationToken cancellationToken)
    {
        var allNodeIds = await db.QuestionSubjectTree.GetAllChildrenAsync(tree.RootQuestionSubjectTreeId, e => e.ParentQuestionSubjectTreeId, cancellationToken);
        var allNodes = await db.QuestionSubjectTree.AsNoTracking()
            .Where(e => allNodeIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        var actualNodeIds = request.Tree.Select(e => e.Id).ToHashSet();

        var nodesForDelete = allNodeIds.Except(actualNodeIds).ToList();

        // Удалить неактуальные ноды, в конце, так как тут может быть родительский нод
        if (nodesForDelete.Count > 0)
        {
            await db.QuestionSubjectTree.Where(e => nodesForDelete.Contains(e.Id)).ExecuteDeleteAsync(cancellationToken);
        }

        // Проверяем были ли изменения в дереве
        // Получаем все ноды старого дерева
        // Составляем список, который нужно удалить
        // Составляем список на добавление
        // Добавляем новые ноды
        // Обновляем существующие ноды
        // Удаляем неактуальные ноды
        throw new NotImplementedException();
    }

    private async Task CreateAsync(UpsertQuestionTreeRequest request, CancellationToken cancellationToken)
    {
        var subjectTrees = request.Tree.Select(e => new QuestionSubjectTree
        {
            Id = e.Id,
            ParentQuestionSubjectTreeId = e.ParentQuestionSubjectTreeId,
            QuestionId = e.QuestionId,
            Type = SEQuestionSubjectTreeType.FromEnumValue(e.Type),
            Order = e.Order,
        }).ToList();
        await db.QuestionSubjectTree.AddRangeAsync(subjectTrees, cancellationToken);

        var tree = new QuestionTree
        {
            Name = request.Name,
            ParentQuestionTreeId = request.ParentQuestionTreeId,
            Order = request.Order,
            RootQuestionSubjectTreeId = subjectTrees.Single(e => e.ParentQuestionSubjectTreeId is null).Id,
        };
        await db.QuestionTree.AddAsync(tree, cancellationToken);
    }

    private Expression<Func<QuestionTree, bool>> BuildSpecByParent(Guid? parentQuestionTreeId)
    {
        return parentQuestionTreeId is null
            ? e => e.ParentQuestionTreeId == null
            : e => e.ParentQuestionTreeId == parentQuestionTreeId;
    }
}
