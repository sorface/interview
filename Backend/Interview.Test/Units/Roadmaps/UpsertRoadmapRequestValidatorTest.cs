using FluentAssertions;
using Interview.Domain.Roadmaps;
using Interview.Domain.Roadmaps.UpsertRoadmap;

namespace Interview.Test.Units.Roadmaps;

public class UpsertRoadmapRequestValidatorTest
{
    private readonly UpsertRoadmapRequestValidator _validator = new();

    public static IEnumerable<object[]> Should_Return_Error_When_VerticalSplit_Have_Next_Non_Milestone_Item_Data
    {
        get
        {
            var qtId = Guid.NewGuid();

            var root1 = Guid.NewGuid();

            yield return new object[]
            {
                new List<UpsertRoadmapItemRequest>
                {
                    new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 },
                    new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
                    new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
                },
                2
            };

            yield return new object[]
            {
                new List<UpsertRoadmapItemRequest>
                {
                    new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 },
                    new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
                    new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
                    new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
                },
                2
            };

            yield return new object[]
            {
                new List<UpsertRoadmapItemRequest>
                {
                    new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 },
                    new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
                    new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
                    new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
                },
                2
            };
        }
    }

    [Fact]
    public void Should_Return_Error_When_Name_Is_Too_Short()
    {
        var request = new UpsertRoadmapRequest
        {
            Name = "A",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 }
            }
        };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.NameTooShort);
    }

    [Fact]
    public void Should_Return_Error_When_Description_Is_Too_Long()
    {
        var request = new UpsertRoadmapRequest
        {
            Name = "Aaaaa",
            Description = new string('A', 200),
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 }
            }
        };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.DescriptionTooLong);
    }

    [Fact]
    public void Should_Return_Error_When_Image_Too_Large()
    {
        var request = new UpsertRoadmapRequest
        {
            Name = "Aaaaa",
            ImageBase64 = new string('A', 300001),
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 }
            }
        };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.ImageTooLarge);
    }

    [Fact]
    public void Should_Return_Error_When_Items_Empty()
    {
        var request = new UpsertRoadmapRequest
        {
            Name = "Valid Name",
            Order = 0,
            Tags = new(),
            Items = new()
        };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.ItemsRequired);
    }

    [Fact]
    public void Should_Return_Error_When_First_Item_Not_Milestone()
    {
        var request = new UpsertRoadmapRequest
        {
            Name = "Valid Name",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = Guid.NewGuid(), Order = 0 }
            }
        };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.FirstMustBeMilestone);
    }

    [Fact]
    public void Should_Return_Error_For_Consecutive_VerticalSplits()
    {
        var request = new UpsertRoadmapRequest
        {
            Name = "Valid",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Type = EVRoadmapItemType.Milestone, Name = "Root", Order = 0 },
                new() { Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = Guid.NewGuid(), Order = 0 },
                new() { Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
                new() { Type = EVRoadmapItemType.VerticalSplit, Order = -1 }
            }
        };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.ConsecutiveVerticalSplit(3));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-2221)]
    [InlineData(int.MinValue)]
    public void Should_Return_Error_For_QuestionTree_With_Negative_Order(int order)
    {
        var request = new UpsertRoadmapRequest
        {
            Name = "Valid",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Type = EVRoadmapItemType.Milestone, Name = "Root", Order = 0 },
                new() { Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = Guid.NewGuid(), Order = order },
            }
        };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.InvalidOrderForQuestionTree(1));
    }

    [Fact]
    public void Should_Return_Error_If_QuestionTree_Has_No_Milestone()
    {
        var request = new UpsertRoadmapRequest
        {
            Name = "Valid",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = Guid.NewGuid(), Order = 0 }
            }
        };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.QuestionTreeMissingParent(0));
    }

    [Fact]
    public void Should_Return_Error_For_Duplicate_QuestionTree_Order()
    {
        var milestone = new UpsertRoadmapItemRequest { Type = EVRoadmapItemType.Milestone, Name = "Root", Order = 0 };
        var qt1 = new UpsertRoadmapItemRequest { Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = Guid.NewGuid(), Order = 1 };
        var qt2 = new UpsertRoadmapItemRequest { Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = Guid.NewGuid(), Order = 1 };

        var request = new UpsertRoadmapRequest
        {
            Name = "Valid",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest> { milestone, qt1, qt2 }
        };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.DuplicateOrderInQuestionTree(2));
    }

    [Fact]
    public void Should_Detect_Cycle_In_Milestone_Tree()
    {
        var id = Guid.NewGuid();

        var milestone1 = new UpsertRoadmapItemRequest { Id = id, Type = EVRoadmapItemType.Milestone, Name = "A", Order = 0 };
        var milestone2 = new UpsertRoadmapItemRequest { Id = id, Type = EVRoadmapItemType.Milestone, Name = "B", Order = -1 };

        var request = new UpsertRoadmapRequest
        {
            Name = "Valid",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest> { milestone1, milestone2 }
        };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.CycleError(id));
    }

    [Fact]
    public void Should_Return_Error_When_Milestone_Not_Followed_By_QuestionTree()
    {
        var request = new UpsertRoadmapRequest
        {
            Name = "Valid",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Type = EVRoadmapItemType.Milestone, Name = "M1", Order = 0 },
                new() { Type = EVRoadmapItemType.Milestone, Name = "M2", Order = 1 }
            }
        };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.MilestoneMustHaveFollowingQuestionTree(0));
    }

    [Fact]
    public void Should_Return_Error_When_VerticalSplit_Have_Name()
    {
        var qtId = Guid.NewGuid();

        var root1 = Guid.NewGuid();
        var root2 = Guid.NewGuid();
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = -1, Name = "N"},
                new() { Id = root2, Type = EVRoadmapItemType.Milestone, Name = "Next", Order = 1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1)
            .And.Contain(UpsertRoadmapRequestValidator.Errors.VerticalSplitMustNotHaveValues);
    }

    [Fact]
    public void Should_Return_Error_When_VerticalSplit_Have_QuestionTreeId()
    {
        var qtId = Guid.NewGuid();

        var root1 = Guid.NewGuid();
        var root2 = Guid.NewGuid();
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = -1, QuestionTreeId = Guid.NewGuid()},
                new() { Id = root2, Type = EVRoadmapItemType.Milestone, Name = "Next", Order = 1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1)
            .And.Contain(UpsertRoadmapRequestValidator.Errors.VerticalSplitMustNotHaveValues);
    }

    [Fact]
    public void Should_Return_Error_When_VerticalSplit_Have_Prev_QuestionTree_Item()
    {
        var root1 = Guid.NewGuid();

        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
            },
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.VerticalSplitWithoutQuestion(1));
    }

    [Fact]
    public void Should_Return_Error_When_VerticalSplit_Have_Prev_VerticalSplit_Item()
    {
        var root1 = Guid.NewGuid();

        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = Guid.NewGuid(), Order = 1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
            },
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.VerticalSplitWithoutQuestion(3));
    }

    [Theory]
    [MemberData(nameof(Should_Return_Error_When_VerticalSplit_Have_Next_Non_Milestone_Item_Data))]
    public void Should_Return_Error_When_VerticalSplit_Have_Next_Non_Milestone_Item(List<UpsertRoadmapItemRequest> items, int index)
    {
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = items,
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.VerticalSplitWithoutMilestone(index));
    }

    [Fact]
    public void Should_Return_Error_When_VerticalSplit_Have_Order()
    {
        var qtId = Guid.NewGuid();

        var root1 = Guid.NewGuid();
        var root2 = Guid.NewGuid();
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = 1 },
                new() { Id = root2, Type = EVRoadmapItemType.Milestone, Name = "Next", Order = 1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1)
            .And.Contain(UpsertRoadmapRequestValidator.Errors.VerticalSplitMustNotHaveValues);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1")]
    [InlineData("  ")]
    [InlineData("   ")]
    public void Should_Return_Error_When_Milestone_Have_Invalid_Name(string milestoneName)
    {
        var qtId = Guid.NewGuid();

        var root1 = Guid.NewGuid();
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = milestoneName, Order = 0 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1)
            .And.Contain(UpsertRoadmapRequestValidator.Errors.MilestoneMustHaveNameAtLeast2Characters);
    }

    [Fact]
    public void Should_Return_Error_When_Milestone_Have_QuestionTreeId()
    {
        var qtId = Guid.NewGuid();

        var root1 = Guid.NewGuid();
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Milestone", Order = 0, QuestionTreeId = Guid.NewGuid() },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1)
            .And.Contain(UpsertRoadmapRequestValidator.Errors.MilestoneMustNotHaveQuestionTreeId);
    }

    [Fact]
    public void Should_Return_Error_When_QuestionTree_Not_Have_QuestionTreeId()
    {
        var root1 = Guid.NewGuid();
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Milestone", Order = 0 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = null, Order = 1 },
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1)
            .And.Contain(UpsertRoadmapRequestValidator.Errors.QuestionTreeMustHaveQuestionTreeId);
    }

    [Fact]
    public void Should_Return_Error_When_QuestionTree_Have_Name()
    {
        var qtId = Guid.NewGuid();

        var root1 = Guid.NewGuid();
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Milestone", Order = 0 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Name = "Test", Order = 1 },
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1)
            .And.Contain(UpsertRoadmapRequestValidator.Errors.QuestionTreeMustNotHaveName);
    }

    [Fact]
    public void Should_Return_Error_When_Child_Mildstone_Have_Invalid_Order()
    {
        var qtId = Guid.NewGuid();

        var root1 = Guid.NewGuid();
        var root1child1 = Guid.NewGuid();
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
                new() { Id = root1child1, Type = EVRoadmapItemType.Milestone, Name = "Start 2", Order = 1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.InvalidOrderForChildMilestone(2));
    }

    [Fact]
    public void Should_Pass_For_Valid_Request()
    {
        var qtId = Guid.NewGuid();

        var root1 = Guid.NewGuid();
        var root1child1 = Guid.NewGuid();
        var root2 = Guid.NewGuid();
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
                new() { Id = root1child1, Type = EVRoadmapItemType.Milestone, Name = "Start 2", Order = -1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
                new() { Id = root2, Type = EVRoadmapItemType.Milestone, Name = "Next", Order = 1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Tree.RootMilestones.Should().HaveCount(2);
        result.Tree.RootMilestones[0].Milestone.Id.Should().Be(root1);
        result.Tree.RootMilestones[0].Children.Should().HaveCount(1);
        result.Tree.RootMilestones[0].Children[0].Milestone.Id.Should().Be(root1child1);
        result.Tree.RootMilestones[1].Milestone.Id.Should().Be(root2);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-2131)]
    [InlineData(int.MinValue)]
    public void Should_Return_Error_For_Root_Milestone_With_Invalid_Order(int rootMilestoneOrder)
    {
        var qtId = Guid.NewGuid();

        var root1 = Guid.NewGuid();
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = rootMilestoneOrder },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.RootMilestoneMustHaveUniqueOrder(0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2131)]
    [InlineData(int.MinValue)]
    public void Should_Return_Error_For_NewRoot_Milestone_With_Invalid_Order(int rootMilestoneOrder)
    {
        var qtId = Guid.NewGuid();

        var root1 = Guid.NewGuid();
        var root2 = Guid.NewGuid();
        var request = new UpsertRoadmapRequest
        {
            Name = "My Roadmap",
            Order = 0,
            Tags = new(),
            Items = new List<UpsertRoadmapItemRequest>
            {
                new() { Id = root1, Type = EVRoadmapItemType.Milestone, Name = "Start", Order = 0 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.VerticalSplit, Order = -1 },
                new() { Id = root2, Type = EVRoadmapItemType.Milestone, Name = "Next", Order = rootMilestoneOrder },
                new() { Id = Guid.NewGuid(), Type = EVRoadmapItemType.QuestionTree, QuestionTreeId = qtId, Order = 1 },
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(UpsertRoadmapRequestValidator.Errors.NewRootMilestoneMustHaveUniqueOrder(3));
    }
}
