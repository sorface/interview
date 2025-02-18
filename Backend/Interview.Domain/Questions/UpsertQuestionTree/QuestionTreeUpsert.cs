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
        var dbNodeIds = await db.QuestionSubjectTree.GetAllChildrenAsync(tree.RootQuestionSubjectTreeId, e => e.ParentQuestionSubjectTreeId, cancellationToken);
        var actualNodeIds = request.Tree.Select(e => e.Id).ToHashSet();

        var addNodeIds = actualNodeIds.Except(dbNodeIds).ToHashSet();
        if (addNodeIds.Count > 0)
        {
            var addNodes = request.Tree
                .Where(e => addNodeIds.Contains(e.Id))
                .Select(e => new QuestionSubjectTree
                {
                    Id = e.Id,
                    ParentQuestionSubjectTreeId = e.ParentQuestionSubjectTreeId,
                    QuestionId = e.QuestionId,
                    Type = SEQuestionSubjectTreeType.FromEnumValue(e.Type),
                    Order = e.Order,
                });
            await db.QuestionSubjectTree.AddRangeAsync(addNodes, cancellationToken);
        }

        var dbNodes = await db.QuestionSubjectTree.AsNoTracking()
            .Where(e => actualNodeIds.Contains(e.Id))
            .ToListAsync(cancellationToken);
        if (dbNodes.Count > 0)
        {
            foreach (var item in dbNodes.Join(
                         request.Tree,
                         e => e.Id,
                         e => e.Id,
                         (subjectTree, treeRequest) => (Db: subjectTree, Request: treeRequest)))
            {
                if (item.Db.QuestionId != item.Request.QuestionId)
                {
                    item.Db.QuestionId = item.Request.QuestionId;
                }

                if (item.Db.ParentQuestionSubjectTreeId != item.Request.ParentQuestionSubjectTreeId)
                {
                    item.Db.ParentQuestionSubjectTreeId = item.Request.ParentQuestionSubjectTreeId;
                }
            }
        }

        tree.RootQuestionSubjectTreeId = request.Tree.Single(e => e.ParentQuestionSubjectTreeId is null).Id;

        var nodesForDelete = dbNodeIds.Except(actualNodeIds).ToList();

        // Удалить неактуальные ноды, в конце, так как тут может быть родительский нод
        if (nodesForDelete.Count > 0)
        {
            await db.QuestionSubjectTree.Where(e => nodesForDelete.Contains(e.Id)).ExecuteDeleteAsync(cancellationToken);
        }
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
