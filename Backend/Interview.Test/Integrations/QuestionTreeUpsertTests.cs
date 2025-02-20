using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Questions.UpsertQuestionTree;
using Microsoft.EntityFrameworkCore;

namespace Interview.Test.Integrations;

public class QuestionTreeUpsertTests
{
    [Fact]
    public async Task UpsertQuestionTreeAsync_ShouldThrowException_WhenNameIsEmpty()
    {
        using var dbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var service = new QuestionTreeUpsert(dbContext);
        var request = new UpsertQuestionTreeRequest { Name = " ", Tree = new List<UpsertQuestionSubjectTreeRequest>() };
        Func<Task> act = async () => await service.UpsertQuestionTreeAsync(request);
        await act.Should().ThrowAsync<UserException>().WithMessage("Name is required");
    }

    [Fact]
    public async Task UpsertQuestionTreeAsync_ShouldThrowException_WhenTreeIsEmpty()
    {
        using var dbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var service = new QuestionTreeUpsert(dbContext);
        var request = new UpsertQuestionTreeRequest { Name = "Valid Name", Tree = new List<UpsertQuestionSubjectTreeRequest>() };
        Func<Task> act = async () => await service.UpsertQuestionTreeAsync(request);
        await act.Should().ThrowAsync<UserException>().WithMessage("Tree has no nodes");
    }

    [Fact]
    public async Task UpsertQuestionTreeAsync_ShouldThrowException_WhenDuplicateExists()
    {
        using var dbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var service = new QuestionTreeUpsert(dbContext);
        var questionId = Guid.NewGuid();
        GenerateQuestion(dbContext, questionId);
        await dbContext.QuestionTree.AddAsync(new QuestionTree
        {
            Id = Guid.NewGuid(),
            Name = "Duplicate Name",
            RootQuestionSubjectTreeId = default,
            RootQuestionSubjectTree = new QuestionSubjectTree { QuestionId = questionId, Type = SEQuestionSubjectTreeType.Question, }
        });
        await dbContext.SaveChangesAsync();

        var request = new UpsertQuestionTreeRequest
        {
            Id = Guid.NewGuid(),
            Name = "Duplicate Name",
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new() { QuestionId = questionId, Type = EVQuestionSubjectTreeType.Question, Order = 0 }
            }
        };
        Func<Task> act = async () => await service.UpsertQuestionTreeAsync(request);
        await act.Should().ThrowAsync<UserException>().WithMessage("Duplicate question tree by name");
    }

    [Fact]
    public async Task UpsertQuestionTreeAsync_ShouldCreateNewTree()
    {
        using var dbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var service = new QuestionTreeUpsert(dbContext);
        var questionId = Guid.NewGuid();
        GenerateQuestion(dbContext, questionId);

        var request = new UpsertQuestionTreeRequest
        {
            Id = Guid.NewGuid(),
            Name = "New Tree",
            Order = 1,
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ParentQuestionSubjectTreeId = null,
                    Type = EVQuestionSubjectTreeType.Question,
                    QuestionId = questionId,
                    Order = 1
                }
            }
        };
        var result = await service.UpsertQuestionTreeAsync(request);
        result.Should().NotBeNull();
        result.Value.Should().Be(request.Id);

        var dbTree = dbContext.QuestionTree
            .Include(e => e.RootQuestionSubjectTree)
            .Single(e => e.Id == request.Id);

        dbTree.Name.Should().Be(request.Name);
        dbTree.Order.Should().Be(request.Order);
        dbTree.ParentQuestionTreeId.Should().Be(request.ParentQuestionTreeId);
        dbTree.RootQuestionSubjectTreeId.Should().Be(request.Tree.Where(e => e.ParentQuestionSubjectTreeId is null).Select(e => e.Id).Single());
        dbTree.RootQuestionSubjectTree.Should().NotBeNull();
        dbTree.RootQuestionSubjectTree!.QuestionId.Should().Be(questionId);
    }

    [Fact]
    public async Task UpsertQuestionTreeAsync_ShouldUpdateExistingTree()
    {
        using var dbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var service = new QuestionTreeUpsert(dbContext);
        var questionId = Guid.NewGuid();
        var questionId2 = Guid.NewGuid();
        GenerateQuestion(dbContext, questionId);
        GenerateQuestion(dbContext, questionId2);

        var existingTree = new QuestionTree
        {
            Id = Guid.NewGuid(),
            Name = "Existing Tree",
            Order = 1,
            RootQuestionSubjectTreeId = default,
            RootQuestionSubjectTree = new QuestionSubjectTree
            {
                QuestionId = questionId,
                Type = SEQuestionSubjectTreeType.Question
            },
        };
        await dbContext.QuestionTree.AddAsync(existingTree);
        await dbContext.SaveChangesAsync();
        var request = new UpsertQuestionTreeRequest
        {
            Id = existingTree.Id,
            Name = "Updated Tree",
            Order = 2,
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new()
                {
                    Id = existingTree.RootQuestionSubjectTree.QuestionId.Value,
                    QuestionId = questionId2,
                    Type = EVQuestionSubjectTreeType.Question,
                    Order = 1,
                }
            }
        };
        var result = await service.UpsertQuestionTreeAsync(request);
        result.Should().NotBeNull();
        result.Value.Should().Be(existingTree.Id);

        var dbTree = dbContext.QuestionTree
            .Include(e => e.RootQuestionSubjectTree)
            .Single(e => e.Id == request.Id);

        dbTree.Name.Should().Be(request.Name);
        dbTree.Order.Should().Be(request.Order);
        dbTree.ParentQuestionTreeId.Should().Be(request.ParentQuestionTreeId);
        dbTree.RootQuestionSubjectTreeId.Should().Be(request.Tree.Where(e => e.ParentQuestionSubjectTreeId is null).Select(e => e.Id).Single());
        dbTree.RootQuestionSubjectTree.Should().NotBeNull();
        dbTree.RootQuestionSubjectTree!.QuestionId.Should().Be(questionId2);
    }

    private void GenerateQuestion(AppDbContext db, Guid id)
    {
        db.Questions.Add(new Question("Test " + id) { Id = id });
        db.SaveChanges();
        db.ChangeTracker.Clear();
    }
}
