using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Questions;
using Interview.Domain.Roadmaps;
using Interview.Domain.Roadmaps.UpsertRoadmap;
using Interview.Domain.Tags;

namespace Interview.Test.Integrations;

public class RoadmapServiceTest
{
    [Fact]
    public async Task UpsertAsync_WhenRequestIdIsNull_ShouldCreateNewRoadmap()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var questionTree = new QuestionTree
        {
            Name = "Test",
            RootQuestionSubjectTree = new QuestionSubjectTree { QuestionId = null, Type = SEQuestionSubjectTreeType.Question, },
            RootQuestionSubjectTreeId = default
        };
        await dbContext.QuestionTree.AddAsync(questionTree);
        await dbContext.SaveChangesAsync();

        var roadmapService = new RoadmapService(dbContext);

        // Arrange
        var request = new UpsertRoadmapRequest
        {
            Id = null,
            Name = "New Roadmap",
            Order = 1,
            Tags = [],
            Items =
            [
                new UpsertRoadmapItemRequest
                {
                    Name = "New Milestone",
                    Order = 0,
                    Type = EVRoadmapItemType.Milestone
                },
                new UpsertRoadmapItemRequest
                {
                    Order = 0,
                    Type = EVRoadmapItemType.QuestionTree,
                    QuestionTreeId = questionTree.Id
                },
            ]
        };

        // Act
        var result = await roadmapService.UpsertAsync(request, CancellationToken.None);

        // Assert
        result.Value.Should().NotBeEmpty();

        var createdRoadmap = await dbContext.Roadmap.FindAsync(result.Value);
        createdRoadmap.Should().NotBeNull();
        createdRoadmap!.Name.Should().Be(request.Name);
        createdRoadmap.Order.Should().Be(request.Order);
    }

    [Fact]
    public async Task UpsertAsync_WhenRequestIdIsNotNull_ShouldUpdateRoadmap()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext);

        // Arrange
        var existingRoadmap = new Roadmap
        {
            Id = Guid.NewGuid(),
            Name = "Existing Roadmap",
            Order = 1,
            Tags = new List<Tag>(),
            Milestones = new List<RoadmapMilestone>()
        };

        await dbContext.Roadmap.AddAsync(existingRoadmap);

        var questionTree = new QuestionTree
        {
            Name = "Test",
            RootQuestionSubjectTree = new QuestionSubjectTree { QuestionId = null, Type = SEQuestionSubjectTreeType.Question, },
            RootQuestionSubjectTreeId = default
        };
        await dbContext.QuestionTree.AddAsync(questionTree);
        await dbContext.SaveChangesAsync();

        var request = new UpsertRoadmapRequest
        {
            Id = existingRoadmap.Id,
            Name = "Updated Roadmap",
            Order = 2,
            Tags = [],
            Items =
            [
                new UpsertRoadmapItemRequest
                {
                    Name = "New Milestone",
                    Order = 0,
                    Type = EVRoadmapItemType.Milestone
                },
                new UpsertRoadmapItemRequest
                {
                    Order = 0,
                    Type = EVRoadmapItemType.QuestionTree,
                    QuestionTreeId = questionTree.Id
                },
            ]
        };

        // Act
        var result = await roadmapService.UpsertAsync(request, CancellationToken.None);

        // Assert
        result.Value.Should().Be(existingRoadmap.Id);

        var updatedRoadmap = await dbContext.Roadmap.FindAsync(existingRoadmap.Id);
        updatedRoadmap!.Name.Should().Be(request.Name);
        updatedRoadmap.Order.Should().Be(request.Order);
    }

    [Fact]
    public async Task UpsertAsync_WhenOrderIsNotUnique_ShouldThrowUserException()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext);

        // Arrange
        var existingRoadmap = new Roadmap
        {
            Id = Guid.NewGuid(),
            Name = "Existing Roadmap",
            Order = 1,
            Tags = new List<Tag>(),
            Milestones = new List<RoadmapMilestone>()
        };

        await dbContext.Roadmap.AddAsync(existingRoadmap);
        await dbContext.SaveChangesAsync();

        var request = new UpsertRoadmapRequest
        {
            Id = null, // Создаём новый Roadmap
            Name = "New Roadmap",
            Order = 1, // Такой же Order, как у существующего
            Tags = [],
            Items = []
        };

        // Act & Assert
        await Assert.ThrowsAsync<UserException>(() =>
            roadmapService.UpsertAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task UpsertAsync_WhenTagsDoNotExist_ShouldThrowNotFoundException()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext);

        var questionTree = new QuestionTree
        {
            Name = "Test",
            RootQuestionSubjectTree = new QuestionSubjectTree { QuestionId = null, Type = SEQuestionSubjectTreeType.Question, },
            RootQuestionSubjectTreeId = default
        };
        await dbContext.QuestionTree.AddAsync(questionTree);
        await dbContext.SaveChangesAsync();

        // Arrange
        var nonExistentTagId = Guid.NewGuid();

        var request = new UpsertRoadmapRequest
        {
            Id = null,
            Name = "New Roadmap",
            Order = 1,
            Tags = [nonExistentTagId], // Несуществующий тег
            Items =
            [

                new UpsertRoadmapItemRequest
                {
                    Name = "New Milestone",
                    Order = 0,
                    Type = EVRoadmapItemType.Milestone
                },
                new UpsertRoadmapItemRequest
                {
                    Order = 0,
                    Type = EVRoadmapItemType.QuestionTree,
                    QuestionTreeId = questionTree.Id
                },
            ]
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            roadmapService.UpsertAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task UpsertAsync_WhenQuestionTreeDoesNotExist_ShouldThrowNotFoundException()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext);

        // Arrange
        var nonExistentQuestionTreeId = Guid.NewGuid();

        var request = new UpsertRoadmapRequest
        {
            Id = null,
            Name = "New Roadmap",
            Order = 1,
            Tags = [],
            Items =
            [
                new UpsertRoadmapItemRequest
                {
                    Name = "New Item",
                    Type = EVRoadmapItemType.Milestone,
                    Order = 0
                },
                new UpsertRoadmapItemRequest
                {
                    Type = EVRoadmapItemType.QuestionTree,
                    Order = 1,
                    QuestionTreeId = nonExistentQuestionTreeId // Несуществующий QuestionTree
                }
            ]
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            roadmapService.UpsertAsync(request, CancellationToken.None));
    }
}
