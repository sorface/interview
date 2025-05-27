using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Roadmaps;
using Interview.Domain.Roadmaps.UpsertRoadmap;
using Interview.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace Interview.Test.Integrations.Roadmap;

public partial class RoadmapServiceTest
{
    [Fact]
    public async Task UpsertAsync_WhenRequestIdIsNull_ShouldCreateNewRoadmap()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var questionTree = await CreateQuestionTreeAsync(dbContext);

        var roadmapService = new RoadmapService(dbContext, new ArchiveService<Domain.Roadmaps.Roadmap>(dbContext));

        // Arrange
        var request = new UpsertRoadmapRequest
        {
            Id = null,
            Name = "New Roadmap",
            Order = 1,
            Tags = [],
            Items =
            [
                new UpsertRoadmapItemRequest { Name = "New Milestone", Order = 0, Type = EVRoadmapItemType.Milestone },
                new UpsertRoadmapItemRequest { Order = 0, Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = questionTree.Id },
            ]
        };

        // Act
        var result = await roadmapService.UpsertAsync(request, CancellationToken.None);

        // Assert
        result.Value.Should().NotBeEmpty();

        var createdRoadmap = await dbContext.Roadmap
            .Include(e => e.Milestones)
            .ThenInclude(e => e.Items)
            .FirstAsync(e => e.Id == result.Value);
        createdRoadmap.Should().NotBeNull();
        createdRoadmap!.Name.Should().Be(request.Name);
        createdRoadmap.Order.Should().Be(request.Order);
        createdRoadmap.Milestones.Should().HaveCount(1).And.ContainSingle(e => e.Name == "New Milestone");
        createdRoadmap.Milestones[0].Items.Should().HaveCount(1).And.ContainSingle(e => e.QuestionTreeId == questionTree.Id);
    }

    [Fact]
    public async Task UpsertAsync_WhenRequestIdIsNotNull_ShouldUpdateRoadmap()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext, new ArchiveService<Domain.Roadmaps.Roadmap>(dbContext));

        // Arrange
        var existingRoadmap = new Domain.Roadmaps.Roadmap
        {
            Id = Guid.NewGuid(),
            Name = "Existing Roadmap",
            Order = 1,
            Tags = new List<Tag>(),
            Milestones = new List<RoadmapMilestone>()
        };

        await dbContext.Roadmap.AddAsync(existingRoadmap);

        var questionTree = await CreateQuestionTreeAsync(dbContext);

        var request = new UpsertRoadmapRequest
        {
            Id = existingRoadmap.Id,
            Name = "Updated Roadmap",
            Order = 2,
            Tags = [],
            Items =
            [
                new UpsertRoadmapItemRequest { Name = "New Milestone", Order = 0, Type = EVRoadmapItemType.Milestone },
                new UpsertRoadmapItemRequest { Order = 0, Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = questionTree.Id },
            ]
        };

        // Act
        var result = await roadmapService.UpsertAsync(request, CancellationToken.None);

        // Assert
        result.Value.Should().Be(existingRoadmap.Id);

        var updatedRoadmap = await dbContext.Roadmap
            .Include(e => e.Milestones)
            .ThenInclude(e => e.Items)
            .FirstAsync(e => e.Id == result.Value);
        updatedRoadmap!.Name.Should().Be(request.Name);
        updatedRoadmap.Order.Should().Be(request.Order);
        updatedRoadmap.Milestones.Should().HaveCount(1).And.ContainSingle(e => e.Name == "New Milestone");
        updatedRoadmap.Milestones[0].Items.Should().HaveCount(1).And.ContainSingle(e => e.QuestionTreeId == questionTree.Id);
    }

    [Fact]
    public async Task UpsertAsync_WhenOrderIsNotUnique_ShouldThrowUserException()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext, new ArchiveService<Domain.Roadmaps.Roadmap>(dbContext));

        // Arrange
        var existingRoadmap = new Domain.Roadmaps.Roadmap
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
        await Assert.ThrowsAsync<UserException>(() => roadmapService.UpsertAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task UpsertAsync_WhenTagsDoNotExist_ShouldThrowNotFoundException()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext, new ArchiveService<Domain.Roadmaps.Roadmap>(dbContext));

        var questionTree = await CreateQuestionTreeAsync(dbContext);

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

                new UpsertRoadmapItemRequest { Name = "New Milestone", Order = 0, Type = EVRoadmapItemType.Milestone },
                new UpsertRoadmapItemRequest { Order = 0, Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = questionTree.Id },
            ]
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => roadmapService.UpsertAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task UpsertAsync_WhenQuestionTreeDoesNotExist_ShouldThrowNotFoundException()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext, new ArchiveService<Domain.Roadmaps.Roadmap>(dbContext));

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
                new UpsertRoadmapItemRequest { Name = "New Item", Type = EVRoadmapItemType.Milestone, Order = 0 },
                // Несуществующий QuestionTree
                new UpsertRoadmapItemRequest { Type = EVRoadmapItemType.QuestionTree, Order = 1, QuestionTreeId = nonExistentQuestionTreeId }
            ]
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => roadmapService.UpsertAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task UpsertAsync_FlatList_CreatesValidTree()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext, new ArchiveService<Domain.Roadmaps.Roadmap>(dbContext));

        // Arrange
        var tags = new[] { new Tag { Value = "T1" }, new Tag { Value = "T2" }, new Tag { Value = "T3" }, };
        dbContext.Tag.AddRange(tags);
        await dbContext.SaveChangesAsync();

        var questionTree = await CreateQuestionTreeAsync(dbContext);
        var questionTreeId = questionTree.Id;

        var request = new UpsertRoadmapRequest
        {
            Name = "Flat Tree",
            Order = 1,
            Items = new List<UpsertRoadmapItemRequest>
            {
                // Root Milestone 1
                new() { Type = EVRoadmapItemType.Milestone, Name = "M1", Order = 1 },
                new() { Type = EVRoadmapItemType.QuestionTree, Order = 1, QuestionTreeId = questionTreeId },
                new() { Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
                
                // Root Milestone 2
                new() { Type = EVRoadmapItemType.Milestone, Name = "M2", Order = 2 },
                new() { Type = EVRoadmapItemType.QuestionTree, Order = 1, QuestionTreeId = questionTreeId }
            },
            Tags = tags.Select(e => e.Id).ToHashSet()
        };

        // Act
        var result = await roadmapService.UpsertAsync(request, CancellationToken.None);

        // Assert
        var roadmap = await dbContext.Roadmap
            .Include(e => e.Tags)
            .Include(r => r.Milestones)
            .ThenInclude(m => m.Items)
            .FirstAsync(e => e.Id == result.Value);

        roadmap.Tags.Should().HaveCount(3)
            .And.ContainSingle(e => e.Id == tags[0].Id)
            .And.ContainSingle(e => e.Id == tags[1].Id)
            .And.ContainSingle(e => e.Id == tags[2].Id);

        roadmap.Milestones.Should().HaveCount(2);
        roadmap.Milestones[0].Items.Should().ContainSingle();
        roadmap.Milestones[1].Items.Should().ContainSingle();

        roadmap.Milestones.Should().HaveCount(2)
            .And.ContainSingle(e => e.Name == "M1" && e.Items.Count == 1 && e.Items[0].QuestionTreeId == questionTreeId)
            .And.ContainSingle(e => e.Name == "M2" && e.Items.Count == 1 && e.Items[0].QuestionTreeId == questionTreeId);
    }

    [Fact]
    public async Task UpsertAsync_NestedStructureViaFlatList_CreatesHierarchy()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext, new ArchiveService<Domain.Roadmaps.Roadmap>(dbContext));
        var questionTree1 = await CreateQuestionTreeAsync(dbContext);
        var questionTree2 = await CreateQuestionTreeAsync(dbContext);

        // Arrange
        var request = new UpsertRoadmapRequest
        {
            Name = "Nested via Flat",
            Order = 1,
            Items = new List<UpsertRoadmapItemRequest>
            {
                // Root Milestone
                new() { Type = EVRoadmapItemType.Milestone, Name = "Root", Order = 1 },
                new() { Type = EVRoadmapItemType.QuestionTree, Order = 1, QuestionTreeId = questionTree1.Id },
                new() { Type = EVRoadmapItemType.Milestone, Name = "Child", Order = -1 },
                new() { Type = EVRoadmapItemType.QuestionTree, Order = 1, QuestionTreeId = questionTree2.Id }
            },
            Tags = new HashSet<Guid>()
        };

        // Act
        var result = await roadmapService.UpsertAsync(request, CancellationToken.None);

        // Assert
        var root = await dbContext.Roadmap
            .Include(e => e.Milestones)
            .ThenInclude(m => m.ChildrenMilestones)
            .ThenInclude(e => e.Items)
            .FirstAsync(e => e.Id == result.Value);

        root.Name.Should().Be(request.Name);
        root.Order.Should().Be(request.Order);

        root.Milestones.Should().HaveCount(2);
        var rootMilestone = root.Milestones.Single(e => e.ParentRoadmapMilestoneId is null);
        rootMilestone.Name.Should().Be("Root");
        rootMilestone.Order.Should().Be(1);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        rootMilestone.Items.Should().NotBeNull().And.HaveCount(1).And.ContainSingle(e => e.QuestionTreeId == questionTree1.Id);

        var childMilestone = root.Milestones.Single(e => e.ParentRoadmapMilestoneId is not null);
        childMilestone.Name.Should().Be("Child");
        childMilestone.Order.Should().Be(-1);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        childMilestone.Items.Should().NotBeNull().And.HaveCount(1).And.ContainSingle(e => e.QuestionTreeId == questionTree2.Id);
    }

    [Fact]
    public async Task UpsertAsync_UpdateExistingStructure_ModifiesTree()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext, new ArchiveService<Domain.Roadmaps.Roadmap>(dbContext));

        // Arrange
        var existingRoadmap = new Domain.Roadmaps.Roadmap
        {
            Milestones = new List<RoadmapMilestone>
            {
                new() {
                    Id = Guid.NewGuid(),
                    Order = 1,
                    Items = new List<RoadmapMilestoneItem>(),
                    Name = "frst",
                    RoadmapId = default,
                    ParentRoadmapMilestoneId = null
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Order = 2,
                    Items = new List<RoadmapMilestoneItem>(),
                    Name = "scnd",
                    RoadmapId = default,
                    ParentRoadmapMilestoneId = null
                }
            },
            Name = "Test",
            Order = 0
        };
        await dbContext.AddAsync(existingRoadmap);
        await dbContext.SaveChangesAsync();
        var questionTree1 = await CreateQuestionTreeAsync(dbContext);

        var updateRequest = new UpsertRoadmapRequest
        {
            Id = existingRoadmap.Id,
            Items = new List<UpsertRoadmapItemRequest>
            {
                // Обновленный корневой Milestone
                new() { Type = EVRoadmapItemType.Milestone, Id = existingRoadmap.Milestones[0].Id, Order = 1, Name = "Updated" },
                new() { Type = EVRoadmapItemType.QuestionTree, Order = 1, QuestionTreeId = questionTree1.Id },
                new() { Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
                
                // Новый корневой Milestone
                new() { Type = EVRoadmapItemType.Milestone, Order = 2, Name = "New Root" },
                new() { Type = EVRoadmapItemType.QuestionTree, Order = 1, QuestionTreeId = questionTree1.Id },
            },
            Name = "New name",
            Order = 0,
            Tags = new HashSet<Guid>()
        };

        // Act
        var result = await roadmapService.UpsertAsync(updateRequest, CancellationToken.None);

        // Assert
        var updated = await dbContext.Roadmap
            .Include(r => r.Milestones)
            .ThenInclude(e => e.Items)
            .FirstAsync(e => e.Id == result.Value);

        updated.Name.Should().Be("New name");
        updated.Order.Should().Be(0);

        updated.Milestones.Should().HaveCount(2);
        var rootMilestone = updated.Milestones.Single(e => e.ParentRoadmapMilestoneId is null && e.Name == "Updated");
        rootMilestone.Name.Should().Be("Updated");
        rootMilestone.Order.Should().Be(1);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        rootMilestone.Items.Should().NotBeNull().And.HaveCount(1).And.ContainSingle(e => e.QuestionTreeId == questionTree1.Id);

        var childMilestone = updated.Milestones.Single(e => e.ParentRoadmapMilestoneId is null && e.Name == "New Root");
        childMilestone.Name.Should().Be("New Root");
        childMilestone.Order.Should().Be(2);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        childMilestone.Items.Should().NotBeNull().And.HaveCount(1).And.ContainSingle(e => e.QuestionTreeId == questionTree1.Id);
    }

    [Fact]
    public async Task UpsertAsync_ComplexTreeWithVerticalSplit_BuildsCorrectly()
    {
        var testSystemClock = new TestSystemClock();
        await using var dbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var roadmapService = new RoadmapService(dbContext, new ArchiveService<Domain.Roadmaps.Roadmap>(dbContext));
        var questionTree1 = await CreateQuestionTreeAsync(dbContext);
        var questionTree2 = await CreateQuestionTreeAsync(dbContext);
        var questionTree3 = await CreateQuestionTreeAsync(dbContext);
        var questionTree4 = await CreateQuestionTreeAsync(dbContext);

        // Arrange
        var request = new UpsertRoadmapRequest
        {
            Name = "Complex Tree",
            Order = 0,
            Tags = new HashSet<Guid>(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                // Root 1
                new() { Type = EVRoadmapItemType.Milestone, Order = 1, Name = "Root 1" },
                new() { Type = EVRoadmapItemType.QuestionTree, Order = 1, QuestionTreeId = questionTree1.Id },
                new() { Type = EVRoadmapItemType.VerticalSplit, Order = -1 },

                // Root 2
                new() { Type = EVRoadmapItemType.Milestone, Order = 2, Name = "Root 2" },
                new() { Type = EVRoadmapItemType.QuestionTree, Order = 1, QuestionTreeId = questionTree2.Id },

                // Child для Root 2
                new() { Type = EVRoadmapItemType.Milestone, Order = -1, Name = "Root 2.Child1" },
                new() { Type = EVRoadmapItemType.QuestionTree, Order = 1, QuestionTreeId = questionTree3.Id },
                new() { Type = EVRoadmapItemType.QuestionTree, Order = 2, QuestionTreeId = questionTree4.Id }
            }
        };

        // Act
        var result = await roadmapService.UpsertAsync(request, CancellationToken.None);

        // Assert
        var roadmap = await dbContext.Roadmap
            .Include(r => r.Milestones)
            .ThenInclude(m => m.ChildrenMilestones)
            .ThenInclude(e => e.Items)
            .FirstAsync(e => e.Id == result.Value);

        roadmap.Milestones.Should().HaveCount(3);
        var rootMilestone = roadmap.Milestones.Single(e => e.ParentRoadmapMilestoneId is null && e.Name == "Root 1");
        rootMilestone.Name.Should().Be("Root 1");
        rootMilestone.Order.Should().Be(1);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        rootMilestone.Items.Should().NotBeNull().And.HaveCount(1).And.ContainSingle(e => e.QuestionTreeId == questionTree1.Id);

        var rootMilestone2 = roadmap.Milestones.Single(e => e.ParentRoadmapMilestoneId is null && e.Name == "Root 2");
        rootMilestone2.Name.Should().Be("Root 2");
        rootMilestone2.Order.Should().Be(2);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        rootMilestone2.Items.Should().NotBeNull().And.HaveCount(1).And.ContainSingle(e => e.QuestionTreeId == questionTree2.Id);

        var childMilestone = roadmap.Milestones.Single(e => e.ParentRoadmapMilestoneId == rootMilestone2.Id);
        childMilestone.Name.Should().Be("Root 2.Child1");
        childMilestone.Order.Should().Be(-1);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        childMilestone.Items.Should().NotBeNull().And.HaveCount(2)
            .And.ContainSingle(e => e.QuestionTreeId == questionTree3.Id && e.Order == 1)
            .And.ContainSingle(e => e.QuestionTreeId == questionTree4.Id && e.Order == 2);
    }

    private async Task<QuestionTree> CreateQuestionTreeAsync(AppDbContext db, Guid? questionTreeId = null)
    {
        var questionTree = new QuestionTree
        {
            Id = questionTreeId ?? Guid.NewGuid(),
            Name = Guid.NewGuid().ToString("N"),
            RootQuestionSubjectTreeId = default,
            RootQuestionSubjectTree = new QuestionSubjectTree
            {
                QuestionId = null,
                Type = SEQuestionSubjectTreeType.Question,
                Question = new Question("test")
            }
        };
        await db.QuestionTree.AddAsync(questionTree);
        await db.SaveChangesAsync();
        return questionTree;
    }
}
