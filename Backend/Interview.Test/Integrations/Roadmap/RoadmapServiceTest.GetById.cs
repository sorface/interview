using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Roadmaps;
using Interview.Domain.Roadmaps.RoadmapById;
using Interview.Domain.Roadmaps.UpsertRoadmap;
using Interview.Domain.Rooms;
using Interview.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace Interview.Test.Integrations.Roadmap;

public partial class RoadmapServiceTest
{
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectRoadmapResponse()
    {
        using var context = new TestAppDbContextFactory().Create(new TestSystemClock());

        await SeedTestDataAsync(context);

        var roadmapService = new RoadmapService(context);

        // Arrange
        var expectedId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var result = await roadmapService.GetByIdAsync(expectedId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedId);
        result.Name.Should().Be("Test Roadmap");
        result.Order.Should().Be(1);

        result.Tags.Should().HaveCount(1);
        result.Tags.First().Value.Should().Be("Tag1");

        result.Items.Should().HaveCount(14);
        CheckMilestone(result.Items[0], new Guid("00000000-0000-0000-0000-000000000001"));
        CheckQuestionTree(result.Items[1], new Guid("00000000-0000-0000-0000-000000000011"));
        CheckQuestionTree(result.Items[2], new Guid("00000000-0000-0000-0000-000000000021"));
        CheckVerticalSplit(result.Items[3]);
        CheckMilestone(result.Items[4], new Guid("00000000-0000-0000-0000-000000000002"));
        CheckQuestionTree(result.Items[5], new Guid("00000000-0000-0000-0000-000000000012"));
        CheckMilestone(result.Items[6], new Guid("00000000-0000-0000-0000-000000000003"));
        CheckQuestionTree(result.Items[7], new Guid("00000000-0000-0000-0000-000000000013"));
        CheckQuestionTree(result.Items[8], new Guid("00000000-0000-0000-0000-000000000023"));
        CheckQuestionTree(result.Items[9], new Guid("00000000-0000-0000-0000-000000000033"));
        CheckMilestone(result.Items[10], new Guid("00000000-0000-0000-0000-000000000004"));
        CheckQuestionTree(result.Items[11], new Guid("00000000-0000-0000-0000-000000000014"));
        CheckMilestone(result.Items[12], new Guid("00000000-0000-0000-0000-000000000005"));
        CheckQuestionTree(result.Items[13], new Guid("00000000-0000-0000-0000-000000000015"));
        return;

        static void CheckMilestone(RoadmapItemResponse item, Guid id)
        {
            item.Type.Should().Be(EVRoadmapItemType.Milestone);
            item.Id.Should().Be(id);
        }

        static void CheckQuestionTree(RoadmapItemResponse item, Guid id)
        {
            item.Type.Should().Be(EVRoadmapItemType.QuestionTree);
            item.Id.Should().Be(id);
        }

        static void CheckVerticalSplit(RoadmapItemResponse item)
        {
            item.Type.Should().Be(EVRoadmapItemType.VerticalSplit);
        }
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ThrowsNotFoundException()
    {
        using var context = new TestAppDbContextFactory().Create(new TestSystemClock());

        await SeedTestDataAsync(context);

        var roadmapService = new RoadmapService(context);

        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await roadmapService.Invoking(s => s.GetByIdAsync(nonExistentId, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage(NotFoundException.Create<Domain.Roadmaps.Roadmap>(nonExistentId).Message);
    }

    private async Task SeedTestDataAsync(AppDbContext context)
    {
        var roadmapId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var tagId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var questionTreeId1 = Guid.Parse("00000000-0000-0000-0000-000000000006");
        var questionTreeId2 = Guid.Parse("00000000-0000-0000-0000-000000000007");
        var roomId = Guid.Parse("00000000-0000-0000-0000-000000000008");

        await CreateQuestionTreeAsync(context, questionTreeId1);
        await CreateQuestionTreeAsync(context, questionTreeId2);

        var roadmap = new Domain.Roadmaps.Roadmap
        {
            Id = roadmapId,
            Name = "Test Roadmap",
            Order = 1,
            Tags = new List<Tag> { new() { Id = tagId, Value = "Tag1", HexColor = "#FF0000" } },
            Milestones = new List<RoadmapMilestone>
            {
                new RoadmapMilestone()
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Name = "Milestone 1",
                    Order = 1,
                    RoadmapId = roadmapId,
                    Items = new List<RoadmapMilestoneItem>
                    {
                        new()
                        {
                            Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                            QuestionTreeId = questionTreeId1,
                            Order = 1,
                            RoadmapMilestoneId = default,
                        },
                        new()
                        {
                            Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                            QuestionTreeId = questionTreeId2,
                            Order = 2,
                            RoadmapMilestoneId = default
                        }
                    },
                    ParentRoadmapMilestoneId = null
                },
                new RoadmapMilestone()
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Name = "Milestone 2",
                    Order = 2,
                    RoadmapId = roadmapId,
                    Items = new List<RoadmapMilestoneItem>
                    {
                        new()
                        {
                            Id = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                            QuestionTreeId = questionTreeId1,
                            Order = 1,
                            RoadmapMilestoneId = default,
                        },
                    },
                    ParentRoadmapMilestoneId = null,
                    ChildrenMilestones = new List<RoadmapMilestone>
                    {
                        new RoadmapMilestone()
                        {
                            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                            Name = "Milestone 2.Child 1",
                            Order = 1,
                            RoadmapId = roadmapId,
                            Items = new List<RoadmapMilestoneItem>
                            {
                                new()
                                {
                                    Id = Guid.Parse("00000000-0000-0000-0000-000000000013"),
                                    QuestionTreeId = questionTreeId1,
                                    Order = 1,
                                    RoadmapMilestoneId = default,
                                },
                                new()
                                {
                                    Id = Guid.Parse("00000000-0000-0000-0000-000000000023"),
                                    QuestionTreeId = questionTreeId1,
                                    Order = 1,
                                    RoadmapMilestoneId = default,
                                },
                                new()
                                {
                                    Id = Guid.Parse("00000000-0000-0000-0000-000000000033"),
                                    QuestionTreeId = questionTreeId1,
                                    Order = 1,
                                    RoadmapMilestoneId = default,
                                },
                            },
                            ParentRoadmapMilestoneId = null,
                        },
                        new RoadmapMilestone()
                        {
                            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                            Name = "Milestone 2.Child 2",
                            Order = 2,
                            RoadmapId = roadmapId,
                            Items = new List<RoadmapMilestoneItem>
                            {
                                new()
                                {
                                    Id = Guid.Parse("00000000-0000-0000-0000-000000000014"),
                                    QuestionTreeId = questionTreeId1,
                                    Order = 1,
                                    RoadmapMilestoneId = default,
                                },
                            },
                            ParentRoadmapMilestoneId = null,
                        },
                        new RoadmapMilestone()
                        {
                            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                            Name = "Milestone 2.Child 3",
                            Order = 3,
                            RoadmapId = roadmapId,
                            Items = new List<RoadmapMilestoneItem>
                            {
                                new()
                                {
                                    Id = Guid.Parse("00000000-0000-0000-0000-000000000015"),
                                    QuestionTreeId = questionTreeId1,
                                    Order = 1,
                                    RoadmapMilestoneId = default,
                                },
                            },
                            ParentRoadmapMilestoneId = null,
                        }
                    }
                }
            }
        };
        context.Roadmap.Add(roadmap);
        context.SaveChanges();

        context.Rooms.Add(new Room("Test", SERoomAccessType.Public, SERoomType.Standard)
        {
            Id = roomId,
            QuestionTreeId = questionTreeId1
        });
        context.SaveChanges();

        context.ChangeTracker.Clear();
    }
}
