using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Questions.UpsertQuestionTree;
using Interview.Domain.ServiceResults.Success;
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
        dbContext.ChangeTracker.Clear();

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
        dbContext.ChangeTracker.Clear();

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

    [Fact]
    public async Task UpsertQuestionTreeAsync_CreateNewTree_ReturnsCreatedResult()
    {
        // Arrange
        using var dbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var service = new QuestionTreeUpsert(dbContext);
        var questionId = Guid.NewGuid();
        GenerateQuestion(dbContext, questionId);

        var request = new UpsertQuestionTreeRequest
        {
            Id = Guid.NewGuid(),
            Name = "New Tree",
            Order = 1,
            ParentQuestionTreeId = null,
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new UpsertQuestionSubjectTreeRequest
                {
                    Id = Guid.NewGuid(),
                    ParentQuestionSubjectTreeId = null,
                    QuestionId = questionId,
                    Type = EVQuestionSubjectTreeType.Question,
                    Order = 1
                }
            }
        };

        // Act
        var result = await service.UpsertQuestionTreeAsync(request);
        dbContext.ChangeTracker.Clear();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CreateServiceResult<Guid>>(result);
        Assert.Equal(request.Id, result.Value);
    }

    [Fact]
    public async Task UpsertQuestionTreeAsync_UpdateExistingTree_ReturnsOkResult()
    {
        // Arrange
        using var dbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var questionId = Guid.NewGuid();
        GenerateQuestion(dbContext, questionId);

        var subjectTree = new QuestionSubjectTree
        {
            QuestionId = questionId,
            Type = SEQuestionSubjectTreeType.Empty,
        };
        await dbContext.QuestionSubjectTree.AddAsync(subjectTree);
        await dbContext.SaveChangesAsync();

        var existingTree = new QuestionTree
        {
            Id = Guid.NewGuid(),
            Name = "Existing Tree",
            Order = 1,
            ParentQuestionTreeId = null,
            RootQuestionSubjectTreeId = subjectTree.Id
        };

        dbContext.QuestionTree.Add(existingTree);
        await dbContext.SaveChangesAsync();

        var service = new QuestionTreeUpsert(dbContext);

        var request = new UpsertQuestionTreeRequest
        {
            Id = existingTree.Id,
            Name = "Updated Tree",
            Order = 2,
            ParentQuestionTreeId = null,
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new UpsertQuestionSubjectTreeRequest
                {
                    Id = Guid.NewGuid(),
                    ParentQuestionSubjectTreeId = null,
                    QuestionId = questionId,
                    Type = EVQuestionSubjectTreeType.Question,
                    Order = 55
                }
            }
        };

        // Act
        var result = await service.UpsertQuestionTreeAsync(request);
        dbContext.ChangeTracker.Clear();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OkServiceResult<Guid>>(result);
        Assert.Equal(existingTree.Id, result.Value);

        var updatedTree = await dbContext.QuestionTree.FindAsync(existingTree.Id);
        Assert.NotNull(updatedTree);
        Assert.Equal("Updated Tree", updatedTree.Name);
        Assert.Equal(2, updatedTree.Order);

        var child = await GetSubjectTree(dbContext, existingTree);
        child.Should().HaveCount(1);
        ShouldBeSame(child[0], request.Tree[0]);
    }

    [Fact]
    public async Task UpsertQuestionTreeAsync_NonExistentQuestion_ThrowsNotFoundException()
    {
        // Arrange
        using var dbContext = new TestAppDbContextFactory().Create(new TestSystemClock());

        var service = new QuestionTreeUpsert(dbContext);

        var request = new UpsertQuestionTreeRequest
        {
            Id = Guid.NewGuid(),
            Name = "New Tree",
            Order = 1,
            ParentQuestionTreeId = null,
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new UpsertQuestionSubjectTreeRequest
                {
                    Id = Guid.NewGuid(),
                    ParentQuestionSubjectTreeId = null,
                    QuestionId = Guid.NewGuid(),
                    Type = EVQuestionSubjectTreeType.Question,
                    Order = 1
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpsertQuestionTreeAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_AddNewNodes_UpdatesTreeSuccessfully()
    {
        // Arrange
        using var dbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var questionId = Guid.NewGuid();
        GenerateQuestion(dbContext, questionId);
        var questionId2 = Guid.NewGuid();
        GenerateQuestion(dbContext, questionId2);

        var subjectTree = new QuestionSubjectTree
        {
            QuestionId = questionId,
            Type = SEQuestionSubjectTreeType.Empty,
        };
        await dbContext.QuestionSubjectTree.AddAsync(subjectTree);
        await dbContext.SaveChangesAsync();

        var existingTree = new QuestionTree
        {
            Id = Guid.NewGuid(),
            Name = "Existing Tree",
            Order = 1,
            ParentQuestionTreeId = null,
            RootQuestionSubjectTreeId = subjectTree.Id
        };

        dbContext.QuestionTree.Add(existingTree);
        await dbContext.SaveChangesAsync();

        var service = new QuestionTreeUpsert(dbContext);

        var rootId = Guid.NewGuid();
        var request = new UpsertQuestionTreeRequest
        {
            Id = existingTree.Id,
            Name = "Updated Tree",
            Order = 2,
            ParentQuestionTreeId = null,
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new UpsertQuestionSubjectTreeRequest
                {
                    Id = rootId,
                    ParentQuestionSubjectTreeId = null,
                    QuestionId = questionId,
                    Type = EVQuestionSubjectTreeType.Question,
                    Order = 1
                },
                new UpsertQuestionSubjectTreeRequest
                {
                    Id = Guid.NewGuid(),
                    ParentQuestionSubjectTreeId = rootId,
                    QuestionId = questionId2,
                    Type = EVQuestionSubjectTreeType.Question,
                    Order = 2
                }
            }
        };

        // Act
        await service.UpsertQuestionTreeAsync(request);
        dbContext.ChangeTracker.Clear();

        // Assert
        var updatedTree = await dbContext.QuestionTree
            .Include(qt => qt.RootQuestionSubjectTree)
            .FirstOrDefaultAsync(qt => qt.Id == existingTree.Id);

        Assert.NotNull(updatedTree);

        var children = await GetSubjectTree(dbContext, updatedTree);

        children.Should().HaveCount(2);

        var subjectTrees = await GetSubjectTree(dbContext, updatedTree);

        var treeChild1 = request.Tree.ElementAt(0);
        ShouldBeSame(subjectTrees.First(e => e.Id == treeChild1.Id), treeChild1);
        var treeChild2 = request.Tree.ElementAt(1);
        ShouldBeSame(subjectTrees.First(e => e.Id == treeChild2.Id), treeChild2);
    }

    [Fact]
    public async Task UpdateAsync_RemoveOutdatedNodes_UpdatesTreeSuccessfully()
    {
        // Arrange
        using var dbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var questionId = Guid.NewGuid();
        GenerateQuestion(dbContext, questionId);

        var outdatedNode = new QuestionSubjectTree
        {
            Id = Guid.NewGuid(),
            ParentQuestionSubjectTreeId = null,
            QuestionId = questionId,
            Type = SEQuestionSubjectTreeType.Question,
            Order = 100
        };
        await dbContext.QuestionSubjectTree.AddAsync(outdatedNode);
        await dbContext.SaveChangesAsync();

        var existingTree = new QuestionTree
        {
            Id = Guid.NewGuid(),
            Name = "Existing Tree",
            Order = 1,
            ParentQuestionTreeId = null,
            RootQuestionSubjectTreeId = outdatedNode.Id
        };

        dbContext.QuestionTree.Add(existingTree);
        await dbContext.SaveChangesAsync();

        var service = new QuestionTreeUpsert(dbContext);

        var request = new UpsertQuestionTreeRequest
        {
            Id = existingTree.Id,
            Name = "Updated Tree",
            Order = 2,
            ParentQuestionTreeId = null,
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new UpsertQuestionSubjectTreeRequest
                {
                    Id = Guid.NewGuid(),
                    ParentQuestionSubjectTreeId = null,
                    QuestionId = questionId,
                    Type = EVQuestionSubjectTreeType.Question,
                    Order = 22
                }
            }
        };

        // Act
        await service.UpsertQuestionTreeAsync(request);
        dbContext.ChangeTracker.Clear();

        // Assert
        var deletedNode = await dbContext.QuestionSubjectTree.FindAsync(outdatedNode.Id);
        Assert.Null(deletedNode);

        var tree = await dbContext.QuestionTree.FindAsync(existingTree.Id);
        var child = await GetSubjectTree(dbContext, tree!);
        child.Should().HaveCount(1);
        ShouldBeSame(child[0], request.Tree.ElementAt(0));
    }

    private async Task<List<QuestionSubjectTree>> GetSubjectTree(AppDbContext dbContext, QuestionTree questionTree)
    {
        var children = await dbContext.QuestionSubjectTree.GetAllChildrenAsync(questionTree.RootQuestionSubjectTreeId, e => e.ParentQuestionSubjectTreeId, true, CancellationToken.None);
        return await dbContext.QuestionSubjectTree
            .Where(e => children.Contains(e.Id))
            .ToListAsync();
    }

    private void ShouldBeSame(QuestionSubjectTree actual, UpsertQuestionSubjectTreeRequest expected)
    {
        actual.Order.Should().Be(expected.Order);
        actual.ParentQuestionSubjectTreeId.Should().Be(expected.ParentQuestionSubjectTreeId);
        actual.QuestionId.Should().Be(expected.QuestionId);
        actual.Id.Should().Be(expected.Id);
    }

    private void GenerateQuestion(AppDbContext db, Guid id)
    {
        db.Questions.Add(new Question("Test " + id) { Id = id });
        db.SaveChanges();
        db.ChangeTracker.Clear();
    }
}
