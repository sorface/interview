using Interview.Domain.Database;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Questions.QuestionTreeById;

public class QuestionTreeByIdResponse
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required Guid RootQuestionSUbjectTreeId { get; set; }

    public required List<QuestionTreeByIdResponseTree> Tree { get; set; }

    public async Task FillTreeAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        Tree.Clear();
        var subjectTreesIds = await db.QuestionSubjectTree.GetAllChildrenAsync(RootQuestionSUbjectTreeId, e => e.ParentQuestionSubjectTreeId, true, cancellationToken);
        Tree.Capacity = subjectTreesIds.Count;

        var subjectTrees = await db.QuestionSubjectTree.AsNoTracking()
            .Include(e => e.Question)
            .Where(e => subjectTreesIds.Contains(e.Id))
            .Select(e => new
            {
                Id = e.Id,
                Type = e.Type,
                ParentQuestionSubjectTreeId = e.ParentQuestionSubjectTreeId,
                Order = e.Order,
                Question = new QuestionTreeByIdResponseQuestionDetail { Id = e.Question!.Id, Value = e.Question!.Value, },
            })
            .ToListAsync(cancellationToken);
        Tree.AddRange(subjectTrees.Select(e => new QuestionTreeByIdResponseTree
        {
            Id = e.Id,
            Type = e.Type.EnumValue,
            ParentQuestionSubjectTreeId = e.ParentQuestionSubjectTreeId,
            Order = e.Order,
            Question = e.Question,
        }));
    }
}
