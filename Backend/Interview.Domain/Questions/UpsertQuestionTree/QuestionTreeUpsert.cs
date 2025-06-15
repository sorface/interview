using System.Linq.Expressions;
using Interview.Domain.Database;
using Interview.Domain.ServiceResults.Success;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Questions.UpsertQuestionTree;

public class QuestionTreeUpsert(AppDbContext db) : ISelfScopeService
{
    public static async Task EnsureNonDuplicateByNameAsync(AppDbContext db, Guid id, string name, Guid? parentQuestionTreeId, CancellationToken cancellationToken)
    {
        var hasDuplicates = await db.QuestionTree
            .AsNoTracking()
            .Where(e => e.Name == name && e.Id != id && !e.IsArchived)
            .Where(BuildSpecByParent(parentQuestionTreeId))
            .AnyAsync(cancellationToken);
        if (hasDuplicates)
        {
            throw new UserException("Duplicate question tree by name");
        }
    }

    public async Task<ServiceResult<Guid>> UpsertQuestionTreeAsync(UpsertQuestionTreeRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new UserException("Name is required");
        }

        request.Name = request.Name.Trim();
        request.ThemeAiDescription = request.ThemeAiDescription?.Trim();

        if (!request.IsValidTree(out var errorMessage))
        {
            throw new UserException(errorMessage);
        }

        await EnsureNonDuplicateByNameAsync(db, request.Id, request.Name, request.ParentQuestionTreeId, cancellationToken);

        var questionIds = request.Tree
            .Where(e => e.QuestionId is not null)
            .Select(e => e.QuestionId!.Value);
        await EnsureAvailableQuestionsAsync(questionIds, cancellationToken);

        var tree = await db.QuestionTree.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        return await db.RunTransactionAsync(async ct =>
        {
            // Shift tree order
            await db.QuestionTree
                .Where(BuildSpecByParent(request.ParentQuestionTreeId))
                .Where(e => e.Order >= request.Order)
                .ExecuteUpdateAsync(e => e.SetProperty(p => p.Order, p => p.Order + 1), cancellationToken);

            var create = false;
            if (tree is null)
            {
                create = true;
                tree = await CreateAsync(request, ct);
            }
            else
            {
                await UpdateAsync(tree, request, ct);
            }

            return create ? ServiceResult.Created(tree.Id) : ServiceResult.Ok(tree.Id);
        },
            cancellationToken);
    }

    private static Expression<Func<QuestionTree, bool>> BuildSpecByParent(Guid? parentQuestionTreeId)
    {
        return parentQuestionTreeId is null
            ? e => e.ParentQuestionTreeId == null
            : e => e.ParentQuestionTreeId == parentQuestionTreeId;
    }

    private async Task EnsureAvailableQuestionsAsync(IEnumerable<Guid> select, CancellationToken cancellationToken)
    {
        var requiredQuestions = select.ToHashSet().ToList();
        var dbQuestions = await db.Questions
            .Where(e => requiredQuestions.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        var unknownQuestions = requiredQuestions.Except(dbQuestions).ToList();
        if (unknownQuestions.Count > 0)
        {
            throw NotFoundException.Create<Question>(unknownQuestions);
        }
    }

    private async Task EnsureNotExistQuestionSubjectTreesAsync(ICollection<Guid> ids, CancellationToken cancellationToken)
    {
        var dbItems = await db.QuestionSubjectTree.AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);
        if (dbItems.Count > 0)
        {
            throw new UserException($"Question subject tree with id [{string.Join(", ", dbItems)}] already exists");
        }
    }

    private async Task UpdateAsync(QuestionTree tree, UpsertQuestionTreeRequest request, CancellationToken cancellationToken)
    {
        var dbNodeIds = await db.QuestionSubjectTree.GetAllChildrenAsync(tree.RootQuestionSubjectTreeId, e => e.ParentQuestionSubjectTreeId, true, cancellationToken);
        var actualNodeIds = request.Tree.Select(e => e.Id).ToHashSet();

        var addNodeIds = actualNodeIds.Except(dbNodeIds).ToHashSet();
        await EnsureNotExistQuestionSubjectTreesAsync(addNodeIds, cancellationToken);
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

        await db.SaveChangesAsync(cancellationToken);

        var dbNodes = await db.QuestionSubjectTree
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
                item.Db.QuestionId = item.Request.QuestionId;
                item.Db.ParentQuestionSubjectTreeId = item.Request.ParentQuestionSubjectTreeId;
                item.Db.Type = SEQuestionSubjectTreeType.FromEnumValue(item.Request.Type);
                item.Db.Order = item.Request.Order;
            }
        }

        tree.Order = request.Order;
        tree.Name = request.Name;
        tree.ThemeAiDescription = request.ThemeAiDescription;
        tree.RootQuestionSubjectTreeId = request.Tree.Single(e => e.ParentQuestionSubjectTreeId is null).Id;

        await db.SaveChangesAsync(cancellationToken);

        var nodesForDelete = dbNodeIds.Except(actualNodeIds).ToList();

        // Удалить неактуальные ноды, в конце, так как тут может быть родительский нод
        if (nodesForDelete.Count > 0)
        {
            await db.QuestionSubjectTree.Where(e => nodesForDelete.Contains(e.Id)).ExecuteDeleteAsync(cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<QuestionTree> CreateAsync(UpsertQuestionTreeRequest request, CancellationToken cancellationToken)
    {
        await EnsureNotExistQuestionSubjectTreesAsync(request.Tree.Select(e => e.Id).ToList(), cancellationToken);
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
            Id = request.Id,
            Name = request.Name,
            ParentQuestionTreeId = request.ParentQuestionTreeId,
            Order = request.Order,
            RootQuestionSubjectTreeId = subjectTrees.Single(e => e.ParentQuestionSubjectTreeId is null).Id,
            ThemeAiDescription = request.ThemeAiDescription,
        };
        await db.QuestionTree.AddAsync(tree, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return tree;
    }
}
