using FluentAssertions;
using Interview.Domain.Questions;
using Interview.Domain.Questions.UpsertQuestionTree;

namespace Interview.Test.Units;

public class UpsertQuestionTreeRequestTest
{
    public static IEnumerable<object[]> ValidTreeData
    {
        get
        {
            var parentNodeId = Guid.NewGuid();
            yield return new object[]
            {
                new List<UpsertQuestionSubjectTreeRequest>
                {
                    new()
                    {
                        Id = parentNodeId,
                        ParentQuestionSubjectTreeId = null,
                        QuestionId = Guid.NewGuid(),
                        Type = EVQuestionSubjectTreeType.Question,
                        Order = 1
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ParentQuestionSubjectTreeId = parentNodeId,
                        QuestionId = Guid.NewGuid(),
                        Type = EVQuestionSubjectTreeType.Question,
                        Order = 2
                    }
                }
            };

            var node2lvl = Guid.NewGuid();
            yield return new object[]
            {
                new List<UpsertQuestionSubjectTreeRequest>
                {
                    new()
                    {
                        Id = parentNodeId,
                        ParentQuestionSubjectTreeId = null,
                        QuestionId = Guid.NewGuid(),
                        Type = EVQuestionSubjectTreeType.Question,
                        Order = 1
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ParentQuestionSubjectTreeId = parentNodeId,
                        QuestionId = Guid.NewGuid(),
                        Type = EVQuestionSubjectTreeType.Question,
                        Order = 1
                    },
                    new()
                    {
                        Id = node2lvl,
                        ParentQuestionSubjectTreeId = parentNodeId,
                        QuestionId = Guid.NewGuid(),
                        Type = EVQuestionSubjectTreeType.Question,
                        Order = 2
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ParentQuestionSubjectTreeId = node2lvl,
                        QuestionId = Guid.NewGuid(),
                        Type = EVQuestionSubjectTreeType.Question,
                        Order = 1
                    },
                }
            };

            var node3lvl = Guid.NewGuid();
            yield return new object[]
            {
                new List<UpsertQuestionSubjectTreeRequest>
                {
                    new()
                    {
                        Id = parentNodeId,
                        ParentQuestionSubjectTreeId = null,
                        QuestionId = Guid.NewGuid(),
                        Type = EVQuestionSubjectTreeType.Question,
                        Order = 1
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ParentQuestionSubjectTreeId = parentNodeId,
                        QuestionId = Guid.NewGuid(),
                        Type = EVQuestionSubjectTreeType.Question,
                        Order = 1
                    },
                    new()
                    {
                        Id = node2lvl,
                        ParentQuestionSubjectTreeId = parentNodeId,
                        QuestionId = Guid.NewGuid(),
                        Type = EVQuestionSubjectTreeType.Question,
                        Order = 2
                    },
                    new()
                    {
                        Id = node3lvl,
                        ParentQuestionSubjectTreeId = node2lvl,
                        QuestionId = Guid.NewGuid(),
                        Type = EVQuestionSubjectTreeType.Question,
                        Order = 1
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ParentQuestionSubjectTreeId = node3lvl,
                        QuestionId = Guid.NewGuid(),
                        Type = EVQuestionSubjectTreeType.Question,
                        Order = 1
                    },
                }
            };
        }
    }

    [Fact]
    public void IsValidTree_WhenTreeHasNoNodes_ShouldReturnFalse()
    {
        // Arrange
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>(),
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Tree has no nodes");
    }

    [Fact]
    public void IsValidTree_WhenTreeHasEmptyNode_With_Question_ShouldReturnFalse()
    {
        // Arrange
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new UpsertQuestionSubjectTreeRequest
                {
                    QuestionId = Guid.NewGuid(),
                    Type = EVQuestionSubjectTreeType.Empty,
                    Order = 0
                }
            },
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Tree has empty node with question");
    }

    [Fact]
    public void IsValidTree_WhenTreeHasQuestionNode_Without_Question_ShouldReturnFalse()
    {
        // Arrange
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new UpsertQuestionSubjectTreeRequest
                {
                    QuestionId = null,
                    Type = EVQuestionSubjectTreeType.Question,
                    Order = 0
                }
            },
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Tree has question node without question");
    }

    [Fact]
    public void IsValidTree_WhenTreeHasNoParentNode_ShouldReturnFalse()
    {
        // Arrange
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new() { Id = Guid.NewGuid(), ParentQuestionSubjectTreeId = Guid.NewGuid(), QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 1 }
            },
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Tree has no parent node");
    }

    [Fact]
    public void IsValidTree_WhenTreeHasMoreThanOneParentNode_ShouldReturnFalse()
    {
        // Arrange
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new() { Id = Guid.NewGuid(), ParentQuestionSubjectTreeId = null, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 1 },
                new() { Id = Guid.NewGuid(), ParentQuestionSubjectTreeId = null, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 2 }
            },
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Tree has more than one parent node");
    }

    [Fact]
    public void IsValidTree_WhenTreeHasDuplicateNodeIds_ShouldReturnFalse()
    {
        // Arrange
        var duplicateId = Guid.NewGuid();
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new() { Id = duplicateId, ParentQuestionSubjectTreeId = null, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 1 },
                new() { Id = duplicateId, ParentQuestionSubjectTreeId = Guid.NewGuid(), QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 2 }
            },
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be($"Duplicate node id: {duplicateId}");
    }

    [Fact]
    public void IsValidTree_WhenTreeHasCycle_ShouldReturnFalse()
    {
        // Arrange
        var node1Id = Guid.NewGuid();
        var node2Id = Guid.NewGuid();
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new() { Id = Guid.NewGuid(), ParentQuestionSubjectTreeId = null, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 1 },
                new() { Id = node1Id, ParentQuestionSubjectTreeId = node2Id, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 1 },
                new() { Id = node2Id, ParentQuestionSubjectTreeId = node1Id, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 2 }
            },
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Tree has cycle");
    }

    [Theory]
    [MemberData(nameof(ValidTreeData))]
    public void IsValidTree_WhenTreeIsValid_ShouldReturnTrue(List<UpsertQuestionSubjectTreeRequest> tree)
    {
        // Arrange
        var request = new UpsertQuestionTreeRequest
        {
            Tree = tree,
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeTrue();
        errorMessage.Should().BeEmpty();
    }
}
